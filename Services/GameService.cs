using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Until
{
    public class GameService
    {
        public List<Game> Games;

        public Game RunningGame(IInteractionContext ctx) => this.Games[this.Games.IndexOf(this.Games.Find(g => g.ChannelID == ctx.Channel.Id && g.Players.Contains(ctx.User.Id)))];

        public bool AlreadyPlaying(IInteractionContext ctx)
        {
            foreach (Game g in this.Games.Where(g => g.ChannelID == ctx.Channel.Id))
                if (g.Players.Contains(ctx.User.Id))
                    return true;
            return false;
        }

        public GameService()
        {
            this.Games = new List<Game>();
        }
    }

    public abstract class Game
    {
        public ulong ChannelID;
        public List<ulong> Players;

        public Game(ulong channelId)
        {
            this.ChannelID = channelId;
            this.Players = new List<ulong>();
        }
    }
}
