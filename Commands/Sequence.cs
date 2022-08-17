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
                await RespondAsync(embed: CurrentMenuEmbed(game), components: CurrentMenuComponents(game));
            }
        }

        private Embed CurrentMenuEmbed(in SequenceGame game)
        {
            StringBuilder players = new StringBuilder();
            foreach (SequencePlayer p in game.Players)
                players.Append($"{p.ColorEmoji} {_client.GetUser(p.ID).Mention}\n");

            EmbedBuilder temp = new EmbedBuilder()
                .WithDescription("A game of Sequence has started. Join to play the game!")
                .AddField("Players:", players)
                .WithColor(new Color(0x5864f2));

            if (game.GameStatus == SequenceGame.Status.Color ||
                game.GameStatus == SequenceGame.Status.Game)
                temp.WithDescription("Players, select your colors in which you will play!");

            return temp.Build();
        }
        private MessageComponent CurrentMenuComponents(in SequenceGame game)
        {
            ComponentBuilder temp = new ComponentBuilder();
            if (game.GameStatus != SequenceGame.Status.Color && game.GameStatus != SequenceGame.Status.Game)
                temp
                    .WithButton("Play", $"sequence-colorselection:{game.ID}", ButtonStyle.Primary, disabled: game.Players.Count == 1)
                    .WithButton("Join", $"sequence-join:{game.ID}", ButtonStyle.Success, disabled: game.Players.Count == 3)
                    .WithButton("Leave", $"sequence-leave:{game.ID}", ButtonStyle.Danger);
            else
            {
                byte i = 0;
                ButtonStyle[] styles = new ButtonStyle[]
                {
                    ButtonStyle.Danger,
                    ButtonStyle.Success,
                    ButtonStyle.Primary,
                    ButtonStyle.Secondary
                };
                foreach (var c in colors.Take(3)) // Random WIP
                {
                    temp.WithButton(c.Key[0].ToString().ToUpper() + c.Key.Substring(1), $"sequence-selectcolor:{game.ID},{c.Key}", styles[i++], disabled: game.Players.Any(p => ((SequencePlayer)p).Color == c.Value) || game.GameStatus == SequenceGame.Status.Game);
                }
            }
            return temp.Build();
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
                    await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentMenuEmbed(game); m.Components = CurrentMenuComponents(game); });
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
                    embed = CurrentMenuEmbed(game);
                    components = CurrentMenuComponents(game);
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
                await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentMenuEmbed(game); m.Components = CurrentMenuComponents(game); });
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

            await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m => { m.Embed = CurrentMenuEmbed(game); m.Components = CurrentMenuComponents(game); });

            if (game.Players.Count == game.Players.Where(p => ((SequencePlayer)p).Color != SequenceGame.Color.None).Select(p => ((SequencePlayer)p).Color).Distinct().Count())
            {
                game.GameStatus = SequenceGame.Status.Game;
                await ((SocketMessageComponent)Context.Interaction).Message.ModifyAsync(m =>
                {
                    m.Content = $"{_emoji.GetEmoji("util_loading")} Loading...";
                    m.Components = CurrentMenuComponents(game);
                }).ConfigureAwait(false);
                game.Players.ForEach(p => ((SequencePlayer)p).FillHand(game));
                await UpdateGameAsync(game);
            }
        }

        private async Task UpdateGameAsync(SequenceGame game) => await UpdateGameAsync(game, 0, "");
        private async Task UpdateGameAsync(SequenceGame game, ulong messageId) => await UpdateGameAsync(game, messageId, "");
        private async Task UpdateGameAsync(SequenceGame game, ulong messageId, string cardName)
        {
            List<FileAttachment> attachments = new List<FileAttachment>();
            if (cardName == "")
                attachments.Add(game.TableImage(_emoji));
            else
                attachments.Add(game.TableImage(_emoji, cardName));

            await (messageId == 0 ? ((SocketMessageComponent)Context.Interaction).Message : (IUserMessage)await Context.Channel.GetMessageAsync(messageId))
                .ModifyAsync(async m =>
                {
                    m.Content = $"{(await _client.GetUserAsync(game.CurrentPlayer.ID)).Mention}'s turn";
                    m.Attachments = attachments;
                    m.Embed = null;
                    m.Components = CurrentGameComponents(game, cardName);
                });
        }
        private MessageComponent CurrentGameComponents(in SequenceGame game, in string cardName)
        {
            ComponentBuilder temp = new ComponentBuilder();
                temp.WithButton("My cards", $"sequence-cards:{game.ID}");
            byte cardCount = game.CountAvaliable(cardName);
            if (cardCount > 1)
                for (byte i = 1; i <= cardCount; i++)
                    temp.WithButton(i.ToString(), $"sequence-selectcardnumber:{game.ID},{cardName},{i - 1}", style: ButtonStyle.Secondary);
            return temp.Build();
        }

        private List<Card> GetHand(SequenceGame game) => ((SequencePlayer)game.GetPlayer(Context.User.Id)).Hand;
        private string CurrentHeldCards(SequenceGame game)
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
            await RespondAsync(CurrentHeldCards(game), components: CurrentSelectMenu(game, ((IComponentInteraction)Context.Interaction).Message.Id), ephemeral: true);
        }

        [ComponentInteraction("sequence-selectcard:*,*")]
        public async Task SelectCard(string gameId, string messageIdStr, string[] selectedCards)
        {
            SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
            SequencePlayer player = game.GetPlayer(Context.User.Id) as SequencePlayer;

            if (game.CurrentPlayer.ID != player.ID)
                return;

            string card = selectedCards[0];
            ulong messageId = ulong.Parse(messageIdStr);
            byte cardCount = game.CountAvaliable(card);

            game.ThrowCard(card);
            await ((IComponentInteraction)Context.Interaction).UpdateAsync(m => { m.Content = CurrentHeldCards(game); m.Components = CurrentSelectMenu(game, messageId); });

            switch (cardCount)
            {
                case 0:
                    return;
                case 1:
                    game.PlaceChip(game.CurrentPlayer.Color, card);
                    game.CurrentPlayerIndex++;
                    await UpdateGameAsync(game, messageId);
                    break;
                default:
                    await UpdateGameAsync(game, messageId, card);
                    break;
            }
        }

        [ComponentInteraction("sequence-selectcardnumber:*,*,*")]
        public async Task SelectCardNumber(string gameId, string cardName, string index)
        {
            SequenceGame game = _game.GetGame(int.Parse(gameId)) as SequenceGame;
            game.PlaceChip(game.CurrentPlayer.Color, cardName, byte.Parse(index));
            game.CurrentPlayerIndex++;
            await UpdateGameAsync(game);
            await DeferAsync();
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
