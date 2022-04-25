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
        public EmojiService _emoji { get; set; }
        public GameService _game { get; set; }

        private Dictionary<string, SequenceGame.Color> colors = new Dictionary<string, SequenceGame.Color>
        {
            { "red", SequenceGame.Color.Red },
            { "green", SequenceGame.Color.Green },
            { "blue", SequenceGame.Color.Blue },
            { "random", SequenceGame.Color.Joker }
        };

        private async Task Update(IInteractionContext ctx)
        {
            SequenceGame game = _game.GetGame(ctx, false) as SequenceGame;
            if (game.GameStatus == SequenceGame.Status.Remove)
                game.Players.RemoveAll(p => p.ID == Context.User.Id);

            if (game.GameStatus == SequenceGame.Status.Init ||
                game.GameStatus == SequenceGame.Status.Remove ||
                game.GameStatus == SequenceGame.Status.Join ||
                game.GameStatus == SequenceGame.Status.Color ||
                game.GameStatus == SequenceGame.Status.Start)
                await UpdateMenu(ctx, game).ConfigureAwait(false);
            else
                await UpdateGame(ctx, game).ConfigureAwait(false);
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
                ButtonStyle[] styles = new ButtonStyle[]
                {
                    ButtonStyle.Danger,
                    ButtonStyle.Success,
                    ButtonStyle.Primary,
                    ButtonStyle.Secondary
                };
                foreach (var c in colors.Take(3))
                {
                    components.WithButton(c.Key[0].ToString().ToUpper() + c.Key.Substring(1), $"sequence-select{c.Key}", styles[i++], disabled: game.Players.Any(p => ((SequencePlayer)p).Color == c.Value) || game.GameStatus == SequenceGame.Status.Start);
                }
            }

            await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)ctx.Interaction).Message.Id, m =>
            {
                m.Embed = embed.Build();
                m.Components = components.Build();
            });

            if (game.GameStatus != SequenceGame.Status.Init)
                await DeferAsync();

            if (game.GameStatus == SequenceGame.Status.Start)
            {
                await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)ctx.Interaction).Message.Id, m => { m.Content = $"{_emoji.GetEmoji("util_loading")} Loading..."; });
                foreach (SequencePlayer p in game.Players)
                    if (p.HeldCardNames.Count == 0)
                        p.FillHand(game);
                await UpdateGame(ctx, game);
            }
        }

        private async Task UpdateGame(IInteractionContext ctx, SequenceGame game)
        {
            List<FileAttachment> attachments = new List<FileAttachment> { ((SequenceGame)_game.GetGame(Context)).TableImage(_emoji) };

            EmbedBuilder embed = new EmbedBuilder()
                .AddField($"{((IGuildUser)_client.GetUser(game.CurrentPlayer.ID)).Nickname}'s turn", "To check your cards, use the `/cards` command!")
                .WithColor(new Color(0x5864f2));

            SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                .WithPlaceholder("Select a card to play!")
                .WithCustomId("sequence-cardselectionmenu")
                .WithMinValues(1)
                .WithMaxValues(1);

            for (int i = 1; i <= 7; i++)
                selectMenu.AddOption(i.ToString(), i.ToString());

            ComponentBuilder components = new ComponentBuilder()
                .WithSelectMenu(selectMenu);

            await ctx.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Content = ""; m.Attachments = attachments; m.Embed = embed.Build(); m.Components = components.Build(); });
        }

        [SlashCommand("sequence", "Start a new game of Sequence")]
        public async Task Run([Summary(description: "Show how to play Sequence")] bool rules = false)
        {
            if (rules)
            {
                Embed rulesEmbed = new EmbedBuilder()
                    .WithAuthor("Rules", EmbedService.InfoIcon)
                    .AddField("Basic concept", "Everyone gets 7 cards. Each round you can put a chip at one of your card's place on the table and get a new card. The goal is to make 2 rows of 5 chips (2 players) or just 1 row when playing with 3 players.")
                    .WithColor(new Color(0x5864f2))
                    .Build();
                await RespondAsync(embed: rulesEmbed);
                return;
            }

            //try
            //{
                //await RespondAsync($"{_emoji.GetEmoji("util_loading")} Loading...");
                await DeferAsync();
                IUserMessage message = await FollowupAsync($"{_emoji.GetEmoji("util_loading")} Loading...");
                _game.AddGame(new SequenceGame(Context.Channel.Id, Context.User.Id, message.Id, _emoji));
            //await Update(Context);
            //await UpdateMenu(Context, _game.GetGameByPlayer(Context) as SequenceGame);
            //}
            //catch (Exception)
            //{
            //    ComponentBuilder components = new ComponentBuilder()
            //        .WithButton("Leave Game", "sequence-leavegame", ButtonStyle.Danger);
            //    await RespondAsync(embed: EmbedService.Error("You are already playing a game here!"), components: components.Build());
            //}
        }

        [ComponentInteraction("sequence-join")]
        public async Task Join()
        {
            //if (_game.WaitingGame(Context).Players.Any(p => p.ID == Context.User.Id))
            //    await RespondAsync(embed: EmbedService.Error("You are already joined!"), ephemeral: true);
            //else
            //{
            //    SequenceGame game = _game.WaitingGame(Context) as SequenceGame;
            //    game.Players.Add(new SequencePlayer(Context.User.Id));
            //    game.GameStatus = SequenceGame.Status.Join;
            //    await Update(Context);
            //}

            try
            {
                _game.GetGameByPlayer(Context);
            }
            catch (Exception)
            {
                SequenceGame game = _game.GetGame(Context) as SequenceGame;
                game.Players.Add(new SequencePlayer(Context.User.Id));
                game.GameStatus = SequenceGame.Status.Join;
                await Update(Context);
            }
        }

        [ComponentInteraction("sequence-leave")]
        public async Task Leave()
        {
            //if (!_game.WaitingGame(Context).Players.Select(p => p.ID).ToList().Contains(Context.User.Id))
            //{
            //    await RespondAsync(embed: EmbedService.Error("You aren't joined!"), ephemeral: true);
            //    return;
            //}

            //SequenceGame game = _game.RunningGame(Context) as SequenceGame;
            //if (game.Players.Count > 1)
            //{
            //    game.GameStatus = SequenceGame.Status.Remove;
            //    await Update(Context);
            //}
            //else
            //{
            //    _game.RemoveGame(_game.RunningGame(Context));
            //    Embed embed = new EmbedBuilder()
            //        .WithDescription("The game has ended.")
            //        .WithColor(new Color(0x5864f2))
            //        .Build();

            //    await Context.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Embed = embed; m.Components = null; });
            //}

            try
            {
                SequenceGame game = _game.GetGame(Context) as SequenceGame;
                if (game.Players.Count > 1)
                {
                    game.GameStatus = SequenceGame.Status.Remove;
                    await Update(Context);
                }
                else
                {
                    _game.RemoveGame(game);
                    Embed embed = new EmbedBuilder()
                        .WithDescription("The game has ended.")
                        .WithColor(new Color(0x5864f2))
                        .Build();

                    await Context.Channel.ModifyMessageAsync(((SocketMessageComponent)Context.Interaction).Message.Id, m => { m.Embed = embed; m.Components = null; });
                }
            }
            catch (Exception)
            {
                await RespondAsync(embed: EmbedService.Error("You aren't joined!"), ephemeral: true);
            }
        }

        [ComponentInteraction("sequence-leavegame")]
        public async Task LeaveGame()
        {
            _game.RemoveGame(_game.GetGameByPlayer(Context));
            await RespondAsync(embed: EmbedService.Info("You've been removed from the game."), ephemeral: true);
        }

        [ComponentInteraction("sequence-colorselection")]
        public async Task ColorSelection()
        {
            SequenceGame game;
            try
            {
                game = _game.GetGame(Context) as SequenceGame;
            }
            catch (Exception)
            {
                await RespondAsync(embed: EmbedService.Error("You're not in a game!"));
                return;
            }

            game.GameStatus = SequenceGame.Status.Color;
            await Update(Context).ConfigureAwait(false);
        }

        [ComponentInteraction("sequence-select*")]
        public async Task SelectColor(string c)
        {
            SequenceGame game;
            try
            {
                game = _game.GetGame(Context) as SequenceGame;
            }
            catch (Exception)
            {
                await RespondAsync(embed: EmbedService.Error("You're not in a game!"), ephemeral: true);
                return;
            }

            if (game.GameStatus != SequenceGame.Status.Color)
            {
                await RespondAsync(embed: EmbedService.Error("You can't select a color now!"), ephemeral: true);
                return;
            }

            try
            {
                if (game.Players.Select(p => ((SequencePlayer)p).Color).ToList().Contains(colors[c]))
                {
                    await RespondAsync(embed: EmbedService.Error("Someone else has that color!"), ephemeral: true);
                    return;
                }
                ((SequencePlayer)game.Players.Find(p => p.ID == Context.User.Id)).Color = colors[c];
            }
            catch (Exception)
            {
                return;
            }

            if (game.Players.Count == game.Players.Where(p => ((SequencePlayer)p).Color != SequenceGame.Color.None).Select(p => ((SequencePlayer)p).Color).Distinct().Count())
                game.GameStatus = SequenceGame.Status.Start;

            await Update(Context);
        }
    }
}
