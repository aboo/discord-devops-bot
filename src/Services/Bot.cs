using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Models;

namespace DiscordDevOpsBot.Services;

public sealed class Bot : BackgroundService
{
  private readonly ILogger<Bot> _logger;
  private readonly Settings _settings;
  private DiscordSocketClient? _client;

  public Bot(ILogger<Bot> logger, Settings settings)
  {
    _logger = logger;
    _settings = settings;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _client = new DiscordSocketClient(new DiscordSocketConfig
    {
      GatewayIntents = GatewayIntents.MessageContent
                | GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
    });
    _client.Log += LogAsync;
    _client.MessageReceived += (message) =>
    {
      _logger.LogInformation($"{message.Author.Username}: {message.Content}");
      return Task.CompletedTask;
    };

    await _client.LoginAsync(TokenType.Bot, _settings.TOKEN);
    await _client.StartAsync();

    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(1_000, stoppingToken);
    }
  }

  private Task LogAsync(LogMessage log)
  {
    _logger.LogInformation(log.Message);
    return Task.CompletedTask;
  }
}
