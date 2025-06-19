using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PERMISSION_CATEGORIES");

            migrationBuilder.AddColumn<int>(
                name: "DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "EMP_EDUTRN",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "USER_PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_ID = table.Column<int>(type: "integer", nullable: true),
                    PERMISSION_ID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_PERMISSIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_PERMISSIONS_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                        column: x => x.PERMISSION_ID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EMPLOYMENTHISTORY_DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EDUTRN_DocumentId",
                table: "EMP_EDUTRN",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_USER_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS",
                column: "PERMISSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_PERMISSIONS_USER_ID",
                table: "USER_PERMISSIONS",
                column: "USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_EDUTRN_EMP_DOCLIST_DocumentId",
                table: "EMP_EDUTRN",
                column: "DocumentId",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_EMPLOYMENTHISTORY_EMP_DOCLIST_DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY",
                column: "DOCUMENT_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_EDUTRN_EMP_DOCLIST_DocumentId",
                table: "EMP_EDUTRN");

            migrationBuilder.DropForeignKey(
                name: "FK_EMP_EMPLOYMENTHISTORY_EMP_DOCLIST_DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY");

            migrationBuilder.DropTable(
                name: "USER_PERMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_EMP_EMPLOYMENTHISTORY_DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY");

            migrationBuilder.DropIndex(
                name: "IX_EMP_EDUTRN_DocumentId",
                table: "EMP_EDUTRN");

            migrationBuilder.DropColumn(
                name: "DOCUMENT_ID",
                table: "EMP_EMPLOYMENTHISTORY");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "EMP_EDUTRN");

            migrationBuilder.CreateTable(
                name: "PERMISSION_CATEGORIES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSION_CATEGORIES", x => x.ID);
                });
        }
    }
}
