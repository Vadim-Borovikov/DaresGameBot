using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Matchmaking.PlayerCheck;
using DaresGameBot.Operations.Data.PlayerListUpdates;

namespace DaresGameBot.Game.Players;

internal sealed class Repository : ICompatibility
{
    public IReadOnlyList<string> GetNames() => _names.Where(n => _infos[n].Active).ToList().AsReadOnly();

    public string Current => _names[_currentIndex];

    public ushort GetPoints(string name) => _infos[name].Points;
    public void AddPoints(string name, ushort points) => _infos[name].Points += points;

    public Repository(List<PlayerListUpdateData> updates) => UpdateList(updates);

    public void MoveNext()
    {
        do
        {
            _currentIndex = (_currentIndex + 1) % _names.Count;
        }
        while (!_infos[Current].Active);
    }

    public bool UpdateList(List<PlayerListUpdateData> updateDatas)
    {
        ushort points = _infos.Count > 0 ? _infos.Values.Where(v => v.Active).Min(v => v.Points) : (ushort)0;
        bool changed = false;

        foreach (PlayerListUpdateData data in updateDatas)
        {
            switch (data)
            {
                case AddOrUpdatePlayerData a:
                    if (_infos.ContainsKey(a.Name))
                    {
                        if (_infos[a.Name].GroupChecker != a.Checker)
                        {
                            _infos[a.Name].GroupChecker = a.Checker;
                            changed = true;
                        }
                        if (!_infos[a.Name].Active)
                        {
                            _infos[a.Name].Active = true;
                            _infos[a.Name].Points = ushort.Max(points, _infos[a.Name].Points);
                            changed = true;
                        }
                    }
                    else
                    {
                        _infos[a.Name] = new PlayerInfo(a.Checker, points);
                        changed = true;
                    }

                    if (!_names.Contains(a.Name))
                    {
                        _names.Add(a.Name);
                    }
                    break;
                case TogglePlayerData t:
                    if (!_infos.ContainsKey(t.Name))
                    {
                        break;
                    }

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
                    changed = true;

                    break;
            }
        }

        return changed;
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

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;
}