using DaresGameBot.Game;
using DaresGameBot.Game.States;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class UnervealCardData : GameButtonData
{
    public readonly Arrangement Arrangement;

    private UnervealCardData(Arrangement arrangement) => Arrangement = arrangement;

    public static UnervealCardData? From(string callbackQueryDataCore, PlayersRepository players)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);
        if (parts.Length != 2)
        {
            return null;
        }

        Arrangement? arrangement = TryGetArrangement(parts[0], parts[1], players);
        return arrangement is null ? null : new UnervealCardData(arrangement);
    }
}