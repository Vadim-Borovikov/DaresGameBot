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

    public void MoveNext() => _currentIndex = GetNextActive(_currentIndex);

    public IEnumerable<(string Id, bool Active)> GetAllIdsWithStatus()
    {
        foreach (string id in _ids)
        {
            yield return (id, _infos[id].Active);
        }
    }

    public bool AddOrUpdatePlayerData(AddOrUpdatePlayerData a, string handlerSeparator)
    {
        bool changed = false;
        string id = GetId(a.Name, a.Handler, handlerSeparator);
        if (_infos.ContainsKey(id))
        {
            if (_infos[id].GroupInfo != a.Info)
            {
                _infos[id].GroupInfo = a.Info;
                changed = true;
            }
            if (!_infos[id].Active)
            {
                _infos[id].Active = true;
                changed = true;
            }
        }
        else
        {
            _infos[id] = new PlayerInfo(a.Name, a.Info);
            changed = true;
        }

        if (!_ids.Contains(id))
        {
            _ids.Add(id);
        }

        return changed;
    }

    public string GetDisplayName(string id)
    {
        return GetActiveIds().Count(activeId => _infos[activeId].Name == _infos[id].Name) > 1 ? id : _infos[id].Name;
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
            int newIndex = GetNextActive(index);
            (_ids[index], _ids[newIndex]) = (_ids[newIndex], _ids[index]);
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
            GroupsInfo i = new(d.GroupsData.Group, d.GroupsData.CompatableGroups);
            _infos[id] = new PlayerInfo(d.Name, i, d.Active);
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

    private static string GetId(string name, string? handler, string separator)
    {
        return string.IsNullOrEmpty(handler) ? name : $"{name}{separator}{handler}";
    }

    private bool AreCompatable((string, string) pair, ICompatibility compatibility)
    {
        return AreCompatable(pair.Item1, pair.Item2, compatibility);
    }

    private int GetNext(int index) => (index + 1) % _ids.Count;

    private int GetNextActive(int index)
    {
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