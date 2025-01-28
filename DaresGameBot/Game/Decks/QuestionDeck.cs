using System.Collections.Generic;
using DaresGameBot.Game.Data;
using GryphonUtilities.Extensions;

namespace DaresGameBot.Game.Decks;

internal sealed class QuestionDeck : Deck<CardData>
{
    public QuestionDeck(IReadOnlyList<CardData> cards) : base(cards) { }

    public CardData Draw()
    {
        ushort id = GetRandomId().Denull("No question found!");
        CardData card = GetCard(id);
        Mark(id);
        return card;
    }
}