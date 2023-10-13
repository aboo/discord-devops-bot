namespace DiscordDevOpsBot.Models;

public class Settings
{
  public string? TOKEN { get; set; }
  public ulong CI_CHANNEL_ID { get; set; }
  public ulong IMPLEMENTATION_CHANNEL_ID { get; set; }
}
