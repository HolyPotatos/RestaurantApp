using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestaurantApp.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTableStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeatTables_TableStatuses_TableStatusID",
                table: "SeatTables");

            migrationBuilder.DropTable(
                name: "TableStatuses");

            migrationBuilder.DropIndex(
                name: "IX_SeatTables_TableStatusID",
                table: "SeatTables");

            migrationBuilder.DropColumn(
                name: "TableStatusID",
                table: "SeatTables");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Menus",
                newName: "isActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Menus",
                newName: "IsActive");

            migrationBuilder.AddColumn<int>(
                name: "TableStatusID",
                table: "SeatTables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TableStatuses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableStatuses", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatTables_TableStatusID",
                table: "SeatTables",
                column: "TableStatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_SeatTables_TableStatuses_TableStatusID",
                table: "SeatTables",
                column: "TableStatusID",
                principalTable: "TableStatuses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
