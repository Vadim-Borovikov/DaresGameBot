using System.Collections.Generic;
using GoogleSheetsManager;

namespace DaresGameBot.Game.Data;

internal sealed class CardAction : Card
{
    public readonly ushort Players;
    public readonly ushort PartnersToAssign;
    public readonly string Tag;

    private CardAction(string description, ushort players, ushort partnersToAssign, string tag) : base(description)
    {
        Players = players;
        PartnersToAssign = partnersToAssign;
        Tag = tag;
    }

    public new static CardAction Load(IDictionary<string, object?> valueSet)
    {
        string description = GetDescription(valueSet);

        ushort players = valueSet[PlayersTitle].ToUshort().GetValue("Empty players");
        ushort partnersToAssign = valueSet[PartnersToAssignTitle].ToUshort().GetValue("Empty players to assign");

        string? tag = valueSet[TagTitle]?.ToString();
        string tagValue = tag.GetValue("Empty card tag");

        return new CardAction(description, players, partnersToAssign, tagValue);
    }

    private const string PlayersTitle = "Минимум";
    private const string PartnersToAssignTitle = "Назначить";
    private const string TagTitle = "Символ";
}
