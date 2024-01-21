using System.Collections.Generic;

namespace DaresGameBot.Operations.Info;

internal sealed class PlayersInfo
{
    public readonly IEnumerable<string> Names;
    public PlayersInfo(IEnumerable<string> names) => Names = names;
}