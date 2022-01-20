using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Until.Commands
{
    public class Restart : InteractionModuleBase
    {
        [SlashCommand("restart", "Restart bot")]
        public async Task Run()
        {
            try
            {
                await RespondAsync(embed: Until.SimpleEmbed("info", "Restarting bot... (This may take a few moments)"));
                string commands =
                    "cd ..\n" +
                    "git pull\n" +
                    "sudo dotnet build -o build\n" +
                    "cd build\n" +
                    "sudo dotnet Until.dll";
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
            catch (Exception) { await RespondAsync(embed: Until.SimpleEmbed("error", "Can't find bash!")); }
        }
    }
}
