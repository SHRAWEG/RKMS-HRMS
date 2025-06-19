using Hrms.Worker.Jobs;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

IHost host = Host.CreateDefaultBuilder(args)
    //.ConfigureAppConfiguration((context, config) =>
    //{
    //    config.SetBasePath(Directory.GetCurrentDirectory())
     //       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    //})
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        //Console.WriteLine(configuration.GetConnectionString("PgConnection"));

        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DevString")));


        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            //var syncWorkHourId = new JobKey("Sync WorkHour.");
            var storeAttendanceId = new JobKey("Store Attendance");
            //var storeAttendanceMsAccessId = new JobKey("Store Attendance MsAccess");
            //var storePreviousAtendaceMsAccessId = new JobKey("Store Previous Attendance MsAccess");
            var syncAttendanceId = new JobKey("Sync Attendance");
            var requestAutomationId = new JobKey("Request Automation");

            //q.AddJob<SyncAttendanceWorkHour>(opts => opts.WithIdentity(syncWorkHourId));
            q.AddJob<StoreAttendance>(opts => opts.WithIdentity(storeAttendanceId));
            //q.AddJob<StoreAttendanceMsAccess>(opts => opts.WithIdentity(storeAttendanceMsAccessId));
            //q.AddJob<StorePreviousAttendanceMsAccess>(opts => opts.WithIdentity(storePreviousAtendaceMsAccessId));
            q.AddJob<SyncAttendanceNoDirection>(opts => opts.WithIdentity(syncAttendanceId));
            q.AddJob<RequestAutomation>(opts => opts.WithIdentity(requestAutomationId));

            //q.AddTrigger(opts => opts
            //    .ForJob(syncWorkHourId)
            //    .WithIdentity(syncWorkHourId.Name + " trigger")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithInterval(TimeSpan.FromHours(1))
            //        .RepeatForever())
            //);

            //q.AddTrigger(opts => opts
            //    .ForJob(storeAttendanceMsAccessId)
            //    .WithIdentity(storeAttendanceMsAccessId.Name + " trigger")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithInterval(TimeSpan.FromHours(1))
            //        .RepeatForever())
            //);

            //q.AddTrigger(opts => opts
            //    .ForJob(storePreviousAtendaceMsAccessId)
            //    .WithIdentity(storePreviousAtendaceMsAccessId.Name + " trigger")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithInterval(TimeSpan.FromDays(1))
            //        .RepeatForever())
            //);

            q.AddTrigger(opts => opts
                .ForJob(storeAttendanceId)
                .WithIdentity(storeAttendanceId.Name + " trigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromHours(1))
                    .RepeatForever())
            );

            q.AddTrigger(opts => opts
                .ForJob(syncAttendanceId)
                .WithIdentity(syncAttendanceId.Name + " trigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromHours(1))
                    .RepeatForever())
            );

            q.AddTrigger(opts => opts
                .ForJob(requestAutomationId)
                .WithIdentity(requestAutomationId.Name + " trigger")
                .StartNow()
                .WithCronSchedule("0 0 12 1 * ? *") // Run on 1st of every month (seconds minute hour date month dayOfWeek year)
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    })
    .UseSystemd()
    .Build();

host.Run();
