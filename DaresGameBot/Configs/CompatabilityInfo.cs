using AbstractBot.Configs.MessageTemplates;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class CompatabilityInfo
{
    [Required]
    [MinLength(1)]
    public string Sign { get; init; } = null!;

    [Required]
    [MinLength(1)]
    public MessageTemplateText Description { get; init; } = null!;
}