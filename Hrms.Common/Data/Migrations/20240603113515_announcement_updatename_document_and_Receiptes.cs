using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class announcement_updatename_document_and_Receiptes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementDocuments_Announcements_AnnouncementId",
                table: "AnnouncementDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_announcementRecipients_Announcements_AnnouncementId",
                table: "announcementRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_announcementRecipients_AspNetUsers_UserId",
                table: "announcementRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_announcementRecipients_UGroups_GroupId",
                table: "announcementRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_announcementRecipients",
                table: "announcementRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementDocuments",
                table: "AnnouncementDocuments");

            migrationBuilder.RenameTable(
                name: "announcementRecipients",
                newName: "Announcement_Recipient");

            migrationBuilder.RenameTable(
                name: "AnnouncementDocuments",
                newName: "Announcement_Documents");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_UserId",
                table: "Announcement_Recipient",
                newName: "IX_Announcement_Recipient_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_GroupId",
                table: "Announcement_Recipient",
                newName: "IX_Announcement_Recipient_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_AnnouncementId",
                table: "Announcement_Recipient",
                newName: "IX_Announcement_Recipient_AnnouncementId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementDocuments_AnnouncementId",
                table: "Announcement_Documents",
                newName: "IX_Announcement_Documents_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcement_Recipient",
                table: "Announcement_Recipient",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcement_Documents",
                table: "Announcement_Documents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Documents_Announcements_AnnouncementId",
                table: "Announcement_Documents",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Recipient_Announcements_AnnouncementId",
                table: "Announcement_Recipient",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Recipient_AspNetUsers_UserId",
                table: "Announcement_Recipient",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcement_Recipient_UGroups_GroupId",
                table: "Announcement_Recipient",
                column: "GroupId",
                principalTable: "UGroups",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Documents_Announcements_AnnouncementId",
                table: "Announcement_Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Recipient_Announcements_AnnouncementId",
                table: "Announcement_Recipient");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Recipient_AspNetUsers_UserId",
                table: "Announcement_Recipient");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcement_Recipient_UGroups_GroupId",
                table: "Announcement_Recipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcement_Recipient",
                table: "Announcement_Recipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcement_Documents",
                table: "Announcement_Documents");

            migrationBuilder.RenameTable(
                name: "Announcement_Recipient",
                newName: "announcementRecipients");

            migrationBuilder.RenameTable(
                name: "Announcement_Documents",
                newName: "AnnouncementDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_Recipient_UserId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_Recipient_GroupId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_Recipient_AnnouncementId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_AnnouncementId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcement_Documents_AnnouncementId",
                table: "AnnouncementDocuments",
                newName: "IX_AnnouncementDocuments_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_announcementRecipients",
                table: "announcementRecipients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementDocuments",
                table: "AnnouncementDocuments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementDocuments_Announcements_AnnouncementId",
                table: "AnnouncementDocuments",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_announcementRecipients_Announcements_AnnouncementId",
                table: "announcementRecipients",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_announcementRecipients_AspNetUsers_UserId",
                table: "announcementRecipients",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_announcementRecipients_UGroups_GroupId",
                table: "announcementRecipients",
                column: "GroupId",
                principalTable: "UGroups",
                principalColumn: "Id");
        }
    }
}
