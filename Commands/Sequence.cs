using System.Text;
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

            try
            {
                var game = _game.GetGame(Context);
                ComponentBuilder components = new ComponentBuilder()
                    .WithButton("Leave Game", $"sequence-leavegame:{game.ID}", ButtonStyle.Danger);
                await RespondAsync(embed: EmbedService.Error("You're already playing a game here"), components: components.Build(), ephemeral: true);
                return;
            }
            catch (Exception)
            {
                SequenceGame game = new SequenceGame(Context.Channel.Id, Context.User.Id, _emoji);
                _game.AddGame(game);
                await RespondAsync(embed: CurrentEmbed(game), components: CurrentComponents(game));
            }
        }

        private Embed CurrentEmbed(in SequenceGame game)
        {
            StringBuilder players = new StringBuilder();
            foreach (SequencePlayer p in game.Players)
                players.Append($"{p.ColorEmoji} {_client.GetUser(p.ID).Mention}\n");

            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription("A game of Sequence has started. Join to play the game!")
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));

            if (game.GameStatus == SequenceGame.Status.Color ||
                game.GameStatus == SequenceGame.Status.Game)
                embed.WithDescription("Players, select your colors in which you will play!");

            return embed.Build();
        }
        private MessageComponent CurrentComponents(in SequenceGame game)
        {
            ComponentBuilder components = null;
            if (game.GameStatus != SequenceGame.Status.Color && game.GameStatus != SequenceGame.Status.Game)
                components = new ComponentBuilder()
                    .WithButton("Play", $"sequence-colorselection:{game.ID}", ButtonStyle.Primary, disabled: game.Players.Count == 1)
                    .WithButton("Join", $"sequence-join:{game.ID}", ButtonStyle.Success, disabled: game.Players.Count == 3)
                    .WithButton("Leave", $"sequence-leave:{game.ID}", ButtonStyle.Danger);
            else
            {
                byte i = 0;
                components = new ComponentBuilder();
                ButtonStyle[] styles = new ButtonStyle[]
                {
                    ButtonStyle.Danger,
                    ButtonStyle.Success,
                    ButtonStyle.Primary,
                    ButtonStyle.Secondary
                };
                foreach (var c in colors.Take(3)) // Random WIP
                {
                    components.WithButton(c.Key[0].ToString().ToUpper() + c.Key.Substring(1), $"sequence-selectcolor:{game.ID},{c.Key}", styles[i++], disabled: game.Players.Any(p => ((SequencePlayer)p).Color == c.Value) || game.GameStatus == SequenceGame.Status.Game);
                }
            }
            return components.Build();
        }

        [ComponentInteraction("sequence-join:*")]
        public async Task Join(string gameId)
        {
            try
            {
                await DeferAsync();
                SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
                if (game.Players.Any(p => p.ID == Context.User.Id))
                    return;
                else
                {
                    game.Players.Add(new SequencePlayer(Context.User.Id));
                    await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentEmbed(game); m.Components = CurrentComponents(game); });
                }
            }
            catch (Exception) { /*ignore*/ }
        }

        [ComponentInteraction("sequence-leave:*")]
        public async Task Leave(string gameId)
        {
            try
            {
                SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
                Embed embed;
                MessageComponent components = null;
                if (game.Players.Count > 1)
                {
                    game.Players.RemoveAll(p => p.ID == Context.User.Id);
                    embed = CurrentEmbed(game);
                    components = CurrentComponents(game);
                }
                else
                {
                    _game.RemoveGame(game);
                    embed = new EmbedBuilder()
                        .WithDescription("The game has ended.")
                        .WithColor(new Color(0x5864f2))
                        .Build();
                }
                await DeferAsync();
                await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = embed; m.Components = components; });
            }
            catch (Exception) { /*ignore*/ }
        }

        [ComponentInteraction("sequence-leavegame:*")]
        public async Task LeaveGame(string gameId)
        {
            _game.RemoveGame(_game.GetGame(int.Parse(gameId)));
            await RespondAsync(embed: EmbedService.Info("You've been removed from the game"), ephemeral: true);
        }

        [ComponentInteraction("sequence-colorselection:*")]
        public async Task ColorSelection(string gameId)
        {
            try
            {
                await DeferAsync();
                SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
                game.GameStatus = SequenceGame.Status.Color;
                await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentEmbed(game); m.Components = CurrentComponents(game); });
            }
            catch (Exception) { /*ignore*/ }
        }

        [ComponentInteraction("sequence-selectcolor:*,*")]
        public async Task SelectColor(string gameId, string color)
        {
            await DeferAsync();
            SequenceGame game;
            try { game = _game.GetGame(int.Parse(gameId)) as SequenceGame; }
            catch (Exception) { return; }

            try
            {
                if (game.Players.Select(p => ((SequencePlayer)p).Color).ToList().Contains(colors[color]))
                    return;
                ((SequencePlayer)game.Players.Find(p => p.ID == Context.User.Id)).Color = colors[color];
            }
            catch (Exception) { return; }

            await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentEmbed(game); m.Components = CurrentComponents(game); });

            if (game.Players.Count == game.Players.Where(p => ((SequencePlayer)p).Color != SequenceGame.Color.None).Select(p => ((SequencePlayer)p).Color).Distinct().Count())
            {
                game.GameStatus = SequenceGame.Status.Game;
                await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Content = $"{_emoji.GetEmoji("util_loading")} Loading..."; m.Components = CurrentComponents(game); }).ConfigureAwait(false);
                game.Players.ForEach(p => ((SequencePlayer)p).FillHand(game));
                await UpdateGameAsync(game);
            }
        }

        private async Task UpdateGameAsync(SequenceGame game) => await UpdateGameAsync(game, 0);
        private async Task UpdateGameAsync(SequenceGame game, ulong messageId)
            => await (messageId == 0 ? ((SocketMessageComponent)Context.Interaction).Message : (IUserMessage)await Context.Channel.GetMessageAsync(messageId))
                .ModifyAsync(async m =>
                {
                    m.Content = $"{(await _client.GetUserAsync(game.CurrentPlayer.ID)).Mention}'s turn";
                    m.Attachments = new List<FileAttachment> { game.TableImage(_emoji) };
                    m.Embed = null;
                    m.Components = new ComponentBuilder()
                        .WithButton("My cards", $"sequence-cards:{game.ID}")
                        .Build();
                });

        private List<Card> GetHand(SequenceGame game) => ((SequencePlayer)game.GetPlayer(Context.User.Id)).Hand;
        private string CurrentCards(SequenceGame game)
        {
            StringBuilder s = new StringBuilder();
            foreach (Card c in GetHand(game))
                s.Append(_emoji.GetEmoji(c.EmoteName));
            return s.ToString();
        }
        private MessageComponent CurrentSelectMenu(SequenceGame game, ulong gameMessage)
        {
            SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                .WithPlaceholder("Select a card to play!")
                .WithCustomId($"sequence-selectcard:{game.ID},{gameMessage}")
                .WithMinValues(1)
                .WithMaxValues(1);
            foreach (Card c in GetHand(game))
                selectMenu.AddOption(c.Name, c.EmoteName, "Click to choose", _emoji.GetEmoji(c.EmoteName));
            return new ComponentBuilder().WithSelectMenu(selectMenu).Build();
        }

        [ComponentInteraction("sequence-cards:*")]
        public async Task Cards(string gameId)
        {
            SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
            await RespondAsync(CurrentCards(game), components: CurrentSelectMenu(game, ((IComponentInteraction)Context.Interaction).Message.Id), ephemeral: true);
        }

        [ComponentInteraction("sequence-selectcard:*,*")]
        public async Task SelectCard(string gameId, string messageIdStr, string[] selectedCards)
        {
            SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
            SequencePlayer player = game.GetPlayer(Context.User.Id) as SequencePlayer;

            if (game.CurrentPlayer.ID != player.ID)
                return;

            string card = selectedCards[0];
            byte cardCount = game.CountAvaliable(card);
            switch (cardCount)
            {
                case 0:
                    return;
                case 1:
                    game.PlaceChip(game.CurrentPlayer.Color, card, 0);
                    break;
                default:
                    game.PlaceChip(game.CurrentPlayer.Color, card, 0);
                    break;
            }

            ulong messageId = ulong.Parse(messageIdStr);
            game.CurrentPlayerIndex++;
            await ((IComponentInteraction)Context.Interaction).UpdateAsync(m => { m.Content = CurrentCards(game); m.Components = CurrentSelectMenu(game, messageId); });
            await UpdateGameAsync(game, messageId);
        }

        private Dictionary<string, SequenceGame.Color> colors = new Dictionary<string, SequenceGame.Color>
        {
            { "red", SequenceGame.Color.Red },
            { "green", SequenceGame.Color.Green },
            { "blue", SequenceGame.Color.Blue },
            { "random", SequenceGame.Color.Joker }
        };
    }
}
