using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestaurantApp.Migrations
{
    /// <inheritdoc />
    public partial class FinalVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrders_OrderStatuses_OrderStatusID",
                table: "CustomerOrders");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrders_OrderStatusID",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "OrderStatusID",
                table: "CustomerOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderStatusID",
                table: "CustomerOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_OrderStatusID",
                table: "CustomerOrders",
                column: "OrderStatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrders_OrderStatuses_OrderStatusID",
                table: "CustomerOrders",
                column: "OrderStatusID",
                principalTable: "OrderStatuses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
