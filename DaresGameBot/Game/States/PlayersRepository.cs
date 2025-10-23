using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using GryphonUtilities.Save;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.States;

internal sealed class PlayersRepository : IStateful<PlayersRepositoryData>
{
    public IEnumerable<string> GetActiveNames() => _names.Where(n => _infos[n].Active);
    public IEnumerable<string> AllNames => _names;

    public string Current => _names[_currentIndex];

    public void MoveNext()
    {
        do
        {
            _currentIndex = (_currentIndex + 1) % _names.Count;
        }
        while (!_infos[Current].Active);
    }

    public IEnumerable<(string Name, bool Active)> GetAllNamesWithStatus()
    {
        foreach (string name in _names)
        {
            yield return (name, _infos[name].Active);
        }
    }

    public bool AddOrUpdatePlayerData(AddOrUpdatePlayerData a)
    {
        bool changed = false;
        if (_infos.ContainsKey(a.Name))
        {
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
            _infos[a.Name] = new PlayerInfo(a.Info);
            changed = true;
        }

        if (_names.Contains(a.Name))
        {
            if (a.Index.HasValue)
            {
                changed |= MoveTo(a.Name, a.Index.Value);
            }
        }
        else
        {
            int index = a.Index.HasValue ? Math.Min(a.Index.Value, _names.Count) : _names.Count;
            _names.Insert(index, a.Name);
        }

        return changed;
    }

    public bool TogglePlayerData(TogglePlayerData data)
    {
        if (!_infos.ContainsKey(data.Name))
        {
            return false;
        }

        if (data.Index.HasValue)
        {
            MoveTo(data.Name, data.Index.Value);
        }

        if (_infos[data.Name].Active)
        {
            _infos[data.Name].Active = false;
            if (Current == data.Name)
            {
                MoveNext();
            }
        }
        else
        {
            _infos[data.Name].Active = true;
        }

        return true;
    }

    public PlayersRepositoryData Save()
    {
        return new PlayersRepositoryData
        {
            Names = _names,
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

        _names.Clear();
        _names.AddRange(data.Names);

        _infos.Clear();
        foreach (string name in data.Infos.Keys)
        {
            PlayerData d = data.Infos[name];
            GroupsInfo i = new(d.GroupsData.Group, d.GroupsData.CompatableGroups);
            _infos[name] = new PlayerInfo(i, d.Active);
        }

        _currentIndex = data.CurrentIndex;
    }

    public bool IsActive(string name) => _infos.ContainsKey(name) && _infos[name].Active;

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

    private bool MoveTo(string name, byte index)
    {
        int newIndex = Math.Min(index, _names.Count);
        if (newIndex == _names.IndexOf(name))
        {
            return false;
        }

        string currentPlayer = Current;

        _names.Insert(newIndex, name);

        int firstIndex = _names.IndexOf(name);
        int lastIndex = _names.LastIndexOf(name);
        _names.RemoveAt(firstIndex == newIndex ? lastIndex : firstIndex);

        _currentIndex = _names.IndexOf(currentPlayer);
        return true;
    }

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;
}