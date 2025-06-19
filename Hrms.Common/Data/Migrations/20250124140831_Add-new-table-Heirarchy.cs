using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddnewtableHeirarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentForLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentForLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HierarchyLeves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LevelName = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepartmentForLevelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarchyLeves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HierarchyLeves_DEPARTMENT_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HierarchyLeves_DepartmentForLevels_DepartmentForLevelId",
                        column: x => x.DepartmentForLevelId,
                        principalTable: "DepartmentForLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HierarchyLeves_HierarchyLeves_ParentId",
                        column: x => x.ParentId,
                        principalTable: "HierarchyLeves",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyLeves_DepartmentForLevelId",
                table: "HierarchyLeves",
                column: "DepartmentForLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyLeves_DepartmentId",
                table: "HierarchyLeves",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HierarchyLeves_ParentId",
                table: "HierarchyLeves",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HierarchyLeves");

            migrationBuilder.DropTable(
                name: "DepartmentForLevels");
        }
    }
}
