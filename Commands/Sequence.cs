using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;

namespace Until.Commands
{
    public class Sequence : InteractionModuleBase
    {
        public GameService _game { get; set; }

        [SlashCommand("sequence", "Play the game Sequence")]
        public async Task Run()
        {
            _game.Games.Add(new SequenceGame(Context.Channel.Id));
            _game.Games.First().Players.Add(Context.User.Id);
            await RespondAsync(((SequenceGame)_game.Games.Find(x => x.ChannelID == Context.Channel.Id)).CurrentStatus.ToString());
        }
    }
}
