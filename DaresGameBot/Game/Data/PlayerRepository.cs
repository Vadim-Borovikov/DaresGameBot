using System.Collections.Generic;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerRepository : List<string>
{
    public string Current => this[_currentIndex];

    public void ResetWith(IEnumerable<string> players)
    {
        Clear();
        AddRange(players);
        _currentIndex = 0;
    }

    public void MoveNext() => _currentIndex = (_currentIndex + 1) % Count;

    private int _currentIndex;
}