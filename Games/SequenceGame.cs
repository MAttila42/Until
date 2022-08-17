using System.Globalization;
using SkiaSharp;
using Discord;
using Until.Services;

namespace Until.Games
{
    public class SequenceGame : Services.Game
    {
        public enum Color
        {
            None,
            Red,
            Green,
            Blue,
            Joker
        }

        public enum Status
        {
            Join,
            Color,
            Game
        }

        private readonly SequenceTable table;
        private readonly List<Card> deck;

        public Status GameStatus { get; set; }
        public byte CurrentPlayerIndex { get; set; }

        public byte CountAvaliable(in string cardName) => this.table.CountCard(cardName);
        public SequencePlayer CurrentPlayer => this.GetPlayer(this.CurrentPlayerIndex) as SequencePlayer;

        public FileAttachment TableImage(in EmojiService emojiService) => TableImage(emojiService, "");
        public FileAttachment TableImage(in EmojiService emojiService, in string highlightedCardName) => this.table.ToImage(emojiService, highlightedCardName);

        public void PlaceChip(in Color color, in string cardName) => PlaceChip(color, cardName, 0);
        public void PlaceChip(in Color color, in string cardName, in byte index) => this.table.PlaceChip(color, cardName, index);

        public void ThrowCard(string cardName)
        {
            this.CurrentPlayer.Hand.Remove(this.CurrentPlayer.Hand.Find(c => c.EmoteName == cardName));
            this.CurrentPlayer.Hand.Add(PullCard());
        }

        public Card PullCard()
        {
            Random r = new Random();
            byte i = (byte)r.Next(0, this.deck.Count);
            Card temp = this.deck[i];
            this.deck.RemoveAt(i);
            return temp;
        }

        public SequenceGame(ulong channelId, ulong userId, EmojiService emojiService) : base(channelId)
        {
            this.Players.Add(new SequencePlayer(userId));
            this.table = new SequenceTable(emojiService);
            this.deck = Deck.French();
            this.GameStatus = Status.Join;
            this.CurrentPlayerIndex = 0;
        }
    }

    public class SequencePlayer : Player
    {
        public List<Card> Hand { get; set; }
        public SequenceGame.Color Color { get; set; }

        public string ColorEmoji
        {
            get
            {
                Dictionary<SequenceGame.Color, string> colors = new Dictionary<SequenceGame.Color, string>
                {
                    { SequenceGame.Color.None, ":black_circle:" },
                    { SequenceGame.Color.Red, ":red_circle:" },
                    { SequenceGame.Color.Green, ":green_circle:" },
                    { SequenceGame.Color.Blue, ":blue_circle:" },
                    { SequenceGame.Color.Joker, ":grey_question:" }
                };
                return colors[this.Color];
            }
        }
        public Color ColorHEX
        {
            get
            {
                Dictionary<SequenceGame.Color, Color> colors = new Dictionary<SequenceGame.Color, Color>
                {
                    { SequenceGame.Color.Red, new(0xbe1931) },
                    { SequenceGame.Color.Green, new(0x4da631) },
                    { SequenceGame.Color.Blue, new(0x445893) }
                };
                return colors[this.Color];
            }
        }

        public void FillHand(in SequenceGame game)
        {
            while (this.Hand.Count < 7)
                this.Hand.Add(game.PullCard());
        }

        public SequencePlayer(ulong userId) : base(userId)
        {
            this.Hand = new List<Card>();
            this.Color = SequenceGame.Color.None;
        }
    }

    public class SequenceTable
    {
        private readonly SequenceTableCell[,] cells;

        public byte CountCard(in string name)
        {
            byte count = 0;
            foreach (SequenceTableCell r in this.cells)
                if (r.CardEmote.Name == name)
                    count++;
            return count;
        }

