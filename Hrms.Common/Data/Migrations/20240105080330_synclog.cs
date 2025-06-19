using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class synclog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SYNC_ATTENDANCE_LOGS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TYPE = table.Column<string>(type: "varchar(10)", nullable: false),
                    STATUS = table.Column<string>(type: "varchar(255)", nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "text", nullable: true),
                    ERROR_TRACE = table.Column<string>(type: "text", nullable: true),
                    SYNCED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYNC_ATTENDANCE_LOGS", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SYNC_ATTENDANCE_LOGS");
        }
    }
}
