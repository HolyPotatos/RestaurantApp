using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Migrations
{
    /// <inheritdoc />
    public partial class TotalyExactlyFinalVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "CustomerOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "CustomerOrders",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
