using AbstractBot.Modules.Context.Localization;
using DaresGameBot.Game.States;
using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;
using Telegram.Bot.Types;

namespace DaresGameBot;

internal sealed class LocalizationUserRegistrator : LocalizationUserRegistrator<UserState, LocalizationUserStateData>
{
    public LocalizationUserRegistrator(BotState state, SaveManager<BotState, BotData> saveManager)
        : base(state.UserStates)
    {
        _saveManager = saveManager;
        _state = state;
    }

    public override void RegistrerUser(User user)
    {
        base.RegistrerUser(user);
        _saveManager.Save(_state);
    }

    private readonly BotState _state;
    private readonly SaveManager<BotState, BotData> _saveManager;
}