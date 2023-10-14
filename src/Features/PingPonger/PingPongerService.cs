using Discord.WebSocket;

using DiscordDevOpsBot.Interfaces;

namespace DiscordDevOpsBot.Features.PingPonger;

public class PingPonger : ISockeMesageProcessor
{
  public async Task ProcessMessageAsync(SocketMessage message)
  {
    // return if message is from a bot
    if (message.Author.IsBot)
    {
      return;
    }

    if (message.Content == "!ping")
    {
      await message.Channel.SendMessageAsync($"pong {message.Channel.Id}");
    }
  }
}
