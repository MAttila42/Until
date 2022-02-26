using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Until.Services;
using Until.Games;

namespace Until.Commands
{
    public class Sequence : InteractionModuleBase
    {
        public Config _config { get; set; }
        public DiscordSocketClient _client { get; set; }
        public EmbedService _embed { get; set; }
        public EmojiService _emoji { get; set; }
        public GameService _game { get; set; }

        [SlashCommand("sequence", "Start a new game of Sequence")]
        public async Task Run([Summary(description: "Show how to play Sequence")]bool rules = false)
        {
            if (rules)
            {
                Embed rulesEmbed = new EmbedBuilder()
                    .WithAuthor("Sequence rules", _embed.InfoIcon)
                    .AddField("Basic concept", "Everyone gets 7 cards. Each round you can put a chip at one of your card's place on the table and get a new card. The goal is to make 2 rows of 5 chips (2 players) or just 1 row when playing with 3 players.")
                    .WithColor(new Color(0x5864f2))
                    .Build();
                await RespondAsync(embed: rulesEmbed);
                return;
            }

            if (_game.Games.Any(g => g.ChannelID == Context.Channel.Id && g.Players.Any(p => p.ID == Context.User.Id)))
            {
                ComponentBuilder components = new ComponentBuilder()
                    .WithButton("Leave Game", "sequence-leavegame", ButtonStyle.Danger);
                await RespondAsync(embed: _embed.Error("You are already playing a game here!"), components: components.Build());
                return;
            }

            _game.Games.Add(new SequenceGame(Context.Channel.Id, Context.User.Id, _emoji));
            await UpdateResponse(Context, true);
        }

        private async Task UpdateResponse(IInteractionContext ctx) => await UpdateResponse(ctx, false); 
        private async Task UpdateResponse(IInteractionContext ctx, bool respond)
        {
            SequenceGame game = _game.RunningGame(ctx) as SequenceGame;
            if (game.Players.Count == 1 && !respond)
                game = _game.WaitingGame(ctx) as SequenceGame;

            Dictionary<SequenceGame.Color, string> colors = new Dictionary<SequenceGame.Color, string>();
            colors.Add(SequenceGame.Color.None, "black");
            colors.Add(SequenceGame.Color.Red, "red");
            colors.Add(SequenceGame.Color.Green, "green");
            colors.Add(SequenceGame.Color.Blue, "blue");
            colors.Add(SequenceGame.Color.Joker, "black");

            StringBuilder players = new StringBuilder();
            foreach (ulong id in game.Players.Select(p => p.ID))
                players.Append($":black_circle: {_client.GetUser(id).Mention}\n");

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor("Sequence")
                .WithDescription("A game of Sequence has started. Join to play the game")
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));

            ComponentBuilder components = new ComponentBuilder()
                .WithButton("Play", "sequence-start", disabled: game.Players.Count == 1)
                .WithButton("Join", "sequence-join", ButtonStyle.Success, disabled: game.Players.Count == 3)
                .WithButton("Leave", "sequence-leave", ButtonStyle.Danger);

            if (respond)
                await ctx.Interaction.RespondAsync(embed: embed.Build(), components: components.Build());
            else
                await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)ctx.Interaction).Message.Id, m =>
                {
                    m.Embed = embed.Build();
                    m.Components = components.Build();
                });
        }

        [ComponentInteraction("sequence-join")]
        public async Task Join()
        {
            if (_game.WaitingGame(Context).Players.Any(p => p.ID == Context.User.Id))
                await RespondAsync(embed: _embed.Error("You are already joined!"), ephemeral: true);
            else
            {
                _game.WaitingGame(Context).Players.Add(new SequencePlayer(Context.User.Id));
                await UpdateResponse(Context);
                await DeferAsync();
            }
        }

        [ComponentInteraction("sequence-leave")]
        public async Task Leave()
        {
            if (!_game.WaitingGame(Context).Players.Select(p => p.ID).ToList().Contains(Context.User.Id))
            {
                await RespondAsync(embed: _embed.Error("You aren't joined!"), ephemeral: true);
                return;
            }

            if (_game.RunningGame(Context).Players.Count > 1)
            {
                _game.RunningGame(Context).Players.RemoveAll(p => p.ID == Context.User.Id);
                await UpdateResponse(Context);
            }
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

        [ComponentInteraction("sequence-leavegame")]
        public async Task LeaveGame()
        {
            _game.Games.Remove(_game.RunningGame(Context));
            await RespondAsync(embed: _embed.Info("You've been removed from the game."));
        }

        [ComponentInteraction("sequence-colorselection")]
        public async Task ColorSelection()
        {

        }

        [ComponentInteraction("sequence-start")]
        public async Task Play()
        {
            List<FileAttachment> attachments = new List<FileAttachment>();
            attachments.Add(((SequenceGame)_game.RunningGame(Context)).Table.ToImage(_emoji));
            await Context.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Attachments = attachments; m.Embed = null; m.Components = null; });
        }
    }
}
