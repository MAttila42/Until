using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Until
{
    public class GameService
    {
        public List<Game> Games;

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

    public class SequenceGame : Game
    {
        public enum Status
        {
            WaitingForPlayers
        }

        public Status CurrentStatus;

        public SequenceGame(ulong channelId) : base(channelId)
        {
            this.CurrentStatus = Status.WaitingForPlayers;
        }
    }
}
