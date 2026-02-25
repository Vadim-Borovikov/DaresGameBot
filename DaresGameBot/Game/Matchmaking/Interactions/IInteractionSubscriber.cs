using DaresGameBot.Game.States;
using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnQuestionCompleted(string player, Arrangement? arrangement, List<string> activePlayers);
    public void OnActionCompleted(string player, Arrangement arrangement, List<string> activePlayers, string tag,
        bool fully);
}