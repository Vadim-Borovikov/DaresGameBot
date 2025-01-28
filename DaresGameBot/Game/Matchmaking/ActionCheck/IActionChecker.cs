using DaresGameBot.Game.Data;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal interface IActionChecker
{
    public bool CanPlay(string player, ArrangementType arrangementType);
}