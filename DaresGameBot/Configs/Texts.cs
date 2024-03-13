using AbstractBot.Configs.MessageTemplates;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class Texts : AbstractBot.Configs.Texts
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
    [MinLength(1)]
    public MessageTemplateText PlayersFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText NewGame { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText NewGameFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText AcceptedFormat { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public string QuestionsTag { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText TurnFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText TurnDescriptionRuEnFormat { get; init; } = null!;
    [Required]
    [MinLength(1)]
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
    public string Helper { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string Helpers { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string PartnersSeparator { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string DrawActionCaption { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string DrawQuestionCaption { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string NewGameCaption { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText ReadingDecks { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText DeckEnded { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText GameOver { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText UpdatePlayersOperationDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string UpdateCommandDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string LangCommandDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public List<string> NoMatchesInDeckLines { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText LangToggledToRu { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText LangToggledToRuEn { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText PlayerFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText CurrentPlayersFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string AllPreferencesCommandDescription { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string MyPreferencesCommandDescription { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText PlayerAccepted { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText PlayerDeclinedNameFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    public MessageTemplateText NoGameFound { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText GameCanNotBeJoined { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string StartGameAndGetLink { get; set; } = null!;
    [UsedImplicitly]
    [Required]
    public MessageTemplateText NewGameLink { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText SetCompatabilityFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText CompatabilityFormat { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public string CompatabilityButtonCaptionFormat { get; init; } = null!;

    [Required]
    public CompatabilityInfo Compatable { get; init; } = null!;
    [Required]
    public CompatabilityInfo NotCompatable { get; init; } = null!;

    internal MessageTemplateText GetPreferences()
    {
        MessageTemplateText templateCompatable = CompatabilityFormat.Format(Compatable.Sign, Compatable.Description);
        MessageTemplateText templateNotCompatable =
            CompatabilityFormat.Format(NotCompatable.Sign, NotCompatable.Description);
        MessageTemplateText[] templates =  { templateCompatable, templateNotCompatable };
        return MessageTemplateText.JoinTexts(templates);
    }
}