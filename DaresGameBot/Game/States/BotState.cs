using AbstractBot.Modules.Context.Localization;
using AbstractBot.Modules.Context;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using System.Collections.Generic;

namespace DaresGameBot.Game.States;

internal sealed class BotState : BotState<BotData, UserState, LocalizationUserStateData>
{
    public readonly BotStateCore Core;

    public Game? Game;

    public int? PlayersMessageId;
    public int? CardAdminMessageId;
    public int? CardPlayerMessageId;

    internal BotState(BotStateCore core, Dictionary<long, UserState> userStates) : base(userStates) => Core = core;

    public override BotData Save()
    {
        BotData data = base.Save();

        data.GameData = Game?.Save();

        data.PlayersMessageId = PlayersMessageId;
        data.CardAdminMessageId = CardAdminMessageId;
        data.CardPlayerMessageId = CardPlayerMessageId;

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
            Game = new Game(Core.ActionOptions, Core.ActionsVersion, Core.QuestionsVersion, Core.SheetInfo);
            Game.LoadFrom(data.GameData);
        }

        PlayersMessageId = data.PlayersMessageId;
        CardAdminMessageId = data.CardAdminMessageId;
        CardPlayerMessageId = data.CardPlayerMessageId;
    }
}