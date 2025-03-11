using AbstractBot.Modules.Context.Localization;

namespace DaresGameBot.Game.States;

internal sealed class UserStateData : LocalizationUserStateData
{
    public int? CardMessageId { get; set; }
}