namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnInteractionPurposed(string player, Arrangement arrangement) { }

    public void OnInteractionCompleted(string player, Arrangement arrangement, string tag, bool completedFully) { }
}