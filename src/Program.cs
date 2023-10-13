using System.Text.Json;
using System.Text.Json.Serialization;

using Discord;
using Discord.WebSocket;

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
    builder.Services.AddHostedService<Bot>();
    builder.Services.AddSingleton(_settings);

    var host = builder.Build();
    host.Run();

    // Console.WriteLine(_settings.CI_CHANNEL_ID);
    // new Program().RunBotAsync().GetAwaiter().GetResult();
  }

  public async Task RunBotAsync()
  {
    _client = new DiscordSocketClient(new DiscordSocketConfig
    {
      GatewayIntents = GatewayIntents.MessageContent
                | GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
    });
    _client.Log += LogAsync;
    _client.MessageReceived += MessageReceivedAsync;

    await RegisterCommandsAsync();
    await _client.LoginAsync(TokenType.Bot, _settings.TOKEN);
    await _client.StartAsync();

    await Task.Delay(-1); // Keep the app running
  }

  private Task LogAsync(LogMessage log)
  {
    Console.WriteLine(log);
    return Task.CompletedTask;
  }

  private async Task RegisterCommandsAsync()
  {
    if (_client == null)
    {
      return;
    }

    _client.MessageReceived += HandleCommandAsync;
    _client.MessageReceived += ProcessCIRequestAsync;
    _client.InteractionCreated += HandleInteractionAsync;
    await Task.CompletedTask;
  }

  private async Task ProcessCIRequestAsync(SocketMessage message)
  {
    // return if client is not defined
    if (_client == null)
    {
      return;
    }

    // return if message is not from github bot
    if (!IsGithubBotMessage(message))
    {
      return;
    }

    // get pull request data from github bot message
    var (url, title) = GetPullRequestDataFromGithubBotMessage(message);

    // return if it's not a pull request url
    if (!IsGithubPullRequestUrl(url))
    {
      return;
    }

    // // extract work item id from pull request title
    // var workItemId = GetWorkItemId(title);

    // extract pull request id from pull request url
    var pullRequestId = GetPullRequestId(url);

    // form the thread name
    var threadName = FormThreadName(pullRequestId, title);

    // get the implementation channel
    if (_client.GetChannel(_settings.IMPLEMENTATION_CHANNEL_ID) is not ITextChannel channel)
    {
      return;
    }

    // check if the thread exists and create if it doesn't
    var thread = await GetThreadByNameAsync(channel, threadName) ?? await channel.CreateThreadAsync(
            name: threadName,
            type: ThreadType.PublicThread
            );

    // send the message to the pull request thread
    await thread.SendMessageAsync(
        text: message.Content,
        embeds: message.Embeds.ToArray()
        );
  }

  private async Task HandleCommandAsync(SocketMessage message)
  {
    var serializedMessage = message.Serialize();
    LogMessage(serializedMessage);

    if (_client == null)
    {
      return;
    }

    if (message.Author.IsBot)
    {
      return;
    }

    if (message.Content.ToLower() == "ping")
    {
      var cb = new ComponentBuilder()
      .WithButton("Click me!", "unique-id", ButtonStyle.Primary);
      Console.WriteLine(message.Channel.Id);
      if (message.Channel is ITextChannel textChannel
      && textChannel.GetType() == typeof(SocketTextChannel)
      && textChannel.Id == requestChannelId)
      {
        var epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var targetChannel = _client.GetChannel(actionChannelId) as ITextChannel;
        if (targetChannel == null)
        {
          return;
        }

        var newThread = await targetChannel.CreateThreadAsync(
                    name: $"[PR] {epochTime}",
                    type: ThreadType.PublicThread
                );

        await newThread.SendMessageAsync("pong", components: cb.Build());
        await newThread.SendMessageAsync($"Also this is the full structure:\n\n```json\n{serializedMessage}\n```");
      }
      else
      {
        await message.Channel.SendMessageAsync($"I cannot ping pong in {message.Channel.Id}");
      }
    }
  }

  private Task MessageReceivedAsync(SocketMessage message)
  {
    Console.WriteLine($"{message.Author.Username}: {message.Content}");
    return Task.CompletedTask;
  }

  private async Task HandleInteractionAsync(SocketInteraction interaction)
  {
    // safety-casting is the best way to prevent something being cast from being null.
    // If this check does not pass, it could not be cast to said type.
    if (interaction is SocketMessageComponent component)
    {
      // Check for the ID created in the button mentioned above.
      if (component.Data.CustomId == "unique-id")
      {
        await interaction.RespondAsync("Thank you for clicking my button!");
      }
      else
      {
        Console.WriteLine("An ID has been received that has no handler!");
      }
    }
  }

  private static void LogMessage(string message)
  {
    Console.WriteLine("------");
    Console.WriteLine(message);
    Console.WriteLine("------");
  }

  // detects if the string url passed in is a github pull request url
  // sample url : https://github.com/username/repository-name/pull/1
  // domain should be github.com
  // path should include /pull/ and be the second last part of the url
  // the last part of the url should be a number
  private static bool IsGithubPullRequestUrl(string url)
  {
    var uri = new Uri(url);
    var path = uri.AbsolutePath.Split('/');
    var domain = uri.Host;
    var isGithub = domain == "github.com";
    var isPullRequest = path.Length >= 3 && path[path.Length - 2] == "pull";
    var isNumber = int.TryParse(path[path.Length - 1], out var _);
    return isGithub && isPullRequest && isNumber;
  }

  // returns id of the pull request
  // from a github pull request url
  private static string GetPullRequestId(string url)
  {
    var uri = new Uri(url);
    var path = uri.AbsolutePath.Split('/');
    return path[path.Length - 1];
  }

  // detects if a socket message is a from a github bot in CI channel
  // by checking if the author is a webhook
  // and if the webhook name is github - case insensitive
  // and if it is a bot
  // and channel is CI channel
  private static bool IsGithubBotMessage(SocketMessage message)
  {
    var isWebhook = message.Author.IsWebhook;
    var isGithub = message.Author.Username.ToLower() == "github";
    var isBot = message.Author.IsBot;
    var isCIChannel = message.Channel.Id == _settings.CI_CHANNEL_ID;
    return isWebhook && isGithub && isBot && isCIChannel;
  }

  // returns the pull request url and title from a github bot message
  // returns empty if the message is not a github bot message
  // source of the data is from the embeds
  private static (string url, string title) GetPullRequestDataFromGithubBotMessage(SocketMessage message)
  {
    if (!IsGithubBotMessage(message))
    {
      return ("", "");
    }

    var embeds = message.Embeds;
    if (embeds.Count == 0)
    {
      return ("", "");
    }

    var embed = embeds.First();
    var title = CleanPullRequestTitle(embed.Title);
    var url = embed.Url;
    return (url, title);
  }

  // clean up the title of the pull request title
  // example: [user/repository-name] Pull request opened: #1 pull request title
  // returns: pull request title
  private static string CleanPullRequestTitle(string title)
  {
    var titleParts = title.Split(':');
    var parts = titleParts[1].Split(' ');
    var cleanedTitle = string.Join(' ', parts.Skip(2));
    return cleanedTitle;
  }

  // form dircord thread name from input parameters
  // template: [PR-PullRequestId] PullRequestTitle
  // example: [PR-1] Add new feature
  private static string FormThreadName(string pullRequestId, string pullRequestTitle)
  {
    return $"[PR-{pullRequestId}] {pullRequestTitle}";
  }

  // return discord thread using the expected thread name
  // return null if the thread does not exist
  private static async Task<IThreadChannel?> GetThreadByNameAsync(ITextChannel channel, string threadName)
  {
    var threads = await channel.GetActiveThreadsAsync();
    var thread = threads.FirstOrDefault(t => t.Name == threadName);
    return thread;
  }
}

static class SocketMessageExtensions
{
  public static string Serialize(this SocketMessage message)
  {
    var serilizerOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      ReferenceHandler = ReferenceHandler.Preserve
    };
    var serilizedMessage = JsonSerializer.Serialize(new
    {
      message.Content,
      channelId = message.Channel.Id,
      channelName = message.Channel.Name,
      author = new
      {
        message.Author.Id,
        message.Author.Username,
        message.Author.Discriminator,
        message.Author.IsBot,
        message.Author.IsWebhook
      },
      message.CleanContent,
      message.CreatedAt,
      message.Flags,
      message.Tags,
      message.Embeds
    }, options: serilizerOptions);

    return serilizedMessage;
  }
}
