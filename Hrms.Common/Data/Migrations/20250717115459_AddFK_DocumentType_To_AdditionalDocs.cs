using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddFK_DocumentType_To_AdditionalDocs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EMP_ADDITIONAL_DOCUMENTS_DOCUMENT_TYPE_ID",
                table: "EMP_ADDITIONAL_DOCUMENTS",
                column: "DOCUMENT_TYPE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_ADDITIONAL_DOCUMENTS_DOCUMENT_TYPE_DOCUMENT_TYPE_ID",
                table: "EMP_ADDITIONAL_DOCUMENTS",
                column: "DOCUMENT_TYPE_ID",
                principalTable: "DOCUMENT_TYPE",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_ADDITIONAL_DOCUMENTS_DOCUMENT_TYPE_DOCUMENT_TYPE_ID",
                table: "EMP_ADDITIONAL_DOCUMENTS");

            migrationBuilder.DropIndex(
                name: "IX_EMP_ADDITIONAL_DOCUMENTS_DOCUMENT_TYPE_ID",
                table: "EMP_ADDITIONAL_DOCUMENTS");
        }
    }
}
