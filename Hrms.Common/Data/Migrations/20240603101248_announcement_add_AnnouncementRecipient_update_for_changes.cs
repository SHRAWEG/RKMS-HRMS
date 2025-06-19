using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class announcement_add_AnnouncementRecipient_update_for_changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementDocuments_announcements_AnnouncementId",
                table: "AnnouncementDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementRecipient_announcements_AnnouncementId",
                table: "AnnouncementRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementRecipient_AspNetUsers_UserId",
                table: "AnnouncementRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementRecipient_UGroups_GroupId",
                table: "AnnouncementRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementRecipient_UGroups_UGroupId1",
                table: "AnnouncementRecipient");

            migrationBuilder.DropForeignKey(
                name: "FK_announcements_Announcement_Category_AnnouncementCategoryId",
                table: "announcements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_announcements",
                table: "announcements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementRecipient",
                table: "AnnouncementRecipient");

            migrationBuilder.DropIndex(
                name: "IX_AnnouncementRecipient_UGroupId1",
                table: "AnnouncementRecipient");

            migrationBuilder.DropColumn(
                name: "UGroupId1",
                table: "AnnouncementRecipient");

            migrationBuilder.RenameTable(
                name: "announcements",
                newName: "Announcements");

            migrationBuilder.RenameTable(
                name: "AnnouncementRecipient",
                newName: "announcementRecipients");

            migrationBuilder.RenameIndex(
                name: "IX_announcements_AnnouncementCategoryId",
                table: "Announcements",
                newName: "IX_Announcements_AnnouncementCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementRecipient_UserId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementRecipient_GroupId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementRecipient_AnnouncementId",
                table: "announcementRecipients",
                newName: "IX_announcementRecipients_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_announcementRecipients",
                table: "announcementRecipients",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Announcement_Category_AnnouncementCategoryId",
                table: "Announcements",
                column: "AnnouncementCategoryId",
                principalTable: "Announcement_Category",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Announcement_Category_AnnouncementCategoryId",
                table: "Announcements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_announcementRecipients",
                table: "announcementRecipients");

            migrationBuilder.RenameTable(
                name: "Announcements",
                newName: "announcements");

            migrationBuilder.RenameTable(
                name: "announcementRecipients",
                newName: "AnnouncementRecipient");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_AnnouncementCategoryId",
                table: "announcements",
                newName: "IX_announcements_AnnouncementCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_UserId",
                table: "AnnouncementRecipient",
                newName: "IX_AnnouncementRecipient_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_GroupId",
                table: "AnnouncementRecipient",
                newName: "IX_AnnouncementRecipient_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_announcementRecipients_AnnouncementId",
                table: "AnnouncementRecipient",
                newName: "IX_AnnouncementRecipient_AnnouncementId");

            migrationBuilder.AddColumn<int>(
                name: "UGroupId1",
                table: "AnnouncementRecipient",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_announcements",
                table: "announcements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementRecipient",
                table: "AnnouncementRecipient",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipient_UGroupId1",
                table: "AnnouncementRecipient",
                column: "UGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementDocuments_announcements_AnnouncementId",
                table: "AnnouncementDocuments",
                column: "AnnouncementId",
                principalTable: "announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementRecipient_announcements_AnnouncementId",
                table: "AnnouncementRecipient",
                column: "AnnouncementId",
                principalTable: "announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementRecipient_AspNetUsers_UserId",
                table: "AnnouncementRecipient",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementRecipient_UGroups_GroupId",
                table: "AnnouncementRecipient",
                column: "GroupId",
                principalTable: "UGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementRecipient_UGroups_UGroupId1",
                table: "AnnouncementRecipient",
                column: "UGroupId1",
                principalTable: "UGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_announcements_Announcement_Category_AnnouncementCategoryId",
                table: "announcements",
                column: "AnnouncementCategoryId",
                principalTable: "Announcement_Category",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
