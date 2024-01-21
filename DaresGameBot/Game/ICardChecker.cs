using DaresGameBot.Game.Data;

namespace DaresGameBot.Game;

internal interface ICardChecker<in T> where T : Card
{
    Turn? TryGetTurn(Player player, T card);
}