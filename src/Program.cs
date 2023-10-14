using System.Text.Json;
using System.Text.Json.Serialization;

using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Features.PingPonger;
using DiscordDevOpsBot.Interfaces;
using DiscordDevOpsBot.Models;
using DiscordDevOpsBot.Services;

namespace DiscordDevOpsBot;

class Program
{
  private const ulong actionChannelId = 1160021059534331996;
  private const ulong requestChannelId = 1160017237680337049;
  private DiscordSocketClient? _client;
  private static IConfigurationRoot? _config;
  private static Settings _settings => _config?.GetRequiredSection("Settings").Get<Settings>() ?? throw new Exception("Settings not found");

  static void Main()
  {
    var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

    _config = configBuilder.Build();

    var builder = Host.CreateApplicationBuilder();
    builder.Services.AddTransient<PingPonger>();
    builder.Services.AddHostedService<Bot>();
    builder.Services.AddSingleton(_settings);

    var host = builder.Build();
    host.Run();

    // Console.WriteLine(_settings.CI_CHANNEL_ID);
    // new Program().RunBotAsync().GetAwaiter().GetResult();
  }

  // public async Task RunBotAsync()
  // {
  //   _client = new DiscordSocketClient(new DiscordSocketConfig
  //   {
  //     GatewayIntents = GatewayIntents.MessageContent
  //               | GatewayIntents.Guilds
  //               | GatewayIntents.GuildMessages
  //   });
  //   _client.Log += LogAsync;
  //   _client.MessageReceived += MessageReceivedAsync;

  //   await RegisterCommandsAsync();
  //   await _client.LoginAsync(TokenType.Bot, _settings.TOKEN);
  //   await _client.StartAsync();

  //   await Task.Delay(-1); // Keep the app running
  // }

  // private Task LogAsync(LogMessage log)
  // {
  //   Console.WriteLine(log);
  //   return Task.CompletedTask;
  // }

  // private async Task RegisterCommandsAsync()
  // {
  //   if (_client == null)
  //   {
  //     return;
  //   }

  //   _client.MessageReceived += HandleCommandAsync;
  //   _client.MessageReceived += ProcessCIRequestAsync;
  //   _client.InteractionCreated += HandleInteractionAsync;
  //   await Task.CompletedTask;
  // }

  // private async Task ProcessCIRequestAsync(SocketMessage message)
  // {
  //   // return if client is not defined
  //   if (_client == null)
  //   {
  //     return;
  //   }

  //   // return if message is not from github bot
  //   if (!IsGithubBotMessage(message))
  //   {
  //     return;
  //   }

  //   // get pull request data from github bot message
  //   var (url, title) = GetPullRequestDataFromGithubBotMessage(message);

  //   // return if it's not a pull request url
  //   if (!IsGithubPullRequestUrl(url))
  //   {
  //     return;
  //   }

  //   // // extract work item id from pull request title
  //   // var workItemId = GetWorkItemId(title);

  //   // extract pull request id from pull request url
  //   var pullRequestId = GetPullRequestId(url);

  //   // form the thread name
  //   var threadName = FormThreadName(pullRequestId, title);

  //   // get the implementation channel
  //   if (_client.GetChannel(_settings.IMPLEMENTATION_CHANNEL_ID) is not ITextChannel channel)
  //   {
  //     return;
  //   }

  //   // check if the thread exists and create if it doesn't
  //   var thread = await GetThreadByNameAsync(channel, threadName) ?? await channel.CreateThreadAsync(
  //           name: threadName,
  //           type: ThreadType.PublicThread
  //           );

  //   // send the message to the pull request thread
  //   await thread.SendMessageAsync(
  //       text: message.Content,
  //       embeds: message.Embeds.ToArray()
  //       );
  // }

  // private async Task HandleCommandAsync(SocketMessage message)
  // {
  //   var serializedMessage = message.Serialize();
  //   LogMessage(serializedMessage);

  //   if (_client == null)
  //   {
  //     return;
  //   }

  //   if (message.Author.IsBot)
  //   {
  //     return;
  //   }

  //   if (message.Content.ToLower() == "ping")
  //   {
  //     var cb = new ComponentBuilder()
  //     .WithButton("Click me!", "unique-id", ButtonStyle.Primary);
  //     Console.WriteLine(message.Channel.Id);
  //     if (message.Channel is ITextChannel textChannel
  //     && textChannel.GetType() == typeof(SocketTextChannel)
  //     && textChannel.Id == requestChannelId)
  //     {
  //       var epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
  //       var targetChannel = _client.GetChannel(actionChannelId) as ITextChannel;
  //       if (targetChannel == null)
  //       {
  //         return;
  //       }

  //       var newThread = await targetChannel.CreateThreadAsync(
  //                   name: $"[PR] {epochTime}",
  //                   type: ThreadType.PublicThread
  //               );

  //       await newThread.SendMessageAsync("pong", components: cb.Build());
  //       await newThread.SendMessageAsync($"Also this is the full structure:\n\n```json\n{serializedMessage}\n```");
  //     }
  //     else
  //     {
  //       await message.Channel.SendMessageAsync($"I cannot ping pong in {message.Channel.Id}");
  //     }
  //   }
  // }

  // private Task MessageReceivedAsync(SocketMessage message)
  // {
  //   Console.WriteLine($"{message.Author.Username}: {message.Content}");
  //   return Task.CompletedTask;
  // }

  // private async Task HandleInteractionAsync(SocketInteraction interaction)
  // {
  //   // safety-casting is the best way to prevent something being cast from being null.
  //   // If this check does not pass, it could not be cast to said type.
  //   if (interaction is SocketMessageComponent component)
  //   {
  //     // Check for the ID created in the button mentioned above.
  //     if (component.Data.CustomId == "unique-id")
  //     {
  //       await interaction.RespondAsync("Thank you for clicking my button!");
  //     }
  //     else
  //     {
  //       Console.WriteLine("An ID has been received that has no handler!");
  //     }
  //   }
  // }

  // private static void LogMessage(string message)
  // {
  //   Console.WriteLine("------");
  //   Console.WriteLine(message);
  //   Console.WriteLine("------");
  // }
}
