using System.Collections.Generic;
using GryphonUtilities;

namespace DaresGameBot.Game.Data;

internal class Card
{
    public readonly string Description;

    protected Card(string description) => Description = description;

    public static Card Load(IDictionary<string, object?> valueSet)
    {
        string description = GetDescription(valueSet);
        return new Card(description);
    }

    protected static string GetDescription(IDictionary<string, object?> valueSet)
    {
        string? description = valueSet[DescriptionTitle]?.ToString();
        return description.GetValue(nameof(description));
    }

    private const string DescriptionTitle = "Текст";
}
