using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class addtableaccountcode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ACCOUNT_CODE_ID",
                table: "EMP_TRAN",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ACCOUNT_CODE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNT_CODE", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_ACCOUNT_CODE_ID",
                table: "EMP_TRAN",
                column: "ACCOUNT_CODE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNT_CODE_NAME",
                table: "ACCOUNT_CODE",
                column: "NAME",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_TRAN_ACCOUNT_CODE_ACCOUNT_CODE_ID",
                table: "EMP_TRAN",
                column: "ACCOUNT_CODE_ID",
                principalTable: "ACCOUNT_CODE",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_TRAN_ACCOUNT_CODE_ACCOUNT_CODE_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropTable(
                name: "ACCOUNT_CODE");

            migrationBuilder.DropIndex(
                name: "IX_EMP_TRAN_ACCOUNT_CODE_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropColumn(
                name: "ACCOUNT_CODE_ID",
                table: "EMP_TRAN");
        }
    }
}
