namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnInteraction(string player, Arrangement arrangement, string tag);
}