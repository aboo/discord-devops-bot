using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Features.PingPonger;
using DiscordDevOpsBot.Features.PullRequestOrganizer;
using DiscordDevOpsBot.Models;

namespace DiscordDevOpsBot.Services;

public sealed class Bot : BackgroundService
{
  private readonly ILogger<Bot> _logger;
  private readonly Settings _settings;
  private DiscordSocketClient? _client;
  private readonly PingPonger _pingPonger;
  private readonly PullRequestOrganizer _pullRequestOrganizer;

  public Bot(ILogger<Bot> logger, Settings settings, PingPonger pingPonger, PullRequestOrganizer pullRequestOrganizer)
  {
    _logger = logger;
    _settings = settings;
    _pingPonger = pingPonger;
    _pullRequestOrganizer = pullRequestOrganizer;
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

    _client.MessageReceived += _pingPonger.ProcessMessageAsync;
    _client.MessageReceived += _pullRequestOrganizer.ProcessMessageAsync;

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
