using System.Collections.Generic;
using System.Linq;
//using System.Text.RegularExpressions;
using Discord;

namespace Until.Services
{
    public class GameService
    {
        private readonly List<Game> games;

        public Game GetGame(IInteractionContext ctx) => GetGame(ctx, true);
        public Game GetGame(IInteractionContext ctx, bool isJoined) => this.games.Find(g => g.MessageID == ((IComponentInteraction)ctx).Message.Id); //  && (isJoined && g.Players.Any(p => p.ID == ctx.User.Id))
        public Game GetGameByPlayer(IInteractionContext ctx) => this.games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Any(p => p.ID == ctx.User.Id));

        //public Game WaitingGame(IInteractionContext ctx) => this.games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Select(p => p.ID).Contains(ulong.Parse(Regex.Matches(((IComponentInteraction)ctx.Interaction).Message.Embeds.First().Fields.First().Value, "\\d*").Where(m => m.Value != "").First().Value)));
        //public Game RunningGame(IInteractionContext ctx) => this.games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Any(p => p.ID == ctx.User.Id));

        public void AddGame(in Game game) => this.games.Add(game);
        public void RemoveGame(in Game game) => this.games.Remove(game);

        public GameService()
        {
            this.games = new List<Game>();
        }
    }

    public abstract class Game
    {
        private List<Player> players;

        public ulong ChannelID { get; private set; }
        public ulong MessageID { get; private set; }

        public List<Player> Players => this.players;

        protected Game(ulong channelId, ulong messageId)
        {
            this.players = new List<Player>();
            this.ChannelID = channelId;
            this.MessageID = messageId;
        }
    }

    public abstract class Player
    {
        public ulong ID { get; private set; }

        protected Player(ulong userId)
        {
            this.ID = userId;
        }
    }

    public class Card
    {
        public readonly static Dictionary<char, string> Faces = new Dictionary<char, string>
        {
            { 'A', "ace" },
            { '2', "two" },
            { '3', "three" },
            { '4', "four" },
            { '5', "five" },
            { '6', "six" },
            { '7', "seven" },
            { '8', "eight" },
            { '9', "nine" },
            { 'T', "ten" },
            { 'J', "jack" },
            { 'Q', "queen" },
            { 'K', "king" },
            { 'X', "joker" }
        };

        public readonly static Dictionary<char, string> Suits = new Dictionary<char, string>
        {
            { 'C', "_of_clubs" },
            { 'S', "_of_spades" },
            { 'H', "_of_hearts" },
            { 'D', "_of_diamonds" },
            { 'X', "" }
        };

        public static List<string> Deck
        {
            get
            {
                List<string> temp = new List<string>();
                foreach (char f in Faces.Keys.Take(13))
                    foreach (char s in Suits.Keys.Take(4))
                        temp.Add($"{f}{s}");
                return temp;
            }
        }

        public static string Name(string code) => Faces[code[0]] + Suits[code[1]];
    }
}
