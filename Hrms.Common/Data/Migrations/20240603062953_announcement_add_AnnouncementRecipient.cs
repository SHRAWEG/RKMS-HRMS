using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class announcement_add_AnnouncementRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnnouncementRecipient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnnouncementId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    ReadOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UGroupId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipient_announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipient_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipient_UGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "UGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipient_UGroups_UGroupId1",
                        column: x => x.UGroupId1,
                        principalTable: "UGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipient_AnnouncementId",
                table: "AnnouncementRecipient",
                column: "AnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipient_GroupId",
                table: "AnnouncementRecipient",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipient_UGroupId1",
                table: "AnnouncementRecipient",
                column: "UGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipient_UserId",
                table: "AnnouncementRecipient",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementRecipient");
        }
    }
}
