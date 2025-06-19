using Hrms.Common.Data.Seeds;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//builder.Services.AddControllersWithViews()
//    .AddJsonOptions(options =>
//        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
//    );

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddFluentValidation(opts =>
    {
        opts.ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;
        opts.RegisterValidatorsFromAssemblyContaining<Program>();
        opts.ImplicitlyValidateChildProperties = true;
        opts.LocalizationEnabled = false;
        opts.DisableDataAnnotationsValidation = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(context.ModelState);
        };
    });

builder.Services.Configure<JsonOptions>(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAttendnaceService, AttendanceService>();

builder.Services.AddIdentityServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds((type) => type.FullName);
    c.DocumentFilter<SwaggerDocumentFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddHttpClient();
builder.Services.Configure<FormOptions>(builder =>
{
    builder.MultipartBodyLengthLimit = 1000000000;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024;
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));

builder.Services.AddDbContext<DataContext>((sp, options) =>
{
    DatabaseSettings settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    options.UseNpgsql(settings.ConnectionString);
});

builder.Services.AddScoped<DbHelper>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    await context.Database.GetPendingMigrationsAsync();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(userManager, roleManager);
    await Seed.SeedSalaryHeadMasters(context);
    await Seed.SeedHiringStages(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Logger.LogInformation("Application starting up...");

try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred during application startup.");
    throw;
}


//app.Run();
