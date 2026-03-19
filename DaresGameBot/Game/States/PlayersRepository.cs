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
    public IEnumerable<long> GetActiveIds() => _ids.Where(n => _infos[n].Active);
    public IEnumerable<long> AllIds => _ids;

    public long Current => _ids[_currentIndex];

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

    public IEnumerable<(long Id, string Name, bool Active)> GetAllIdsWithNamesAndStatuses()
    {
        foreach (long id in _ids)
        {
            yield return (id, _infos[id].Name, _infos[id].Active);
        }
    }

    public bool AddOrUpdatePlayerData(AddOrUpdatePlayerData a)
    {
        bool changed = false;
        if (_infos.ContainsKey(a.Id))
        {
            if (_infos[a.Id].GroupInfo != a.Info)
            {
                _infos[a.Id].GroupInfo = a.Info;
                changed = true;
            }
            if (!_infos[a.Id].Active)
            {
                _infos[a.Id].Active = true;
                changed = true;
            }
        }
        else
        {
            _infos[a.Id] = new PlayerInfo(a.Name, a.Info);
            changed = true;
        }

        if (!_ids.Contains(a.Id))
        {
            _ids.Add(a.Id);
        }

        return changed;
    }

    public string GetDisplayName(long id) => _infos[id].Name;

    public bool Toggle(long id)
    {
        if (!_infos.ContainsKey(id))
        {
            return false;
        }

        _infos[id].Active = !_infos[id].Active;
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
        foreach (long id in data.Infos.Keys)
        {
            PlayerData d = data.Infos[id];
            GroupsInfo i = new(d.GroupsData.Group, d.GroupsData.CompatableGroups);
            _infos[id] = new PlayerInfo(d.Name, i, d.Active);
        }

        _currentIndex = data.CurrentIndex;
    }

    public bool IsActive(long id) => _infos.ContainsKey(id) && _infos[id].Active;

    public bool IsIntercompatable(IReadOnlyList<long> group, ICompatibility compatibility)
    {
        return ListHelper.EnumeratePairs(group).All(pair => AreCompatable(pair, compatibility));
    }

    public bool AreCompatable(long p1, long p2, ICompatibility compatibility)
    {
        return (p1 != p2) && compatibility.AreCompatable(_infos[p1], _infos[p2]);
    }

    public bool IsCompatableWith(long player, IEnumerable<long> group, ICompatibility compatibility)
    {
        return group.All(p => AreCompatable(player, p, compatibility));
    }

    private bool AreCompatable((long, long) pair, ICompatibility compatibility)
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

    private readonly List<long> _ids = new();
    private readonly Dictionary<long, PlayerInfo> _infos = new();
    private int _currentIndex;
}