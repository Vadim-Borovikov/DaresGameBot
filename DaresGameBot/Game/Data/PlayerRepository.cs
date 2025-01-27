using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerRepository : ICompatibility, IInteractionSubscriber
{
    public IReadOnlyList<string> GetNames() => _names.Where(n => _infos[n].Active).ToList().AsReadOnly();

    public string Current => _names[_currentIndex];

    public HashSet<int> PlayableArrangementsForCurrent => _infos[Current].PlayableArrangements;

    public ushort GetPoints(string name) => _infos[name].Points;

    public PlayerRepository(List<PlayerListUpdate> updates) => UpdateList(updates);

    public void MoveNext()
    {
        do
        {
            _currentIndex = (_currentIndex + 1) % _names.Count;
        }
        while (!_infos[Current].Active);
    }

    public void UpdateList(List<PlayerListUpdate> updates)
    {
        ushort points = _infos.Count > 0 ? _infos.Values.Where(v => v.Active).Min(v => v.Points) : (ushort) 0;

        foreach (PlayerListUpdate update in updates)
        {
            switch (update)
            {
                case AddOrUpdatePlayer a:
                    if (_infos.ContainsKey(a.Name))
                    {
                        _infos[a.Name].GroupChecker = a.Checker;
                        if (!_infos[a.Name].Active)
                        {
                            _infos[a.Name].Active = true;
                            _infos[a.Name].Points = ushort.Max(points, _infos[a.Name].Points);
                        }
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
                case TogglePlayer t:
                    if (_infos.ContainsKey(t.Name))
                    {
                        if (_infos[t.Name].Active)
                        {
                            _infos[t.Name].Active = false;
                            if (Current == t.Name)
                            {
                                MoveNext();
                            }
                        }
                        else
                        {
                            _infos[t.Name].Active = true;
                        }
                    }
                    break;
            }
        }
    }

    public void UpdateActions(ActionDeck actionDeck)
    {
        foreach (string name in _infos.Keys.Where(n => _infos[n].Active))
        {
            _infos[name].PlayableArrangements =
                new HashSet<int>(actionDeck.Cards
                                           .Values
                                           .Where(c => actionDeck.Checker.CanPlay(name, c.Arrangement))
                                           .Select(c => c.Arrangement.GetHashCode()));
        }
    }

    public bool AreCompatable(string p1, string p2)
    {
        if (p1 == p2)
        {
            return false;
        }

        if (!_infos[p1].Active || !_infos[p2].Active)
        {
            return false;
        }

        GroupChecker info1 = _infos[p1].GroupChecker;
        GroupChecker info2 = _infos[p2].GroupChecker;

        return info1.WouldInteractWith(info2) && info2.WouldInteractWith(info1);
    }

    public void OnInteraction(string player, IEnumerable<string> partners, bool actionsBetweenPartners, ushort points,
        IEnumerable<string> helpers, ushort helpPoints)
    {
        _infos[player].Points += points;
        foreach (string partner in partners)
        {
            _infos[partner].Points += points;
        }

        foreach (string helper in helpers)
        {
            _infos[helper].Points += helpPoints;
        }
    }

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;

}