namespace Hrms.Worker.AttendanceData
{
    public class AttendanceDataContext : DbContext
    {
        public AttendanceDataContext(DbContextOptions<AttendanceDataContext> options) : base(options) { }

        public DbSet<DeviceLogsInfo> DeviceLogsInfos { get; set; }

        public DbSet<AttendanceSyncStatus> AttendanceSyncStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeviceLogsInfo>(entity =>
            {
                entity.HasKey(e => new { e.DeviceLogId, e.LogDate, e.UserId })
                    .HasName("PRIMARY");

                entity.ToTable("devicelogsinfo");

                entity.HasIndex(e => new { e.LogDate, e.UserId }, "PK_DeviceLogsInfo_01")
                    .IsUnique();

                entity.Property(e => e.DeviceLogId)
                    .HasColumnType("bigint(20)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LogDate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'1971-01-01 00:00:01'");

                entity.Property(e => e.UserId).HasMaxLength(50);

                entity.Property(e => e.AttDirection).HasMaxLength(255);

                entity.Property(e => e.C1).HasMaxLength(255);

                entity.Property(e => e.C2).HasMaxLength(255);

                entity.Property(e => e.C3).HasMaxLength(255);

                entity.Property(e => e.C4).HasMaxLength(255);

                entity.Property(e => e.C5).HasMaxLength(255);

                entity.Property(e => e.C6).HasMaxLength(255);

                entity.Property(e => e.C7).HasMaxLength(255);

                entity.Property(e => e.DeviceId).HasColumnType("bigint(20)");

                entity.Property(e => e.Direction).HasMaxLength(255);

                entity.Property(e => e.DownloadDate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.HrappSyncstatus)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("hrapp_syncstatus");

                entity.Property(e => e.WorkCode).HasMaxLength(255);
            });

            modelBuilder.Entity<AttendanceSyncStatus>(entity =>
            {
                entity.HasKey(e => new { e.Id })
                    .HasName("PRIMARY");

                entity.ToTable("attendancesyncstatus");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DeviceLogId).HasColumnType("bigint(20)");
            });
        }
    }
}
