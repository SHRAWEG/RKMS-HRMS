using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddTablesForLoanApplicationandCandidateDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentCTC",
                table: "CANDIDATES",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedCTC",
                table: "CANDIDATES",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoticePeriod",
                table: "CANDIDATES",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CandidateDocuments",
                columns: table => new
                {
                    DOC_NO = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILE_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    FILE_EXT = table.Column<string>(type: "varchar(50)", nullable: false),
                    FILE_DESC = table.Column<string>(type: "varchar(250)", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(250)", nullable: true),
                    CandidateId = table.Column<int>(type: "integer", nullable: true),
                    UPLOAD_DT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateDocuments", x => x.DOC_NO);
                    table.ForeignKey(
                        name: "FK_CandidateDocuments_CANDIDATES_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "LOANAPPLICATION",
                columns: table => new
                {
                    LOAN_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMPLOYEE_ID = table.Column<int>(type: "integer", nullable: true),
                    LOAN_TYPE = table.Column<string>(type: "varchar(50)", nullable: true),
                    LOAN_AMOUNT = table.Column<decimal>(type: "numeric", nullable: true),
                    REPAYMENT_PERIOD = table.Column<decimal>(type: "numeric", nullable: true),
                    INTEREST_RATE = table.Column<decimal>(type: "numeric", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOANAPPLICATION", x => x.LOAN_ID);
                    table.ForeignKey(
                        name: "FK_LOANAPPLICATION_EMP_DETAIL_EMPLOYEE_ID",
                        column: x => x.EMPLOYEE_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "LOAN_DOCUMENT",
                columns: table => new
                {
                    DOC_NO = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILE_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    FILE_EXT = table.Column<string>(type: "varchar(50)", nullable: false),
                    FILE_DESC = table.Column<string>(type: "varchar(250)", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(250)", nullable: true),
                    LOAN_ID = table.Column<int>(type: "integer", nullable: true),
                    UPLOAD_DT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOAN_DOCUMENT", x => x.DOC_NO);
                    table.ForeignKey(
                        name: "FK_LOAN_DOCUMENT_LOANAPPLICATION_LOAN_ID",
                        column: x => x.LOAN_ID,
                        principalTable: "LOANAPPLICATION",
                        principalColumn: "LOAN_ID");
                });

            migrationBuilder.CreateTable(
                name: "LoanDisbursements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoanApplicationLoanId = table.Column<int>(type: "integer", nullable: true),
                    LoanApplicatonStatus = table.Column<string>(type: "text", nullable: true),
                    LoanDisbursementAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    DisbursementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanDisbursements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanDisbursements_LOANAPPLICATION_LoanApplicationLoanId",
                        column: x => x.LoanApplicationLoanId,
                        principalTable: "LOANAPPLICATION",
                        principalColumn: "LOAN_ID");
                });

            migrationBuilder.CreateTable(
                name: "LoanRepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoanApplicationLoanId = table.Column<int>(type: "integer", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRepayments_LOANAPPLICATION_LoanApplicationLoanId",
                        column: x => x.LoanApplicationLoanId,
                        principalTable: "LOANAPPLICATION",
                        principalColumn: "LOAN_ID");
                });

            migrationBuilder.CreateTable(
                name: "LoanStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoanApplicationLoanId = table.Column<int>(type: "integer", nullable: true),
                    LoanApplicatonStatus = table.Column<string>(type: "text", nullable: true),
                    LoanStatusAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanStatus_LOANAPPLICATION_LoanApplicationLoanId",
                        column: x => x.LoanApplicationLoanId,
                        principalTable: "LOANAPPLICATION",
                        principalColumn: "LOAN_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateDocuments_CandidateId",
                table: "CandidateDocuments",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_LOAN_DOCUMENT_LOAN_ID",
                table: "LOAN_DOCUMENT",
                column: "LOAN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LOANAPPLICATION_EMPLOYEE_ID",
                table: "LOANAPPLICATION",
                column: "EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDisbursements_LoanApplicationLoanId",
                table: "LoanDisbursements",
                column: "LoanApplicationLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepayments_LoanApplicationLoanId",
                table: "LoanRepayments",
                column: "LoanApplicationLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanStatus_LoanApplicationLoanId",
                table: "LoanStatus",
                column: "LoanApplicationLoanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateDocuments");

            migrationBuilder.DropTable(
                name: "LOAN_DOCUMENT");

            migrationBuilder.DropTable(
                name: "LoanDisbursements");

            migrationBuilder.DropTable(
                name: "LoanRepayments");

            migrationBuilder.DropTable(
                name: "LoanStatus");

            migrationBuilder.DropTable(
                name: "LOANAPPLICATION");

            migrationBuilder.DropColumn(
                name: "CurrentCTC",
                table: "CANDIDATES");

            migrationBuilder.DropColumn(
                name: "ExpectedCTC",
                table: "CANDIDATES");

            migrationBuilder.DropColumn(
                name: "NoticePeriod",
                table: "CANDIDATES");
        }
    }
}
