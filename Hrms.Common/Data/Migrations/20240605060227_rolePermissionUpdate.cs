using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class rolePermissionUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ROLE_PERMISSIONS_AspNetRoles_USER_ID",
                table: "ROLE_PERMISSIONS");

            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "ROLE_PERMISSIONS",
                newName: "ROLE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_ROLE_PERMISSIONS_USER_ID",
                table: "ROLE_PERMISSIONS",
                newName: "IX_ROLE_PERMISSIONS_ROLE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ROLE_PERMISSIONS_AspNetRoles_ROLE_ID",
                table: "ROLE_PERMISSIONS",
                column: "ROLE_ID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ROLE_PERMISSIONS_AspNetRoles_ROLE_ID",
                table: "ROLE_PERMISSIONS");

            migrationBuilder.RenameColumn(
                name: "ROLE_ID",
                table: "ROLE_PERMISSIONS",
                newName: "USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_ROLE_PERMISSIONS_ROLE_ID",
                table: "ROLE_PERMISSIONS",
                newName: "IX_ROLE_PERMISSIONS_USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ROLE_PERMISSIONS_AspNetRoles_USER_ID",
                table: "ROLE_PERMISSIONS",
                column: "USER_ID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }
    }
}
