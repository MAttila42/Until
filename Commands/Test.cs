using System.Threading.Tasks;
using Discord.Interactions;

namespace Until.Commands
{
	public class Test : InteractionModuleBase
	{
		[SlashCommand("test", "Test something")]
		public async Task Run(string input)
		{
			await RespondAsync(input);
		}
	}
}
