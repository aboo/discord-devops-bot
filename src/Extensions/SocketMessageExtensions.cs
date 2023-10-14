
using System.Text.Json;
using System.Text.Json.Serialization;

using Discord.WebSocket;

namespace DiscordDevOpsBot.Extensions;

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
