using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class update_wishlistremoveholiday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_HOLIDAY_HolidayId",
                table: "Wishlist");

            migrationBuilder.DropIndex(
                name: "IX_Wishlist_HolidayId",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "HolidayId",
                table: "Wishlist");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HolidayId",
                table: "Wishlist",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_HolidayId",
                table: "Wishlist",
                column: "HolidayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlist_HOLIDAY_HolidayId",
                table: "Wishlist",
                column: "HolidayId",
                principalTable: "HOLIDAY",
                principalColumn: "HOLIDAY_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
