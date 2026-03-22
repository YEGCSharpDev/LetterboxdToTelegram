using LetterboxdToCinephilesChannel;
using LetterboxdToCinephilesChannel.Configuration;
using LetterboxdToCinephilesChannel.Infrastructure.Data;
using LetterboxdToCinephilesChannel.Infrastructure.Http;
using LetterboxdToCinephilesChannel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting host");
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog();

    // Configuration
    builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection("Telegram"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TmdbOptions>()
    .Bind(builder.Configuration.GetSection("Tmdb"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<RssOptions>()
    .Bind(builder.Configuration.GetSection("Rss"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Persistence
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=movies.db";
    options.UseSqlite(connectionString);
});

// Clients
builder.Services.AddHttpClient<TmdbClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TmdbOptions>>().Value;
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ReadAccessToken);
})
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient<RssClient>()
    .AddStandardResilienceHandler();

// Telegram Bot Client
builder.Services.AddHttpClient("TelegramBotClient")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var telegramOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TelegramOptions>>().Value;
        return new TelegramBotClient(telegramOptions.BotToken, httpClient);
    });

// Services
builder.Services.AddTransient<HistorySeedingService>();
builder.Services.AddTransient<ErrorReportingService>();
builder.Services.AddTransient<TelegramService>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddHealthChecks();
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(30);
    options.Period = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<IHealthCheckPublisher, HealthCheckFilePublisher>();

var host = builder.Build();

// Apply Migrations, Ensure WAL Mode, and Seed History
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

    if (args.Contains("--db-check-wal"))
    {
        var walCheck = db.Database.SqlQueryRaw<string>("PRAGMA journal_mode;").ToList().FirstOrDefault();
        Console.WriteLine($"SQLite Journal Mode: {walCheck}");
        if (walCheck?.ToLower() == "wal")
        {
            Console.WriteLine("WAL mode check PASSED.");
            return;
        }
        else
        {
            Console.WriteLine("WAL mode check FAILED.");
            Environment.Exit(1);
        }
    }

    var seeder = scope.ServiceProvider.GetRequiredService<HistorySeedingService>();
    await seeder.SeedAsync();
}

await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
