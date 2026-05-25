using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Migrations
{
    /// <inheritdoc />
    public partial class ExactlyFinalVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PriceWithDiscount",
                table: "CustomerOrders");

            migrationBuilder.RenameColumn(
                name: "PriceWithOutDiscount",
                table: "CustomerOrders",
                newName: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "CustomerOrders",
                newName: "PriceWithOutDiscount");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "OrderDetails",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceWithDiscount",
                table: "CustomerOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
