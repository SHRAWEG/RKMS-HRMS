using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class UserCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ROLE_PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_ID = table.Column<int>(type: "integer", nullable: true),
                    PERMISSION_ID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE_PERMISSIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSIONS_AspNetRoles_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                        column: x => x.PERMISSION_ID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "USER_COMPANIES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    COMPANY_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_COMPANIES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_COMPANIES_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_COMPANIES_COMPANY_COMPANY_ID",
                        column: x => x.COMPANY_ID,
                        principalTable: "COMPANY",
                        principalColumn: "Company_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSIONS_PERMISSION_ID",
                table: "ROLE_PERMISSIONS",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSIONS_USER_ID",
                table: "ROLE_PERMISSIONS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_COMPANIES_COMPANY_ID",
                table: "USER_COMPANIES",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_COMPANIES_USER_ID",
                table: "USER_COMPANIES",
                column: "USER_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROLE_PERMISSIONS");

            migrationBuilder.DropTable(
                name: "USER_COMPANIES");
        }
    }
}
