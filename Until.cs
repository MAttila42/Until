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
        private readonly IServiceProvider _services;

        public Until(Config config)
        {
            this._config = config;
            this._client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });
            this._interaction = new InteractionService(_client.Rest);

            this._services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddSingleton(_config)
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
                else await ctx.Interaction.RespondAsync(embed: SimpleEmbed("error", "You can't use that command here!"), ephemeral: true);
            };

            _client.Ready += async () =>
            {
                await _interaction.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
                //await _interaction.RegisterCommandsGloballyAsync();
                await _interaction.RegisterCommandsToGuildAsync(_config.DevServerID);
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

        public static Embed SimpleEmbed(string type, string msg)
        {
            var embed = new EmbedBuilder();
            switch (type)
            {
                case "info":
                    embed.WithAuthor(msg, "https://media.discordapp.net/attachments/932549944705970186/934527869785358406/noun-info-2631565.png");
                    embed.WithColor(new Color(0x5864f2));
                    break;
                case "error":
                    embed.WithAuthor(msg, "https://media.discordapp.net/attachments/932549944705970186/932551072621404200/noun_Close_1984788.png");
                    embed.WithColor(new Color(0xff1821));
                    break;
                default:
                    break;
            }
            return embed.Build();
        }
    }
}
