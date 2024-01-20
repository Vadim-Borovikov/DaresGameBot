using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using AbstractBot.Configs.MessageTemplates;

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
    public string PercentFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText PlayersFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText ChanceFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText NewGameFormat { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public MessageTemplateText AcceptedFormat { get; init; } = null!;

    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string Choosable { get; init; } = null!;
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
    public string Partner { get; init; } = null!;
    [UsedImplicitly]
    [Required]
    [MinLength(1)]
    public string Partners { get; init; } = null!;
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
    public MessageTemplateText GameOver { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText UpdateChoiceChanceOperationDescriptionFormat { get; init; } = null!;
    [Required]
    [MinLength(1)]
    public MessageTemplateText UpdatePlayersAmountOperationDescription { get; init; } = null!;
}