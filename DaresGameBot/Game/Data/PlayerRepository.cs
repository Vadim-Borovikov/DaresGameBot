using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerRepository : ICompatibility, IInteractionSubscriber
{
    public IReadOnlyList<string> Names => _names.AsReadOnly();

    public string Current => _names[_currentIndex];

    public PlayerRepository(List<PlayerListUpdate> updates, Dictionary<string, ushort> points)
    {
        _points = points;
        Update(updates);
    }

    public void MoveNext() => _currentIndex = (_currentIndex + 1) % _names.Count;

    public void Update(List<PlayerListUpdate> updates)
    {
        ushort points = _infos.Count > 0 ? _infos.Values.Min(v => v.Points) : (ushort) 0;

        foreach (PlayerListUpdate update in updates)
        {
            switch (update)
            {
                case AddOrUpdatePlayer a:
                    if (_infos.ContainsKey(a.Name))
                    {
                        _infos[a.Name].GroupChecker = a.Checker;
                    }
                    else
                    {
                        _infos[a.Name] = new PlayerInfo(a.Checker, points);
                    }

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
                    _infos.Remove(r.Name);
                    break;
            }
        }
    }

    public bool AreCompatable(string p1, string p2)
    {
        if (p1 == p2)
        {
            return false;
        }

        GroupChecker info1 = _infos[p1].GroupChecker;
        GroupChecker info2 = _infos[p2].GroupChecker;

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public void OnInteraction(string player, IReadOnlyList<string> partners, bool actionsBetweenPartners, string deck)
    {
        _infos[player].Points += _points[deck];
        foreach (string partner in partners)
        {
            _infos[partner].Points += _points[deck];
        }
    }

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;

    private readonly Dictionary<string, ushort> _points;
}