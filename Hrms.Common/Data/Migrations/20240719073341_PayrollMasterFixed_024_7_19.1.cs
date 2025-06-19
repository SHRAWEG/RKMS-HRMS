using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class PayrollMasterFixed_024_7_191 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SALARY_HEADS_SALARY_HEAD_CATEGORY_SH_CATEGORY_ID",
                table: "SALARY_HEADS");

            migrationBuilder.RenameColumn(
                name: "SH_CATEGORY_ID",
                table: "SALARY_HEADS",
                newName: "SHC_ID");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "SALARY_HEADS",
                newName: "SH_ID");

            migrationBuilder.RenameIndex(
                name: "IX_SALARY_HEADS_SH_CATEGORY_ID",
                table: "SALARY_HEADS",
                newName: "IX_SALARY_HEADS_SHC_ID");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "SALARY_HEAD_CATEGORY",
                newName: "SHC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SALARY_HEADS_SALARY_HEAD_CATEGORY_SHC_ID",
                table: "SALARY_HEADS",
                column: "SHC_ID",
                principalTable: "SALARY_HEAD_CATEGORY",
                principalColumn: "SHC_ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SALARY_HEADS_SALARY_HEAD_CATEGORY_SHC_ID",
                table: "SALARY_HEADS");

            migrationBuilder.RenameColumn(
                name: "SHC_ID",
                table: "SALARY_HEADS",
                newName: "SH_CATEGORY_ID");

            migrationBuilder.RenameColumn(
                name: "SH_ID",
                table: "SALARY_HEADS",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_SALARY_HEADS_SHC_ID",
                table: "SALARY_HEADS",
                newName: "IX_SALARY_HEADS_SH_CATEGORY_ID");

            migrationBuilder.RenameColumn(
                name: "SHC_ID",
                table: "SALARY_HEAD_CATEGORY",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SALARY_HEADS_SALARY_HEAD_CATEGORY_SH_CATEGORY_ID",
                table: "SALARY_HEADS",
                column: "SH_CATEGORY_ID",
                principalTable: "SALARY_HEAD_CATEGORY",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
