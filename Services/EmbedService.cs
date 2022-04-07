using Discord;

namespace Until.Services
{
    public class EmbedService
    {
        public readonly string CloseIcon = "https://media.discordapp.net/attachments/932549944705970186/940878875716632626/noun_Close_1984788.png"; // Close by Bismillah from NounProject.com
        public readonly string InfoIcon = "https://media.discordapp.net/attachments/932549944705970186/940878880980488192/noun-info-1228105.png"; // Info by David Khai from NounProject.com

        public Embed Error(in string msg) => new EmbedBuilder()
            .WithAuthor(msg, CloseIcon)
            .WithColor(new Color(0xff1821))
            .Build();

        public Embed Info(in string msg) => new EmbedBuilder()
            .WithAuthor(msg, InfoIcon)
            .WithColor(new Color(0x5864f2))
            .Build();
    }
}
