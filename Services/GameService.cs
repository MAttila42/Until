using System.Collections.Generic;
using Discord;

namespace Until
{
    public class GameService
    {
        public List<Game> Games;

        public Game RunningGame(IInteractionContext ctx) => this.Games[this.Games.IndexOf(this.Games.Find(g => g.ChannelID == ctx.Channel.Id))];

        public GameService()
        {
            this.Games = new List<Game>();
        }
    }

    public class Game
    {
        public ulong ChannelID;
        public List<ulong> Players;
        public EmbedBuilder TempEmbed;

        public Game(ulong channelId, ulong userId = 0)
        {
            this.ChannelID = channelId;
            this.Players = new List<ulong>();
            if (userId != 0)
                this.Players.Add(userId);
        }
    }
}
