using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class add_ImageFolder_and_ImagesCollection_To_ShowGallary_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageCollection_ImagesFolders_ImagesFolderImageId",
                table: "ImageCollection");

            migrationBuilder.DropIndex(
                name: "IX_ImageCollection_ImagesFolderImageId",
                table: "ImageCollection");

            migrationBuilder.DropColumn(
                name: "ImagesFolderImageId",
                table: "ImageCollection");

            migrationBuilder.CreateIndex(
                name: "IX_ImageCollection_FolderId",
                table: "ImageCollection",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageCollection_ImagesFolders_FolderId",
                table: "ImageCollection",
                column: "FolderId",
                principalTable: "ImagesFolders",
                principalColumn: "ImageId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageCollection_ImagesFolders_FolderId",
                table: "ImageCollection");

            migrationBuilder.DropIndex(
                name: "IX_ImageCollection_FolderId",
                table: "ImageCollection");

            migrationBuilder.AddColumn<Guid>(
                name: "ImagesFolderImageId",
                table: "ImageCollection",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageCollection_ImagesFolderImageId",
                table: "ImageCollection",
                column: "ImagesFolderImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageCollection_ImagesFolders_ImagesFolderImageId",
                table: "ImageCollection",
                column: "ImagesFolderImageId",
                principalTable: "ImagesFolders",
                principalColumn: "ImageId");
        }
    }
}
