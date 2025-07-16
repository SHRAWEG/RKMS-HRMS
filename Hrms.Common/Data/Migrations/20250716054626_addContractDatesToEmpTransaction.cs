using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class addContractDatesToEmpTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CONTRACT_END_DATE",
                table: "EMP_TRAN",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CONTRACT_START_DATE",
                table: "EMP_TRAN",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CONTRACT_END_DATE",
                table: "EMP_TRAN");

            migrationBuilder.DropColumn(
                name: "CONTRACT_START_DATE",
                table: "EMP_TRAN");
        }
    }
}
