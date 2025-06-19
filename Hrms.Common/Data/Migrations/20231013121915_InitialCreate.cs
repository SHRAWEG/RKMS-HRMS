using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ASSET_TYPE",
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
                    table.PrimaryKey("PK_ASSET_TYPE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ATTENDANCE_LOG",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVICE_LOG_ID = table.Column<long>(type: "bigint", nullable: false),
                    DEVICE_CODE = table.Column<string>(type: "varchar(30)", nullable: false),
                    DIRECTION = table.Column<string>(type: "varchar(10)", nullable: false),
                    DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TIME = table.Column<string>(type: "varchar(10)", nullable: false),
                    IS_SUCCESS = table.Column<bool>(type: "boolean", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTENDANCE_LOG", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BLOOD_GROUP",
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
                    table.PrimaryKey("PK_BLOOD_GROUP", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BUSINESS_UNIT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BUSINESS_UNIT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CALENDAR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(50)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALENDAR", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CLASS",
                columns: table => new
                {
                    CLASS_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CLASS_NAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLASS", x => x.CLASS_ID);
                });

            migrationBuilder.CreateTable(
                name: "COMPANY",
                columns: table => new
                {
                    Company_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Company_Name = table.Column<string>(type: "varchar(250)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    Company_Add1 = table.Column<string>(type: "varchar(50)", nullable: true),
                    Company_Add2 = table.Column<string>(type: "varchar(50)", nullable: true),
                    Company_Tel = table.Column<string>(type: "varchar(50)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMPANY", x => x.Company_Id);
                });

            migrationBuilder.CreateTable(
                name: "COST_CENTER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COST_CENTER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "COUNTRY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COUNTRY", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DEVICE_SETTINGS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVICE_MODEL = table.Column<string>(type: "varchar(255)", nullable: false),
                    DEVICE_IP = table.Column<string>(type: "varchar(255)", nullable: false),
                    PORT_NUMBER = table.Column<int>(type: "integer", nullable: false),
                    CLEAR_DEVICE_LOG = table.Column<bool>(type: "boolean", nullable: false),
                    AttendanceMode = table.Column<string>(type: "varchar(255)", nullable: false),
                    LAST_FETCHED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    LAST_FETCHED_TIME = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEVICE_SETTINGS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DIVISION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DIVISION", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EDUCATION_LEVEL",
                columns: table => new
                {
                    ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEVELNAME = table.Column<string>(type: "varchar(100)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDUCATION_LEVEL", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EMP_DAILYOUT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_Id = table.Column<int>(type: "integer", nullable: true),
                    OutDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OutTime = table.Column<string>(type: "varchar(15)", nullable: true),
                    OutRemarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    InDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InTime = table.Column<string>(type: "varchar(15)", nullable: true),
                    InRemarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    ATTID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_DAILYOUT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EMP_LOG",
                columns: table => new
                {
                    PID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    TDATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_LOG", x => x.PID);
                });

            migrationBuilder.CreateTable(
                name: "GRADE_TYPE",
                columns: table => new
                {
                    GType = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRADE_TYPE", x => x.GType);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE",
                columns: table => new
                {
                    LEAVE_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEAVE_NAME = table.Column<string>(type: "varchar(50)", nullable: true),
                    LEAVE_DAYS = table.Column<int>(type: "integer", nullable: true),
                    LEAVE_TYPE = table.Column<byte>(type: "smallint", nullable: true),
                    LEAVE_MAX = table.Column<short>(type: "smallint", nullable: true),
                    ISPAIDLEAVE = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVE_PAY_QTY = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Abbr = table.Column<string>(type: "varchar(10)", nullable: false),
                    UseLimit = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVEEARN = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVEEARN_DAYS = table.Column<double>(type: "double precision", nullable: true),
                    LEAVEEARN_QUANTITY = table.Column<decimal>(type: "numeric", nullable: true),
                    IS_HALF_LEAVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE", x => x.LEAVE_ID);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE_YEAR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YEAR = table.Column<string>(type: "varchar(255)", nullable: false),
                    BY_DATE = table.Column<string>(type: "varchar(255)", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_YEAR", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MODE",
                columns: table => new
                {
                    MODE_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MODE_NAME = table.Column<string>(type: "varchar(30)", nullable: false),
                    MODE_ABB = table.Column<string>(type: "varchar(2)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MODE", x => x.MODE_ID);
                });

            migrationBuilder.CreateTable(
                name: "PLANT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLANT", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "REGION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGION", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RELIGION",
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
                    table.PrimaryKey("PK_RELIGION", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SALARY_HEAD_CATEGORY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    CATEGORY = table.Column<string>(type: "varchar(250)", nullable: false),
                    SHOW_CATEGORY = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_HEAD_CATEGORY", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "STATE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CODE = table.Column<string>(type: "text", nullable: false),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STATE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "STATUS",
                columns: table => new
                {
                    STATUS_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STATUS_NAME = table.Column<string>(type: "varchar(30)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STATUS", x => x.STATUS_ID);
                });

            migrationBuilder.CreateTable(
                name: "tblBank",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankName = table.Column<string>(type: "varchar(200)", nullable: false),
                    BankAddress = table.Column<string>(type: "varchar(200)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBank", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "tblFATTLOG",
                columns: table => new
                {
                    FAttId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_Id = table.Column<int>(type: "integer", nullable: false),
                    TDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TTime = table.Column<string>(type: "varchar(15)", nullable: false),
                    ModeNM = table.Column<string>(type: "varchar(50)", nullable: false),
                    Mode = table.Column<byte>(type: "smallint", nullable: false),
                    Flag = table.Column<byte>(type: "smallint", nullable: false),
                    TimeVal = table.Column<int>(type: "integer", nullable: false),
                    TrnUser = table.Column<string>(type: "varchar(50)", nullable: false),
                    TrnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(300)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblFATTLOG", x => x.FAttId);
                });

            migrationBuilder.CreateTable(
                name: "TITLES",
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
                    table.PrimaryKey("PK_TITLES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UNIFORM_TYPE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UNIFORM_TYPE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WORK_HOUR",
                columns: table => new
                {
                    WORK_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORK_HOUR = table.Column<string>(type: "varchar(6)", nullable: false),
                    IN_START = table.Column<string>(type: "varchar(15)", nullable: true),
                    OUT_START = table.Column<string>(type: "varchar(15)", nullable: true),
                    LUNCHTIME = table.Column<short>(type: "smallint", nullable: true),
                    TIFFINTIME = table.Column<short>(type: "smallint", nullable: true),
                    IS_NIGHTSHIFT = table.Column<bool>(type: "boolean", nullable: true),
                    WORK_TYPE = table.Column<byte>(type: "smallint", nullable: false),
                    WORK_DAYCOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    INSTARTGRACE = table.Column<short>(type: "smallint", nullable: false),
                    INENDGRACE = table.Column<short>(type: "smallint", nullable: false),
                    OUTSTARTGRACE = table.Column<short>(type: "smallint", nullable: false),
                    OUTENDGRACE = table.Column<short>(type: "smallint", nullable: false),
                    STMVAL = table.Column<short>(type: "smallint", nullable: false),
                    ETMVAL = table.Column<short>(type: "smallint", nullable: false),
                    T_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TUser = table.Column<string>(type: "varchar(50)", nullable: true),
                    LOCKIN = table.Column<byte>(type: "smallint", nullable: false),
                    LOCKINTIME = table.Column<string>(type: "varchar(15)", nullable: true),
                    LOCKOUT = table.Column<byte>(type: "smallint", nullable: false),
                    LOCKOUTTIME = table.Column<string>(type: "varchar(15)", nullable: true),
                    LOCKLT = table.Column<byte>(type: "smallint", nullable: false),
                    LOCKLTTIME = table.Column<string>(type: "varchar(15)", nullable: true),
                    IS_MIN_DURATION = table.Column<bool>(type: "boolean", nullable: false),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: true),
                    START_TIME = table.Column<string>(type: "varchar(255)", nullable: true),
                    END_TIME = table.Column<string>(type: "varchar(255)", nullable: true),
                    HALF_DAY_START_TIME = table.Column<string>(type: "varchar(255)", nullable: true),
                    HALF_DAY_END_TIME = table.Column<string>(type: "varchar(255)", nullable: true),
                    FLEXI_DURATION = table.Column<int>(type: "integer", nullable: true),
                    LATE_IN_GRACE_TIME = table.Column<int>(type: "integer", nullable: true),
                    MIN_HALF_DAY_TIME = table.Column<int>(type: "integer", nullable: true),
                    MIN_DUTY_TIME = table.Column<int>(type: "integer", nullable: true),
                    IS_EARLY_GOING_BUT_NO_OT = table.Column<bool>(type: "boolean", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WORK_HOUR", x => x.WORK_ID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DEPARTMENT",
                columns: table => new
                {
                    DEPT_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    DEPT_PARENT = table.Column<string>(type: "varchar(100)", nullable: true),
                    DEPT_NAME = table.Column<string>(type: "varchar(100)", nullable: true),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    LEVEL = table.Column<int>(name: "[LEVEL]", type: "integer", nullable: true),
                    FLDTYPE = table.Column<string>(type: "varchar(1)", nullable: true),
                    NOSTAFF = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEPARTMENT", x => x.DEPT_ID);
                    table.ForeignKey(
                        name: "FK_DEPARTMENT_COMPANY_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANY",
                        principalColumn: "Company_Id");
                });

            migrationBuilder.CreateTable(
                name: "ATTENDANCE_LOG_NO_DIRECTION",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVICE_CODE = table.Column<string>(type: "varchar(20)", nullable: false),
                    DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TIME = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IS_SUCCESS = table.Column<bool>(type: "boolean", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    DEVICE_ID = table.Column<int>(type: "integer", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTENDANCE_LOG_NO_DIRECTION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ATTENDANCE_LOG_NO_DIRECTION_DEVICE_SETTINGS_DEVICE_ID",
                        column: x => x.DEVICE_ID,
                        principalTable: "DEVICE_SETTINGS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DEVICE_LOG",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEVICE_ID = table.Column<int>(type: "integer", nullable: true),
                    IS_SUCCESS = table.Column<bool>(type: "boolean", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(255)", nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "text", nullable: true),
                    ERROR_TRACE = table.Column<string>(type: "text", nullable: true),
                    DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEVICE_LOG", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DEVICE_LOG_DEVICE_SETTINGS_DEVICE_ID",
                        column: x => x.DEVICE_ID,
                        principalTable: "DEVICE_SETTINGS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "GRADE",
                columns: table => new
                {
                    GRADE_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GRADE_NAME = table.Column<string>(type: "varchar(30)", nullable: true),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    GRADE_TYPE = table.Column<string>(type: "varchar(50)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRADE", x => x.GRADE_ID);
                    table.ForeignKey(
                        name: "FK_GRADE_GRADE_TYPE_GRADE_TYPE",
                        column: x => x.GRADE_TYPE,
                        principalTable: "GRADE_TYPE",
                        principalColumn: "GType");
                });

            migrationBuilder.CreateTable(
                name: "EMP_LEAVE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PID = table.Column<int>(type: "integer", nullable: false),
                    LEAVEID = table.Column<short>(type: "smallint", nullable: false),
                    DAYS = table.Column<short>(type: "smallint", nullable: false),
                    LEAVEBY = table.Column<string>(type: "varchar(12)", nullable: true),
                    MAXDAYS = table.Column<short>(type: "smallint", nullable: true),
                    PAID = table.Column<string>(type: "varchar(7)", nullable: true),
                    UPTOMONTH = table.Column<short>(type: "smallint", nullable: true),
                    UPTOYEAR = table.Column<short>(type: "smallint", nullable: true),
                    BYDATE = table.Column<string>(type: "varchar(10)", nullable: true),
                    TRNUSER = table.Column<string>(type: "varchar(20)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_LEAVE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_LEAVE_LEAVE_LEAVEID",
                        column: x => x.LEAVEID,
                        principalTable: "LEAVE",
                        principalColumn: "LEAVE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE_LEDGER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Leave_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    LEAVE_ID = table.Column<short>(type: "smallint", nullable: true),
                    GIVEN = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    TAKEN = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    REMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    GIVENMONTH = table.Column<short>(type: "smallint", nullable: true),
                    GivenYear = table.Column<int>(type: "integer", nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    IsRegular = table.Column<byte>(type: "smallint", nullable: false),
                    ADJUSTED = table.Column<byte>(type: "smallint", nullable: false),
                    NOHRS = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    TUSER = table.Column<string>(type: "varchar(50)", nullable: true),
                    TDATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REFATTDT = table.Column<DateOnly>(type: "date", nullable: true),
                    HLEAVETYPE = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false),
                    IS_YEARLY = table.Column<bool>(type: "boolean", nullable: false),
                    CONTACT_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    ADDRESS = table.Column<string>(type: "varchar(255)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_LEDGER", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LEAVE_LEDGER_LEAVE_LEAVE_ID",
                        column: x => x.LEAVE_ID,
                        principalTable: "LEAVE",
                        principalColumn: "LEAVE_ID");
                    table.ForeignKey(
                        name: "FK_LEAVE_LEDGER_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE_LEDGER_TEMP",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Leave_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    LEAVE_ID = table.Column<short>(type: "smallint", nullable: true),
                    GIVEN = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    TAKEN = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    REMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    GIVENMONTH = table.Column<short>(type: "smallint", nullable: true),
                    GivenYear = table.Column<int>(type: "integer", nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    IsRegular = table.Column<byte>(type: "smallint", nullable: false),
                    ADJUSTED = table.Column<byte>(type: "smallint", nullable: false),
                    NOHRS = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    TUSER = table.Column<string>(type: "varchar(50)", nullable: true),
                    TDATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    REFATTDT = table.Column<DateOnly>(type: "date", nullable: true),
                    HLEAVETYPE = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_LEDGER_TEMP", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LEAVE_LEDGER_TEMP_LEAVE_LEAVE_ID",
                        column: x => x.LEAVE_ID,
                        principalTable: "LEAVE",
                        principalColumn: "LEAVE_ID");
                    table.ForeignKey(
                        name: "FK_LEAVE_LEDGER_TEMP_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE_YEAR_MONTHS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false),
                    MONTH_SEQUENCE = table.Column<int>(type: "integer", nullable: false),
                    MONTH = table.Column<int>(type: "integer", nullable: false),
                    YEAR = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_YEAR_MONTHS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LEAVE_YEAR_MONTHS_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SETTING",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GRANT_LEAVE_TYPE = table.Column<string>(type: "varchar(255)", nullable: false),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SETTING", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SETTING_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SALARY_HEADS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TRN_CODE = table.Column<string>(type: "varchar(200)", nullable: false),
                    SH_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    SH_CATEGORY_ID = table.Column<int>(type: "integer", nullable: false),
                    REF_ID = table.Column<int>(type: "integer", nullable: false),
                    DRCR = table.Column<char>(type: "char(2)", nullable: true),
                    SH_CALC_DATATYPE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_TYPE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_MODE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_CATEGORY = table.Column<int>(type: "integer", nullable: false),
                    IS_TAXABLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    MIN_HOURS = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MAX_NOS = table.Column<int>(type: "integer", nullable: false),
                    PER_UNIT_RATE = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UNIT_NAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    OFFICE_CONTRIBUTION = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CONTRIBUTION_TYPE = table.Column<int>(type: "integer", nullable: false),
                    DED_TAX_FREE_LIMIT_CHECK = table.Column<bool>(type: "boolean", nullable: false),
                    DED_TAX_FREE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ESTIMATE_POST_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    IS_LOCKED = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_HEADS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SALARY_HEADS_SALARY_HEAD_CATEGORY_SH_CATEGORY_ID",
                        column: x => x.SH_CATEGORY_ID,
                        principalTable: "SALARY_HEAD_CATEGORY",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CITY",
                columns: table => new
                {
                    CITY_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STATE_ID = table.Column<int>(type: "integer", nullable: false),
                    CITY_NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CITY", x => x.CITY_ID);
                    table.ForeignKey(
                        name: "FK_CITY_STATE_STATE_ID",
                        column: x => x.STATE_ID,
                        principalTable: "STATE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_DETAIL",
                columns: table => new
                {
                    EMP_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_TITLE = table.Column<string>(type: "varchar(10)", nullable: true),
                    EMP_FIRSTNAME = table.Column<string>(type: "varchar(30)", nullable: true),
                    EMP_MIDDLENAME = table.Column<string>(type: "varchar(30)", nullable: true),
                    EMP_LASTNAME = table.Column<string>(type: "varchar(30)", nullable: true),
                    EMP_TADD = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_TSTREET = table.Column<string>(type: "varchar(50)", nullable: true),
                    EMP_TDISTRICT = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_TZONE = table.Column<string>(type: "varchar(15)", nullable: true),
                    EMP_TCOUNTRY = table.Column<string>(type: "varchar(15)", nullable: true),
                    EMP_PADD = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PADD2 = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PCITY = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PPINCODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PSTATE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PDISTRICT = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PSTREET = table.Column<string>(type: "varchar(50)", nullable: true),
                    EMP_PZONE = table.Column<string>(type: "varchar(15)", nullable: true),
                    EMP_PCOUNTRY = table.Column<string>(type: "varchar(15)", nullable: true),
                    EMP_CADD = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CADD2 = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CCITY = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CPINCODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CSTATE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CDISTRICT = table.Column<string>(type: "varchar(20)", nullable: true),
                    EMP_NATIONALITY = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_PASSPORT_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_ADHAR_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_CITIZENSHIPNO = table.Column<string>(type: "varchar(50)", nullable: true),
                    EMP_PHONE = table.Column<string>(type: "varchar(14)", nullable: true),
                    EMP_DOB = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_BIRTH_COUNTRY_ID = table.Column<int>(type: "integer", nullable: true),
                    EMP_RELIGION_ID = table.Column<int>(type: "integer", nullable: true),
                    EMP_BIRTH_STATE_ID = table.Column<int>(type: "integer", nullable: true),
                    EMP_BIRTH_PLACE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_EDUCATION = table.Column<string>(type: "varchar(100)", nullable: true),
                    PHOTO = table.Column<string>(type: "text", nullable: true),
                    EMP_PASSWORD = table.Column<string>(type: "varchar(20)", nullable: true),
                    EMP_SIGNINMODE = table.Column<string>(type: "varchar(10)", nullable: true),
                    EMP_JOINDATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_MARRIAGE_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_RELEVING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    UpTo_Date = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_CARDID = table.Column<string>(type: "varchar(20)", nullable: true),
                    EMP_MOBILE = table.Column<string>(type: "varchar(15)", nullable: true),
                    EMP_GENDER = table.Column<char>(type: "char(1)", nullable: true),
                    EMP_MARITALSTATUS = table.Column<char>(type: "char(1)", nullable: true),
                    ATTBWISE = table.Column<byte>(type: "smallint", nullable: true),
                    EMP_APPDATE = table.Column<DateOnly>(type: "date", nullable: true),
                    EMP_PANNO = table.Column<string>(type: "varchar(20)", nullable: true),
                    EMP_APPOINTED = table.Column<byte>(type: "smallint", nullable: false),
                    EMP_EMAIL = table.Column<string>(type: "varchar(100)", nullable: true),
                    EMP_BLOOD_GROUP = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_CONTACT_PERSON = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_FATHER_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_MOTHER_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_GRAND_FATHER_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_DRVLISCENCENO = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_CONTACTNO = table.Column<string>(type: "varchar(250)", nullable: false),
                    T_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TUser = table.Column<string>(type: "varchar(250)", nullable: false),
                    EMP_BDay_CelebDt = table.Column<DateOnly>(type: "date", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_DETAIL", x => x.EMP_ID);
                    table.ForeignKey(
                        name: "FK_EMP_DETAIL_COUNTRY_EMP_BIRTH_COUNTRY_ID",
                        column: x => x.EMP_BIRTH_COUNTRY_ID,
                        principalTable: "COUNTRY",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_DETAIL_RELIGION_EMP_RELIGION_ID",
                        column: x => x.EMP_RELIGION_ID,
                        principalTable: "RELIGION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_DETAIL_STATE_EMP_BIRTH_STATE_ID",
                        column: x => x.EMP_BIRTH_STATE_ID,
                        principalTable: "STATE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DESIGNATION",
                columns: table => new
                {
                    DEG_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DEPARTMENT_ID = table.Column<int>(type: "integer", nullable: true),
                    DEG_PARENT = table.Column<string>(type: "varchar(100)", nullable: true),
                    DEG_NAME = table.Column<string>(type: "varchar(100)", nullable: true),
                    CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    Level = table.Column<short>(name: "[Level]", type: "smallint", nullable: true),
                    Deg_Rank = table.Column<int>(type: "integer", nullable: false),
                    NOSTAFF = table.Column<int>(type: "integer", nullable: false),
                    JOBDESC = table.Column<string>(type: "text", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DESIGNATION", x => x.DEG_ID);
                    table.ForeignKey(
                        name: "FK_DESIGNATION_DEPARTMENT_DEPARTMENT_ID",
                        column: x => x.DEPARTMENT_ID,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID");
                });

            migrationBuilder.CreateTable(
                name: "BRANCH",
                columns: table => new
                {
                    BRANCH_ID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BRANCH_NAME = table.Column<string>(type: "varchar(50)", nullable: true),
                    CITY_ID = table.Column<int>(type: "integer", nullable: true),
                    STATE_ID = table.Column<int>(type: "integer", nullable: true),
                    ISOUTBRANCH = table.Column<byte>(type: "smallint", nullable: true),
                    Address = table.Column<string>(type: "varchar(250)", nullable: false),
                    Contact = table.Column<string>(type: "varchar(150)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BRANCH", x => x.BRANCH_ID);
                    table.ForeignKey(
                        name: "FK_BRANCH_CITY_CITY_ID",
                        column: x => x.CITY_ID,
                        principalTable: "CITY",
                        principalColumn: "CITY_ID");
                    table.ForeignKey(
                        name: "FK_BRANCH_STATE_STATE_ID",
                        column: x => x.STATE_ID,
                        principalTable: "STATE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EmpId = table.Column<int>(type: "integer", nullable: true),
                    InOutTmChange = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    ExtDayChange = table.Column<bool>(type: "boolean", nullable: false),
                    BatchUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    UserAlert = table.Column<bool>(type: "boolean", nullable: false),
                    SelfAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    MonthlyBulkAttendance = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_EMP_DETAIL_EmpId",
                        column: x => x.EmpId,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "ASSET",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_Id = table.Column<int>(type: "integer", nullable: false),
                    ASSET_TYPE_ID = table.Column<int>(type: "integer", nullable: false),
                    GIVEN_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    RETURN_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    ASSET_DETAILS = table.Column<string>(type: "varchar(255)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSET", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ASSET_ASSET_TYPE_ASSET_TYPE_ID",
                        column: x => x.ASSET_TYPE_ID,
                        principalTable: "ASSET_TYPE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASSET_EMP_DETAIL_EMP_Id",
                        column: x => x.EMP_Id,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DEFAULT_WORKHOUR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_Id = table.Column<int>(type: "integer", nullable: true),
                    DAY_ID = table.Column<short>(type: "smallint", nullable: false),
                    WorkHour_Id = table.Column<short>(type: "smallint", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEFAULT_WORKHOUR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DEFAULT_WORKHOUR_EMP_DETAIL_Emp_Id",
                        column: x => x.Emp_Id,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_DEFAULT_WORKHOUR_WORK_HOUR_WorkHour_Id",
                        column: x => x.WorkHour_Id,
                        principalTable: "WORK_HOUR",
                        principalColumn: "WORK_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EARNED_LEAVE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    LEAVE_ID = table.Column<short>(type: "smallint", nullable: false),
                    DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EARNED_LEAVE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EARNED_LEAVE_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EARNED_LEAVE_LEAVE_LEAVE_ID",
                        column: x => x.LEAVE_ID,
                        principalTable: "LEAVE",
                        principalColumn: "LEAVE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_CALENDAR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CALENDAR_ID = table.Column<int>(type: "integer", nullable: false),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_CALENDAR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_CALENDAR_CALENDAR_CALENDAR_ID",
                        column: x => x.CALENDAR_ID,
                        principalTable: "CALENDAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMP_CALENDAR_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_DEVICE_CODE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    DEVICE_CODE = table.Column<string>(type: "varchar(255)", nullable: false),
                    DEVICE_ID = table.Column<int>(type: "integer", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LAST_FETCHED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_DEVICE_CODE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMP_DEVICE_CODE_DEVICE_SETTINGS_DEVICE_ID",
                        column: x => x.DEVICE_ID,
                        principalTable: "DEVICE_SETTINGS",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_DEVICE_CODE_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_DOCLIST",
                columns: table => new
                {
                    DOC_NO = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    FILE_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    FILE_EXT = table.Column<string>(type: "varchar(50)", nullable: false),
                    FILE_DESC = table.Column<string>(type: "varchar(250)", nullable: false),
                    REMARKS = table.Column<string>(type: "varchar(250)", nullable: true),
                    UPLOAD_DT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_DOCLIST", x => x.DOC_NO);
                    table.ForeignKey(
                        name: "FK_EMP_DOCLIST_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_EDUTRN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_Id = table.Column<int>(type: "integer", nullable: false),
                    ELevel = table.Column<short>(type: "smallint", nullable: true),
                    TName = table.Column<string>(type: "varchar(100)", nullable: false),
                    TDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TDateEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    MSubject = table.Column<string>(type: "varchar(255)", nullable: true),
                    Institute = table.Column<string>(type: "varchar(250)", nullable: true),
                    EDivision = table.Column<string>(type: "varchar(250)", nullable: true),
                    University = table.Column<string>(type: "varchar(255)", nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    flg = table.Column<byte>(type: "smallint", nullable: false),
                    Duration = table.Column<string>(type: "varchar(150)", nullable: true),
                    InstituteAdd = table.Column<string>(type: "varchar(250)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_EDUTRN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMP_EDUTRN_COUNTRY_CountryId",
                        column: x => x.CountryId,
                        principalTable: "COUNTRY",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_EDUTRN_EDUCATION_LEVEL_ELevel",
                        column: x => x.ELevel,
                        principalTable: "EDUCATION_LEVEL",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_EDUTRN_EMP_DETAIL_Emp_Id",
                        column: x => x.Emp_Id,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_EMPLOYMENTHISTORY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_Id = table.Column<int>(type: "integer", nullable: false),
                    EH_ORG = table.Column<string>(type: "varchar(100)", nullable: true),
                    FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DESIGNATION = table.Column<string>(type: "varchar(255)", nullable: true),
                    LOCATION = table.Column<string>(type: "varchar(255)", nullable: true),
                    CITY = table.Column<string>(type: "varchar(255)", nullable: true),
                    EH_ADD = table.Column<string>(type: "varchar(100)", nullable: true),
                    EH_JOBDESC = table.Column<string>(type: "varchar(100)", nullable: true),
                    EH_DURATION = table.Column<string>(type: "varchar(100)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_EMPLOYMENTHISTORY", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_EMPLOYMENTHISTORY_EMP_DETAIL_Emp_Id",
                        column: x => x.Emp_Id,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_FAMILY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    RELATIONSHIP_TYPE = table.Column<string>(type: "varchar(255)", nullable: false),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    GENDER = table.Column<string>(type: "varchar(255)", nullable: false),
                    DATE_OF_BIRTH = table.Column<DateOnly>(type: "date", nullable: false),
                    IS_WORKING = table.Column<bool>(type: "boolean", nullable: false),
                    PLACE_OF_BIRTH = table.Column<string>(type: "varchar(255)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_FAMILY", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_FAMILY_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "EMP_HOBBY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    TYPE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_HOBBY", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_HOBBY_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "EMP_LANGUAGE_KNOWN",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    LANGUAGE = table.Column<string>(type: "varchar(255)", nullable: false),
                    CAN_READ = table.Column<bool>(type: "boolean", nullable: false),
                    CAN_WRITE = table.Column<bool>(type: "boolean", nullable: false),
                    CAN_SPEAK = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_LANGUAGE_KNOWN", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_LANGUAGE_KNOWN_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "HOLIDAY",
                columns: table => new
                {
                    HOLIDAY_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    HOLIDAY_NAME = table.Column<string>(type: "varchar(50)", nullable: true),
                    HOLIDAY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DAY = table.Column<string>(type: "varchar(255)", nullable: true),
                    DAY_TYPE = table.Column<string>(type: "varchar(255)", nullable: true),
                    TYPE = table.Column<string>(type: "varchar(255)", nullable: true),
                    HOLIDAY_QTY = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HOLIDAY", x => x.HOLIDAY_ID);
                    table.ForeignKey(
                        name: "FK_HOLIDAY_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                });

            migrationBuilder.CreateTable(
                name: "EMP_TRAN",
                columns: table => new
                {
                    PID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_TRANMODE = table.Column<string>(type: "varchar(20)", nullable: false),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    DEPT_ID = table.Column<int>(type: "integer", nullable: true),
                    DEG_ID = table.Column<short>(type: "smallint", nullable: true),
                    BRANCH_ID = table.Column<short>(type: "smallint", nullable: true),
                    GRADE = table.Column<int>(type: "integer", nullable: true),
                    MODE_ID = table.Column<short>(type: "smallint", nullable: true),
                    STATUS_ID = table.Column<short>(type: "smallint", nullable: true),
                    BSALARY = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    BTAX = table.Column<float>(type: "real", nullable: true),
                    JDATE = table.Column<DateOnly>(type: "date", nullable: true),
                    JBSDATE = table.Column<string>(type: "varchar(12)", nullable: true),
                    BYDATE = table.Column<string>(type: "varchar(5)", nullable: true),
                    NEGATIVELEAVE = table.Column<bool>(type: "boolean", nullable: false),
                    GENERATEDBY = table.Column<string>(type: "varchar(50)", nullable: true),
                    REMARKS = table.Column<string>(type: "varchar(100)", nullable: true),
                    EMP_CATEGORY = table.Column<byte>(type: "smallint", nullable: true),
                    EMP_ATTGROUP = table.Column<byte>(type: "smallint", nullable: true),
                    TDS = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    EXT = table.Column<string>(type: "varchar(10)", nullable: true),
                    CLASS = table.Column<int>(type: "integer", nullable: true),
                    AccountNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    TDSTYPE = table.Column<byte>(type: "smallint", nullable: false),
                    EMP_CHECKIN_MODE = table.Column<byte>(type: "smallint", nullable: false),
                    DEF_CALC_DUTYHOUR = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TUser = table.Column<string>(type: "varchar(100)", nullable: false),
                    GRADEAMNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GDEAMNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DailyWage = table.Column<byte>(type: "smallint", nullable: false),
                    SAL_BANKID = table.Column<int>(type: "integer", nullable: true),
                    PF_BANKID = table.Column<int>(type: "integer", nullable: true),
                    PF_ACCOUNTNO = table.Column<string>(type: "varchar(50)", nullable: true),
                    T_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAYROLLMODE = table.Column<string>(type: "varchar(50)", nullable: false),
                    CalcLate = table.Column<byte>(type: "smallint", nullable: true),
                    Emp_Terminate = table.Column<byte>(type: "smallint", nullable: false),
                    Emp_Terminate_Month = table.Column<int>(type: "integer", nullable: false),
                    Emp_Terminate_Year = table.Column<int>(type: "integer", nullable: false),
                    OFFICIAL_CONTACT_NUMBER = table.Column<string>(type: "varchar(15)", nullable: true),
                    OFFICIAL_EMAIL = table.Column<string>(type: "varchar(30)", nullable: true),
                    RM_EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    HOD_EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    COMPANY_ID = table.Column<int>(type: "integer", nullable: true),
                    BUSINESS_UNIT_ID = table.Column<int>(type: "integer", nullable: true),
                    PLANT_ID = table.Column<int>(type: "integer", nullable: true),
                    REGION_ID = table.Column<int>(type: "integer", nullable: true),
                    DIVISION_ID = table.Column<int>(type: "integer", nullable: true),
                    SUB_DEPARTMENT_ID = table.Column<int>(type: "integer", nullable: true),
                    PERSONAL_AREA = table.Column<string>(type: "varchar(255)", nullable: true),
                    SUB_AREA = table.Column<string>(type: "varchar(255)", nullable: true),
                    COST_CENTER_ID = table.Column<int>(type: "integer", nullable: true),
                    POSITION_CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    EMP_SUB_TYPE = table.Column<string>(type: "varchar(255)", nullable: true),
                    PERSON_TYPE = table.Column<string>(type: "varchar(255)", nullable: true),
                    UNIFORM_STATUS = table.Column<bool>(type: "boolean", nullable: true),
                    UNIFORM_TYPE_ID = table.Column<int>(type: "integer", nullable: true),
                    EXTRA_UNIFORM = table.Column<bool>(type: "boolean", nullable: true),
                    ESI_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    UAN_NUMBER = table.Column<string>(type: "varchar(255)", nullable: true),
                    PF_APPLICABLE = table.Column<bool>(type: "boolean", nullable: true),
                    IS_CEILING = table.Column<bool>(type: "boolean", nullable: true),
                    EPS_APPLICABLE = table.Column<bool>(type: "boolean", nullable: true),
                    IFSC_CODE = table.Column<string>(type: "varchar(255)", nullable: true),
                    VPF_APPLICABLE = table.Column<bool>(type: "boolean", nullable: true),
                    VPF_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_TRAN", x => x.PID);
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_BRANCH_BRANCH_ID",
                        column: x => x.BRANCH_ID,
                        principalTable: "BRANCH",
                        principalColumn: "BRANCH_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_BUSINESS_UNIT_BUSINESS_UNIT_ID",
                        column: x => x.BUSINESS_UNIT_ID,
                        principalTable: "BUSINESS_UNIT",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_COMPANY_COMPANY_ID",
                        column: x => x.COMPANY_ID,
                        principalTable: "COMPANY",
                        principalColumn: "Company_Id");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_COST_CENTER_COST_CENTER_ID",
                        column: x => x.COST_CENTER_ID,
                        principalTable: "COST_CENTER",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_DEPARTMENT_DEPT_ID",
                        column: x => x.DEPT_ID,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_DEPARTMENT_SUB_DEPARTMENT_ID",
                        column: x => x.SUB_DEPARTMENT_ID,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_DESIGNATION_DEG_ID",
                        column: x => x.DEG_ID,
                        principalTable: "DESIGNATION",
                        principalColumn: "DEG_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_DIVISION_DIVISION_ID",
                        column: x => x.DIVISION_ID,
                        principalTable: "DIVISION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_EMP_DETAIL_HOD_EMP_ID",
                        column: x => x.HOD_EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_EMP_DETAIL_RM_EMP_ID",
                        column: x => x.RM_EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_GRADE_GRADE",
                        column: x => x.GRADE,
                        principalTable: "GRADE",
                        principalColumn: "GRADE_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_MODE_MODE_ID",
                        column: x => x.MODE_ID,
                        principalTable: "MODE",
                        principalColumn: "MODE_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_PLANT_PLANT_ID",
                        column: x => x.PLANT_ID,
                        principalTable: "PLANT",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_REGION_REGION_ID",
                        column: x => x.REGION_ID,
                        principalTable: "REGION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_STATUS_STATUS_ID",
                        column: x => x.STATUS_ID,
                        principalTable: "STATUS",
                        principalColumn: "STATUS_ID");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_tblBank_PF_BANKID",
                        column: x => x.PF_BANKID,
                        principalTable: "tblBank",
                        principalColumn: "BankId");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_tblBank_SAL_BANKID",
                        column: x => x.SAL_BANKID,
                        principalTable: "tblBank",
                        principalColumn: "BankId");
                    table.ForeignKey(
                        name: "FK_EMP_TRAN_UNIFORM_TYPE_UNIFORM_TYPE_ID",
                        column: x => x.UNIFORM_TYPE_ID,
                        principalTable: "UNIFORM_TYPE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "WEEKEND_DETAIL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emp_ID = table.Column<int>(type: "integer", nullable: true),
                    Branch_Id = table.Column<short>(type: "smallint", nullable: true),
                    TDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Sun = table.Column<bool>(type: "boolean", nullable: false),
                    Mon = table.Column<bool>(type: "boolean", nullable: false),
                    Tue = table.Column<bool>(type: "boolean", nullable: false),
                    Wed = table.Column<bool>(type: "boolean", nullable: false),
                    Thu = table.Column<bool>(type: "boolean", nullable: false),
                    Fri = table.Column<bool>(type: "boolean", nullable: false),
                    Sat = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WEEKEND_DETAIL", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WEEKEND_DETAIL_BRANCH_Branch_Id",
                        column: x => x.Branch_Id,
                        principalTable: "BRANCH",
                        principalColumn: "BRANCH_ID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DOCUMENTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILENAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    FILE_PATH = table.Column<string>(type: "varchar(255)", nullable: false),
                    UPLOADED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCUMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DOCUMENTS_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IMPORT_ATTENDANCE_LOGS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILENAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    ATTENDANCE_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    STATUS = table.Column<string>(type: "varchar(255)", nullable: false),
                    IS_UPlOADED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_SYNCED = table.Column<bool>(type: "boolean", nullable: false),
                    ERROR_MESSAGE = table.Column<string>(type: "text", nullable: true),
                    ERROR_TRACE = table.Column<string>(type: "text", nullable: true),
                    FILE_PATH = table.Column<string>(type: "text", nullable: false),
                    UPLOADED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SYNCED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMPORT_ATTENDANCE_LOGS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IMPORT_ATTENDANCE_LOGS_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LEAVE_APPLICATION_HISTORY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    HALF_LEAVE_TYPE = table.Column<byte>(type: "smallint", nullable: false),
                    LEAVE_ID = table.Column<short>(type: "smallint", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TOTAL_DAYS = table.Column<decimal>(type: "numeric", nullable: true),
                    CONTACT_NUMBER = table.Column<string>(type: "varchar(255)", nullable: false),
                    ADDRESS = table.Column<string>(type: "varchar(255)", nullable: true),
                    REASON = table.Column<string>(type: "varchar(255)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "varchar(255)", nullable: true),
                    ApprovedBy_Id = table.Column<int>(type: "integer", nullable: true),
                    DisapprovedBy_Id = table.Column<int>(type: "integer", nullable: true),
                    Cancellation_Remarks = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_APPLICATION_HISTORY", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_ApprovedBy_Id",
                        column: x => x.ApprovedBy_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_DisapprovedBy_Id",
                        column: x => x.DisapprovedBy_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LEAVE_APPLICATION_HISTORY_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LEAVE_APPLICATION_HISTORY_LEAVE_LEAVE_ID",
                        column: x => x.LEAVE_ID,
                        principalTable: "LEAVE",
                        principalColumn: "LEAVE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LEAVE_APPLICATION_HISTORY_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PDFSERVICEDOCUMENTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILENAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    FILE_PATH = table.Column<string>(type: "varchar(255)", nullable: false),
                    UPLOADED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFSERVICEDOCUMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PDFSERVICEDOCUMENTS_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "REGULARISATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    REGULARISATION_TYPE = table.Column<string>(type: "varchar(255)", nullable: false),
                    FROM_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    FROM_TIME = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    TO_TIME = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CONTACT_NUMBER = table.Column<string>(type: "varchar(10)", nullable: true),
                    PLACE = table.Column<string>(type: "varchar(255)", nullable: true),
                    REASON = table.Column<string>(type: "varchar(255)", nullable: true),
                    STATUS = table.Column<string>(type: "varchar(255)", nullable: true),
                    APPROVED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    DISAPPROVED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    REMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    CANCELLATION_REMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARISATION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARISATION_AspNetUsers_APPROVED_BY_USER_ID",
                        column: x => x.APPROVED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_REGULARISATION_AspNetUsers_DISAPPROVED_BY_USER_ID",
                        column: x => x.DISAPPROVED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_REGULARISATION_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ROSTER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    WORK_HOUR_ID = table.Column<short>(type: "smallint", nullable: false),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROSTER", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ROSTER_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROSTER_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROSTER_WORK_HOUR_WORK_HOUR_ID",
                        column: x => x.WORK_HOUR_ID,
                        principalTable: "WORK_HOUR",
                        principalColumn: "WORK_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HOLIDAY_CALENDAR",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HOLIDAY_ID = table.Column<int>(type: "integer", nullable: false),
                    CALENDAR_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HOLIDAY_CALENDAR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HOLIDAY_CALENDAR_CALENDAR_CALENDAR_ID",
                        column: x => x.CALENDAR_ID,
                        principalTable: "CALENDAR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HOLIDAY_CALENDAR_HOLIDAY_HOLIDAY_ID",
                        column: x => x.HOLIDAY_ID,
                        principalTable: "HOLIDAY",
                        principalColumn: "HOLIDAY_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ATTENDANCE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKHOUR_ID = table.Column<short>(type: "smallint", nullable: true),
                    Emp_Id = table.Column<int>(type: "integer", nullable: false),
                    TDATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TDATE_OUT = table.Column<DateOnly>(type: "date", nullable: true),
                    INTIME = table.Column<string>(type: "varchar(15)", nullable: true),
                    INMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    INREMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    OUTTIME = table.Column<string>(type: "varchar(15)", nullable: true),
                    OUTMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    OUTREMARKS = table.Column<string>(type: "varchar(255)", nullable: true),
                    LUNCHIN = table.Column<string>(type: "varchar(15)", nullable: true),
                    LUNCHINMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    LUNCHINREMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    LUNCHOUT = table.Column<string>(type: "varchar(15)", nullable: true),
                    LUNCHOUTMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    LUNCHOUTREMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    TIFFININ = table.Column<string>(type: "varchar(15)", nullable: true),
                    TIFFININMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    TIFFININREMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    TIFFINOUT = table.Column<string>(type: "varchar(15)", nullable: true),
                    TIFFINOUTMODE = table.Column<string>(type: "varchar(20)", nullable: true),
                    TIFFINOUTREMARKS = table.Column<string>(type: "varchar(50)", nullable: true),
                    ATT_STATUS = table.Column<byte>(type: "smallint", nullable: false),
                    OUT_VNO = table.Column<string>(type: "varchar(20)", nullable: true),
                    IS_HALTED = table.Column<byte>(type: "smallint", nullable: true),
                    FLGIN = table.Column<bool>(type: "boolean", nullable: false),
                    FLGOUT = table.Column<bool>(type: "boolean", nullable: false),
                    DT_LOUT = table.Column<DateOnly>(type: "date", nullable: true),
                    DT_LIN = table.Column<DateOnly>(type: "date", nullable: true),
                    DT_TOUT = table.Column<DateOnly>(type: "date", nullable: true),
                    DT_TIN = table.Column<DateOnly>(type: "date", nullable: true),
                    ATT_TYPE = table.Column<byte>(type: "smallint", nullable: false),
                    CHECKIN_MODE = table.Column<char>(type: "char(1)", nullable: false),
                    ATTID = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    SIN_TIMEVAL = table.Column<long>(type: "bigint", nullable: false),
                    SOUT_TIMEVAL = table.Column<long>(type: "bigint", nullable: false),
                    REGULARISATION_ID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTENDANCE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ATTENDANCE_EMP_DETAIL_Emp_Id",
                        column: x => x.Emp_Id,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ATTENDANCE_REGULARISATION_REGULARISATION_ID",
                        column: x => x.REGULARISATION_ID,
                        principalTable: "REGULARISATION",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ATTENDANCE_WORK_HOUR_WORKHOUR_ID",
                        column: x => x.WORKHOUR_ID,
                        principalTable: "WORK_HOUR",
                        principalColumn: "WORK_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmpId",
                table: "AspNetUsers",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_ASSET_TYPE_ID",
                table: "ASSET",
                column: "ASSET_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ASSET_EMP_Id",
                table: "ASSET",
                column: "EMP_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ATTENDANCE_Emp_Id",
                table: "ATTENDANCE",
                column: "Emp_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ATTENDANCE_REGULARISATION_ID",
                table: "ATTENDANCE",
                column: "REGULARISATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ATTENDANCE_WORKHOUR_ID",
                table: "ATTENDANCE",
                column: "WORKHOUR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ATTENDANCE_LOG_NO_DIRECTION_DEVICE_ID",
                table: "ATTENDANCE_LOG_NO_DIRECTION",
                column: "DEVICE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BRANCH_CITY_ID",
                table: "BRANCH",
                column: "CITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BRANCH_STATE_ID",
                table: "BRANCH",
                column: "STATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CITY_STATE_ID",
                table: "CITY",
                column: "STATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DEFAULT_WORKHOUR_Emp_Id",
                table: "DEFAULT_WORKHOUR",
                column: "Emp_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DEFAULT_WORKHOUR_WorkHour_Id",
                table: "DEFAULT_WORKHOUR",
                column: "WorkHour_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DEPARTMENT_CompanyId",
                table: "DEPARTMENT",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DESIGNATION_DEPARTMENT_ID",
                table: "DESIGNATION",
                column: "DEPARTMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DEVICE_LOG_DEVICE_ID",
                table: "DEVICE_LOG",
                column: "DEVICE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DOCUMENTS_USER_ID",
                table: "DOCUMENTS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EARNED_LEAVE_EMP_ID",
                table: "EARNED_LEAVE",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EARNED_LEAVE_LEAVE_ID",
                table: "EARNED_LEAVE",
                column: "LEAVE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_CALENDAR_CALENDAR_ID",
                table: "EMP_CALENDAR",
                column: "CALENDAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_CALENDAR_EMP_ID",
                table: "EMP_CALENDAR",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_EMP_BIRTH_COUNTRY_ID",
                table: "EMP_DETAIL",
                column: "EMP_BIRTH_COUNTRY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_EMP_BIRTH_STATE_ID",
                table: "EMP_DETAIL",
                column: "EMP_BIRTH_STATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_EMP_RELIGION_ID",
                table: "EMP_DETAIL",
                column: "EMP_RELIGION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DEVICE_CODE_DEVICE_ID",
                table: "EMP_DEVICE_CODE",
                column: "DEVICE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DEVICE_CODE_EMP_ID",
                table: "EMP_DEVICE_CODE",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DOCLIST_EMP_ID",
                table: "EMP_DOCLIST",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EDUTRN_CountryId",
                table: "EMP_EDUTRN",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EDUTRN_ELevel",
                table: "EMP_EDUTRN",
                column: "ELevel");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EDUTRN_Emp_Id",
                table: "EMP_EDUTRN",
                column: "Emp_Id");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_EMPLOYMENTHISTORY_Emp_Id",
                table: "EMP_EMPLOYMENTHISTORY",
                column: "Emp_Id");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_FAMILY_EMP_ID",
                table: "EMP_FAMILY",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_HOBBY_EMP_ID",
                table: "EMP_HOBBY",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_LANGUAGE_KNOWN_EMP_ID",
                table: "EMP_LANGUAGE_KNOWN",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_LEAVE_LEAVEID",
                table: "EMP_LEAVE",
                column: "LEAVEID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_BRANCH_ID",
                table: "EMP_TRAN",
                column: "BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_BUSINESS_UNIT_ID",
                table: "EMP_TRAN",
                column: "BUSINESS_UNIT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_COMPANY_ID",
                table: "EMP_TRAN",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_COST_CENTER_ID",
                table: "EMP_TRAN",
                column: "COST_CENTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_DEG_ID",
                table: "EMP_TRAN",
                column: "DEG_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_DEPT_ID",
                table: "EMP_TRAN",
                column: "DEPT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_DIVISION_ID",
                table: "EMP_TRAN",
                column: "DIVISION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_EMP_ID",
                table: "EMP_TRAN",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_GRADE",
                table: "EMP_TRAN",
                column: "GRADE");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_HOD_EMP_ID",
                table: "EMP_TRAN",
                column: "HOD_EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_MODE_ID",
                table: "EMP_TRAN",
                column: "MODE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_PF_BANKID",
                table: "EMP_TRAN",
                column: "PF_BANKID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_PLANT_ID",
                table: "EMP_TRAN",
                column: "PLANT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_REGION_ID",
                table: "EMP_TRAN",
                column: "REGION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_RM_EMP_ID",
                table: "EMP_TRAN",
                column: "RM_EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_SAL_BANKID",
                table: "EMP_TRAN",
                column: "SAL_BANKID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_STATUS_ID",
                table: "EMP_TRAN",
                column: "STATUS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_SUB_DEPARTMENT_ID",
                table: "EMP_TRAN",
                column: "SUB_DEPARTMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_UNIFORM_TYPE_ID",
                table: "EMP_TRAN",
                column: "UNIFORM_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_GRADE_GRADE_TYPE",
                table: "GRADE",
                column: "GRADE_TYPE");

            migrationBuilder.CreateIndex(
                name: "IX_HOLIDAY_EMP_ID",
                table: "HOLIDAY",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_HOLIDAY_CALENDAR_CALENDAR_ID",
                table: "HOLIDAY_CALENDAR",
                column: "CALENDAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_HOLIDAY_CALENDAR_HOLIDAY_ID",
                table: "HOLIDAY_CALENDAR",
                column: "HOLIDAY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_IMPORT_ATTENDANCE_LOGS_USER_ID",
                table: "IMPORT_ATTENDANCE_LOGS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_ApprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "ApprovedBy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_DisapprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "DisapprovedBy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_EMP_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_LEAVE_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "LEAVE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_LEAVE_YEAR_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_LEDGER_LEAVE_ID",
                table: "LEAVE_LEDGER",
                column: "LEAVE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_LEDGER_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_LEDGER_TEMP_LEAVE_ID",
                table: "LEAVE_LEDGER_TEMP",
                column: "LEAVE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_LEDGER_TEMP_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER_TEMP",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_YEAR_MONTHS_LEAVE_YEAR_ID",
                table: "LEAVE_YEAR_MONTHS",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PDFSERVICEDOCUMENTS_USER_ID",
                table: "PDFSERVICEDOCUMENTS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_REGULARISATION_APPROVED_BY_USER_ID",
                table: "REGULARISATION",
                column: "APPROVED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_REGULARISATION_DISAPPROVED_BY_USER_ID",
                table: "REGULARISATION",
                column: "DISAPPROVED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_REGULARISATION_EMP_ID",
                table: "REGULARISATION",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROSTER_EMP_ID",
                table: "ROSTER",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROSTER_USER_ID",
                table: "ROSTER",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ROSTER_WORK_HOUR_ID",
                table: "ROSTER",
                column: "WORK_HOUR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_HEADS_SH_CATEGORY_ID",
                table: "SALARY_HEADS",
                column: "SH_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SETTING_LEAVE_YEAR_ID",
                table: "SETTING",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WEEKEND_DETAIL_Branch_Id",
                table: "WEEKEND_DETAIL",
                column: "Branch_Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ASSET");

            migrationBuilder.DropTable(
                name: "ATTENDANCE");

            migrationBuilder.DropTable(
                name: "ATTENDANCE_LOG");

            migrationBuilder.DropTable(
                name: "ATTENDANCE_LOG_NO_DIRECTION");

            migrationBuilder.DropTable(
                name: "BLOOD_GROUP");

            migrationBuilder.DropTable(
                name: "CLASS");

            migrationBuilder.DropTable(
                name: "DEFAULT_WORKHOUR");

            migrationBuilder.DropTable(
                name: "DEVICE_LOG");

            migrationBuilder.DropTable(
                name: "DOCUMENTS");

            migrationBuilder.DropTable(
                name: "EARNED_LEAVE");

            migrationBuilder.DropTable(
                name: "EMP_CALENDAR");

            migrationBuilder.DropTable(
                name: "EMP_DAILYOUT");

            migrationBuilder.DropTable(
                name: "EMP_DEVICE_CODE");

            migrationBuilder.DropTable(
                name: "EMP_DOCLIST");

            migrationBuilder.DropTable(
                name: "EMP_EDUTRN");

            migrationBuilder.DropTable(
                name: "EMP_EMPLOYMENTHISTORY");

            migrationBuilder.DropTable(
                name: "EMP_FAMILY");

            migrationBuilder.DropTable(
                name: "EMP_HOBBY");

            migrationBuilder.DropTable(
                name: "EMP_LANGUAGE_KNOWN");

            migrationBuilder.DropTable(
                name: "EMP_LEAVE");

            migrationBuilder.DropTable(
                name: "EMP_LOG");

            migrationBuilder.DropTable(
                name: "EMP_TRAN");

            migrationBuilder.DropTable(
                name: "HOLIDAY_CALENDAR");

            migrationBuilder.DropTable(
                name: "IMPORT_ATTENDANCE_LOGS");

            migrationBuilder.DropTable(
                name: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropTable(
                name: "LEAVE_LEDGER");

            migrationBuilder.DropTable(
                name: "LEAVE_LEDGER_TEMP");

            migrationBuilder.DropTable(
                name: "LEAVE_YEAR_MONTHS");

            migrationBuilder.DropTable(
                name: "PDFSERVICEDOCUMENTS");

            migrationBuilder.DropTable(
                name: "ROSTER");

            migrationBuilder.DropTable(
                name: "SALARY_HEADS");

            migrationBuilder.DropTable(
                name: "SETTING");

            migrationBuilder.DropTable(
                name: "tblFATTLOG");

            migrationBuilder.DropTable(
                name: "TITLES");

            migrationBuilder.DropTable(
                name: "WEEKEND_DETAIL");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ASSET_TYPE");

            migrationBuilder.DropTable(
                name: "REGULARISATION");

            migrationBuilder.DropTable(
                name: "DEVICE_SETTINGS");

            migrationBuilder.DropTable(
                name: "EDUCATION_LEVEL");

            migrationBuilder.DropTable(
                name: "BUSINESS_UNIT");

            migrationBuilder.DropTable(
                name: "COST_CENTER");

            migrationBuilder.DropTable(
                name: "DESIGNATION");

            migrationBuilder.DropTable(
                name: "DIVISION");

            migrationBuilder.DropTable(
                name: "GRADE");

            migrationBuilder.DropTable(
                name: "MODE");

            migrationBuilder.DropTable(
                name: "PLANT");

            migrationBuilder.DropTable(
                name: "REGION");

            migrationBuilder.DropTable(
                name: "STATUS");

            migrationBuilder.DropTable(
                name: "tblBank");

            migrationBuilder.DropTable(
                name: "UNIFORM_TYPE");

            migrationBuilder.DropTable(
                name: "CALENDAR");

            migrationBuilder.DropTable(
                name: "HOLIDAY");

            migrationBuilder.DropTable(
                name: "LEAVE");

            migrationBuilder.DropTable(
                name: "WORK_HOUR");

            migrationBuilder.DropTable(
                name: "SALARY_HEAD_CATEGORY");

            migrationBuilder.DropTable(
                name: "LEAVE_YEAR");

            migrationBuilder.DropTable(
                name: "BRANCH");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DEPARTMENT");

            migrationBuilder.DropTable(
                name: "GRADE_TYPE");

            migrationBuilder.DropTable(
                name: "CITY");

            migrationBuilder.DropTable(
                name: "EMP_DETAIL");

            migrationBuilder.DropTable(
                name: "COMPANY");

            migrationBuilder.DropTable(
                name: "COUNTRY");

            migrationBuilder.DropTable(
                name: "RELIGION");

            migrationBuilder.DropTable(
                name: "STATE");
        }
    }
}
