using System;
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

        private Dictionary<string, SequenceGame.Color> colors = new Dictionary<string, SequenceGame.Color>()
        {
            { "red", SequenceGame.Color.Red },
            { "green", SequenceGame.Color.Green },
            { "blue", SequenceGame.Color.Blue },
            { "random", SequenceGame.Color.Joker }
        };

        private async Task Update(IInteractionContext ctx)
        {
            SequenceGame game = _game.RunningGame(ctx) as SequenceGame;
            if (game.GameStatus == SequenceGame.Status.Remove)
                game.Players.RemoveAll(p => p.ID == Context.User.Id);

            if (game.GameStatus == SequenceGame.Status.Init ||
                game.GameStatus == SequenceGame.Status.Remove ||
                game.GameStatus == SequenceGame.Status.Join ||
                game.GameStatus == SequenceGame.Status.Color ||
                game.GameStatus == SequenceGame.Status.Start)
                await UpdateMenu(ctx, game);
            else
                await UpdateGame(ctx, game);
        }

        private async Task UpdateMenu(IInteractionContext ctx, SequenceGame game)
        {
            StringBuilder players = new StringBuilder();
            foreach (SequencePlayer p in game.Players)
                players.Append($"{p.ColorEmoji} {_client.GetUser(p.ID).Mention}\n");

            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription("A game of Sequence has started. Join to play the game!")
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));

            ComponentBuilder components = null;
            if (game.GameStatus != SequenceGame.Status.Color && game.GameStatus != SequenceGame.Status.Start)
                components = new ComponentBuilder()
                    .WithButton("Play", "sequence-colorselection", ButtonStyle.Primary, disabled: game.Players.Count == 1)
                    .WithButton("Join", "sequence-join", ButtonStyle.Success, disabled: game.Players.Count == 3)
                    .WithButton("Leave", "sequence-leave", ButtonStyle.Danger);
            else
            {
                byte i = 0;
                components = new ComponentBuilder();
                embed.WithDescription("Players, select your colors in which you will play!");
                ButtonStyle[] styles = new ButtonStyle[3]
                {
                    ButtonStyle.Danger,
                    ButtonStyle.Success,
                    ButtonStyle.Primary
                    //ButtonStyle.Secondary
                };
                foreach (var c in colors.Take(3))
                {
                    components.WithButton(c.Key[0].ToString().ToUpper() + c.Key.Substring(1), $"sequence-select{c.Key}", styles[i++], disabled: game.Players.Any(p => ((SequencePlayer)p).Color == c.Value) || game.GameStatus == SequenceGame.Status.Start);
                }
            }

            if (game.GameStatus == SequenceGame.Status.Init)
                await ctx.Interaction.RespondAsync(embed: embed.Build(), components: components.Build());
            else
            {
                await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)ctx.Interaction).Message.Id, m =>
                {
                    m.Embed = embed.Build();
                    m.Components = components.Build();
                });
                await DeferAsync();
            }

            if (game.GameStatus == SequenceGame.Status.Start)
                await UpdateGame(ctx, game);
        }

        private async Task UpdateGame(IInteractionContext ctx, SequenceGame game)
        {
            List <FileAttachment> attachments = new List<FileAttachment>() { ((SequenceGame)_game.RunningGame(Context)).Table.ToImage(_emoji) };
            await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Attachments = attachments; m.Embed = null; m.Components = null; });
        }

        [SlashCommand("sequence", "Start a new game of Sequence")]
        public async Task Run([Summary(description: "Show how to play Sequence")]bool rules = false)
        {
            if (rules)
            {
                Embed rulesEmbed = new EmbedBuilder()
                    .WithAuthor("Rules", _embed.InfoIcon)
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
            await Update(Context);
        }

        [ComponentInteraction("sequence-join")]
        public async Task Join()
        {
            if (_game.WaitingGame(Context).Players.Any(p => p.ID == Context.User.Id))
                await RespondAsync(embed: _embed.Error("You are already joined!"), ephemeral: true);
            else
            {
                SequenceGame game = _game.WaitingGame(Context) as SequenceGame;
                game.Players.Add(new SequencePlayer(Context.User.Id));
                game.GameStatus = SequenceGame.Status.Join;
                await Update(Context);
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

            SequenceGame game = _game.RunningGame(Context) as SequenceGame;
            if (game.Players.Count > 1)
            {
                game.GameStatus = SequenceGame.Status.Remove;
                await Update(Context);
            }
            else
            {
                _game.Games.Remove(_game.RunningGame(Context));
                Embed embed = new EmbedBuilder()
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
            await RespondAsync(embed: _embed.Info("You've been removed from the game."), ephemeral: true);
        }

        [ComponentInteraction("sequence-colorselection")]
        public async Task ColorSelection()
        {
            SequenceGame game;
            try
            {
                game = _game.RunningGame(Context) as SequenceGame;
            }
            catch (Exception)
            {
                await RespondAsync(embed: _embed.Error("You're not in a game!"));
                return;
            }

            game.GameStatus = SequenceGame.Status.Color;
            await Update(Context);
        }

        [ComponentInteraction("sequence-select*")]
        public async Task SelectColor(string c)
        {
            SequenceGame game;
            try
            {
                game = _game.RunningGame(Context) as SequenceGame;
            }
            catch (Exception)
            {
                await RespondAsync(embed: _embed.Error("You're not in a game!"), ephemeral: true);
                return;
            }

            if (game.GameStatus != SequenceGame.Status.Color)
            {
                await RespondAsync(embed: _embed.Error("You can't select a color now!"), ephemeral: true);
                return;
            }

            try
            {
                if (game.Players.Select(p => ((SequencePlayer)p).Color).ToList().Contains(colors[c]))
                {
                    await RespondAsync(embed: _embed.Error("Someone else has that color!"), ephemeral: true);
                    return;
                }
                ((SequencePlayer)game.Players.Find(p => p.ID == Context.User.Id)).Color = colors[c];
            }
            catch (Exception) { }

            if (game.Players.Count == game.Players.Where(p => ((SequencePlayer)p).Color != SequenceGame.Color.None).Select(p => ((SequencePlayer)p).Color).Distinct().Count())
                game.GameStatus = SequenceGame.Status.Start;

            await Update(Context);
        }
    }
}
