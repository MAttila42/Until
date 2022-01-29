using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Until
{
    class Until
    {
        private readonly Config _config;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly EmbedService _embed;

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

            this._services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton(_embed)
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
                var ctx = new SocketInteractionContext<SocketSlashCommand>(_client, interaction);
                if (HasPerm(ctx))
                    await _interaction.ExecuteCommandAsync(ctx, _services);
                else await ctx.Interaction.RespondAsync(embed: _embed.Error("You can't use that command here!"), ephemeral: true);
            };

            _client.Ready += async () =>
            {
                await _interaction.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
                foreach (var g in _client.Guilds)
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

        private bool HasPerm(SocketInteractionContext<SocketSlashCommand> ctx)
        {
            var permissions = ctx.Guild.GetUser(_client.CurrentUser.Id).GetPermissions(ctx.Guild.GetChannel(ctx.Channel.Id));
            return permissions.ViewChannel && permissions.SendMessages;
        }
    }
}
