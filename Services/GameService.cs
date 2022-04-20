using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;

namespace Until.Services
{
    public class GameService
    {
        public List<Game> Games;

        public Game WaitingGame(IInteractionContext ctx) => this.Games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Select(p => p.ID).Contains(ulong.Parse(Regex.Matches(((IComponentInteraction)ctx.Interaction).Message.Embeds.First().Fields.First().Value, "\\d*").Where(m => m.Value != "").First().Value)));
        public Game RunningGame(IInteractionContext ctx) => this.Games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Any(p => p.ID == ctx.User.Id));

        public GameService()
        {
            this.Games = new List<Game>();
        }
    }

    public abstract class Game
    {
        public ulong ChannelID;
        public List<Player> Players;

        public Game(ulong channelId)
        {
            this.ChannelID = channelId;
            this.Players = new List<Player>();
        }
    }

    public abstract class Player
    {
        public ulong ID;

        public Player(ulong userId)
        {
            this.ID = userId;
        }
    }

    public class Card
    {
        public static Dictionary<char, string> Faces = new Dictionary<char, string>()
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

        public static Dictionary<char, string> Suits = new Dictionary<char, string>()
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
