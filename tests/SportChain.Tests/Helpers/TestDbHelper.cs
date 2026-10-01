using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportChain.Shared.Constants;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;

namespace SportChain.Tests.Helpers;

public static class TestDbHelper
{
    public static AppDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new AppDbContext(options);
        SeedInitialData(context);
        return context;
    }

    private static void SeedInitialData(AppDbContext context)
    {
        // 1. 17 Ca chuẩn hóa (06:00 -> 23:00)
        var timeSlots = new List<TimeSlot>();
        for (int hour = 6; hour < 23; hour++)
        {
            timeSlots.Add(new TimeSlot
            {
                Id = hour - 5, // 1 to 17
                StartTime = new TimeSpan(hour, 0, 0),
                EndTime = new TimeSpan(hour + 1, 0, 0)
            });
        }
        context.TimeSlots.AddRange(timeSlots);

        // 2. Chi nhánh mẫu
        var branch1 = new Branch
        {
            Id = 1,
            Name = "SportChain Cầu Giấy",
            Address = "12 Dịch Vọng Hậu, Cầu Giấy, Hà Nội",
            City = "Hà Nội",
            PhoneNumber = "0243.888.666",
            OpenTime = new TimeSpan(6, 0, 0),
            CloseTime = new TimeSpan(23, 0, 0),
            IsApproved = true,
            IsActive = true
        };

        var branch2 = new Branch
        {
            Id = 2,
            Name = "SportChain Hai Bà Trưng",
            Address = "45 Đại Cồ Việt, Hai Bà Trưng, Hà Nội",
            City = "Hà Nội",
            PhoneNumber = "0243.999.777",
            OpenTime = new TimeSpan(6, 0, 0),
            CloseTime = new TimeSpan(23, 0, 0),
            IsApproved = true,
            IsActive = true
        };
        context.Branches.AddRange(branch1, branch2);

        // 3. Sân thể thao
        var court1 = new Court
        {
            Id = 1,
            BranchId = 1,
            Name = "Sân Cầu Lông 1 (Trong nhà)",
            SportType = SportType.Badminton,
            SurfaceType = "Thảm PVC",
            Status = SlotStatus.Available,
            IsActive = true,
            RowVersion = new byte[] { 1, 2, 3 }
        };

        var court2 = new Court
        {
            Id = 2,
            BranchId = 1,
            Name = "Sân Tennis 1 (Ngoài trời)",
            SportType = SportType.Tennis,
            SurfaceType = "Sân cứng",
            Status = SlotStatus.Available,
            IsActive = true,
            RowVersion = new byte[] { 1, 2, 3 }
        };

        var courtMaint = new Court
        {
            Id = 3,
            BranchId = 1,
            Name = "Sân Cầu Lông 2 (Bảo trì)",
            SportType = SportType.Badminton,
            SurfaceType = "Thảm PVC",
            Status = SlotStatus.Maintenance,
            IsActive = true,
            RowVersion = new byte[] { 1, 2, 3 }
        };

        var courtBranch2 = new Court
        {
            Id = 4,
            BranchId = 2,
            Name = "Sân Pickleball 1 (Hai Bà Trưng)",
            SportType = SportType.Pickleball,
            SurfaceType = "Sơn Acrylic",
            Status = SlotStatus.Available,
            IsActive = true,
            RowVersion = new byte[] { 1, 2, 3 }
        };
        context.Courts.AddRange(court1, court2, courtMaint, courtBranch2);

        // 4. Giá giờ thường & giờ cao điểm cho Court 1
        var priceRules = new List<PriceRule>();
        foreach (var slot in timeSlots)
        {
            bool isPeak = slot.StartTime >= new TimeSpan(17, 0, 0) && slot.StartTime < new TimeSpan(21, 0, 0);
            decimal price = isPeak ? 160000m : 100000m;
            priceRules.Add(new PriceRule
            {
                CourtId = 1,
                TimeSlotId = slot.Id,
                PricePerHour = price,
                IsPeakHour = isPeak
            });
        }
        context.PriceRules.AddRange(priceRules);

        // 5. Dịch vụ bán kèm F&B / Thuê vợt
        var item1 = new ServiceItem
        {
            Id = 1,
            BranchId = 1,
            Name = "Nước Revive Chanh Muối",
            Category = "Drink",
            Price = 15000m,
            IsActive = true
        };
        var item2 = new ServiceItem
        {
            Id = 2,
            BranchId = 1,
            Name = "Thuê Vợt Cầu Lông Yonex",
            Category = "Rental",
            Price = 40000m,
            IsRental = true,
            IsActive = true
        };
        context.ServiceItems.AddRange(item1, item2);

        // 6. Tài khoản người dùng mẫu
        var customer1 = new User
        {
            Id = 10,
            FullName = "Nguyễn Văn Khách",
            Email = "customer@sportchain.vn",
            PhoneNumber = "0988111222",
            PasswordHash = "HASH",
            Role = UserRole.Customer,
            IsActive = true
        };

        var customer2 = new User
        {
            Id = 11,
            FullName = "Trần Thị Khách 2",
            Email = "customer2@sportchain.vn",
            PhoneNumber = "0988333444",
            PasswordHash = "HASH",
            Role = UserRole.Customer,
            IsActive = true
        };

        var receptionist1 = new User
        {
            Id = 20,
            FullName = "Lê Lễ Tân Cầu Giấy",
            Email = "reception.caugiay@sportchain.vn",
            PhoneNumber = "0988555666",
            PasswordHash = "HASH",
            Role = UserRole.Receptionist,
            BranchId = 1,
            IsActive = true
        };

        var manager1 = new User
        {
            Id = 30,
            FullName = "Trần Quản Lý Cầu Giấy",
            Email = "manager.caugiay@sportchain.vn",
            PhoneNumber = "0988777888",
            PasswordHash = "HASH",
            Role = UserRole.BranchManager,
            BranchId = 1,
            IsActive = true
        };

        var superAdmin = new User
        {
            Id = 40,
            FullName = "Admin Toàn Chuỗi",
            Email = "admin@sportchain.vn",
            PhoneNumber = "0988999000",
            PasswordHash = "HASH",
            Role = UserRole.SuperAdmin,
            BranchId = null,
            IsActive = true
        };
        context.Users.AddRange(customer1, customer2, receptionist1, manager1, superAdmin);

        context.SaveChanges();
    }
}
