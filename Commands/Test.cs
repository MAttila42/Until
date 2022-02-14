using System.Threading.Tasks;
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
            //if (Context.User.Id != _config.OwnerID)
            //{
            //    await RespondAsync(embed: _embed.Error("You can't use that command!"), ephemeral: true);
            //    return;
            //}

            await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
            await Context.Channel.SendMessageAsync($"ping||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||{Context.User.Mention}");
        }
    }
}
