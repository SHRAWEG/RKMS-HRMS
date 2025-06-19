using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class GatePassTypeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GATE_PASS_TYPE_ID",
                table: "REGULARISATION",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GATE_PASS_TYPES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GATE_PASS_TYPES", x => x.ID);
                });

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

            migrationBuilder.CreateTable(
                name: "PERMISSIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CATEGORY = table.Column<string>(type: "varchar(255)", nullable: false),
                    SUB_CATEGORY = table.Column<string>(type: "varchar(255)", nullable: false),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    DISPLAY_NAME = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REGULARISATION_GATE_PASS_TYPE_ID",
                table: "REGULARISATION",
                column: "GATE_PASS_TYPE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_REGULARISATION_GATE_PASS_TYPES_GATE_PASS_TYPE_ID",
                table: "REGULARISATION",
                column: "GATE_PASS_TYPE_ID",
                principalTable: "GATE_PASS_TYPES",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REGULARISATION_GATE_PASS_TYPES_GATE_PASS_TYPE_ID",
                table: "REGULARISATION");

            migrationBuilder.DropTable(
                name: "GATE_PASS_TYPES");

            migrationBuilder.DropTable(
                name: "PERMISSION_CATEGORIES");

            migrationBuilder.DropTable(
                name: "PERMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REGULARISATION_GATE_PASS_TYPE_ID",
                table: "REGULARISATION");

            migrationBuilder.DropColumn(
                name: "GATE_PASS_TYPE_ID",
                table: "REGULARISATION");
        }
    }
}
