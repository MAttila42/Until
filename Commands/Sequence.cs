using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Until.Commands
{
    public class Sequence : InteractionModuleBase
    {
        public EmbedService _embed { get; set; }
        public GameService _game { get; set; }

        [SlashCommand("sequence", "Start a new game of Sequence")]
        public async Task Run()
        {
            if (_game.AlreadyPlaying(Context))
                await RespondAsync(embed: _embed.Error("You are already playing a game here!"), ephemeral: true);

            _game.Games.Add(new SequenceGame(Context.Channel.Id, Context.User.Id));

            // TODO: Nice looking embed message

            MessageComponent buttons = new ComponentBuilder()
                .WithButton("Join", "sequence-join", style: ButtonStyle.Success)
                .WithButton("Leave", "sequence-leave", style: ButtonStyle.Danger)
                .Build();

            await RespondAsync($"{((SequenceGame)_game.RunningGame(Context)).CurrentStatus}", components: buttons);
        }

        [ComponentInteraction("sequence-join")]
        public async Task Join()
        {
            if (_game.AlreadyPlaying(Context))
                await RespondAsync(embed: _embed.Error("You are already playing a game here!"), ephemeral: true);

            _game.RunningGame(Context).Players.Add(Context.User.Id);
            // TODO: Update the message
        }

        [ComponentInteraction("sequence-leave")]
        public async Task Leave()
        {
            // TODO: Leave the game and update the message
            return;
        }
    }
}
