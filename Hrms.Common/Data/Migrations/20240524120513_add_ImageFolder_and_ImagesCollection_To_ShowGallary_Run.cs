using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class add_ImageFolder_and_ImagesCollection_To_ShowGallary_Run : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateTable(
                name: "ImagesFolders",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagesFolders", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "ImageCollection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    UploadedBy = table.Column<string>(type: "text", nullable: true),
                    ImagesFolderImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageCollection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageCollection_ImagesFolders_ImagesFolderImageId",
                        column: x => x.ImagesFolderImageId,
                        principalTable: "ImagesFolders",
                        principalColumn: "ImageId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageCollection_ImagesFolderImageId",
                table: "ImageCollection",
                column: "ImagesFolderImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageCollection");

            migrationBuilder.DropTable(
                name: "ImagesFolders");

            migrationBuilder.DropColumn(
                name: "OVERALL_RATING",
                table: "CANDIDATE_STAGES");

            migrationBuilder.DropColumn(
                name: "REMARKS",
                table: "CANDIDATE_STAGES");
        }
    }
}
