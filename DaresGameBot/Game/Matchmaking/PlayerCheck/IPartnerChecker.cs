namespace DaresGameBot.Game.Matchmaking.PlayerCheck;

internal interface IPartnerChecker
{
    public bool WouldInteractWith(IPartnerChecker other);
}