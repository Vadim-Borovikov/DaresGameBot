using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.ActionCheck;

internal interface IActionChecker
{
    public bool Check(Player player, CardAction action);
}