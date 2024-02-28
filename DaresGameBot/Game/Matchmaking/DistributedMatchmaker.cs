using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Helpers;
using DaresGameBot.Game.Data.Players;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Matchmaking;

internal sealed class DistributedMatchmaker : Matchmaker
{
    public DistributedMatchmaker(Compatibility compatibility) : base(compatibility) { }

    public bool AreThereAnyMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        List<Player> choices = EnumerateCompatiblePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return false;
        }

        return !compatableWithEachOther || EnumerateIntercompatibleGroups(choices, amount).Any();
    }

    public IEnumerable<Player>? EnumerateMatches(Player player, IEnumerable<Player> all, byte amount,
        bool compatableWithEachOther)
    {
        List<Player> choices = EnumerateCompatiblePlayers(player, all).ToList();
        if (choices.Count < amount)
        {
            return null;
        }

        if (compatableWithEachOther)
        {
            IEnumerable<IReadOnlyList<Player>> groups = EnumerateIntercompatibleGroups(choices, amount);
            List<IReadOnlyList<Player>> bestGroups = groups.GroupBy(g => GetActionsPerformed(player, g))
                                                           .OrderBy(g => g.Key)
                                                           .First()
                                                           .ToList();
            return RandomHelper.SelectItem(bestGroups);
        }

        List<Player> bestChoices = new();
        while (bestChoices.Count < amount)
        {
            int toAdd = amount - bestChoices.Count;

            List<Player> batch = choices.GroupBy(c => GetActionsPerformed(player, c))
                                        .OrderBy(g => g.Key)
                                        .First()
                                        .ToList();

            if (batch.Count <= toAdd)
            {
                bestChoices.AddRange(batch);
                foreach (Player p in batch)
                {
                    choices.Remove(p);
                }
                continue;
            }

            IEnumerable<Player> selection = RandomHelper.EnumerateUniqueItems(batch, toAdd).Denull("Logic error");
            bestChoices.AddRange(selection);
            break;
        }
        return bestChoices;
    }

    public void RegisterActions(Player player, IReadOnlyList<Player> partners, bool actionsBetweenPartners)
    {
        foreach (Player p in partners)
        {
            RegisterAction(player, p);
        }

        if (!actionsBetweenPartners)
        {
            return;
        }
        foreach ((Player, Player) pair in ListHelper.EnumeratePairs(partners))
        {
            RegisterAction(pair.Item1, pair.Item2);
        }
    }

    private void RegisterAction(Player p1, Player p2)
    {
        int hash = GetHash(p1, p2);
        if (_actionsPerformed.ContainsKey(hash))
        {
            ++_actionsPerformed[hash];
        }
        else
        {
            _actionsPerformed[hash] = 1;
        }
    }

    private ushort GetActionsPerformed(Player player, IReadOnlyList<Player> players)
    {
        ushort result = 0;

        foreach (Player p in players)
        {
            result += GetActionsPerformed(player, p);
        }

        foreach ((Player, Player) pair in ListHelper.EnumeratePairs(players))
        {
            result += GetActionsPerformed(pair.Item1, pair.Item2);
        }

        return result;
    }

    private ushort GetActionsPerformed(Player p1, Player p2)
    {
        int hash = GetHash(p1, p2);
        return _actionsPerformed.GetValueOrDefault(hash);
    }

    private static int GetHash(Player p1, Player p2) => p1.Name.GetHashCode() ^ p2.Name.GetHashCode();

    private readonly Dictionary<int, ushort> _actionsPerformed = new();
}