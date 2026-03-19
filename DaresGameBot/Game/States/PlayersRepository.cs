using System;
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
    public IEnumerable<string> GetActiveIds() => _idsOld.Where(n => _infosOld[n].Active);
    public IEnumerable<string> AllIds => _idsOld;

    public string Current => _idsOld[_currentIndex];

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

    public PlayersRepository(string playerFillNamePrefix) => _playerFillNamePrefix = playerFillNamePrefix;

    public IEnumerable<(string Id, bool Active)> GetAllIdsWithStatus()
    {
        foreach (string id in _idsOld)
        {
            yield return (id, _infosOld[id].Active);
        }
    }

    public PlayerInfo GetOrAddInfo(long id)
    {
        if (!_infos.ContainsKey(id))
        {
            _infos[id] = new PlayerInfo();
        }
        return _infos[id];
    }

    public bool IsNameVacant(string name, long exeptId)
    {
        return !_infos.Any(p => (p.Value.Name == name) && (p.Key != exeptId));
    }

    public bool AddOrUpdatePlayerData(AddOrUpdatePlayerData a, string handlerSeparator)
    {
        bool changed = false;
        string id = GetId(a.Name, a.Handler, handlerSeparator);
        if (_infosOld.ContainsKey(id))
        {
            if (_infosOld[id].GroupInfo != a.Info)
            {
                _infosOld[id].GroupInfo = a.Info;
                changed = true;
            }
            if (!_infosOld[id].Active)
            {
                _infosOld[id].Active = true;
                changed = true;
            }
        }
        else
        {
            _infosOld[id] = new PlayerInfo(a.Name, a.Info);
            changed = true;
        }

        if (!_idsOld.Contains(id))
        {
            _idsOld.Add(id);
        }

        return changed;
    }

    public string GetDisplayName(string id, bool activeOnly)
    {
        if (id.StartsWith(_playerFillNamePrefix, StringComparison.InvariantCulture))
        {
            return id.Substring(_playerFillNamePrefix.Length);
        }

        IEnumerable<string> players = activeOnly ? GetActiveIds() : _idsOld;
        return players.Count(i => _infosOld[i].Name == _infosOld[id].Name) > 1 ? id : _infosOld[id].Name;
    }

    public string GetDisplayName(string id) => GetDisplayName(id, true);

    public bool Toggle(string id)
    {
        if (!_infosOld.ContainsKey(id))
        {
            return false;
        }

        _infosOld[id].Active = !_infosOld[id].Active;
        return true;
    }

    public bool Select(string id)
    {
        if (!GetActiveIds().Contains(id) || (Current == id))
        {
            return false;
        }

        _currentIndex = _idsOld.IndexOf(id);
        return true;
    }

    public void DeactivateAll()
    {
        foreach (string id in _idsOld)
        {
            _infosOld[id].Active = false;
        }
    }

    public bool MoveDown(string id, bool toBottom, bool preserveCurrent)
    {
        int index = _idsOld.IndexOf(id);
        if (index == -1)
        {
            return false;
        }

        string currentPlayer = Current;

        if (toBottom)
        {
            if (_idsOld.Count < 2)
            {
                return false;
            }
            _idsOld.RemoveAt(index);
            _idsOld.Add(id);
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
            (_idsOld[index], _idsOld[newIndex.Value]) = (_idsOld[newIndex.Value], _idsOld[index]);
        }

        if (preserveCurrent)
        {
            _currentIndex = _idsOld.IndexOf(currentPlayer);
        }
        return true;
    }

    public PlayersRepositoryData Save()
    {
        return new PlayersRepositoryData
        {
            Ids = _idsOld,
            Infos = _infosOld.ToDictionary(i => i.Key, i => i.Value.Save()),
            CurrentIndex = _currentIndex
        };
    }

    public void LoadFrom(PlayersRepositoryData? data)
    {
        if (data is null)
        {
            return;
        }

        _idsOld.Clear();
        _idsOld.AddRange(data.Ids);

        _infosOld.Clear();
        foreach (string id in data.Infos.Keys)
        {
            PlayerData d = data.Infos[id];
            GroupsInfo i = new(d.GroupsData.Group, d.GroupsData.CompatableGroups);
            _infosOld[id] = new PlayerInfo(d.Name, i, d.Active);
        }

        _currentIndex = data.CurrentIndex;
    }

    public bool IsActive(string id) => _infosOld.ContainsKey(id) && _infosOld[id].Active;

    public bool IsIntercompatable(IReadOnlyList<string> group, ICompatibility compatibility)
    {
        return ListHelper.EnumeratePairs(group).All(pair => AreCompatable(pair, compatibility));
    }

    public bool AreCompatable(string p1, string p2, ICompatibility compatibility)
    {
        return (p1 != p2) && compatibility.AreCompatable(_infosOld[p1], _infosOld[p2]);
    }

    public bool IsCompatableWith(string player, IEnumerable<string> group, ICompatibility compatibility)
    {
        return group.All(p => AreCompatable(player, p, compatibility));
    }

    public bool AddOrUpdateGenderPlayerGender(long id, string gender)
    {
        HashSet<string> compatableGroups;
        if (_ids.Contains(id))
        {
            if (_infos[id].GroupInfo.Group == gender)
            {
                return false;
            }
            compatableGroups = _infos[id].GroupInfo.CompatableGroups;
        }
        else
        {
            _ids.Add(id);

            compatableGroups = new HashSet<string>();
        }
        GroupsInfo info = new(gender, compatableGroups);
        _infos[id] = new PlayerInfo(id.ToString(), info);
        return true;
    }

    public bool HasCompatableGroups(long id) => _infos.ContainsKey(id) && _infos[id].GroupInfo.CompatableGroups.Any();

    private static string GetId(string name, string? handler, string separator)
    {
        return string.IsNullOrEmpty(handler) ? name : $"{name}{separator}{handler}";
    }

    private bool AreCompatable((string, string) pair, ICompatibility compatibility)
    {
        return AreCompatable(pair.Item1, pair.Item2, compatibility);
    }

    private int GetNext(int index) => (index + 1) % _idsOld.Count;

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
        while (!_infosOld[_idsOld[index]].Active);
        return index;
    }

    private readonly List<long> _ids = new();
    private readonly Dictionary<long, PlayerInfo> _infos = new();

    private readonly List<string> _idsOld = new();
    private readonly Dictionary<string, PlayerInfo> _infosOld = new();
    private int _currentIndex;
    private readonly string _playerFillNamePrefix;
}