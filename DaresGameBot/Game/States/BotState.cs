using AbstractBot.Modules.Context;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using System.Collections.Generic;

namespace DaresGameBot.Game.States;

internal sealed class BotState : BotState<BotData, UserState, UserStateData>
{
    public readonly BotStateCore Core;

    public Game? Game;

    public int? PlayersMessageId;
    public bool InactivePlayersVisible;

    public UserState? AdminState => UserStates.GetValueOrDefault(_adminId);
    public UserState? PlayerState => UserStates.GetValueOrDefault(_playerId);

    internal BotState(BotStateCore core, Dictionary<long, UserState> userStates, long adminId, long playerId)
        : base(userStates)
    {
        Core = core;
        _adminId = adminId;
        _playerId = playerId;
    }

    public bool ShouldIncludeEnFor(long userId) => UserStates.ContainsKey(userId) && UserStates[userId].IncludeEn;

    public void SetUserMessageId(long userId, int messageId)
    {
        if (!UserStates.ContainsKey(userId))
        {
            UserStates[userId] = new UserState();
        }
        UserStates[userId].CardMessageId = messageId;
    }
    public void ResetUserMessageId(long userId)
    {
        if (UserStates.ContainsKey(userId))
        {
            UserStates[userId].CardMessageId = null;
        }
    }

    public override BotData Save()
    {
        BotData data = base.Save();

        data.GameData = Game?.Save();

        data.PlayersMessageId = PlayersMessageId;

        data.InactivePlayersVisible = InactivePlayersVisible;

        return data;
    }

    public override void LoadFrom(BotData? data)
    {
        if (data is null)
        {
            return;
        }

        base.LoadFrom(data);

        if (Core.SheetInfo is not null && data.GameData is not null)
        {
            Game = new Game(Core.ActionOptions, Core.QuestionPoints, Core.ActionsVersion, Core.QuestionsVersion,
                Core.SheetInfo);
            Game.LoadFrom(data.GameData);
        }

        PlayersMessageId = data.PlayersMessageId;
        InactivePlayersVisible = data.InactivePlayersVisible;
    }

    private readonly long _adminId;
    private readonly long _playerId;
}