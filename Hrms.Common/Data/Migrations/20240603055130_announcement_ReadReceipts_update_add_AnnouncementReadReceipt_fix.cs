using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class announcement_ReadReceipts_update_add_AnnouncementReadReceipt_fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcementReadReceipts");

            migrationBuilder.AddColumn<DateTime>(
                name: "CREATED_AT",
                table: "UGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UPDATED_AT",
                table: "UGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CREATED_AT",
                table: "UGroups");

            migrationBuilder.DropColumn(
                name: "UPDATED_AT",
                table: "UGroups");

            migrationBuilder.CreateTable(
                name: "announcementReadReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnnouncementId = table.Column<int>(type: "integer", nullable: false),
                    UGroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    ReadOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcementReadReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_announcementReadReceipts_announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_announcementReadReceipts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_announcementReadReceipts_UGroups_UGroupId",
                        column: x => x.UGroupId,
                        principalTable: "UGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_announcementReadReceipts_AnnouncementId",
                table: "announcementReadReceipts",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_announcementReadReceipts_UGroupId",
                table: "announcementReadReceipts",
                column: "UGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_announcementReadReceipts_UserId",
                table: "announcementReadReceipts",
                column: "UserId");
        }
    }
}
