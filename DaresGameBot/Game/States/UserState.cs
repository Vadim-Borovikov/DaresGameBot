using AbstractBot.Modules.Context.Localization;

namespace DaresGameBot.Game.States;

internal sealed class UserState : LocalizationUserState<UserStateData>
{
    public int? CardMessageId;
    public bool IsLanguageEn => LanguageCode == LocalizationEn;

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

    public void ToggleLanguage() => LanguageCode = IsLanguageEn ? LocalizationRu : LocalizationEn;

    private const string LocalizationRu = "ru";
    private const string LocalizationEn = "en";
}