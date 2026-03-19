using DaresGameBot.Game.States;
using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnQuestionCompleted(long player, Arrangement? arrangement, List<long> activePlayers);
    public void OnActionCompleted(long player, Arrangement arrangement, List<long> activePlayers, string tag,
        bool fully);
}