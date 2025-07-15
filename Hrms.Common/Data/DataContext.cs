using Hrms.Common.Models;
using MathNet.Numerics.RootFinding;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hrms.Common.Data
{
    public class DataContext : IdentityDbContext<User, Role, int,
        IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementCategory> AnnouncementCategories { get; set; }
        public DbSet<AnnouncementDocuments> AnnouncementDocuments { get; set; }
        public DbSet<AnnouncementRecipient> AnnouncementRecipients { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<AttendanceLogNoDirection> AttendanceLogNoDirections { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BloodGroup> BloodGroups { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BusinessUnit> BusinessUnits { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateActivityLog> CandidateActivityLogs { get; set; }
        public DbSet<CandidateSource> CandidateSources { get; set; }
        public DbSet<CandidateStage> CandidateStages { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CostCenter> CostCenters { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<DefaultWorkHour> DefaultWorkHours { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<DeviceLog> DeviceLogs { get; set; }
        public DbSet<DeviceSetting> DeviceSettings { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<EarnedLeave> EarnedLeaves { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<EducationLevel> EducationLevels { get; set; }
        public DbSet<EmpCalendar> EmpCalendars { get; set; }
        public DbSet<EmpDetail> EmpDetails { get; set; }
        public DbSet<EmpDeviceCode> EmpDeviceCodes { get; set; }
        public DbSet<EmpDocument> EmpDocuments { get; set; }
        public DbSet<EmpLeave> EmpLeaves { get; set; }
        public DbSet<EmpLog> EmpLogs { get; set; }
        public DbSet<EmploymentHistory> EmploymentHistories { get; set; }
        public DbSet<EmploymentType> EmploymentTypes { get; set; }
        public DbSet<EmpSalaryRecord> EmpSalaryRecords { get; set; }
        public DbSet<EmpSalaryHead> EmpSalaryHeads { get; set; }
        public DbSet<EmpSalaryHeadDetail> EmpSalaryHeadDetails { get; set; }
        public DbSet<EmpTransaction> EmpTransactions { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<DepartmentForLevel> DepartmentForLevels { get; set; }
        public DbSet<HierarchyLevel> HierarchyLeves { get; set; }
        public DbSet<ForcedAttendance> ForcedAttendances { get; set; }
        public DbSet<GatePassType> GatePassTypes { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeType> GradeTypes { get; set; }
        public DbSet<HiringStage> HiringStages { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<HolidayCalendar> HolidayCalendars { get; set; }
        public DbSet<ImageCollection> ImageCollection { get; set; }
        public DbSet<ImagesFolder> ImagesFolders { get; set; }
        public DbSet<ImportAttendanceLog> ImportAttendanceLogs { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<LanguageKnown> LanguagesKnown { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveApplicationHistory> LeaveApplicationHistories { get; set; }
        public DbSet<LeaveLedger> LeaveLedgers { get; set; }
        public DbSet<LeaveLedgerTemp> LeaveLedgersTemp { get; set; }
        public DbSet<LeaveYear> LeaveYears { get; set; }
        public DbSet<LeaveYearCompany> LeaveYearCompanies { get; set; }
        public DbSet<LeaveYearMonths> LeaveYearMonths { get; set; }
        public DbSet<ManpowerRequisition> ManpowerRequisitions { get; set; }
        public DbSet<Mode> Modes { get; set; }
        public DbSet<OfficeOut> OfficeOuts { get; set; }
        public DbSet<PayBill> PayBills { get; set; }
        public DbSet<PayBillSalaryHead> PayBillSalaryHeads { get; set; }
        public DbSet<PdfServiceDocument> PdfServiceDocuments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegularisationType> RegularisationTypes { get; set; }
        public DbSet<Regularisation> Regularisations { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Roster> Rosters { get; set; }
        public DbSet<SalaryHead> SalaryHeads { get; set; }
        public DbSet<SalaryHeadOld> SalaryHeadOlds { get; set; }
        public DbSet<SalaryHeadCategory> SalaryHeadCategories { get; set; }
        public DbSet<SalaryAnnexure> SalaryAnnexures { get; set; }
        public DbSet<SalaryAnnexureHead> SalaryAnnexureHeads { get; set; }
        public DbSet<SalaryAnnexureHeadDetail> SalaryAnnexureHeadDetails { get; set; }
        public DbSet<SalarySlipDocument> SalarySlipDocuments { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<SyncAttendanceLog> SyncAttendanceLogs { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<UGroup> UGroups { get; set; }
        public DbSet<UniformType> UniformTypes { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<WeekendDetail> WeekendDetails { get; set; }
        public DbSet<Wishlist> Wishlist { get; set; }
        public DbSet<WorkHour> WorkHours { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<LoanStatus> LoanStatus { get; set; }
        public DbSet<LoanDisbursement> LoanDisbursements { get; set; }
        public DbSet<CandidateDocument> CandidateDocuments { get; set; }
        public DbSet<LoanRepayment> LoanRepayments { get; set; }
        public DbSet<EncashmentHistory> EncashmentHistories { get; set; }
        public DbSet<LeaveEncashmentRequest> leaveEncashmentRequests { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<InterviewAttendece> interviewAttendeces { get; set; }
        public DbSet<EmpMedicalRegistration> empMedicalRegistrations { get; set; }
        public DbSet<Bhandar> Bhandars { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<AccountCode> AccountCodes { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<Role>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            builder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });

            builder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId);

            builder.Entity<UserGroup>()
                .HasOne(ug => ug.UGroup)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId);

            builder.Entity<AnnouncementDocuments>()
                .Property(d => d.FilePath)
                .IsRequired();

            builder.Entity<AnnouncementRecipient>()
                .HasOne(ar => ar.Announcement)
                .WithMany(a => a.AnnouncementRecipients)
                .HasForeignKey(ar => ar.AnnouncementId);

            builder.Entity<AnnouncementRecipient>()
                .HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId);

            builder.Entity<AnnouncementRecipient>()
                .HasOne(ar => ar.UGroup)
                .WithMany()
                .HasForeignKey(ar => ar.GroupId);

            builder.Entity<Announcement>()
                .HasMany(a => a.AnnouncementDocuments)
                .WithOne(ad => ad.Announcement)
                .HasForeignKey(ad => ad.AnnouncementId);

            builder.Entity<Announcement>()
                .HasMany(a => a.AnnouncementRecipients)
                .WithOne(ar => ar.Announcement)
                .HasForeignKey(ar => ar.AnnouncementId);

            builder.Entity<LeaveYearCompany>()
                .HasOne(lyc => lyc.LeaveYear)
                .WithMany(ly => ly.LeaveYearCompanies)
                .HasForeignKey(lyc => lyc.LeaveYearId);

            builder.Entity<LeaveYearCompany>()
                .HasOne(lyc => lyc.Company)
                .WithMany(c => c.LeaveYearCompanies)
                .HasForeignKey(lyc => lyc.CompanyId);
            builder.Entity<SalaryHead>()
                .HasOne(a => a.ShCategory)
                .WithMany(ar => ar.SalaryHeads)
                .HasForeignKey(ar => ar.ShcId);

            builder.Entity<SalaryAnnexureHead>()
                .HasOne(sah => sah.SalaryAnnexure)
                .WithMany(sa => sa.SalaryAnnexureHeads)
                .HasForeignKey(sah => sah.AnxId);

            builder.Entity<SalaryAnnexureHeadDetail>()
                .HasOne(sahd => sahd.SalaryAnnexureHead)
                .WithMany(sah => sah.SalaryAnnexureHeadDetails)
                .HasForeignKey(sahd => sahd.SalaryAnnexureHeadId);

            builder.Entity<EmpSalaryHead>()
                .HasOne(esh => esh.EmpSalaryRecord)
                .WithMany(esr => esr.EmpSalaryHeads)
                .HasForeignKey(esh => esh.EmpSalaryRecordId);

            builder.Entity<EmpSalaryHeadDetail>()
                .HasOne(eshd => eshd.EmpSh)
                .WithMany(esh => esh.EmpSalaryHeadDetails)
                .HasForeignKey(eshd => eshd.EmpShId);

            builder.Entity<Bhandar>()
                .HasIndex(b => b.Name)
                .IsUnique();

            builder.Entity<Guest>()
                .HasIndex(b => b.Name)
                .IsUnique();

            builder.Entity<AccountType>()
                  .HasIndex(a => a.Name)
                       .IsUnique();

            builder.Entity<AccountCode>()
                  .HasIndex(a => a.Name)
                  .IsUnique();

            builder.Entity<EmpTransaction>()
                   .HasOne(e => e.Guest)
                   .WithMany()
                   .HasForeignKey(e => e.GuestId);

            builder.Entity<EmpTransaction>()
                .HasOne(e => e.Bhandar)
                .WithMany()
                .HasForeignKey(e => e.BhandarId);

            builder.Entity<EmpTransaction>()
                .HasOne(e => e.AccountType)
                .WithMany()
                .HasForeignKey(e => e.AccountTypeId);

            builder.Entity<EmpTransaction>()
                .HasOne(e => e.AccountCode)
                .WithMany()
                .HasForeignKey(e => e.AccountCodeId);
        }
    }
}