        public void PlaceChip(in SequenceGame.Color color, in string name, in byte i)
        {
            byte temp = 0;
            for (byte y = 0; y < 10; y++)
                for (byte x = 0; x < 10; x++)
                    if (this.cells[x, y].CardEmote.Name == name)
                        if (i == temp++)
                            this.cells[x, y].Color = color;
        }

        public FileAttachment ToImage(in EmojiService emojiService, in string name)
        {
            SKSurface tempSurface = SKSurface.Create(new SKImageInfo(640, 766));
            SKCanvas canvas = tempSurface.Canvas;
            canvas.Clear(SKColors.Transparent);

            byte temp = 1;
            for (byte y = 0; y < 10; y++)
                for (byte x = 0; x < 10; x++)
                {
                    SequenceTableCell cell = cells[x, y];
                    SequenceGame.Color color = cell.Color;
                    bool unclaimed = color == SequenceGame.Color.None || color == SequenceGame.Color.Joker;
                    string imageName;

                    if (unclaimed)
                        imageName = cell.CardEmote.Name;
                    else
                        imageName = emojiService.GetEmoji($"{color.ToString().ToLower(CultureInfo.InvariantCulture)}_chip_card").Name;

                    if (name != "")
                    {
                        if (cell.CardEmote.Name == name)
                            imageName = $"card_{numbers[temp++]}";
                        else
                            imageName = $"faded_{imageName}";
                        if (!unclaimed)
                            imageName = $"half_{imageName}";
                    }

                    canvas.DrawBitmap(
                        emojiService
                            .GetImage(imageName)
                            .Resize(new SKImageInfo(64, 64), SKFilterQuality.Low), 
                        SKRect.Create(x * 64, y * 78, 64, 64));
                }
            return new FileAttachment(tempSurface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream(), "Until-Sequence.png");
        }
        private Dictionary<byte, string> numbers = new Dictionary<byte, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
            { 4, "four" },
            { 5, "five" },
            { 6, "six" },
            { 7, "seven" },
            { 8, "eight" },
            { 9, "nine" },
        };

        public SequenceTable(EmojiService emoji)
        {
            this.cells = new SequenceTableCell[10, 10];

            string[] tableBase = new[]
            {
                "XX", "2S", "3S", "4S", "5S", "6S", "7S", "8S", "9S", "XX",
                "6C", "5C", "4C", "3C", "2C", "AH", "KH", "QH", "TH", "TS",
                "7C", "AS", "2D", "3D", "4D", "5D", "6D", "7D", "9H", "QS",
                "8C", "KS", "6C", "5C", "4C", "3C", "2C", "8D", "8H", "KS",
                "9C", "QS", "7C", "6H", "5H", "4H", "AH", "9D", "7H", "AS",
                "TC", "TS", "8C", "7H", "2H", "3H", "KH", "TD", "6H", "2D",
                "QC", "9S", "9C", "8H", "9H", "TH", "QH", "QD", "5H", "3D",
                "KC", "8S", "TC", "QC", "KC", "AC", "AD", "KD", "4H", "4D",
                "AC", "7S", "6S", "5S", "4S", "3S", "2S", "2H", "3H", "5D",
                "XX", "AD", "KD", "QD", "TD", "9D", "8D", "7D", "6D", "XX"
            };

            byte i = 0;
            for (byte y = 0; y < 10; y++)
                for (byte x = 0; x < 10; x++)
                {
                    string c = tableBase[i++];
                    this.cells[x, y] = new SequenceTableCell(emoji.GetEmoji(new Card(c).EmoteName), c == "XX" ? SequenceGame.Color.Joker : SequenceGame.Color.None);
                }
        }
    }

    class SequenceTableCell
    {
        public GuildEmote CardEmote { get; set; }
        public SequenceGame.Color Color { get; set; }

        public SequenceTableCell(GuildEmote cardEmote, SequenceGame.Color color)
        {
            this.CardEmote = cardEmote;
            this.Color = color;
        }
    }
}
