using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnInteraction(string player, IReadOnlyList<string> partners, bool actionsBetweenPartners);
}