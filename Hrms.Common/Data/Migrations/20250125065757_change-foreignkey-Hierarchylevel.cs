using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class changeforeignkeyHierarchylevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HierarchyLeves_DEPARTMENT_DepartmentId",
                table: "HierarchyLeves");

            migrationBuilder.DropForeignKey(
                name: "FK_HierarchyLeves_DepartmentForLevels_DepartmentForLevelId",
                table: "HierarchyLeves");

            migrationBuilder.DropIndex(
                name: "IX_HierarchyLeves_DepartmentForLevelId",
                table: "HierarchyLeves");

            migrationBuilder.DropColumn(
                name: "DepartmentForLevelId",
                table: "HierarchyLeves");

            migrationBuilder.AddForeignKey(
                name: "FK_HierarchyLeves_DepartmentForLevels_DepartmentId",
                table: "HierarchyLeves",
                column: "DepartmentId",
                principalTable: "DepartmentForLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HierarchyLeves_DepartmentForLevels_DepartmentId",
                table: "HierarchyLeves");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentForLevelId",
                table: "HierarchyLeves",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyLeves_DepartmentForLevelId",
                table: "HierarchyLeves",
                column: "DepartmentForLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_HierarchyLeves_DEPARTMENT_DepartmentId",
                table: "HierarchyLeves",
                column: "DepartmentId",
                principalTable: "DEPARTMENT",
                principalColumn: "DEPT_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HierarchyLeves_DepartmentForLevels_DepartmentForLevelId",
                table: "HierarchyLeves",
                column: "DepartmentForLevelId",
                principalTable: "DepartmentForLevels",
                principalColumn: "Id");
        }
    }
}
