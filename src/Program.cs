﻿using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Features.PingPonger;
using DiscordDevOpsBot.Features.PullRequestOrganizer;
using DiscordDevOpsBot.Models;
using DiscordDevOpsBot.Services;

namespace DiscordDevOpsBot;

sealed class Program
{
  const string ERROR_SETTINGS_NOT_DEFINED = "Settings not found";
  const string APP_SETTINGS_FILE = "appsettings.json";
  const string CONFIG_SETTINGS_SECTION_KEY = "Settings";

  static void Main()
  {
    var configBuilder = new ConfigurationBuilder()
    .AddJsonFile(APP_SETTINGS_FILE, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

    var config = configBuilder.Build();
    var settings = config.GetRequiredSection(CONFIG_SETTINGS_SECTION_KEY).Get<Settings>() ?? throw new Exception(ERROR_SETTINGS_NOT_DEFINED);

    var client = new DiscordSocketClient(new DiscordSocketConfig
    {
      GatewayIntents = GatewayIntents.MessageContent
                    | GatewayIntents.Guilds
                    | GatewayIntents.GuildMessages
    });

    var builder = Host.CreateApplicationBuilder();
    builder.Services.AddSingleton(client);
    builder.Services.AddSingleton(settings);
    builder.Services.AddTransient<PingPonger>();
    builder.Services.AddTransient<PullRequestOrganizer>();
    builder.Services.AddHostedService<Bot>();

    var host = builder.Build();
    host.Run();
  }
}
