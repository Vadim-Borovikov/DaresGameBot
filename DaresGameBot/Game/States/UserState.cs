using AbstractBot.Modules.Context.Localization;

namespace DaresGameBot.Game.States;

internal sealed class UserState : LocalizationUserState<UserStateData>
{
    public int? CardMessageId;
    public bool IncludeEn => LanguageCode == LocalizationRuEn;

    public override UserStateData Save()
    {
        UserStateData result = base.Save();
        result.CardMessageId = CardMessageId;
        return result;
    }

    public override void LoadFrom(UserStateData? data)
    {
        if (data is null)
        {
            return;
        }

        base.LoadFrom(data);
        CardMessageId = data.CardMessageId;
    }

    public const string LocalizationRu = "ru";
    public const string LocalizationRuEn = "ruen";
}