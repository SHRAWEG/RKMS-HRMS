using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class createAndSeedRegularisationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REGULARISATION_TYPE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    DISPLAY_NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARISATION_TYPE", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "REGULARISATION_TYPE",
                columns: new[] { "ID", "NAME", "DISPLAY_NAME", "CREATED_AT", "UPDATED_AT" },
                values: new object[,]
                {
                        { 1, "in-punch-regularisation", "In Punch Regularisation", DateTime.UtcNow, DateTime.UtcNow },
                        {2, "out-punch-regularisation", "Out Punch Regularisation", DateTime.UtcNow, DateTime.UtcNow},
                        {3, "on-duty", "On Duty", DateTime.UtcNow, DateTime.UtcNow},
                        {4, "work-from-home", "Work From Home", DateTime.UtcNow, DateTime.UtcNow},
                        {5, "gate-pass", "Gate Pass", DateTime.UtcNow, DateTime.UtcNow}
                });

            migrationBuilder.AddColumn<int>(
                name: "REGULARISATION_TYPE_ID",
                table: "REGULARISATION",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""REGULARISATION""
                SET ""REGULARISATION_TYPE_ID"" = rt.""ID""
                FROM ""REGULARISATION_TYPE"" rt
                WHERE ""REGULARISATION_TYPE"" = rt.""NAME"";
            ");

            migrationBuilder.AlterColumn<int>(
                name: "REGULARISATION_TYPE_ID",
                table: "REGULARISATION",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "REGULARISATION_TYPE",
                table: "REGULARISATION");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REGULARISATION_REGULARISATION_TYPE_REGULARISATION_TYPE_ID",
                table: "REGULARISATION");

            migrationBuilder.DropTable(
                name: "REGULARISATION_TYPE");

            migrationBuilder.DropIndex(
                name: "IX_REGULARISATION_REGULARISATION_TYPE_ID",
                table: "REGULARISATION");

            migrationBuilder.DropColumn(
                name: "REGULARISATION_TYPE_ID",
                table: "REGULARISATION");

            migrationBuilder.AddColumn<string>(
                name: "REGULARISATION_TYPE",
                table: "REGULARISATION",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");
        }
    }
}
