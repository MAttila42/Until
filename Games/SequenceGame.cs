using System.Collections.Generic;
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

        public SequenceTable Table;

        public SequenceGame(ulong channelId, ulong userId, EmojiService emojiService) : base(channelId)
        {
            this.Players.Add(new SequencePlayer(userId));
            this.Table = new SequenceTable(emojiService);
        }
    }

    public class SequencePlayer : Player
    {
        private SequenceGame.Color color;

        public SequencePlayer(ulong userId) : base(userId)
        {
            this.color = SequenceGame.Color.None;
        }
    }

    public class SequenceTable
    {
        private SequenceTableCell[,] cells;

        public FileAttachment ToImage(EmojiService emojiService)
        {
            SKSurface tempSurface = SKSurface.Create(new SKImageInfo(640, 766));
            SKCanvas canvas = tempSurface.Canvas;
            canvas.Clear(SKColors.Transparent);

            for (byte y = 0; y < 10; y++)
                for (byte x = 0; x < 10; x++)
                    canvas.DrawBitmap(emojiService.GetImage(cells[x, y].Card.Name).Resize(new SKImageInfo(64, 64), SKFilterQuality.Low), SKRect.Create(x * 64, y * 78, 64, 64));

            return new FileAttachment(tempSurface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream(), "Sequence.png");
        }

        public SequenceTable(EmojiService emojiService)
        {
            this.cells = new SequenceTableCell[10, 10];

            string[] tableBase = new string[100]
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

            Dictionary<char, string> faces = new Dictionary<char, string>();
            faces.Add('A', "ace");
            faces.Add('2', "two");
            faces.Add('3', "three");
            faces.Add('4', "four");
            faces.Add('5', "five");
            faces.Add('6', "six");
            faces.Add('7', "seven");
            faces.Add('8', "eight");
            faces.Add('9', "nine");
            faces.Add('T', "ten");
            faces.Add('J', "jack");
            faces.Add('Q', "queen");
            faces.Add('K', "king");
            faces.Add('X', "joker");

            Dictionary<char, string> suits = new Dictionary<char, string>();
            suits.Add('C', "_of_clubs");
            suits.Add('S', "_of_spades");
            suits.Add('H', "_of_hearts");
            suits.Add('D', "_of_diamonds");
            suits.Add('X', "");

            byte i = 0;
            for (byte y = 0; y < 10; y++)
                for (byte x = 0; x < 10; x++)
                {
                    string c = tableBase[i++];
                    string cf = faces[c[0]];
                    string cs = suits[c[1]];
                    this.cells[x, y] = new SequenceTableCell(x, y, emojiService.GetEmoji(cf + cs), cf == "X" ? SequenceGame.Color.Joker : SequenceGame.Color.None);
                }
        }
    }

    class SequenceTableCell
    {
        public byte X;
        public byte Y;
        public GuildEmote Card;

        private SequenceGame.Color color;

        public SequenceTableCell(byte x, byte y, GuildEmote card, SequenceGame.Color color)
        {
            this.X = x;
            this.Y = y;
            this.Card = card;
            this.color = color;
        }
    }
}
