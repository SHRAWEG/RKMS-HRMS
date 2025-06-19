using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class add_wishlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
              name: "Wishlist",
              columns: table => new
              {
                  Wish_ID = table.Column<short>(type: "smallint", nullable: false)
                      .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                  Wish_Title = table.Column<string>(type: "varchar(30)", nullable: false),
                  Wish_Template = table.Column<string>(type: "TEXT", nullable: false),
                  CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                  UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Wishlist", x => x.Wish_ID);
              });

            migrationBuilder.AddColumn<short>(
                name: "HOLIDAY_ID",
                table: "Wishlist",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HolidayId",
                table: "Wishlist",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Wish_Type",
                table: "Wishlist",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
              name: "Wishlist");

            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_HOLIDAY_HolidayId",
                table: "Wishlist");

            migrationBuilder.DropIndex(
                name: "IX_Wishlist_HolidayId",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "HOLIDAY_ID",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "HolidayId",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "Wish_Type",
                table: "Wishlist");
        }
    }
}
