using System.Collections.Generic;
using Discord;
using Until.Services;

namespace Until.Games
{
    public class SequenceGame : Services.Game
    {
        public SequenceTable Table;

        public SequenceGame(ulong channelId, ulong userId, EmojiService emojiService) : base(channelId, userId)
        {
            this.Table = new SequenceTable(emojiService);
        }
    }

    public class SequenceTable
    {
        private List<SequenceTableCell> cells;

        public override string ToString()
        {
            string output = "";
            byte i = 1;
            foreach (SequenceTableCell c in cells)
            {
                output += c.Card;
                if (i++ % 10 == 0)
                    output += "\n";
            }
            return output;
        }

        public SequenceTable(EmojiService emojiService)
        {
            this.cells = new List<SequenceTableCell>();

            string[] tableBase = new string[100]
            {
                "XX", "6D", "7D", "8D", "9D", "TD", "QD", "KD", "AD", "XX",
                "5D", "3H", "2H", "2S", "3S", "4S", "5S", "6S", "7S", "AC",
                "4D", "4H", "KD", "AD", "AC", "KC", "QC", "TC", "8S", "KC",
                "3D", "5H", "QD", "QH", "TH", "9H", "8H", "9C", "9S", "QC",
                "2D", "6H", "TD", "KH", "3H", "2H", "7H", "8C", "TS", "TC",
                "AS", "7H", "9D", "AH", "4H", "5H", "6H", "7C", "QS", "9C",
                "KC", "8H", "8D", "2C", "3C", "4C", "5C", "6C", "KS", "8C",
                "QC", "9H", "7D", "6D", "5D", "4D", "3D", "2D", "AS", "7C",
                "TC", "TH", "QH", "KH", "AH", "2C", "3C", "4C", "5C", "6C",
                "XX", "9C", "8C", "7C", "6C", "5C", "4C", "3C", "2C", "XX"
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
            for (byte x = 0; x < 10; x++)
                for (byte y = 0; y < 10; y++)
                {
                    string c = tableBase[i++];
                    string cf = faces[c[0]];
                    string cs = suits[c[1]];
                    this.cells.Add(new SequenceTableCell(x, y, emojiService.GetEmoji(cf + cs), cf == "X" ? Chip.Color.Joker : Chip.Color.None));
                }
        }
    }

    class SequenceTableCell
    {
        public GuildEmote Card;

        private byte x;
        private byte y;
        private Chip.Color color;

        public SequenceTableCell(byte x, byte y, GuildEmote card, Chip.Color color)
        {
            this.x = x;
            this.y = y;
            this.Card = card;
            this.color = color;
        }
    }

    public class Chip
    {
        public enum Color
        {
            None,
            Red,
            Green,
            Blue,
            Joker
        }
    }
}
