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

        private async Task UpdateGame(IInteractionContext ctx)
        {
            SequenceGame game = _game.RunningGame(ctx) as SequenceGame;
            if (game.GameStatus == SequenceGame.Status.Remove)
                game.Players.RemoveAll(p => p.ID == Context.User.Id);

            StringBuilder players = new StringBuilder();
            foreach (SequencePlayer p in game.Players)
                players.Append($"{p.ColorEmoji} {_client.GetUser(p.ID).Mention}\n");

            EmbedBuilder embed = new EmbedBuilder()
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));
            if (game.GameStatus == SequenceGame.Status.Init ||
                game.GameStatus == SequenceGame.Status.Join)
                embed.WithDescription("A game of Sequence has started. Join to play the game!");
            else if (game.GameStatus == SequenceGame.Status.Color)
                embed.WithDescription("Players, select your colors in which you will play!");

            ComponentBuilder components = null;
            if (game.GameStatus != SequenceGame.Status.Color)
                components = new ComponentBuilder()
                    .WithButton("Play", "sequence-colorselection", ButtonStyle.Primary, disabled: game.Players.Count == 1)
                    .WithButton("Join", "sequence-join", ButtonStyle.Success, disabled: game.Players.Count == 3)
                    .WithButton("Leave", "sequence-leave", ButtonStyle.Danger);
            else
                components = new ComponentBuilder()
                    .WithButton("Red", "sequence-selectred", ButtonStyle.Danger, disabled: game.Players.Any(p => ((SequencePlayer)p).Color == SequenceGame.Color.Red))
                    .WithButton("Green", "sequence-selectgreen", ButtonStyle.Success, disabled: game.Players.Any(p => ((SequencePlayer)p).Color == SequenceGame.Color.Green))
                    .WithButton("Blue", "sequence-selectblue", ButtonStyle.Primary, disabled: game.Players.Any(p => ((SequencePlayer)p).Color == SequenceGame.Color.Blue))
                    .WithButton("Random", "sequence-selectrandomcolor", ButtonStyle.Secondary, disabled: true);

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
            await UpdateGame(Context);
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
                await UpdateGame(Context);
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
                await UpdateGame(Context);
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
            await UpdateGame(Context);
        }

        private void SetColor(in SequenceGame game, in SequenceGame.Color color) => ((SequencePlayer)game.Players.Find(p => p.ID == Context.User.Id)).Color = color;

        [ComponentInteraction("sequence-select*")]
        public async Task SelectColor(string color)
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

            switch (color)
            {
                case "red":
                    SetColor(game, SequenceGame.Color.Red);
                    break;
                case "green":
                    SetColor(game, SequenceGame.Color.Green);
                    break;
                case "blue":
                    SetColor(game, SequenceGame.Color.Blue);
                    break;
                default:
                    // Random - WIP
                    break;
            }
            await UpdateGame(Context);
        }

        public async Task Play()
        {
            List<FileAttachment> attachments = new List<FileAttachment>();
            attachments.Add(((SequenceGame)_game.RunningGame(Context)).Table.ToImage(_emoji));
            await Context.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Attachments = attachments; m.Embed = null; m.Components = null; });
        }
    }
}
