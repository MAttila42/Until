using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Until.Services
{
    public class GameService
    {
        public List<Game> Games;

        public Game WaitingGame(IInteractionContext ctx)
        {
            List<Game> games = this.Games.Where(g => g.ChannelID == ctx.Channel.Id).ToList();
            if (games.Count() > 1)
            {
                this.Games.RemoveAll(g => g.ChannelID == ctx.Channel.Id && g.Players.Count(p => p.ID == ctx.User.Id) == 1);
                games.ForEach(g => this.Games.Add(g));
            }
            return games.Last();
        }

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
        public EmbedBuilder TempEmbed;

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
}
