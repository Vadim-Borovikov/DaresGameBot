using DaresGameBot.Game;
using DaresGameBot.Game.States;
using DaresGameBot.Utilities.Extensions;
using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Data.GameButtons;

internal abstract class CompleteCardData : GameButtonData
{
    public readonly Arrangement? Arrangement;

    protected CompleteCardData(Arrangement? arrangement) => Arrangement = arrangement;

    public static CompleteCardData? From(string callbackQueryDataCore, PlayersRepository players)
    {
        string[] parts = callbackQueryDataCore.Split(FieldSeparator);

        ushort? id;
        switch (parts.Length)
        {
            case 1:
                id = parts[0].ToUshort();
                return id is null ? null : new CompleteQuestionData(id.Value);
            case < 3: return null;
        }

        Arrangement? arrangement = TryGetArrangement(parts[0], parts[1], players);
        if (arrangement is null)
        {
            return null;
        }

        id = parts[2].ToUshort();
        if (id is null)
        {
            return null;
        }

        switch (parts.Length)
        {
            case 3:
                return new CompleteQuestionData(id.Value, arrangement);
            case 4:
                ActionInfo actionInfo = new(id.Value, arrangement);

                bool? completedFully = parts[3].ToBool();
                return completedFully is null ? null : new CompleteActionData(actionInfo, completedFully.Value);
            default: return null;
        }
    }
}