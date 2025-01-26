using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Data.PlayerListUpdates;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.Matchmaking.PlayerCheck;

namespace DaresGameBot.Game.Data;

internal sealed class PlayerRepository : ICompatibility, IInteractionSubscriber
{
    public IReadOnlyList<string> Names => _names.AsReadOnly();

    public string Current => _names[_currentIndex];

    public PlayerRepository(List<PlayerListUpdate> updates) => UpdateList(updates);

    public void MoveNext() => _currentIndex = (_currentIndex + 1) % _names.Count;

    public ushort UpdateList(List<PlayerListUpdate> updates)
    {
        ushort points = _infos.Count > 0 ? _infos.Values.Min(v => v.Points) : (ushort) 0;
        bool newPlayers = false;

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
                        newPlayers = true;
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

        return newPlayers ? points : (ushort) 0;
    }

    public void UpdateActions(ActionDeck actionDeck)
    {
        foreach (string name in _infos.Keys)
        {
            _infos[name].PlayableActions =
                new HashSet<ushort>(actionDeck.Cards
                                              .Keys
                                              .Where(id => actionDeck.Checker.CanPlay(name, actionDeck.Cards[id])));
        }
    }

    public IEnumerable<ushort> EnumerateBestIdsOf(IEnumerable<ushort> currentCardIds)
    {
        HashSet<ushort> possibleCardIds = new();
        possibleCardIds.UnionWith(currentCardIds);
        possibleCardIds.IntersectWith(_infos[Current].PlayableActions);

        if (possibleCardIds.Count == 0)
        {
            return possibleCardIds;
        }

        return possibleCardIds.GroupBy(id => _infos.Values.Count(i => i.PlayableActions.Contains(id)))
                              .OrderBy(g => g.Key)
                              .First();
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

    public void OnInteraction(string player, IReadOnlyList<string> partners, bool actionsBetweenPartners,
        ushort points)
    {
        _infos[player].Points += points;
        foreach (string partner in partners)
        {
            _infos[partner].Points += points;
        }
    }

    private readonly List<string> _names = new();
    private readonly Dictionary<string, PlayerInfo> _infos = new();
    private int _currentIndex;
}