using AbstractBot.Modules.Context.Localization;

namespace DaresGameBot.Game.States;

internal sealed class UserStateData : LocalizationUserStateData
{
    public string? State { get; set; }
    public int? InfoMessageId { get; set; }
    public int? CardMessageId { get; set; }
}