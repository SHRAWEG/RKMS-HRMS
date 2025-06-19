using Hrms.DesktopWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace Hrms.DesktopWorker.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<AttendanceLogNoDirection> AttendanceLogNoDirections { get; set; }
        public DbSet<DeviceLog> DeviceLogs { get; set; }
        public DbSet<DeviceSetting> DeviceSettings { get; set; }
    }
}