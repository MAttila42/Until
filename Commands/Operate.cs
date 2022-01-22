using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Until.Commands
{
    public class Operate : InteractionModuleBase
    {
        public Config _config { get; set; }

        public enum Operation
        {
            ShutDown,
            Restart
        }

        [SlashCommand("operate", "[DEV] Operate bot")]
        public async Task Run(Operation operation)
        {
            if (Context.User.Id != _config.OwnerID)
            {
                await RespondAsync(embed: Until.SimpleEmbed("error", "You can't use that command!"), ephemeral: true);
                return;
            }

            try
            {
                string commands = "";
                switch (operation)
                {
                    case Operation.ShutDown:
                        await RespondAsync(embed: Until.SimpleEmbed("info", "Shutting down..."));
                        Environment.Exit(0);
                        break;
                    case Operation.Restart:
                        await RespondAsync(embed: Until.SimpleEmbed("info", "Restarting bot... (This may take a few moments)"));
                        commands =
                            "cd ..\n" +
                            "git pull\n" +
                            "sudo dotnet build -o build\n" +
                            "cd build\n" +
                            "sudo dotnet Until.dll";
                        break;
                    default:
                        await RespondAsync(embed: Until.SimpleEmbed("error", "Can't do operation!"));
                        return;
                }

                var process = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{commands}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process.Start(process);
                Environment.Exit(0);
            }
            catch (Exception) { await RespondAsync(embed: Until.SimpleEmbed("error", "Can't find bash!"), ephemeral: true); }
        }
    }
}
