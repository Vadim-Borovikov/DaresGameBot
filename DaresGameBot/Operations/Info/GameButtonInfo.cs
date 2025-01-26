using System.Collections.Generic;
using System.Linq;
using DaresGameBot.Game.Data;

namespace DaresGameBot.Operations.Info;

internal abstract class GameButtonInfo
{
    public static GameButtonInfo? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(Separator);
        if ((parts.Length == 0) || (parts[0] == string.Empty))
        {
            return null;
        }

        switch (parts.Length)
        {
            case 1: return new GameButtonInfoQuestion(parts[0]);
            case < 4: return null;
            default:
                string tag = parts[0];

                ushort id = ushort.Parse(parts[1]);

                string player = parts[2];

                List<string> partners = parts.Skip(3).ToList();

                ActionInfo info = new(player, partners, id);

                return new GameButtonInfoAction(info, tag);
        }
    }

    internal const string Separator = ";";
}