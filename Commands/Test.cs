using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using Discord;
using Discord.Interactions;
using Until.Services;

namespace Until.Commands
{
    public class Test : InteractionModuleBase
    {
        public Config _config { get; set; }
        public EmbedService _embed { get; set; }

        [SlashCommand("test", "[DEV] Test something")]
        [DefaultPermission(false)]
        public async Task Run()
        {
            await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
            await Context.Channel.SendMessageAsync($"ping||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||{Context.User.Mention}");

            string[] urls = new string[2]
            {
                "https://cdn.discordapp.com/attachments/932549944705970186/940878880980488192/noun-info-1228105.png",
                "https://cdn.discordapp.com/attachments/932549944705970186/940878875716632626/noun_Close_1984788.png"
            };

            List<SKBitmap> images = new List<SKBitmap>();
            foreach (string u in urls)
                images.Add(SKBitmap.Decode(new MemoryStream(await new System.Net.Http.HttpClient().GetByteArrayAsync(u))));

            SKSurface tempSurface = SKSurface.Create(new SKImageInfo(images.Max(i => i.Width), images.Sum(i => i.Height)));
            SKCanvas canvas = tempSurface.Canvas;
            canvas.Clear(SKColors.Transparent);

            int offset = 0;
            foreach (SKBitmap i in images)
            {
                canvas.DrawBitmap(i, SKRect.Create(0, offset, i.Width, i.Height));
                offset += i.Height;
            }

            FileAttachment file = new FileAttachment(tempSurface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream(), "asd");
            await Context.Channel.SendFileAsync(file);

            foreach (SKBitmap i in images)
                i.Dispose();
        }
    }
}
