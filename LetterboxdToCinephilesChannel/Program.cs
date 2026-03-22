using LetterboxdToCinephilesChannel;
using LetterboxdToCinephilesChannel.Configuration;
using LetterboxdToCinephilesChannel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.AddOptions<TelegramOptions>()
    .Bind(builder.Configuration.GetSection("Telegram"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<OmdbOptions>()
    .Bind(builder.Configuration.GetSection("Omdb"))
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

// Services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Apply Migrations and Ensure WAL Mode
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
}

host.Run();
