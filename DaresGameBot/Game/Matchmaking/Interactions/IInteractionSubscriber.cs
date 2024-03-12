using System.Collections.Generic;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnInteraction(Player player, IReadOnlyList<Player> partners, bool actionsBetweenPartners);
}