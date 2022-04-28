using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Until.Services;
using Until.Games;

namespace Until.Commands
{
    public class Cards : InteractionModuleBase
    {
        public EmojiService _emoji { get; set; }
        public GameService _game { get; set; }

        [SlashCommand("cards", "Check the cards in your hand.")]
        public async Task Run()
        {
            try
            {
                var game = _game.GetGameByContextPlayer(Context);
                switch (game)
                {
                    case SequenceGame g:
                        List<string> hand = ((SequencePlayer)g.Players.Find(p => p.ID == Context.User.Id)).HeldCardNames;
                        if (hand.Count == 0)
                            throw new ArgumentException("Player has no cards in their hand!");
                        StringBuilder sb = new StringBuilder();
                        foreach (var c in hand)
                            sb.Append(_emoji.GetEmoji(c));
                        await RespondAsync(sb.ToString(), ephemeral: true);
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch (Exception)
            {
                await RespondAsync(embed: EmbedService.Error("You don't have any cards!"), ephemeral: true);
            }
        }
    }
}
