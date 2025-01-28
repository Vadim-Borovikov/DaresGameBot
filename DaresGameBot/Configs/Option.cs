using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class Option
{
    [Required]
    public ushort Points { get; init; }

    [Required]
    [MinLength(1)]
    public string Tag { get; init; } = null!;
}