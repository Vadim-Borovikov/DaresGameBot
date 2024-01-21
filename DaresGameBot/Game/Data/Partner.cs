using System;

namespace DaresGameBot.Game.Data;

internal sealed class Partner : IComparable<Partner>
{
    public static string Choosable = "";

    public Partner(Player? player = null) => _player = player;

    public int CompareTo(Partner? other)
    {
        if (_player is null && other?._player is not null)
        {
            return 1;
        }
        if (_player is not null && other?._player is null)
        {
            return -1;
        }
        return 0;
    }

    public override string ToString() => _player?.Name ?? Choosable;

    private readonly Player? _player;
}