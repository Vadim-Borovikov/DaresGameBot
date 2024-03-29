using DaresGameBot.Game.Data.Cards;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal interface IActionChecker
{
    public bool CanPlay(string player, CardAction action);
}