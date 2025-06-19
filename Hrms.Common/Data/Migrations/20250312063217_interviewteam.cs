using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class interviewteam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CandidateId = table.Column<int>(type: "integer", nullable: true),
                    CreatedById = table.Column<int>(type: "integer", nullable: true),
                    UpdatedById = table.Column<int>(type: "integer", nullable: true),
                    CandidateEmail = table.Column<string>(type: "text", nullable: true),
                    InterviewerEmail = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    MeetingLink = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_CANDIDATES_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Interviews_EMP_DETAIL_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_Interviews_EMP_DETAIL_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "interviewAttendeces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attendeesId = table.Column<int>(type: "integer", nullable: true),
                    InterviewId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interviewAttendeces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interviewAttendeces_EMP_DETAIL_attendeesId",
                        column: x => x.attendeesId,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_interviewAttendeces_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_interviewAttendeces_attendeesId",
                table: "interviewAttendeces",
                column: "attendeesId");

            migrationBuilder.CreateIndex(
                name: "IX_interviewAttendeces_InterviewId",
                table: "interviewAttendeces",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_CandidateId",
                table: "Interviews",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_CreatedById",
                table: "Interviews",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_UpdatedById",
                table: "Interviews",
                column: "UpdatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interviewAttendeces");

            migrationBuilder.DropTable(
                name: "Interviews");
        }
    }
}
