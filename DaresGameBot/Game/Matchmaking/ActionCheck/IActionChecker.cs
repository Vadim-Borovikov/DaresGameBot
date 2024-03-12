using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Players;

namespace DaresGameBot.Game.Matchmaking.ActionCheck;

internal interface IActionChecker
{
    public bool CanPlay(Player player, CardAction action);
}