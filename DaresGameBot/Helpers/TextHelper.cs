using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Helpers;

internal static class TextHelper
{
    public static string FormatAndJoin(IEnumerable<string> items, string format, string separator)
    {
        return string.Join(separator, items.Select(i => string.Format(format, i)));
    }
}