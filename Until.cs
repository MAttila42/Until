using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Until.Services;

namespace Until
{
    public class Until
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly EmojiService _emoji;
        private readonly GameService _game;

        private readonly IServiceProvider _services;

        public Until(Config config)
        {
            this._config = config;
            this._client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildEmojis | GatewayIntents.GuildMembers,
                UseInteractionSnowflakeDate = false
            });
            this._interaction = new InteractionService(_client.Rest);
            this._emoji = new EmojiService();
            this._game = new GameService();

            this._services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton(_emoji)
                .AddSingleton(_game)
                .BuildServiceProvider();
        }

        public async Task MainAsync()
        {
            _client.Log += Log;
            _interaction.Log += Log;

            await _client.LoginAsync(TokenType.Bot, this._config.Token);
            await _client.StartAsync();

            _client.SlashCommandExecuted += ExecuteInteractionAsync;
            _client.ButtonExecuted += ExecuteInteractionAsync;
            _client.ModalSubmitted += ExecuteInteractionAsync;

            _client.Ready += async () =>
            {
                await _emoji.LoadEmojis(_client, _config.EmojiGuilds);
                await _interaction.AddModulesAsync(typeof(Until).Assembly, _services);
                #if DEBUG
                await _interaction.RegisterCommandsToGuildAsync(_config.DebugGuild, true);
                #else
                await _interaction.RegisterCommandsGloballyAsync(true);
                #endif
            };

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task ExecuteInteractionAsync<T>(T i)
            where T : SocketInteraction
        {
            SocketInteractionContext<T> ctx = new SocketInteractionContext<T>(_client, i);
            await _interaction.ExecuteCommandAsync(ctx, _services);
        }
    }
}
