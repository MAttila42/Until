using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Until.Commands
{
    class Restart : InteractionModuleBase
    {
        [SlashCommand("restart", "Restart bot")]
        public async Task Run()
        {
            try
            {
                await RespondAsync("Restarting bot... (This may take a few moments)");
                string commands =
                    "cd ..\n" +
                    "git pull\n" +
                    "sudo dotnet build -o build\n" +
                    "cd build\n" +
                    "sudo dotnet GroundedBot.dll";
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
            catch (Exception) { await RespondAsync(embed: Until.SimpleEmbed("info", "Can't find bash!")); }
        }
    }
}
