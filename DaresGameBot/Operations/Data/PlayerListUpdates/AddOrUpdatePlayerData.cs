using System.Collections.Generic;
using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.PlayerListUpdates;

internal sealed class AddOrUpdatePlayerData
{
    public readonly string Name;
    public readonly string? Username;
    public readonly HashSet<byte> Rounds;
    public readonly GroupsInfo Info;

    public AddOrUpdatePlayerData(string name, string? username, HashSet<byte> rounds, GroupsInfo info)
    {
        Name = name;
        Username = username;
        Rounds = rounds;
        Info = info;
    }
}