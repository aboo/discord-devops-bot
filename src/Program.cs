using Discord;
using Discord.WebSocket;

namespace DiscordPingPongBot
{
    class Program
    {
        private const string TOKEN = "MTE1OTY5OTQyMTYyNjM4NDQzNg.Gf5mbd.83m7CPrE20JamjyYm2Vkxp7VSynYSaFyOlA-VE";
        private DiscordSocketClient? _client;

        static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
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
            await _client.LoginAsync(TokenType.Bot, TOKEN);
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
            if(_client == null) return;
            _client.MessageReceived += HandleCommandAsync;
            await Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content.ToLower() == "ping")
            {
                await message.Channel.SendMessageAsync("pong :)");
            }
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            Console.WriteLine($"{message.Author.Username}: {message.Content}");
            return Task.CompletedTask;
        }
    }
}
