namespace Until
{
    public class SequenceGame : Game
    {
        public enum Status
        {
            WaitingForPlayers
        }

        public Status CurrentStatus;

        public SequenceGame(ulong channelId, ulong userId) : base(channelId)
        {
            this.CurrentStatus = Status.WaitingForPlayers;
            this.Players.Add(userId);
        }
    }
}
