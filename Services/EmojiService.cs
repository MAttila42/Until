using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using Discord;
using Discord.WebSocket;

namespace Until.Services
{
    public class EmojiService
    {
        private List<GuildEmote> emojis;
        private Dictionary<string, SKBitmap> images;

        public GuildEmote GetEmoji(string name) => this.emojis.First(e => e.Name == name);
        public SKBitmap GetImage(in string name) => this.images[name];

        private void Log(in string msg) => Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {"Service", -12}{msg}");

        public async Task LoadEmojis(DiscordSocketClient client, List<ulong> emojiServers)
        {
            Log("Loading emojis");

            this.emojis = new List<GuildEmote>();
            foreach (ulong s in emojiServers)
                foreach (GuildEmote e in client.GetGuild(s).Emotes)
                    this.emojis.Add(e);

            this.images = new Dictionary<string, SKBitmap>();
            foreach (GuildEmote e in this.emojis)
                this.images.Add(e.Name, SKBitmap.Decode(new MemoryStream(await new System.Net.Http.HttpClient().GetByteArrayAsync(e.Url))).Resize(new SKImageInfo(64, 64), SKFilterQuality.Low));

            Log("Emojis loaded");
        }

        //public EmojiService() { }
    }
}
