using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnInteraction(string player, IEnumerable<string> partners, bool actionsBetweenPartners, ushort points,
        IEnumerable<string> helpers, ushort helpPoints);
}