using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.Data.Players;

internal sealed class PlayerRepository : List<Player>
{
    public Player Current => this[_currentIndex];

    public PlayerRepository(IEnumerable<Player> players) : base(players) => _currentIndex = 0;

    public IEnumerable<string> EnumerateNames() => this.Select(p => p.Name);

    public void MoveNext() => _currentIndex = (_currentIndex + 1) % Count;

    private int _currentIndex;
}