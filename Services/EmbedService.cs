using Discord;

namespace Until
{
    public class EmbedService
    {
        public string CloseIcon = "https://media.discordapp.net/attachments/932549944705970186/932551072621404200/noun_Close_1984788.png"; // Close by Bismillah from the Noun Project
        public string InfoIcon = "https://media.discordapp.net/attachments/932549944705970186/934527869785358406/noun-info-2631565.png";

        public Embed Error(string msg) => new EmbedBuilder()
            .WithAuthor(msg, CloseIcon)
            .WithColor(new Color(0xff1821))
            .Build();

        public Embed Info(string msg) => new EmbedBuilder()
            .WithAuthor(msg, InfoIcon)
            .WithColor(new Color(0x5864f2))
            .Build();
    }
}
