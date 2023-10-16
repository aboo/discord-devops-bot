using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Features.PingPonger;
using DiscordDevOpsBot.Features.PullRequestOrganizer;
using DiscordDevOpsBot.Interfaces;
using DiscordDevOpsBot.Models;

namespace DiscordDevOpsBot.Services;

public sealed class Bot : BackgroundService
{
  private readonly ILogger<Bot> _logger;
  private readonly Settings _settings;
  private readonly DiscordSocketClient _client;
  private readonly List<ISockeMesageProcessor> _processors;

  public Bot(DiscordSocketClient client, ILogger<Bot> logger, Settings settings, PingPonger pingPonger, PullRequestOrganizer pullRequestOrganizer)
  {
    _processors = new List<ISockeMesageProcessor>();

    _logger = logger;
    _settings = settings;
    _client = client;

    _processors.Add(pingPonger);
    _processors.Add(pullRequestOrganizer);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _client.Log += LogAsync;
    _client.MessageReceived += (message) =>
    {
      _logger.LogInformation("{Username}: {Content}", message.Author.Username, message.Content);
      return Task.CompletedTask;
    };

    foreach (var processor in _processors)
    {
      _client.MessageReceived += processor.ProcessMessageAsync;
    }

    await _client.LoginAsync(TokenType.Bot, _settings.TOKEN);
    await _client.StartAsync();

    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(1_000, stoppingToken);
    }
  }

  public override void Dispose()
  {
    if (_client != null)
    {
      foreach (var processor in _processors)
      {
        _client.MessageReceived -= processor.ProcessMessageAsync;
      }
      _client.Dispose();
    }

    base.Dispose();
  }

  private Task LogAsync(LogMessage log)
  {
    _logger.LogInformation("", log.Message);
    return Task.CompletedTask;
  }
}
