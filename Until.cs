using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Until.Services;

namespace Until
{
    class Until
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly EmbedService _embed;
        private readonly EmojiService _emoji;
        private readonly GameService _game;

        private readonly IServiceProvider _services;

        public Until(Config config)
        {
            this._config = config;
            this._client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                UseInteractionSnowflakeDate = false
            });
            this._interaction = new InteractionService(_client.Rest);
            this._embed = new EmbedService();
            this._emoji = new EmojiService();
            this._game = new GameService();

            this._services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton(_embed)
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

            _client.SlashCommandExecuted += async (interaction) =>
            {
                SocketInteractionContext<SocketSlashCommand> ctx = new SocketInteractionContext<SocketSlashCommand>(_client, interaction);
                if (!HasPerm(ctx))
                    await ctx.Interaction.RespondAsync(embed: _embed.Error("You can't use that command here!"), ephemeral: true);
                else
                    await _interaction.ExecuteCommandAsync(ctx, _services);
            };

            _client.ButtonExecuted += async (interaction) =>
            {
                SocketInteractionContext<SocketMessageComponent> ctx = new SocketInteractionContext<SocketMessageComponent>(_client, interaction);
                await _interaction.ExecuteCommandAsync(ctx, _services);
            };

            _client.Ready += async () =>
            {
                _emoji.LoadEmojis(_client, _config.EmojiServers);
                await _interaction.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
                foreach (SocketGuild g in _client.Guilds)
                    await _interaction.RegisterCommandsToGuildAsync(g.Id);
            };
            _client.JoinedGuild += async (guild) =>
            {
                await _interaction.RegisterCommandsToGuildAsync(guild.Id);
            };

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public bool HasPerm(SocketInteractionContext<SocketSlashCommand> ctx)
        {
            var permissions = ctx.Guild.GetUser(_client.CurrentUser.Id).GetPermissions(ctx.Guild.GetChannel(ctx.Channel.Id));
            return permissions.ViewChannel && permissions.SendMessages;
        }
    }
}
