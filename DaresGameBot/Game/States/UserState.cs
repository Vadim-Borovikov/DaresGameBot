using System.Collections.Generic;
using AbstractBot.Modules.Context.Localization;
using DaresGameBot.Utilities.Extensions;

namespace DaresGameBot.Game.States;

internal sealed class UserState : LocalizationUserState<UserStateData>
{
    public enum StateType
    {
        Default,
        EnteringName
    }
    public int? InfoMessageId;
    public StateType State;
    public int? CardMessageId;

    public bool IsLanguageEn => LanguageCode == LocalizationEn;

    public override UserStateData Save()
    {
        UserStateData result = base.Save();
        result.InfoMessageId = InfoMessageId;
        result.State = State.ToString();
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
        InfoMessageId = data.InfoMessageId;
        State = data.State.ToEnum<StateType>() ?? StateType.Default;
        CardMessageId = data.CardMessageId;
    }

    public void ToggleLanguage() => LanguageCode = IsLanguageEn ? LocalizationRu : LocalizationEn;

    private const string LocalizationRu = "ru";
    private const string LocalizationEn = "en";
}