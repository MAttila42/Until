using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace Until.Services
{
    public class EmojiService
    {
        private List<GuildEmote> emojis;

        public GuildEmote GetEmoji(string name) => this.emojis.Find(e => e.Name == name);

        public EmojiService(List<ulong> emojiServers)
        {
            DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                UseInteractionSnowflakeDate = false
            });
            this.emojis = new List<GuildEmote>();
            foreach (ulong s in emojiServers)
                foreach (GuildEmote e in client.GetGuild(s).Emotes)
                    this.emojis.Add(e);
        }
    }
}
