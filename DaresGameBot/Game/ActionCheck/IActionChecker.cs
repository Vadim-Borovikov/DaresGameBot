using DaresGameBot.Game.Data;
using DaresGameBot.Game.Data.Cards;

namespace DaresGameBot.Game.ActionCheck;

internal interface IActionChecker
{
    public bool Check(Player player, CardAction card);
}