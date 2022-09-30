using AbstractBot;
using System;

namespace DaresGameBot;

public sealed class Config : ConfigGoogleSheets
{
    public readonly ushort InitialPlayersAmount;
    public readonly float InitialChoiceChance;
    public readonly string ActionsGoogleRange;
    public readonly string QuestionsGoogleRange;

    public Config(string token, string systemTimeZoneId, string dontUnderstandStickerFileId,
        string forbiddenStickerFileId, TimeSpan sendMessageDelayPrivate, TimeSpan sendMessageDelayGroup,
        TimeSpan sendMessageDelayGlobal, string googleCredentialJson, string applicationName, string googleSheetId,
        ushort initialPlayersAmount, float initialChoiceChance, string actionsGoogleRange,
        string questionsGoogleRange)
        : base(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId, sendMessageDelayPrivate,
            sendMessageDelayGroup, sendMessageDelayGlobal, googleCredentialJson, applicationName, googleSheetId)
    {
        InitialPlayersAmount = initialPlayersAmount;
        InitialChoiceChance = initialChoiceChance;
        ActionsGoogleRange = actionsGoogleRange;
        QuestionsGoogleRange = questionsGoogleRange;
    }
}