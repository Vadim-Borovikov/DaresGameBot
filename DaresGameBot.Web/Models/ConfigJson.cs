using GoogleSheetsManager;
using Newtonsoft.Json;

namespace DaresGameBot.Web.Models;

public sealed class ConfigJson : IConvertibleTo<Config>
{
    [JsonProperty]
    public string? Token { get; set; }
    [JsonProperty]
    public string? SystemTimeZoneId { get; set; }
    [JsonProperty]
    public string? DontUnderstandStickerFileId { get; set; }
    [JsonProperty]
    public string? ForbiddenStickerFileId { get; set; }

    [JsonProperty]
    public string? Host { get; set; }
    [JsonProperty]
    public List<string?>? About { get;set; }
    [JsonProperty]
    public List<string?>? ExtraCommands { get;set; }
    [JsonProperty]
    public List<long?>? AdminIds { get;set; }
    [JsonProperty]
    public long? SuperAdminId { get;set; }

    [JsonProperty]
    public string? GoogleCredentialJson { get; set; }
    [JsonProperty]
    public string? ApplicationName { get; set; }
    [JsonProperty]
    public string? GoogleSheetId { get; set; }

    [JsonProperty]
    public ushort? InitialPlayersAmount { get; set; }
    [JsonProperty]
    public float? InitialChoiceChance { get; set; }
    [JsonProperty]
    public string? ActionsGoogleRange { get; set; }
    [JsonProperty]
    public string? QuestionsGoogleRange { get; set; }

    [JsonProperty]
    public Dictionary<string, string?>? GoogleCredential { get; set; }

    public Config Convert()
    {
        string token = Token.GetValue(nameof(Token));
        string systemTimeZoneId = SystemTimeZoneId.GetValue(nameof(SystemTimeZoneId));
        string dontUnderstandStickerFileId = DontUnderstandStickerFileId.GetValue(nameof(DontUnderstandStickerFileId));
        string forbiddenStickerFileId = ForbiddenStickerFileId.GetValue(nameof(ForbiddenStickerFileId));

        string googleCredentialJson = string.IsNullOrWhiteSpace(GoogleCredentialJson)
            ? JsonConvert.SerializeObject(GoogleCredential)
            : GoogleCredentialJson;

        string applicationName = ApplicationName.GetValue(nameof(ApplicationName));
        string googleSheetId = GoogleSheetId.GetValue(nameof(GoogleSheetId));

        ushort initialPlayersAmount = InitialPlayersAmount.GetValue(nameof(InitialPlayersAmount));
        float initialChoiceChance = InitialChoiceChance.GetValue(nameof(InitialChoiceChance));
        string actionsGoogleRange = ActionsGoogleRange.GetValue(nameof(ActionsGoogleRange));
        string questionsGoogleRange = QuestionsGoogleRange.GetValue(nameof(QuestionsGoogleRange));

        return new Config(token, systemTimeZoneId, dontUnderstandStickerFileId, forbiddenStickerFileId,
            googleCredentialJson, applicationName, googleSheetId, initialPlayersAmount, initialChoiceChance,
            actionsGoogleRange, questionsGoogleRange)
        {
            Host = Host,
            About = About is null ? null : string.Join(Environment.NewLine, About),
            ExtraCommands = ExtraCommands is null ? null : string.Join(Environment.NewLine, ExtraCommands),
            AdminIds = AdminIds?.Select(id => id.GetValue("Admin id")).ToList(),
            SuperAdminId = SuperAdminId
        };
    }
}
