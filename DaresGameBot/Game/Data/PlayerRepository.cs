using System.Collections.Generic;
using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerRepository
{
    public readonly Compatibility Compatibility = new();

    public IReadOnlyList<string> Names => _names.AsReadOnly();

    public string Current => _names[_currentIndex];

    public PlayerRepository(List<PlayerListUpdate> updates) => Update(updates);

    public void MoveNext() => _currentIndex = (_currentIndex + 1) % _names.Count;

    public void Update(List<PlayerListUpdate> updates)
    {
        foreach (PlayerListUpdate update in updates)
        {
            switch (update)
            {
                case AddOrUpdatePlayer a:
                    Compatibility[a.Name] = a.Checker;
                    if (!_names.Contains(a.Name))
                    {
                        _names.Add(a.Name);
                    }
                    break;
                case RemovePlayer r:
                    int index = _names.IndexOf(r.Name);
                    if (index > -1)
                    {
                        _names.RemoveAt(index);

                        if (_currentIndex > index)
                        {
                            --_currentIndex;
                        }
                    }
                    Compatibility.Remove(r.Name);
                    break;
            }
        }
    }

    private readonly List<string> _names = new();
    private int _currentIndex;
}