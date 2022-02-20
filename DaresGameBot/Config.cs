using AbstractBot;

namespace DaresGameBot;

public sealed class Config : ConfigGoogleSheets
{
    public readonly ushort InitialPlayersAmount;
    public readonly float InitialChoiceChance;
    public readonly string ActionsGoogleRange;
    public readonly string QuestionsGoogleRange;

    public Config(string token, string systemTimeZoneId, string dontUnderstandStickerFileId,
        string forbiddenStickerFileId, string googleCredentialJson, string applicationName, string googleSheetId,
        ushort initialPlayersAmount, float initialChoiceChance, string actionsGoogleRange,
        string questionsGoogleRange)
        : base(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId, googleCredentialJson,
            applicationName, googleSheetId)
    {
        InitialPlayersAmount = initialPlayersAmount;
        InitialChoiceChance = initialChoiceChance;
        ActionsGoogleRange = actionsGoogleRange;
        QuestionsGoogleRange = questionsGoogleRange;
    }
}
