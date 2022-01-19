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
            this._client = new DiscordSocketClient();
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
                await _interaction.ExecuteCommandAsync(ctx, _services);
            };

            _client.Ready += async () =>
            {
                await _interaction.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
                await _interaction.RegisterCommandsGloballyAsync();
                await _interaction.RegisterCommandsToGuildAsync(712287958274801695);
            };

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
