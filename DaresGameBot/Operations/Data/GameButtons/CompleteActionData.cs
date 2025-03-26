namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class CompleteActionData : CompleteCardData
{
    public readonly bool CompletedFully;

    public CompleteActionData(ushort id, bool completedFully) : base(id) => CompletedFully = completedFully;
}