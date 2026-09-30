using Microsoft.EntityFrameworkCore;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<PriceRule> PriceRules => Set<PriceRule>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<BookingServiceItem> BookingServiceItems => Set<BookingServiceItem>();
    public DbSet<FacilityEquipment> FacilityEquipments => Set<FacilityEquipment>();
    public DbSet<CourtIncident> CourtIncidents => Set<CourtIncident>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingCode)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.CheckInCode)
            .IsUnique();

        // Composite Indexes tối ưu hóa truy vấn ma trận lịch, POS & Background Worker
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.BranchId, b.BookingDate, b.Status })
            .HasDatabaseName("IX_Bookings_Branch_Date_Status");

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.CustomerId)
            .HasDatabaseName("IX_Bookings_CustomerId");

        modelBuilder.Entity<BookingDetail>()
            .HasIndex(bd => new { bd.BookingId, bd.TimeSlotId })
            .HasDatabaseName("IX_BookingDetails_Booking_TimeSlot");

        modelBuilder.Entity<Court>()
            .HasIndex(c => new { c.BranchId, c.IsActive })
            .HasDatabaseName("IX_Courts_Branch_IsActive");

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.Timestamp, a.UserId })
            .HasDatabaseName("IX_AuditLogs_Timestamp_UserId");

        // Ràng buộc số thập phân tiền tệ
        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.DepositAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.RefundAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingDetail>()
            .Property(bd => bd.SlotPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PriceRule>()
            .Property(pr => pr.PricePerHour)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ServiceItem>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingServiceItem>()
            .Property(bsi => bsi.UnitPrice)
            .HasPrecision(18, 2);

        // Quan hệ Booking & User (Customer)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ Booking & Branch
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Branch)
            .WithMany(br => br.Bookings)
            .HasForeignKey(b => b.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ Booking & Court
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Court)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CourtId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
