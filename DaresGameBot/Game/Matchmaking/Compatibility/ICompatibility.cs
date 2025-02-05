using DaresGameBot.Game.Players;

namespace DaresGameBot.Game.Matchmaking.Compatibility;

internal interface ICompatibility
{
    bool AreCompatable(PlayerInfo p1, PlayerInfo p2);
}