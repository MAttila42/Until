using Discord;

namespace Until.Services
{
    public class GameService
    {
        private readonly List<Game> games;

        public Game GetGame(int id) => this.games.Find(g => g.ID == id);
        public Game GetGame(IInteractionContext ctx) => this.games.Find(g => g.GetPlayer(ctx.User.Id) != null && g.Channel == ctx.Channel.Id);
        public void RemoveGame(in Game game) => this.games.Remove(game);

        public void AddGame(in Game game)
        {
            try { game.ID = this.games.Max(g => g.ID) + 1; }
            catch (Exception) { /*ignore*/ }
            this.games.Add(game);
        }

        public GameService()
        {
            this.games = new List<Game>();
        }
    }

    public abstract class Game
    {
        public int ID { get; set; }
        public ulong Channel { get; private set; }
        public List<Player> Players { get; private set; }

        public Player GetPlayer(ulong id) => this.Players.Find(p => p.ID == id);
        public Player GetPlayer(byte index) => this.Players[index % this.Players.Count];

        protected Game(ulong channelId)
        {
            this.ID = 0;
            this.Channel = channelId;
            this.Players = new List<Player>();
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
        public string Face { get; set; }
        public string Suit { get; set; }

        public string Name => $"{Capital(this.Face)} of {Capital(this.Suit)}";
        public string EmoteName => $"{this.Face}{(this.Suit == "joker" ? "" : $"_of_{this.Suit}")}";
        public string Code => $"{Deck.Faces[this.Face]}{Deck.Suits[this.Suit]}";

        private string Capital(in string s) => char.ToUpper(s[0]) + s.Substring(1);

        public Card(string code)
        {
            this.Face = Deck.Faces.First(f => f.Value == code[0]).Key;
            this.Suit = Deck.Suits.First(s => s.Value == code[1]).Key;
        }
        public Card(string face, string suit)
        {
            this.Face = face;
            this.Suit = suit;
        }
    }

    public static class Deck
    {
        public static List<Card> French() => French(0);
        public static List<Card> French(in byte jokers)
        {
            List<Card> temp = new List<Card>();
            foreach (string f in Faces.Keys.Take(13))
                foreach (string s in Suits.Keys.Take(4))
                    temp.Add(new(f, s));
            for (byte i = 0; i < jokers; i++)
                temp.Add(new("XX"));
            return temp;
        }

        public readonly static Dictionary<string, char> Faces = new Dictionary<string, char>
        {
            { "ace", 'A' },
            { "two", '2' },
            { "three", '3' },
            { "four", '4' },
            { "five", '5' },
            { "six", '6' },
            { "seven", '7' },
            { "eight", '8' },
            { "nine", '9' },
            { "ten", 'T' },
            { "jack", 'J' },
            { "queen", 'Q' },
            { "king", 'K' },
            { "joker", 'X' }
        };
        public readonly static Dictionary<string, char> Suits = new Dictionary<string, char>
        {
            { "clubs", 'C' },
            { "spades", 'S' },
            { "hearts", 'H' },
            { "diamonds", 'D' },
            { "joker", 'X' }
        };
    }
}
