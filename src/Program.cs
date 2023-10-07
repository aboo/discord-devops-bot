using Discord;
using Discord.WebSocket;

namespace DiscordPingPongBot
{
    class Program
    {
        private const ulong actionChannelId = 1160021059534331996;
        private const ulong requestChannelId = 1160017237680337049;
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
            _client.InteractionCreated+= HandleInteractionAsync;
            await Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if(_client == null) return;
            if (message.Author.IsBot) return;

            if (message.Content.ToLower() == "ping")
            {
                var cb = new ComponentBuilder()
                    .WithButton("Click me!", "unique-id", ButtonStyle.Primary);
                Console.WriteLine(message.Channel.Id);
                if(message.Channel is ITextChannel textChannel 
                    && textChannel.GetType() == typeof(SocketTextChannel)
                    && textChannel.Id == requestChannelId){
                    var epochTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    var targetChannel = _client.GetChannel(actionChannelId) as ITextChannel;
                    if(targetChannel == null) return;

                    var newThread = await targetChannel.CreateThreadAsync(
                        name: $"[PR] {epochTime}",
                        type: ThreadType.PublicThread
                    );

                    await newThread.SendMessageAsync("pong", components: cb.Build());
                }else{
                    await message.Channel.SendMessageAsync("I cannot ping pong here");
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
                    await interaction.RespondAsync("Thank you for clicking my button!");

                else
                    Console.WriteLine("An ID has been received that has no handler!");
            }
        }
    }
}
