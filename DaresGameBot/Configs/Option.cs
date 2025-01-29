using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DaresGameBot.Configs;

[PublicAPI]
public class Option
{
    [Required]
    public ushort Points { get; init; }

    [Required]
    public ushort? PartialPoints { get; init; }
}