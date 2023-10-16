using Discord.WebSocket;

namespace DiscordDevOpsBot.Interfaces;

public interface ISockeMesageProcessor
{
  Task ProcessMessageAsync(SocketMessage message);
}
