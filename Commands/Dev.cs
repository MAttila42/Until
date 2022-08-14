using Discord;
using System.Diagnostics;
using Discord.Interactions;
using Until.Services;

namespace Until.Commands
{
    public class Dev : InteractionModuleBase
    {
        public enum Command
        {
            Test,
            ShutDown,
            Restart
        }

        [SlashCommand("dev", "[DEV] Developer commands")]
        [RequireOwner]
        public async Task Run(Command command)
        {
            try
            {
                switch (command)
                {
                    case Command.Test:
                        await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
                        break;
                    case Command.ShutDown:
                        await RespondAsync(embed: EmbedService.Info("Shutting down..."));
                        Environment.Exit(0);
                        break;
                    case Command.Restart:
                        var process = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"" +
                                "cd ..\n" +
                                "sudo git pull\n" +
                                "sudo dotnet build -c Release -o build\n" +
                                "cd build\n" +
                                "sudo dotnet Until.dll" +
                                "\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(process);
                        await RespondAsync(embed: EmbedService.Info("Restarting...", "This may take a while."));
                        Environment.Exit(0);
                        break;
                    default:
                        await RespondAsync(embed: EmbedService.Error("Couldn't run command", $"There's no {command} command."), ephemeral: true);
                        break;
                }
            }
            catch (Exception e) { await RespondAsync(embed: EmbedService.Error("Error", $"Couldn't find bash at `/bin/bash`.\n```{e.Message}```"), ephemeral: true); }
        }
    }
}
