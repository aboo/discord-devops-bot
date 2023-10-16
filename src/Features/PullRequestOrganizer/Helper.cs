using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Models;

namespace DiscordDevOpsBot.Features.PullRequestOrganizer;

public static class Helper
{
  // detects if the string url passed in is a github pull request url
  // sample url : https://github.com/username/repository-name/pull/1
  // domain should be github.com
  // path should include /pull/ and be the second last part of the url
  // the last part of the url should be a number
  public static bool IsGithubPullRequestUrl(string url)
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
  public static string GetPullRequestId(string url)
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
  public static bool IsGithubBotMessage(SocketMessage message, ulong expectedCIChannelId)
  {
    var isWebhook = message.Author.IsWebhook;
    var isGithub = message.Author.Username.ToLower() == "github";
    var isBot = message.Author.IsBot;
    var isCIChannel = message.Channel.Id == expectedCIChannelId;
    return isWebhook && isGithub && isBot && isCIChannel;
  }

  // returns the pull request url and title from a github bot message
  // returns empty if the message is not a github bot message
  // source of the data is from the embeds
  public static (string url, string title) GetPullRequestDataFromGithubBotMessage(SocketMessage message, Settings settings)
  {
    if (!IsGithubBotMessage(message, settings.CI_CHANNEL_ID))
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
  public static string FormThreadName(string pullRequestId, string pullRequestTitle)
  {
    return $"[PR-{pullRequestId}] {pullRequestTitle}";
  }

  // return discord thread using the expected thread name
  // return null if the thread does not exist
  public static async Task<IThreadChannel?> GetThreadByNameAsync(ITextChannel channel, string threadName)
  {
    var threads = await channel.GetActiveThreadsAsync();
    var thread = threads.FirstOrDefault(t => t.Name == threadName);
    return thread;
  }
}
