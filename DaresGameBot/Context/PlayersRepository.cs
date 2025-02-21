using System.Collections.Generic;
using System.Linq;
using AbstractBot;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Save;

namespace DaresGameBot.Context;

internal sealed class PlayersRepository : IContext<PlayersRepository, PlayersRepositoryData, object>
{
    public IEnumerable<string> GetActiveNames() => _names.Where(n => _infos[n].Active);
    public IEnumerable<string> AllNames => _names;

    public string Current => _names[_currentIndex];

    public PlayersRepository() { }

    private PlayersRepository(List<string> names, Dictionary<string, PlayerInfo> infos, int currentIndex)
    {
        _names = names;
        _infos = infos;
        _currentIndex = currentIndex;
    }

    public void MoveNext()
    {
        do
        {
            _currentIndex = (_currentIndex + 1) % _names.Count;
        }
        while (!_infos[Current].Active);
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

        if (!_names.Contains(a.Name))
        {
            _names.Add(a.Name);
        }

        return changed;
    }

    public bool TogglePlayerData(TogglePlayerData data)
    {
        if (!_infos.ContainsKey(data.Name))
        {
            return false;
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

    public static PlayersRepository Load(PlayersRepositoryData data, object? meta)
    {
        Dictionary<string, PlayerInfo> infos =
            data.Infos.ToDictionary(i => i.Key, i => PlayerInfo.Load(i.Value, meta));
        return new PlayersRepository(data.Names, infos, data.CurrentIndex);
    }

    public bool AreCompatable(string p1, string p2, ICompatibility compatibility)
    {
        return (p1 != p2) && compatibility.AreCompatable(_infos[p1], _infos[p2]);
    }

    public bool AreCompatable(IReadOnlyList<string> players, ICompatibility compatibility)
    {
        return ListHelper.EnumeratePairs(players).All(p => AreCompatable(p, compatibility));
    }

    private bool AreCompatable((string, string) pair, ICompatibility compatibility)
    {
        return AreCompatable(pair.Item1, pair.Item2, compatibility);
    }

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;
}