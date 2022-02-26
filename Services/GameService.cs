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
