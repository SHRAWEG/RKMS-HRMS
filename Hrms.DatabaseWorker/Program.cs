using Hrms.Common.Data;
using Hrms.DatabaseWorker;
using Hrms.DatabaseWorker.AttendanceData;
using Hrms.DatabaseWorker.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
IConfiguration configuration = builder.Configuration;

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Attendance Service";
});

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddHostedService<SyncAttendance>();

// See: https://github.com/dotnet/runtime/issues/47303
builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

IHost host = builder.Build();
host.Run();