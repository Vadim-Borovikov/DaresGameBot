using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class OptionInfo
{
    [Required]
    public ushort Points { get; init; }

    [Required]
    public ushort HelpPoints { get; init; }

    [Required]
    [MinLength(1)]
    public string Tag { get; init; } = null!;
}