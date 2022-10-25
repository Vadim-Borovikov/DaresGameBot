using System.ComponentModel.DataAnnotations;

// ReSharper disable NullableWarningSuppressionIsUsed

namespace DaresGameBot.Web.Models;

public sealed class Config : DaresGameBot.Config
{
    [Required]
    [MinLength(1)]
    public string CultureInfoName { get; init; } = null!;
}