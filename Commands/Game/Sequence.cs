using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Until.Commands
{
    public class Sequence : InteractionModuleBase
    {
        public DiscordSocketClient _client { get; set; }
        public EmbedService _embed { get; set; }
        public GameService _game { get; set; }

        [SlashCommand("sequence", "Start a new game of Sequence")]
        public async Task Run([Summary(description: "Show how to play Sequence")]bool rules = false)
        {
            if (rules)
            {
                Embed rulesEmbed = new EmbedBuilder()
                    .WithAuthor("Sequence rules", "https://media.discordapp.net/attachments/932549944705970186/934527869785358406/noun-info-2631565.png")
                    .AddField("Basic concept", "Everyone gets 7 card. Each round you can put a chip at one of your card's place and get a new card. The goal is to make 2 rows of 5 chips (2 players) or just 1 row when playing with 3 players.")
                    .WithColor(new Color(0x5864f2))
                    .Build();
                await RespondAsync(embed: rulesEmbed);
                return;
            }

            if (_game.Games.Count(g => g.ChannelID == Context.Channel.Id) != 0)
            {
                await RespondAsync(embed: _embed.Error("A game is already being played here!"), ephemeral: true);
                return;
            }

            _game.Games.Add(new Game(Context.Channel.Id, Context.User.Id));

            string players = "";
            foreach (ulong id in _game.RunningGame(Context).Players)
                players += $"{_client.GetUser(id).Mention}\n";

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor("Sequence")
                .WithDescription("A game of Sequence has started. Join to play the game")
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));
            _game.RunningGame(Context).TempEmbed = embed;

            MessageComponent components = new ComponentBuilder()
                .WithButton("Play", "sequence-play")
                .WithButton("Join", "sequence-join", style: ButtonStyle.Success)
                .WithButton("Leave", "sequence-leave", style: ButtonStyle.Danger)
                .Build();

            await RespondAsync(embed: embed.Build(), components: components);
        }

        private async Task UpdatePlayerList(IInteractionContext ctx)
        {
            string players = "";
            foreach (ulong id in _game.RunningGame(ctx).Players)
                players += $"{_client.GetUser(id).Mention}\n";

            _game.RunningGame(ctx).TempEmbed.Fields[0].Value = players;

            await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)ctx.Interaction).Message.Id, m => m.Embed = _game.RunningGame(ctx).TempEmbed.Build());
            await RespondAsync();
        }

        [ComponentInteraction("sequence-play")]
        public async Task Play()
        {

        }

        [ComponentInteraction("sequence-join")]
        public async Task Join()
        {
            if (_game.RunningGame(Context).Players.Contains(Context.User.Id))
            {
                await RespondAsync(embed: _embed.Error("You are already joined!"), ephemeral: true);
                return;
            }

            _game.RunningGame(Context).Players.Add(Context.User.Id);
            await UpdatePlayerList(Context);
        }

        [ComponentInteraction("sequence-leave")]
        public async Task Leave()
        {
            if (!_game.RunningGame(Context).Players.Contains(Context.User.Id))
            {
                await RespondAsync(embed: _embed.Error("You aren't joined!"), ephemeral: true);
                return;
            }

            _game.RunningGame(Context).Players.Remove(Context.User.Id);
            if (_game.RunningGame(Context).Players.Count > 0)
                await UpdatePlayerList(Context);
            else
            {
                _game.Games.Remove(_game.RunningGame(Context));
                Embed embed = new EmbedBuilder()
                    .WithAuthor("Sequence")
                    .WithDescription("The game has ended.")
                    .WithColor(new Color(0x5864f2))
                    .Build();

                await Context.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Embed = embed; m.Components = null; });
            }
        }
    }
}
