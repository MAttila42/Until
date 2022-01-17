using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Until.Commands
{
	public class Test : InteractionModuleBase
	{
		public Config _config { get; set; }

		[SlashCommand("test", "Test something")]
		public async Task Run()
		{
			if (Context.User.Id != _config.OwnerID)
			{
				var embed = new EmbedBuilder()
					.WithAuthor(author =>
					{
						author
							.WithName("You can't use that command!")
							.WithIconUrl("https://media.discordapp.net/attachments/932549944705970186/932551072621404200/noun_Close_1984788.png"); // Close by Bismillah from the Noun Project
					})
					.WithColor(new Color(0xff1821)).Build();
				await RespondAsync(embed: embed, ephemeral: true);
				return;
			}
			await RespondAsync(Context.User.Mention, allowedMentions: AllowedMentions.None);
			await Context.Channel.SendMessageAsync($"ping||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||{Context.User.Mention}");
		}
	}
}
