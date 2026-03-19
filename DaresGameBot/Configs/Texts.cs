using System.Collections.Generic;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using AbstractBot.Models.MessageTemplates;

namespace DaresGameBot.Configs;

[PublicAPI]
public class Texts : AbstractBot.Models.Config.Texts
{
    [UsedImplicitly]
    [Required]
    public Dictionary<string, string> Genders { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public Dictionary<string, string> PartnersGenders { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText JoinGameQrCaptionFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText NoPlayersYet { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText WrongGuid { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText NewPlayerGreeting { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText PlayerInfoFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public string Unknown { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public string EnterPlayerName { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public string EditPlayerName { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText UpdatePlayerName { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText NameIsPresent { get; init; } = null!;

    /*[UsedImplicitly]
    [Required]
    public MessageTemplateText SelectGender { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText SelectPartnersGenders { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public string Continue { get; init; } = null!;*/

    [UsedImplicitly]
    [Required]
    public MessageTemplateText PlayersFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public string PlayersSeparator { get; init; } = null!;
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
    public string UpdatePlayerSeparator { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string UpdateGroupsSeparator { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText ReadingDecks { get; init; } = null!;

    [Required]
    public MessageTemplateText UpdatePlayersOperationDescription { get; init; } = null!;

    [Required]
    public MessageTemplateText LangToggled { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string Unreveal { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string Delete { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string CurrentPlayerFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ActivePlayerFormat { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string InactivePlayerFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string Completed { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string ActionCompletedPartially { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText PlayerFormatActive { get; init; } = null!;
    [Required]
    public MessageTemplateText PlayerFormatInactive { get; init; } = null!;

    [Required]
    public MessageTemplateText StatusMessageEndSuccessFormat { get; init; } = null!;
    [Required]
    public MessageTemplateText EquipmentPrefixFormat { get; init; } = null!;

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
    [MinLength(1)]
    public string DrawCard { get; init; } = null!;

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
    [Required]
    public MessageTemplateText RateFormatHidden { get; init; } = null!;

    [Required]
    public MessageTemplateText CompletedCardFormat { get; init; } = null!;
}