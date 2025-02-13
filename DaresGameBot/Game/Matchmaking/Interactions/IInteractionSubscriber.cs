namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnQuestionCompleted(string player, Arrangement? declinedArrangement);
    public void OnActionCompleted(string player, ActionInfo info, bool fully);
}