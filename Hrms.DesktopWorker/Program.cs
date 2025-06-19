using Hrms.DesktopWorker;
using Hrms.DesktopWorker.Settings;
using Hrms.DesktopWorker.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Newtonsoft.Json;
using Hrms.DesktopWorker.Data;

//const string ServiceName = "Attendance Service";

//if (args is { Length: 1 })
//{
//    string executablePath =
//        Path.Combine(AppContext.BaseDirectory, "Hrms.DesktopWorker.exe");

//    if (args[0] is "/Install")
//    {
//        await Cli.Wrap("sc")
//            .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
//            .ExecuteAsync();
//    }
//    else if (args[0] is "/Uninstall")
//    {
//        await Cli.Wrap("sc")
//            .WithArguments(new[] { "stop", ServiceName })
//            .ExecuteAsync();

//        await Cli.Wrap("sc")
//            .WithArguments(new[] { "delete", ServiceName })
//            .ExecuteAsync();
//    }

//    return;
//}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Attendance Service";
});

builder.Services.AddDbContext<DataContext>(options =>
{
    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Attendance");

    using (StreamReader sr = new StreamReader(Path.Combine(appDataFolder, "database.config.json")))
    {
        string json = sr.ReadToEnd();
        DatabaseSetting config = JsonConvert.DeserializeObject<DatabaseSetting>(json);

        var connectionString = AesOperation.DecryptString(config.ConnectionString);

        options.UseNpgsql(connectionString);
    };
});

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services); 

builder.Services.AddHostedService<Worker>();

// See: https://github.com/dotnet/runtime/issues/47303
builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

IHost host = builder.Build();
host.Run(); 