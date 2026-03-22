using LetterboxdToCinephilesChannel;
using LetterboxdToCinephilesChannel.Configuration;
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

// Services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
