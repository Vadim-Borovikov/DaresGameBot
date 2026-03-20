using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using GryphonUtilities.Save;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.States;

internal sealed class PlayersRepository : IStateful<PlayersRepositoryData>
{
    public IEnumerable<string> GetActiveIds() => _ids.Where(n => _infos[n].Active);
    public IEnumerable<string> AllIds => _ids;

    public string Current => _ids[_currentIndex];

    public bool MoveNext()
    {
        int? next = GetNextActive(_currentIndex);
        if (next is null || (next == _currentIndex))
        {
            return false;
        }
        _currentIndex = next.Value;
        return true;
    }

    public IEnumerable<(string Id, PlayerInfo Info)> GetAllIdsWithInfo()
    {
        foreach (string id in _ids)
        {
            yield return (id, _infos[id]);
        }
    }

    public bool AddOrUpdatePlayerData(AddOrUpdatePlayerData a)
    {
        bool changed = false;
        if (_infos.ContainsKey(a.Name))
        {
            if (!_infos[a.Name].Rounds.SetEquals(a.Rounds))
            {
                _infos[a.Name].Rounds = a.Rounds;
                changed = true;
            }
            if (_infos[a.Name].GroupInfo != a.Info)
            {
                _infos[a.Name].GroupInfo = a.Info;
                changed = true;
            }
            if (!_infos[a.Name].Active)
            {
                _infos[a.Name].Active = true;
                changed = true;
            }
        }
        else
        {
            _infos[a.Name] = new PlayerInfo(a.Username, a.Rounds, a.Info);
            changed = true;
        }

        if (!_ids.Contains(a.Name))
        {
            _ids.Add(a.Name);
        }

        return changed;
    }

    public bool Toggle(string id)
    {
        if (!_infos.ContainsKey(id))
        {
            return false;
        }

        _infos[id].Active = !_infos[id].Active;
        return true;
    }

    public bool Select(string id)
    {
        if (!GetActiveIds().Contains(id) || (Current == id))
        {
            return false;
        }

        _currentIndex = _ids.IndexOf(id);
        return true;
    }

    public bool MoveDown(string id, bool toBottom, bool preserveCurrent)
    {
        int index = _ids.IndexOf(id);
        if (index == -1)
        {
            return false;
        }

        string currentPlayer = Current;

        if (toBottom)
        {
            if (_ids.Count < 2)
            {
                return false;
            }
            _ids.RemoveAt(index);
            _ids.Add(id);
        }
        else
        {
            List<string> activeIds = GetActiveIds().ToList();
            if (!activeIds.Contains(id) || (activeIds.Count < 2))
            {
                return false;
            }
            int? newIndex = GetNextActive(index);
            if (newIndex is null)
            {
                return false;
            }
            (_ids[index], _ids[newIndex.Value]) = (_ids[newIndex.Value], _ids[index]);
        }

        if (preserveCurrent)
        {
            _currentIndex = _ids.IndexOf(currentPlayer);
        }
        return true;
    }

    public PlayersRepositoryData Save()
    {
        return new PlayersRepositoryData
        {
            Ids = _ids,
            Infos = _infos.ToDictionary(i => i.Key, i => i.Value.Save()),
            CurrentIndex = _currentIndex
        };
    }

    public void LoadFrom(PlayersRepositoryData? data)
    {
        if (data is null)
        {
            return;
        }

        _ids.Clear();
        _ids.AddRange(data.Ids);

        _infos.Clear();
        foreach (string id in data.Infos.Keys)
        {
            PlayerData d = data.Infos[id];
            _infos[id] = new PlayerInfo(d);
        }

        _currentIndex = data.CurrentIndex;
    }

    public bool IsActive(string id) => _infos.ContainsKey(id) && _infos[id].Active;

    public bool IsIntercompatable(IReadOnlyList<string> group, ICompatibility compatibility)
    {
        return ListHelper.EnumeratePairs(group).All(pair => AreCompatable(pair, compatibility));
    }

    public bool AreCompatable(string p1, string p2, ICompatibility compatibility)
    {
        return (p1 != p2) && compatibility.AreCompatable(_infos[p1], _infos[p2]);
    }

    public bool IsCompatableWith(string player, IEnumerable<string> group, ICompatibility compatibility)
    {
        return group.All(p => AreCompatable(player, p, compatibility));
    }

    private bool AreCompatable((string, string) pair, ICompatibility compatibility)
    {
        return AreCompatable(pair.Item1, pair.Item2, compatibility);
    }

    private int GetNext(int index) => (index + 1) % _ids.Count;

    private int? GetNextActive(int index)
    {
        if (!GetActiveIds().Any())
        {
            return null;
        }

        do
        {
            index = GetNext(index);
        }
        while (!_infos[_ids[index]].Active);
        return index;
    }

    private readonly List<string> _ids = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;
}