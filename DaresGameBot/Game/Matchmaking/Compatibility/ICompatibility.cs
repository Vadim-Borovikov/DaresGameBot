using DaresGameBot.Game.States;

namespace DaresGameBot.Game.Matchmaking.Compatibility;

internal interface ICompatibility
{
    bool AreCompatable(PlayerInfo p1, PlayerInfo p2);
}