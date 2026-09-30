using Microsoft.EntityFrameworkCore;
using SportChain.Shared.Enums;
using SportChain.WebApi.Entities;
using SportChain.WebApi.Security;

namespace SportChain.WebApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
    {
        // 1. Seed Nền tảng ban đầu (nếu chưa có)
        if (!await context.Branches.AnyAsync())
        {
            await SeedCoreEntitiesAsync(context, hasher);
        }

        // 2. Seed Toàn bộ Dữ liệu Nghiệp Vụ Bổ Sung (nếu chưa có)
        if (!await context.FacilityEquipments.AnyAsync())
        {
            await SeedOperationalDataAsync(context);
        }

        // 3. Đảm bảo toàn bộ tài khoản mẫu cho các Role luôn sẵn sàng để test
        await SeedMissingUsersAsync(context, hasher);
    }

    private static async Task SeedCoreEntitiesAsync(AppDbContext context, IPasswordHasher hasher)
    {
        // Ca Giờ Chuẩn Hóa (TimeSlots - 1 tiếng/ca từ 06:00 đến 23:00)
        var timeSlots = new List<TimeSlot>();
        for (int hour = 6; hour < 23; hour++)
        {
            timeSlots.Add(new TimeSlot
            {
                StartTime = new TimeSpan(hour, 0, 0),
                EndTime = new TimeSpan(hour + 1, 0, 0)
            });
        }
        await context.TimeSlots.AddRangeAsync(timeSlots);
        await context.SaveChangesAsync();

        // 3 Chi Nhánh Đại Diện
        var branchCauGiay = new Branch
        {
            Name = "SportChain Cầu Giấy",
            Address = "Số 12 Dịch Vọng Hậu, Quận Cầu Giấy, Hà Nội",
            City = "Hà Nội",
            Latitude = 21.0313,
            Longitude = 105.7832,
            PhoneNumber = "0243.888.666",
            OpenTime = new TimeSpan(6, 0, 0),
            CloseTime = new TimeSpan(23, 0, 0),
            IsApproved = true,
            IsActive = true
        };

        var branchHaiBaTrung = new Branch
        {
            Name = "SportChain Hai Bà Trưng",
            Address = "Số 45 Đại Cồ Việt, Quận Hai Bà Trưng, Hà Nội",
            City = "Hà Nội",
            Latitude = 21.0089,
            Longitude = 105.8455,
            PhoneNumber = "0243.999.777",
            OpenTime = new TimeSpan(6, 0, 0),
            CloseTime = new TimeSpan(23, 0, 0),
            IsApproved = true,
            IsActive = true
        };

        var branchDongDa = new Branch
        {
            Name = "SportChain Đống Đa",
            Address = "Số 88 Thái Hà, Quận Đống Đa, Hà Nội",
            City = "Hà Nội",
            Latitude = 21.0145,
            Longitude = 105.8174,
            PhoneNumber = "0243.777.555",
            OpenTime = new TimeSpan(6, 0, 0),
            CloseTime = new TimeSpan(23, 0, 0),
            IsApproved = true,
            IsActive = true
        };

        await context.Branches.AddRangeAsync(branchCauGiay, branchHaiBaTrung, branchDongDa);
        await context.SaveChangesAsync();

        // Sân Thể Thao
        var courts = new List<Court>
        {
            new Court { BranchId = branchCauGiay.Id, Name = "Sân Cầu Lông 1 (Thảm Yonex)", SportType = SportType.Badminton, SurfaceType = "Trong nhà - Thảm PVC", Status = SlotStatus.Available },
            new Court { BranchId = branchCauGiay.Id, Name = "Sân Cầu Lông 2 (Thảm Yonex)", SportType = SportType.Badminton, SurfaceType = "Trong nhà - Thảm PVC", Status = SlotStatus.Available },
            new Court { BranchId = branchCauGiay.Id, Name = "Sân Cầu Lông 3 (VIP)", SportType = SportType.Badminton, SurfaceType = "Trong nhà - Gỗ phong", Status = SlotStatus.Available },
            new Court { BranchId = branchCauGiay.Id, Name = "Sân Bóng Đá Mini 1", SportType = SportType.Football, SurfaceType = "Cỏ nhân tạo 7 người", Status = SlotStatus.Available },
            new Court { BranchId = branchCauGiay.Id, Name = "Sân Pickleball 1", SportType = SportType.Pickleball, SurfaceType = "Sơn Acrylic chuẩn giải", Status = SlotStatus.Available },

            new Court { BranchId = branchHaiBaTrung.Id, Name = "Sân Cầu Lông 1", SportType = SportType.Badminton, SurfaceType = "Trong nhà - Thảm PVC", Status = SlotStatus.Available },
            new Court { BranchId = branchHaiBaTrung.Id, Name = "Sân Cầu Lông 2", SportType = SportType.Badminton, SurfaceType = "Trong nhà - Thảm PVC", Status = SlotStatus.Available },
            new Court { BranchId = branchHaiBaTrung.Id, Name = "Sân Tennis 1", SportType = SportType.Tennis, SurfaceType = "Mặt sân cứng Hard Court", Status = SlotStatus.Available },

            new Court { BranchId = branchDongDa.Id, Name = "Sân Pickleball VIP 1", SportType = SportType.Pickleball, SurfaceType = "Sơn đệm Silicon PU", Status = SlotStatus.Available },
            new Court { BranchId = branchDongDa.Id, Name = "Sân Pickleball VIP 2", SportType = SportType.Pickleball, SurfaceType = "Sơn đệm Silicon PU", Status = SlotStatus.Available },
            new Court { BranchId = branchDongDa.Id, Name = "Sân Bóng Rổ Trong Nhà", SportType = SportType.Basketball, SurfaceType = "Sàn gỗ thi đấu", Status = SlotStatus.Available }
        };
        await context.Courts.AddRangeAsync(courts);
        await context.SaveChangesAsync();

        // Bảng Giá (PriceRules)
        var priceRules = new List<PriceRule>();
        foreach (var court in courts)
        {
            decimal normalPrice = court.SportType switch
            {
                SportType.Badminton => 100_000m,
                SportType.Football => 350_000m,
                SportType.Pickleball => 120_000m,
                SportType.Tennis => 250_000m,
                SportType.Basketball => 200_000m,
                _ => 150_000m
            };

            decimal peakPrice = court.SportType switch
            {
                SportType.Badminton => 160_000m,
                SportType.Football => 550_000m,
                SportType.Pickleball => 190_000m,
                SportType.Tennis => 350_000m,
                SportType.Basketball => 300_000m,
                _ => 220_000m
            };

            foreach (var slot in timeSlots)
            {
                bool isPeak = slot.StartTime.Hours >= 17 && slot.StartTime.Hours < 21;
                priceRules.Add(new PriceRule
                {
                    CourtId = court.Id,
                    TimeSlotId = slot.Id,
                    IsPeakHour = isPeak,
                    PricePerHour = isPeak ? peakPrice : normalPrice
                });
            }
        }
        await context.PriceRules.AddRangeAsync(priceRules);
        await context.SaveChangesAsync();

        // Users
        var defaultPasswordHash = hasher.HashPassword("123456");
        var users = new List<User>
        {
            new User { FullName = "Nguyễn Hoàng Admin", Email = "admin@sportchain.vn", PhoneNumber = "0988111222", PasswordHash = defaultPasswordHash, Role = UserRole.SuperAdmin, IsActive = true },
            new User { FullName = "Trần Văn Quản Lý", Email = "manager.caugiay@sportchain.vn", PhoneNumber = "0988333444", PasswordHash = defaultPasswordHash, Role = UserRole.BranchManager, BranchId = branchCauGiay.Id, IsActive = true },
            new User { FullName = "Lê Thị Lễ Tân", Email = "reception.caugiay@sportchain.vn", PhoneNumber = "0988555666", PasswordHash = defaultPasswordHash, Role = UserRole.Receptionist, BranchId = branchCauGiay.Id, IsActive = true },
            new User { FullName = "Phạm Minh Khách", Email = "khachhang@gmail.com", PhoneNumber = "0977888999", PasswordHash = defaultPasswordHash, Role = UserRole.Customer, IsActive = true }
        };
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Dịch vụ
        var services = new List<ServiceItem>
        {
            new ServiceItem { BranchId = branchCauGiay.Id, Name = "Nước tăng lực Revive bù khoáng", Category = "Drink", Price = 15_000m, IsRental = false, IsActive = true },
            new ServiceItem { BranchId = branchCauGiay.Id, Name = "Nước suối Aquafina 500ml", Category = "Drink", Price = 10_000m, IsRental = false, IsActive = true },
            new ServiceItem { BranchId = branchCauGiay.Id, Name = "Thuê vợt Yonex Astrox", Category = "Rental", Price = 30_000m, IsRental = true, IsActive = true },
            new ServiceItem { BranchId = branchCauGiay.Id, Name = "Khăn lạnh ướp đá cao cấp", Category = "Equipment", Price = 5_000m, IsRental = false, IsActive = true },
            new ServiceItem { BranchId = branchHaiBaTrung.Id, Name = "Nước điện giải Pocari Sweat", Category = "Drink", Price = 20_000m, IsRental = false, IsActive = true },
            new ServiceItem { BranchId = branchHaiBaTrung.Id, Name = "Thuê bóng Tennis tiêu chuẩn", Category = "Rental", Price = 40_000m, IsRental = true, IsActive = true }
        };
        await context.ServiceItems.AddRangeAsync(services);
        await context.SaveChangesAsync();
    }

    private static async Task SeedOperationalDataAsync(AppDbContext context)
    {
        var branches = await context.Branches.ToListAsync();
        var courts = await context.Courts.ToListAsync();
        var customers = await context.Users.Where(u => u.Role == UserRole.Customer).ToListAsync();
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.SuperAdmin);
        var timeSlots = await context.TimeSlots.OrderBy(s => s.StartTime).ToListAsync();
        var serviceItems = await context.ServiceItems.ToListAsync();

        var cauGiay = branches.First(b => b.Name.Contains("Cầu Giấy"));
        var haiBaTrung = branches.First(b => b.Name.Contains("Hai Bà Trưng"));
        var dongDa = branches.First(b => b.Name.Contains("Đống Đa"));

        var customer = customers.First();

        // 1. Seed Thiết Bị Cơ Sở Vật Chất (FacilityEquipments - 9 thiết bị)
        var equipments = new List<FacilityEquipment>
        {
            new FacilityEquipment { BranchId = cauGiay.Id, CourtId = courts.First(c => c.BranchId == cauGiay.Id).Id, Name = "Hệ thống đèn LED Philips 200W chống chói", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-2) },
            new FacilityEquipment { BranchId = cauGiay.Id, CourtId = courts.First(c => c.BranchId == cauGiay.Id).Id, Name = "Lưới thi đấu Yonex Pro chuẩn BWF", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-5) },
            new FacilityEquipment { BranchId = cauGiay.Id, CourtId = courts.First(c => c.SportType == SportType.Football).Id, Name = "Khung cầu môn & lưới sợi Polyethylene 4mm", Condition = "Cần bảo dưỡng", LastInspectedAt = DateTime.UtcNow.AddDays(-1) },
            new FacilityEquipment { BranchId = cauGiay.Id, CourtId = courts.First(c => c.SportType == SportType.Pickleball).Id, Name = "Bộ cột lưới di động Pickleball Tourna", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-7) },
            new FacilityEquipment { BranchId = cauGiay.Id, Name = "Bảng điểm điện tử LED đa năng hiển thị điểm", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-3) },
            new FacilityEquipment { BranchId = haiBaTrung.Id, CourtId = courts.First(c => c.SportType == SportType.Tennis).Id, Name = "Máy bắn bóng Tennis tự động Spinshot", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-4) },
            new FacilityEquipment { BranchId = haiBaTrung.Id, Name = "Hệ thống quạt làm mát công nghiệp hơi nước", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-10) },
            new FacilityEquipment { BranchId = dongDa.Id, CourtId = courts.First(c => c.SportType == SportType.Basketball).Id, Name = "Bảng rổ kính cường lực 12mm & vành lò xo Pro", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-6) },
            new FacilityEquipment { BranchId = dongDa.Id, Name = "Tủ locker thông minh mở khóa thẻ từ (30 ngăn)", Condition = "Tốt", LastInspectedAt = DateTime.UtcNow.AddDays(-8) }
        };
        await context.FacilityEquipments.AddRangeAsync(equipments);
        await context.SaveChangesAsync();

        // 2. Seed Báo Cáo Sự Cố (CourtIncidents - 3 sự cố thực tế)
        var badmintonCourt = courts.First(c => c.SportType == SportType.Badminton);
        var footballCourt = courts.First(c => c.SportType == SportType.Football);
        var pickleballCourt = courts.First(c => c.SportType == SportType.Pickleball);

        var incidents = new List<CourtIncident>
        {
            new CourtIncident
            {
                CourtId = badmintonCourt.Id,
                Description = "Đèn LED cột số 2 bị chập chờn khi bật công tắc.",
                Severity = "Nhẹ",
                ReportedAt = DateTime.UtcNow.AddDays(-3),
                ResolvedAt = DateTime.UtcNow.AddDays(-2)
            },
            new CourtIncident
            {
                CourtId = footballCourt.Id,
                Description = "Rách một đoạn lưới chắn bóng phía sau cầu môn hướng Tây.",
                Severity = "Trung bình",
                ReportedAt = DateTime.UtcNow.AddDays(-1),
                ResolvedAt = null // Đang xử lý
            },
            new CourtIncident
            {
                CourtId = pickleballCourt.Id,
                Description = "Mặt sơn góc sân hơi trơn trượt sau khi dọn rửa ca sáng.",
                Severity = "Nhẹ",
                ReportedAt = DateTime.UtcNow.AddHours(-10),
                ResolvedAt = DateTime.UtcNow.AddHours(-8)
            }
        };
        await context.CourtIncidents.AddRangeAsync(incidents);
        await context.SaveChangesAsync();

        // 3. Seed Các Đơn Đặt Sân Đa Dạng Trạng Thái (Bookings)
        // - Booking InUse (Đang chơi tại sân)
        var bookingInUse = new Booking
        {
            BookingCode = "BK-2026-0002",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CustomerId = customer.Id,
            BranchId = cauGiay.Id,
            CourtId = badmintonCourt.Id,
            BookingDate = DateOnly.FromDateTime(DateTime.Today),
            Status = BookingStatus.InUse,
            TotalAmount = 160_000m,
            DepositAmount = 80_000m,
            PaymentMethod = PaymentMethod.VietQr,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            CheckedInAt = DateTime.UtcNow.AddMinutes(-45),
            ExpireHoldingAt = DateTime.UtcNow.AddHours(-1)
        };

        // - Booking Completed (Hôm qua đã chơi xong và thanh toán)
        var bookingCompleted = new Booking
        {
            BookingCode = "BK-2026-0003",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CustomerId = customer.Id,
            BranchId = haiBaTrung.Id,
            CourtId = courts.First(c => c.BranchId == haiBaTrung.Id && c.SportType == SportType.Badminton).Id,
            BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            Status = BookingStatus.Completed,
            TotalAmount = 200_000m,
            DepositAmount = 100_000m,
            PaymentMethod = PaymentMethod.VnPay,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            CheckedInAt = DateTime.UtcNow.AddDays(-1).AddHours(18),
            ExpireHoldingAt = DateTime.UtcNow.AddDays(-2).AddMinutes(15)
        };

        // - Booking NoShow (Quá 15p không đến nhận sân, đã thu cọc)
        var bookingNoShow = new Booking
        {
            BookingCode = "BK-2026-0004",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CustomerId = customer.Id,
            BranchId = dongDa.Id,
            CourtId = courts.First(c => c.BranchId == dongDa.Id).Id,
            BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
            Status = BookingStatus.NoShow,
            TotalAmount = 180_000m,
            DepositAmount = 90_000m,
            PaymentMethod = PaymentMethod.VietQr,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ExpireHoldingAt = DateTime.UtcNow.AddDays(-3).AddMinutes(15)
        };

        // - Booking Cancelled (Khách hủy trước giờ quy định, hoàn cọc 80%)
        var bookingCancelled = new Booking
        {
            BookingCode = "BK-2026-0005",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CustomerId = customer.Id,
            BranchId = cauGiay.Id,
            CourtId = footballCourt.Id,
            BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            Status = BookingStatus.Cancelled,
            TotalAmount = 550_000m,
            DepositAmount = 200_000m,
            RefundAmount = 160_000m,
            CancellationReason = "Khách bận lịch công tác đột xuất",
            PaymentMethod = PaymentMethod.VietQr,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            CancelledAt = DateTime.UtcNow.AddHours(-5),
            ExpireHoldingAt = DateTime.UtcNow.AddDays(-1).AddMinutes(15)
        };

        // - Booking PendingPayment (Vừa tạo đơn, đang đếm ngược 15p giữ chỗ)
        var bookingPending = new Booking
        {
            BookingCode = "BK-2026-0006",
            CheckInCode = Guid.NewGuid().ToString("N").ToUpper(),
            CustomerId = customer.Id,
            BranchId = dongDa.Id,
            CourtId = courts.First(c => c.BranchId == dongDa.Id).Id,
            BookingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Status = BookingStatus.PendingPayment,
            TotalAmount = 120_000m,
            DepositAmount = 60_000m,
            PaymentMethod = PaymentMethod.VietQr,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpireHoldingAt = DateTime.UtcNow.AddMinutes(10)
        };

        await context.Bookings.AddRangeAsync(bookingInUse, bookingCompleted, bookingNoShow, bookingCancelled, bookingPending);
        await context.SaveChangesAsync();

        // 4. Seed BookingDetails cho các đơn trên
        var slot18 = timeSlots.First(s => s.StartTime.Hours == 18);
        var slot19 = timeSlots.First(s => s.StartTime.Hours == 19);

        var details = new List<BookingDetail>
        {
            new BookingDetail { BookingId = bookingInUse.Id, TimeSlotId = slot18.Id, SlotPrice = 160_000m },
            new BookingDetail { BookingId = bookingCompleted.Id, TimeSlotId = slot19.Id, SlotPrice = 200_000m },
            new BookingDetail { BookingId = bookingNoShow.Id, TimeSlotId = slot18.Id, SlotPrice = 180_000m },
            new BookingDetail { BookingId = bookingCancelled.Id, TimeSlotId = slot19.Id, SlotPrice = 550_000m },
            new BookingDetail { BookingId = bookingPending.Id, TimeSlotId = slot18.Id, SlotPrice = 120_000m }
        };
        await context.BookingDetails.AddRangeAsync(details);
        await context.SaveChangesAsync();

        // 5. Seed Dịch Vụ Gọi Thêm Vào Hóa Đơn (BookingServiceItems)
        // Ví dụ: Đơn đang chơi bookingInUse có gọi thêm 2 chai Revive và 1 lần thuê vợt
        var reviveService = serviceItems.First(s => s.Name.Contains("Revive"));
        var racketRental = serviceItems.First(s => s.Name.Contains("Yonex"));

        var bookingServices = new List<BookingServiceItem>
        {
            new BookingServiceItem { BookingId = bookingInUse.Id, ServiceItemId = reviveService.Id, Quantity = 2, UnitPrice = reviveService.Price },
            new BookingServiceItem { BookingId = bookingInUse.Id, ServiceItemId = racketRental.Id, Quantity = 1, UnitPrice = racketRental.Price },
            new BookingServiceItem { BookingId = bookingCompleted.Id, ServiceItemId = reviveService.Id, Quantity = 4, UnitPrice = reviveService.Price }
        };
        await context.BookingServiceItems.AddRangeAsync(bookingServices);
        await context.SaveChangesAsync();

        // 6. Seed Đánh Giá Nhận Xét (Reviews - 4 đánh giá thực tế)
        var reviews = new List<Review>
        {
            new Review
            {
                CustomerId = customer.Id,
                BranchId = haiBaTrung.Id,
                BookingId = bookingCompleted.Id,
                Rating = 5,
                Comment = "Sân thảm PVC mới đánh rất bám giày và êm chân, ánh sáng đèn chuẩn không bị chói mắt!",
                ManagerReply = "SportChain cảm ơn bạn! Chúc bạn và đồng đội luôn có những buổi tập tràn đầy năng lượng.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Review
            {
                CustomerId = customer.Id,
                BranchId = cauGiay.Id,
                BookingId = bookingInUse.Id,
                Rating = 5,
                Comment = "Đặt sân online trên web rất tiện lợi, nhận mã QR quét check-in vào sân chỉ mất 5 giây.",
                ManagerReply = "Rất vui vì tính năng vé điện tử QR Code giúp ích cho trải nghiệm của bạn!",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new Review
            {
                CustomerId = customer.Id,
                BranchId = dongDa.Id,
                BookingId = bookingNoShow.Id,
                Rating = 4,
                Comment = "Sân Pickleball đẹp và mới, tuy nhiên vào giờ cao điểm khu vực gửi xe máy hơi đông một chút.",
                ManagerReply = "Cơ sở Đống Đa đã mở thêm bãi xe phụ phía sau tòa nhà, cảm ơn góp ý quý báu của bạn.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();

        // 7. Seed Khiếu Nại & Xử Lý (Complaints - 3 tình huống)
        var complaints = new List<Complaint>
        {
            new Complaint
            {
                CustomerId = customer.Id,
                BranchId = cauGiay.Id,
                BookingId = bookingInUse.Id,
                Content = "Máy lạnh khu vực phòng chờ tầng 1 chạy hơi yếu vào buổi chiều nắng gắt.",
                Status = "Đã giải quyết",
                Resolution = "Bộ phận kỹ thuật đã bảo dưỡng và nạp thêm gas điều hòa trong sáng nay. Tặng voucher giảm 10% ca chơi tiếp theo.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ResolvedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Complaint
            {
                CustomerId = customer.Id,
                BranchId = haiBaTrung.Id,
                BookingId = bookingCompleted.Id,
                Content = "Ca trước bị trễ 5 phút khi bàn giao sân.",
                Status = "Đã giải quyết",
                Resolution = "Lễ tân đã chủ động bù thêm 10 phút chơi cho khách hàng và nhắc nhở nghiêm túc ca trước.",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ResolvedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Complaint
            {
                CustomerId = customer.Id,
                BranchId = dongDa.Id,
                BookingId = null,
                Content = "Đề nghị cơ sở bổ sung thêm quạt hút mùi ở khu vực phòng thay đồ nam.",
                Status = "Đang xử lý",
                Resolution = "Đang đặt hàng thêm 2 quạt thông gió công nghiệp, dự kiến lắp đặt xong cuối tuần này.",
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                ResolvedAt = null
            }
        };
        await context.Complaints.AddRangeAsync(complaints);
        await context.SaveChangesAsync();

        // 8. Seed Nhật Ký Ghi Vết Hệ Thống (AuditLogs - 6 bản ghi quan trọng)
        var auditLogs = new List<AuditLog>
        {
            new AuditLog { UserId = admin?.Id, Action = "ApproveBranch", EntityName = "Branch", EntityId = cauGiay.Id, Details = "SuperAdmin phê duyệt cấp phép hoạt động cho cơ sở SportChain Cầu Giấy", IpAddress = "192.168.1.10", Timestamp = DateTime.UtcNow.AddDays(-10) },
            new AuditLog { UserId = admin?.Id, Action = "ApproveBranch", EntityName = "Branch", EntityId = haiBaTrung.Id, Details = "SuperAdmin phê duyệt cấp phép hoạt động cho cơ sở SportChain Hai Bà Trưng", IpAddress = "192.168.1.10", Timestamp = DateTime.UtcNow.AddDays(-8) },
            new AuditLog { UserId = admin?.Id, Action = "ModifyPriceRule", EntityName = "PriceRule", EntityId = null, Details = "Áp dụng bảng giá giờ cao điểm mới (17h-21h) cho toàn chuỗi", IpAddress = "192.168.1.10", Timestamp = DateTime.UtcNow.AddDays(-7) },
            new AuditLog { UserId = customer.Id, Action = "CreateBooking", EntityName = "Booking", EntityId = bookingInUse.Id, Details = "Khách hàng tạo đơn đặt sân BK-2026-0002 và thanh toán cọc thành công qua VietQR", IpAddress = "14.232.180.5", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new AuditLog { UserId = null, Action = "QrCheckIn", EntityName = "Booking", EntityId = bookingInUse.Id, Details = "Lễ tân quét mã QR Check-in hợp lệ, chuyển trạng thái đơn sang InUse", IpAddress = "192.168.1.25", Timestamp = DateTime.UtcNow.AddMinutes(-45) },
            new AuditLog { UserId = null, Action = "NoShowExpired", EntityName = "Booking", EntityId = bookingNoShow.Id, Details = "Hệ thống Background Worker tự động kích hoạt thu cọc do khách vắng mặt quá 15 phút", IpAddress = "127.0.0.1", Timestamp = DateTime.UtcNow.AddDays(-2) }
        };
        await context.AuditLogs.AddRangeAsync(auditLogs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMissingUsersAsync(AppDbContext context, IPasswordHasher hasher)
    {
        var branches = await context.Branches.ToListAsync();
        var cauGiay = branches.FirstOrDefault(b => b.Name.Contains("Cầu Giấy"));
        var haiBaTrung = branches.FirstOrDefault(b => b.Name.Contains("Hai Bà Trưng"));
        var dongDa = branches.FirstOrDefault(b => b.Name.Contains("Đống Đa"));

        var defaultPasswordHash = hasher.HashPassword("123456");

        var expectedUsers = new List<User>
        {
            // 1. SuperAdmin: Quản trị toàn hệ thống chuỗi
            new User { FullName = "Nguyễn Hoàng Admin", Email = "admin@sportchain.vn", PhoneNumber = "0988111222", PasswordHash = defaultPasswordHash, Role = UserRole.SuperAdmin, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-3) },

            // 2. Quản lý chi nhánh (Branch Managers)
            new User { FullName = "Trần Văn Quản Lý", Email = "manager.caugiay@sportchain.vn", PhoneNumber = "0988333444", PasswordHash = defaultPasswordHash, Role = UserRole.BranchManager, BranchId = cauGiay?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new User { FullName = "Hoàng Thị Thu Hà", Email = "manager.haibatrung@sportchain.vn", PhoneNumber = "0988333555", PasswordHash = defaultPasswordHash, Role = UserRole.BranchManager, BranchId = haiBaTrung?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new User { FullName = "Vũ Đình Long", Email = "manager.dongda@sportchain.vn", PhoneNumber = "0988333777", PasswordHash = defaultPasswordHash, Role = UserRole.BranchManager, BranchId = dongDa?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2) },

            // 3. Nhân viên lễ tân chi nhánh (Receptionists)
            new User { FullName = "Lê Thị Lễ Tân", Email = "reception.caugiay@sportchain.vn", PhoneNumber = "0988555666", PasswordHash = defaultPasswordHash, Role = UserRole.Receptionist, BranchId = cauGiay?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { FullName = "Lê Thị Lễ Tân (QuickLogin)", Email = "letan.caugiay@sportchain.vn", PhoneNumber = "0988555666", PasswordHash = defaultPasswordHash, Role = UserRole.Receptionist, BranchId = cauGiay?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { FullName = "Nguyễn Văn Tiếp Đón", Email = "reception.haibatrung@sportchain.vn", PhoneNumber = "0988555777", PasswordHash = defaultPasswordHash, Role = UserRole.Receptionist, BranchId = haiBaTrung?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { FullName = "Đặng Mỹ Linh", Email = "reception.dongda@sportchain.vn", PhoneNumber = "0988555888", PasswordHash = defaultPasswordHash, Role = UserRole.Receptionist, BranchId = dongDa?.Id, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },

            // 4. Khách hàng thể thao (Customers)
            new User { FullName = "Phạm Minh Khách", Email = "khachhang@gmail.com", PhoneNumber = "0977888999", PasswordHash = defaultPasswordHash, Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { FullName = "Phạm Minh Khách (QuickLogin)", Email = "khachhang@sportchain.vn", PhoneNumber = "0977888999", PasswordHash = defaultPasswordHash, Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new User { FullName = "Bùi Anh Đức", Email = "anhduc.tennis@gmail.com", PhoneNumber = "0912345678", PasswordHash = defaultPasswordHash, Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new User { FullName = "Đỗ Lan Phương", Email = "lanphuong.badminton@gmail.com", PhoneNumber = "0934567890", PasswordHash = defaultPasswordHash, Role = UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };

        var existingEmails = (await context.Users.Select(u => u.Email.ToLower()).ToListAsync()).ToHashSet();

        var usersToAdd = expectedUsers.Where(u => !existingEmails.Contains(u.Email.ToLower())).ToList();
        if (usersToAdd.Any())
        {
            await context.Users.AddRangeAsync(usersToAdd);
            await context.SaveChangesAsync();
        }
    }
}
