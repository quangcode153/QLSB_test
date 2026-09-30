using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SportChain.Shared.Constants;
using SportChain.Shared.Enums;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Hubs;
using SportChain.WebApi.Repositories;

namespace SportChain.WebApi.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly ICourtRepository _courtRepo;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEmailService _emailService;
    private readonly IHubContext<CourtHub> _hubContext;
    private readonly SportChain.WebApi.Data.AppDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepo,
        ICourtRepository courtRepo,
        IQrCodeService qrCodeService,
        IEmailService emailService,
        IHubContext<CourtHub> hubContext,
        SportChain.WebApi.Data.AppDbContext context,
        ILogger<BookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _courtRepo = courtRepo;
        _qrCodeService = qrCodeService;
        _emailService = emailService;
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task<Booking> CreateHoldingBookingAsync(int customerId, int courtId, DateOnly date, List<int> slotIds)
    {
        if (slotIds == null || slotIds.Count == 0)
        {
            throw new ArgumentException("Vui lòng chọn ít nhất một ca để đặt sân.");
        }

        if (date < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new InvalidOperationException("Không thể đặt sân vào ngày đã qua trong quá khứ.");
        }

        var court = await _courtRepo.GetByIdAsync(courtId);
        if (court == null || !court.IsActive)
        {
            throw new InvalidOperationException("Sân không tồn tại hoặc đã ngừng hoạt động.");
        }

        if (court.Status == SlotStatus.Maintenance)
        {
            throw new InvalidOperationException("Sân này hiện đang trong trạng thái bảo trì, tạm thời không nhận đặt lịch.");
        }

        var timeSlots = await _courtRepo.GetAllTimeSlotsAsync();
        var selectedSlots = timeSlots.Where(s => slotIds.Contains(s.Id)).ToList();
        if (selectedSlots.Count != slotIds.Count)
        {
            throw new ArgumentException("Một hoặc nhiều ca giờ được chọn không tồn tại.");
        }

        // Chặn chọn ca giờ đã kết thúc nếu đặt cho ngày hôm nay (Rule 06 Anti-Hoarding & Logic Integrity)
        if (date == DateOnly.FromDateTime(DateTime.Today))
        {
            var nowTime = DateTime.Now.TimeOfDay;
            var pastSlot = selectedSlots.FirstOrDefault(s => s.EndTime <= nowTime);
            if (pastSlot != null)
            {
                throw new InvalidOperationException($"Ca '{pastSlot.DisplayLabel}' đã kết thúc trong ngày hôm nay, không thể đặt chỗ.");
            }
        }

        // Chống đầu cơ/Spam/DoS: 1 khách chỉ được có tối đa 1 đơn PendingPayment đang trong thời hạn 15 phút
        var hasActivePending = await _context.Bookings
            .AnyAsync(b => b.CustomerId == customerId 
                        && b.Status == BookingStatus.PendingPayment 
                        && b.ExpireHoldingAt > DateTime.UtcNow);

        if (hasActivePending)
        {
            throw new InvalidOperationException("Bạn đang có một đơn giữ chỗ chờ thanh toán cọc chưa hoàn tất (tối đa 1 đơn giữ chỗ cùng lúc). Vui lòng thanh toán cọc hoặc hủy đơn trước khi giữ thêm ca mới.");
        }

        // Bọc trong Database Transaction với IsolationLevel RepeatableRead để triệt tiêu Race Conditions
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            // 1. Kiểm tra chống trùng lịch: Xem các ca được chọn đã có ai đặt/giữ chưa
            foreach (var slotId in slotIds)
            {
                var isBooked = await _bookingRepo.IsSlotAlreadyBookedAsync(courtId, date, slotId);
                if (isBooked)
                {
                    throw new InvalidOperationException($"Ca giờ này vừa được một khách hàng khác thao tác giữ trước đó ít giây. Vui lòng chọn ca giờ khác!");
                }
            }

            // 2. Tính toán tiền thuê theo bảng giá PriceRule (fallback chuẩn theo Rule BR-PRICE-01)
            var priceRules = await _courtRepo.GetPriceRulesAsync(courtId);

            decimal totalAmount = 0;
            var details = new List<BookingDetail>();

            foreach (var slot in selectedSlots)
            {
                bool isPeak = slot.StartTime >= new TimeSpan(17, 0, 0) && slot.StartTime < new TimeSpan(21, 0, 0);
                var rule = priceRules.FirstOrDefault(r => r.TimeSlotId == slot.Id && (r.DayOfWeek == null || r.DayOfWeek == date.DayOfWeek));
                decimal slotPrice = rule?.PricePerHour ?? SystemPolicies.GetStandardPrice(court.SportType, isPeak);

                totalAmount += slotPrice;
                details.Add(new BookingDetail
                {
                    TimeSlotId = slot.Id,
                    SlotPrice = slotPrice
                });
            }

            decimal depositAmount = totalAmount * SystemPolicies.DefaultDepositPercentage;

            // 3. Khởi tạo đơn đặt với trạng thái PendingPayment và hạn giữ 15 phút
            var booking = new Booking
            {
                CustomerId = customerId,
                BranchId = court.BranchId,
                CourtId = courtId,
                BookingDate = date,
                TotalAmount = totalAmount,
                DepositAmount = depositAmount,
                Status = BookingStatus.PendingPayment,
                BookingCode = $"SC-{date:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
                CreatedAt = DateTime.UtcNow,
                ExpireHoldingAt = DateTime.UtcNow.AddMinutes(SystemPolicies.DefaultDepositHoldingMinutes),
                Details = details
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Ghi nhật ký kiểm toán cho hành động giữ chỗ 15 phút
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = customerId,
                Action = "CreateHoldingBooking",
                EntityName = nameof(Booking),
                EntityId = booking.Id,
                Details = $"Khách hàng giữ chỗ 15 phút ca sân {court.Name} ({string.Join(", ", selectedSlots.Select(s => s.DisplayLabel))}) ngày {date:dd/MM/yyyy}. Cọc 30%: {booking.DepositAmount:N0} đ.",
                IpAddress = "API",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("✅ [Booking] Đã tạo đơn giữ cọc 15p an toàn: {BookingCode}, Tổng tiền: {Total}, Cọc: {Deposit}", 
                booking.BookingCode, booking.TotalAmount, booking.DepositAmount);

            // 4. Phát sóng Realtime qua SignalR cho tất cả client đang xem chi nhánh này đổi màu sang Vàng (Holding) kèm ngày
            var dateStr = date.ToString("yyyy-MM-dd");
            foreach (var slotId in slotIds)
            {
                await _hubContext.Clients.Group($"Branch_{court.BranchId}")
                    .SendAsync("OnSlotStatusChanged", courtId, slotId, SlotStatus.Holding, dateStr);
            }

            return booking;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Xảy ra xung đột giữ chỗ cùng thời điểm. Ca giờ này đã có người thao tác, vui lòng chọn ca khác.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ConfirmDepositAsync(int bookingId, PaymentMethod paymentMethod)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null) return false;

        if (booking.Status != BookingStatus.PendingPayment)
        {
            _logger.LogWarning("Đơn {BookingCode} không ở trạng thái chờ cọc ({Status})", booking.BookingCode, booking.Status);
            return false;
        }

        if (DateTime.UtcNow > booking.ExpireHoldingAt)
        {
            _logger.LogWarning("Đơn {BookingCode} đã quá 15 phút giữ chỗ", booking.BookingCode);
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Quá hạn 15 phút đặt cọc";
            booking.CancelledAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = booking.CustomerId,
                Action = "AutoReleaseHoldingAtDeposit",
                EntityName = nameof(Booking),
                EntityId = booking.Id,
                Details = $"Đơn {booking.BookingCode} bị hủy do quá 15 phút giữ chỗ khi đang thực hiện đặt cọc.",
                IpAddress = "PAYMENT_GATEWAY",
                Timestamp = DateTime.UtcNow
            });

            await _bookingRepo.SaveChangesAsync();

            // Phát sóng giải phóng slot về Available kèm ngày
            var dateStr = booking.BookingDate.ToString("yyyy-MM-dd");
            foreach (var detail in booking.Details)
            {
                await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                    .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Available, dateStr);
            }

            return false;
        }

        booking.Status = BookingStatus.Confirmed;
        booking.PaymentMethod = paymentMethod;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = booking.CustomerId,
            Action = "ConfirmDeposit",
            EntityName = nameof(Booking),
            EntityId = booking.Id,
            Details = $"Xác nhận thanh toán cọc thành công {booking.DepositAmount:N0} đ (30%) cho đơn {booking.BookingCode} qua {paymentMethod}.",
            IpAddress = "PAYMENT_GATEWAY",
            Timestamp = DateTime.UtcNow
        });

        await _bookingRepo.SaveChangesAsync();

        // Phát sóng Realtime sang Đỏ (Đã đặt) kèm ngày
        var confirmedDateStr = booking.BookingDate.ToString("yyyy-MM-dd");
        foreach (var detail in booking.Details)
        {
            await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Booked, confirmedDateStr);
        }

        // Sinh mã QR và gửi email vé điện tử
        var qrBase64 = _qrCodeService.GenerateQrCodeBase64(booking.CheckInCode);
        if (booking.Customer != null && !string.IsNullOrEmpty(booking.Customer.Email))
        {
            _ = _emailService.SendBookingSuccessEmailAsync(booking.Customer.Email, booking.BookingCode, booking.Court?.Name ?? "Sân Thể Thao", qrBase64);
        }

        _logger.LogInformation("✅ [Booking] Đơn {BookingCode} đã xác nhận cọc thành công.", booking.BookingCode);
        return true;
    }

    public async Task<(bool Success, string Message)> CheckInWithQrAsync(string checkInCode, int? staffBranchId = null, bool isSuperAdmin = false, int? staffUserId = null)
    {
        var booking = await _bookingRepo.GetByCheckInCodeAsync(checkInCode);
        if (booking == null)
        {
            _logger.LogWarning("Mã check-in không tồn tại: {Code}", checkInCode);
            return (false, "Mã check-in không tồn tại hoặc không hợp lệ.");
        }

        // Quy tắc BR-MULTI-01: Kiểm tra quyền chi nhánh của nhân viên lễ tân
        if (!isSuperAdmin && staffBranchId.HasValue && booking.BranchId != staffBranchId.Value)
        {
            _logger.LogWarning("Nhân viên chi nhánh {StaffBranch} cố gắng check-in đơn của chi nhánh {BookingBranch}", staffBranchId, booking.BranchId);
            return (false, $"Bạn không có quyền check-in đơn của cơ sở khác (Đơn này thuộc cơ sở ID {booking.BranchId})!");
        }

        if (booking.Status == BookingStatus.InUse)
        {
            return (false, "Đơn đặt sân này đã được check-in trước đó!");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            _logger.LogWarning("Đơn {BookingCode} ở trạng thái {Status}, không thể check-in", booking.BookingCode, booking.Status);
            return (false, $"Đơn đặt sân đang ở trạng thái '{booking.Status}', không thể nhận sân.");
        }

        // Quy tắc BR-POS-01: Kiểm tra ngày và cửa sổ giờ check-in [-15p, +15p]
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (booking.BookingDate < today)
        {
            return (false, "Đơn đặt sân này đã quá hạn ngày thi đấu trong quá khứ!");
        }
        if (booking.BookingDate > today)
        {
            return (false, $"Chưa đến ngày nhận sân! Ngày chơi của đơn này là {booking.BookingDate:dd/MM/yyyy}.");
        }

        // Lấy giờ bắt đầu ca sớm nhất trong đơn
        var earliestStartTime = booking.Details.Any()
            ? booking.Details.Min(d => d.TimeSlot.StartTime)
            : new TimeSpan(6, 0, 0);

        var nowTime = DateTime.Now.TimeOfDay;
        var diffMinutes = (nowTime - earliestStartTime).TotalMinutes;

        if (diffMinutes < -15)
        {
            return (false, $"Chưa đến giờ nhận sân. Ca bắt đầu lúc {earliestStartTime:hh\\:mm}. Bạn chỉ có thể check-in trước giờ chơi tối đa 15 phút!");
        }

        var bookingDateStr = booking.BookingDate.ToString("yyyy-MM-dd");

        if (diffMinutes > 15)
        {
            // Quá 15 phút tính từ giờ bắt đầu ca mà không check-in -> Tự động đánh dấu No-show theo BR-POS-02
            booking.Status = BookingStatus.NoShow;
            booking.CancellationReason = "Khách vắng mặt quá 15 phút tính từ giờ bắt đầu ca (No-show tại quầy). Toàn bộ tiền cọc bị tịch thu.";

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = staffUserId,
                Action = "MarkNoShowAtCheckIn",
                EntityName = nameof(Booking),
                EntityId = booking.Id,
                Details = $"Khách quá hạn check-in 15 phút tại cơ sở ID {booking.BranchId}. Đơn chuyển sang NoShow, tịch thu {booking.DepositAmount:N0} đ tiền cọc.",
                IpAddress = "POS_TERMINAL",
                Timestamp = DateTime.UtcNow
            });

            await _bookingRepo.SaveChangesAsync();

            // QUY TẮC BR-POS-02: Giải phóng sân về Available để bán lại cho khách vãng lai
            foreach (var detail in booking.Details)
            {
                await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                    .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Available, bookingDateStr);
            }

            return (false, "Đã quá thời hạn nhận sân 15 phút! Đơn đã bị đánh dấu vắng mặt (No-show) và tiền cọc không được hoàn lại.");
        }

        booking.Status = BookingStatus.InUse;
        booking.CheckedInAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = staffUserId,
            Action = "QrCheckIn",
            EntityName = nameof(Booking),
            EntityId = booking.Id,
            Details = $"Khách check-in QR thành công tại cơ sở ID {booking.BranchId}. Sân chuyển sang trạng thái đang sử dụng.",
            IpAddress = "POS_TERMINAL",
            Timestamp = DateTime.UtcNow
        });

        await _bookingRepo.SaveChangesAsync();

        // Broadcast Realtime sang InUse kèm ngày
        foreach (var detail in booking.Details)
        {
            await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.InUse, bookingDateStr);
        }

        _logger.LogInformation("✅ [CheckIn] Đơn {BookingCode} đã check-in thành công lúc {Time}", booking.BookingCode, booking.CheckedInAt);
        return (true, "Check-in thành công! Khách hàng có thể vào sân thi đấu.");
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, string reason, int? currentUserId = null, UserRole? userRole = null, int? staffBranchId = null)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null) return (false, "Không tìm thấy đơn đặt sân.");

        // Kiểm tra quyền hủy đơn (Rule BR-AUTH-01 & BR-MULTI-01):
        // Nếu là Customer: Chỉ được phép hủy đơn của chính mình
        if (userRole == UserRole.Customer && currentUserId.HasValue && booking.CustomerId != currentUserId.Value)
        {
            return (false, "Bạn chỉ có quyền hủy đơn đặt của chính mình!");
        }

        // Nếu là Staff (Receptionist, BranchManager): Chỉ được phép can thiệp đơn thuộc chi nhánh mình
        if ((userRole == UserRole.Receptionist || userRole == UserRole.BranchManager) 
            && staffBranchId.HasValue && booking.BranchId != staffBranchId.Value)
        {
            return (false, "Bạn không có quyền can thiệp vào đơn của cơ sở khác!");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return (false, "Đơn đặt sân này đã được hủy trước đó.");
        }

        if (booking.Status == BookingStatus.Completed)
        {
            return (false, "Đơn đặt sân đã hoàn tất thi đấu, không thể hủy.");
        }

        if (booking.Status == BookingStatus.InUse)
        {
            return (false, "Sân đang trong giờ thi đấu, không thể hủy!");
        }

        decimal refund = 0;
        string auditAction = "CustomerCancelBooking";

        if (userRole == UserRole.SuperAdmin || userRole == UserRole.BranchManager)
        {
            // Quy tắc BR-PRICE-03: Ban điều hành hủy vì lý do bất khả kháng (thời tiết, bảo trì, sự cố)
            // Tự động hoàn trả 100% tiền cọc cho khách hàng bất kể thời gian
            if (booking.Status == BookingStatus.Confirmed)
            {
                refund = booking.DepositAmount;
            }
            auditAction = "AdminForceCancel";
        }
        else if (userRole == UserRole.Receptionist)
        {
            // Lễ tân hỗ trợ khách hủy tại quầy theo biểu phí BR-PRICE-02
            auditAction = "StaffCancelBooking";
            if (booking.Status == BookingStatus.Confirmed)
            {
                var earliestStartTime = booking.Details.Any()
                    ? booking.Details.Min(d => d.TimeSlot.StartTime)
                    : new TimeSpan(6, 0, 0);

                var startDateTime = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(earliestStartTime));
                var hoursUntilPlay = (startDateTime - DateTime.Now).TotalHours;

                if (hoursUntilPlay >= 24)
                {
                    refund = booking.DepositAmount; // Hoàn 100% tiền cọc
                }
                else if (hoursUntilPlay >= 12)
                {
                    refund = booking.DepositAmount * 0.5m; // Hoàn 50% tiền cọc
                }
                else
                {
                    refund = 0; // Sát giờ: Phạt 100% cọc
                }
            }
        }
        else
        {
            // Khách hàng tự hủy trên website / desktop: Quy tắc BR-PRICE-02
            auditAction = "CustomerCancelBooking";
            if (booking.Status == BookingStatus.Confirmed)
            {
                var earliestStartTime = booking.Details.Any()
                    ? booking.Details.Min(d => d.TimeSlot.StartTime)
                    : new TimeSpan(6, 0, 0);

                var startDateTime = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(earliestStartTime));
                var hoursUntilPlay = (startDateTime - DateTime.Now).TotalHours;

                if (hoursUntilPlay >= 24)
                {
                    refund = booking.DepositAmount; // Hoàn 100% tiền cọc
                }
                else if (hoursUntilPlay >= 12)
                {
                    refund = booking.DepositAmount * 0.5m; // Hoàn 50% tiền cọc
                }
                else
                {
                    refund = 0; // Sát giờ: Phạt 100% cọc
                }
            }
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = reason;
        booking.CancelledAt = DateTime.UtcNow;
        booking.RefundAmount = refund;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = auditAction,
            EntityName = nameof(Booking),
            EntityId = booking.Id,
            Details = $"Hủy đơn {booking.BookingCode} ({auditAction}). Lý do: {reason}. Hoàn cọc: {refund:N0} đ.",
            IpAddress = "API",
            Timestamp = DateTime.UtcNow
        });

        await _bookingRepo.SaveChangesAsync();

        // Giải phóng slot về Xanh lá (Trống) kèm ngày
        var cancelDateStr = booking.BookingDate.ToString("yyyy-MM-dd");
        foreach (var detail in booking.Details)
        {
            await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Available, cancelDateStr);
        }

        _logger.LogInformation("⚠️ [Cancel] Đơn {BookingCode} đã hủy. Action: {Action}, Lý do: {Reason}, Hoàn tiền: {Refund:N0} đ", 
            booking.BookingCode, auditAction, reason, refund);

        var msg = auditAction == "AdminForceCancel"
            ? $"Đã hủy đơn bất khả kháng thành công (Lý do: {reason}). Tiền cọc {refund:N0} đ được hoàn trả 100% cho khách hàng."
            : (refund > 0 
                ? $"Đã hủy đơn thành công. Tiền cọc được hoàn lại theo quy định: {refund:N0} đ."
                : "Đã hủy đơn thành công.");

        return (true, msg);
    }

    public async Task AutoReleaseExpiredHoldingsAsync()
    {
        var expiredBookings = await _bookingRepo.GetExpiredHoldingsAsync(DateTime.UtcNow);
        if (expiredBookings.Count == 0) return;

        _logger.LogInformation("🔄 [Worker] Phát hiện {Count} đơn giữ cọc quá 15 phút, đang tự động giải phóng...", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Hệ thống tự động hủy: Quá thời hạn 15 phút giữ chỗ cọc.";
            booking.CancelledAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = null,
                Action = "AutoReleaseHolding",
                EntityName = nameof(Booking),
                EntityId = booking.Id,
                Details = $"Hệ thống tự động giải phóng ca cho đơn giữ cọc {booking.BookingCode} do quá thời hạn 15 phút.",
                IpAddress = "SYSTEM_WORKER",
                Timestamp = DateTime.UtcNow
            });

            var holdingDateStr = booking.BookingDate.ToString("yyyy-MM-dd");
            foreach (var detail in booking.Details)
            {
                await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                    .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Available, holdingDateStr);
            }
        }

        await _bookingRepo.SaveChangesAsync();
    }

    public async Task AutoMarkNoShowsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var graceThreshold = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMinutes(SystemPolicies.DefaultCheckInGraceMinutes));

        var noShowBookings = await _bookingRepo.GetNoShowBookingsAsync(today, graceThreshold);
        if (noShowBookings.Count == 0) return;

        _logger.LogInformation("⚠️ [Worker] Phát hiện {Count} đơn quá 15 phút sau giờ bắt đầu ca mà khách không check-in, đánh dấu No-show...", noShowBookings.Count);

        foreach (var booking in noShowBookings)
        {
            booking.Status = BookingStatus.NoShow;
            booking.CancellationReason = "Khách vắng mặt quá 15 phút tính từ giờ bắt đầu ca (No-show). Toàn bộ tiền cọc không được hoàn lại.";

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = null,
                Action = "AutoMarkNoShow",
                EntityName = nameof(Booking),
                EntityId = booking.Id,
                Details = $"Hệ thống tự động đánh dấu No-show cho đơn {booking.BookingCode} do quá 15 phút giờ bắt đầu ca mà khách không check-in.",
                IpAddress = "SYSTEM_WORKER",
                Timestamp = DateTime.UtcNow
            });

            // Chỉ giải phóng ca trên lưới thời gian thực nếu đơn thuộc ngày hôm nay
            if (booking.BookingDate == today)
            {
                var todayStr = today.ToString("yyyy-MM-dd");
                foreach (var detail in booking.Details)
                {
                    await _hubContext.Clients.Group($"Branch_{booking.BranchId}")
                        .SendAsync("OnSlotStatusChanged", booking.CourtId, detail.TimeSlotId, SlotStatus.Available, todayStr);
                }
            }
        }

        await _bookingRepo.SaveChangesAsync();
    }
}
