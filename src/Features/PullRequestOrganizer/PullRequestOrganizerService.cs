using Discord;
using Discord.WebSocket;

using DiscordDevOpsBot.Interfaces;
using DiscordDevOpsBot.Models;

namespace DiscordDevOpsBot.Features.PullRequestOrganizer;

public class PullRequestOrganizerService : ISockeMesageProcessor
{
  private readonly DiscordSocketClient _client;
  private readonly Settings _settings;

  public PullRequestOrganizerService(DiscordSocketClient client, Settings settings)
  {
    _client = client;
    _settings = settings;
  }

  public async Task ProcessMessageAsync(SocketMessage message)
  {
    // return if message is not from github bot
    if (!Helper.IsGithubBotMessage(message, _settings.CI_CHANNEL_ID))
    {
      return;
    }

    // get pull request data from github bot message
    var (url, title) = Helper.GetPullRequestDataFromGithubBotMessage(message, _settings);

    // return if it's not a pull request url
    if (!Helper.IsGithubPullRequestUrl(url))
    {
      return;
    }

    // // extract work item id from pull request title
    // var workItemId = GetWorkItemId(title);

    // extract pull request id from pull request url
    var pullRequestId = Helper.GetPullRequestId(url);

    // form the thread name
    var threadName = Helper.FormThreadName(pullRequestId, title);

    // get the implementation channel
    if (_client.GetChannel(_settings.IMPLEMENTATION_CHANNEL_ID) is not ITextChannel channel)
    {
      return;
    }

    // check if the thread exists and create if it doesn't
    var thread = await Helper.GetThreadByNameAsync(channel, threadName) ?? await channel.CreateThreadAsync(
            name: threadName,
            type: ThreadType.PublicThread
            );

    // send the message to the pull request thread
    await thread.SendMessageAsync(
        text: message.Content,
        embeds: message.Embeds.ToArray()
        );
  }
}
