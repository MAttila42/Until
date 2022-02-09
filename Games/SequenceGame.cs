using System.Collections.Generic;
using Until.Services;

namespace Until.Games
{
    public class SequenceGame : Game
    {
        private Table table;

        public SequenceGame(ulong channelId, ulong userId) : base(channelId, userId) { }
    }

    class Table
    {
        private List<Cell> cells;

        public Table()
        {
            this.cells = new List<Cell>();

            for (byte x = 0; x < 10; x++)
                for (byte y = 0; y < 10; y++)
                    this.cells.Add(new Cell(x, y));
        }
    }

    class Cell
    {
        private byte x;
        private byte y;

        public Cell(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
