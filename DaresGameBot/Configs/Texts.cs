using AbstractBot.Configs.MessageTemplates;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class Texts : AbstractBot.Configs.TextsBasic
{
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ActionsTitle { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string QuestionsTitle { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText PlayersFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public string PlayersSeparator { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText NewGame { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText NewGameStart { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText Accepted { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText Refuse { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string QuestionsTag { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText TurnFormatFull { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText TurnFormatShort { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText TurnDescriptionRuEnFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText TurnPartnersFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string Partner { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string Partners { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string DefaultSeparator { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string CompatablePartnersSeparator { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string UpdatePartsSeparator { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string UpdateGroupsSeparator { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string NewGameCaption { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string ShowRatesCaption { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText ReadingDecks { get; init; } = null!;

    [Required]
    public MessageTemplateText UpdatePlayersOperationDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string UpdateCommandDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string LangCommandDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText LangToggledToRu { get; init; } = null!;

    [Required]
    public MessageTemplateText LangToggledToRuEn { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string Unreveal { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string Completed { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ActionCompletedPartially { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string WrongTagsFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string WrongArrangementFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string WrongArrangementLineFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string PlayerFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText StatusMessageEndSuccessFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText StatusMessageEndFailedFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText EquipmentPrefixFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string TagSeparator { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ErrorsSeparator { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string ErrorFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string EquipmentFormat { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string EquipmentSeparatorMessage { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string EquipmentSeparatorSheet { get; init; } = null!;

    [Required]
    public MessageTemplateText UnknownToggleFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText NothingChanges { get; init; } = null!;

    [Required]
    public MessageTemplateText EndGameWarning { get; init; } = null!;

    [Required]
    public MessageTemplateText RatesFormat { get; init; } = null!;

    [Required]
    public MessageTemplateText NoRates { get; init; } = null!;

    [Required]
    public MessageTemplateText RateLineFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText BestRateFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText RateFormat { get; init; } = null!;
}