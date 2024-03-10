namespace DaresGameBot.Game.Matchmaking;

internal interface IInteractabilityProvider
{
    public bool WouldInteractWith(IInteractabilityProvider other);
}