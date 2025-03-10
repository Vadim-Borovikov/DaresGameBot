using AbstractBot.Modules.Context.Localization;

namespace DaresGameBot.Game.States;

internal sealed class UserState : LocalizationUserState<LocalizationUserStateData>
{
    public bool IncludeEn => LanguageCode == LocalizationRuEn;

    public const string LocalizationRu = "ru";
    public const string LocalizationRuEn = "ruen";
}