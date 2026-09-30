using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportChain.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courts_BranchId",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BranchId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingDetails_BookingId",
                table: "BookingDetails");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_Branch_IsActive",
                table: "Courts",
                columns: new[] { "BranchId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Branch_Date_Status",
                table: "Bookings",
                columns: new[] { "BranchId", "BookingDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_Booking_TimeSlot",
                table: "BookingDetails",
                columns: new[] { "BookingId", "TimeSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp_UserId",
                table: "AuditLogs",
                columns: new[] { "Timestamp", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courts_Branch_IsActive",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Branch_Date_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingDetails_Booking_TimeSlot",
                table: "BookingDetails");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Timestamp_UserId",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_BranchId",
                table: "Courts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BranchId",
                table: "Bookings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_BookingId",
                table: "BookingDetails",
                column: "BookingId");
        }
    }
}
