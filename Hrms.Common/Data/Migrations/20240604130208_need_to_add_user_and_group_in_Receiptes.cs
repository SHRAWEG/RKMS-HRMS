using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class need_to_add_user_and_group_in_Receiptes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnouncementRecipientId",
                table: "UGroups",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnouncementRecipientId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UGroups_AnnouncementRecipientId",
                table: "UGroups",
                column: "AnnouncementRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AnnouncementRecipientId",
                table: "AspNetUsers",
                column: "AnnouncementRecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Announcement_Recipient_AnnouncementRecipientId",
                table: "AspNetUsers",
                column: "AnnouncementRecipientId",
                principalTable: "Announcement_Recipient",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UGroups_Announcement_Recipient_AnnouncementRecipientId",
                table: "UGroups",
                column: "AnnouncementRecipientId",
                principalTable: "Announcement_Recipient",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Announcement_Recipient_AnnouncementRecipientId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UGroups_Announcement_Recipient_AnnouncementRecipientId",
                table: "UGroups");

            migrationBuilder.DropIndex(
                name: "IX_UGroups_AnnouncementRecipientId",
                table: "UGroups");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AnnouncementRecipientId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AnnouncementRecipientId",
                table: "UGroups");

            migrationBuilder.DropColumn(
                name: "AnnouncementRecipientId",
                table: "AspNetUsers");
        }
    }
}
