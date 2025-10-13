using DaresGameBot.Game.States;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnQuestionCompleted(string player, Arrangement? arrangement);
    public void OnActionCompleted(string player, Arrangement arrangement, string tag, bool fully);
}