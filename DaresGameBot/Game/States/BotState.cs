using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using GryphonUtilities.Save;

namespace DaresGameBot.Game.States;

internal sealed class BotState : IStateful<BotData>
{
    public readonly BotStateCore Core;

    public Game? Game;

    public bool IncludeEn;
    public int? PlayersMessageId;
    public int? CardAdminMessageId;
    public int? CardPlayerMessageId;

    internal BotState(BotStateCore core) => Core = core;

    public BotData Save()
    {
        return new BotData
        {
            GameData = Game?.Save(),
            IncludeEn = IncludeEn,
            PlayersMessageId = PlayersMessageId,
            CardAdminMessageId = CardAdminMessageId,
            CardPlayerMessageId = CardPlayerMessageId,
        };
    }

    public void LoadFrom(BotData? data)
    {
        if (data is null)
        {
            return;
        }

        if (Core.SheetInfo is not null && data.GameData is not null)
        {
            Game = new Game(Core.ActionOptions, Core.ActionsVersion, Core.QuestionsVersion, Core.SheetInfo);
            Game.LoadFrom(data.GameData);
        }

        IncludeEn = data.IncludeEn;
        PlayersMessageId = data.PlayersMessageId;
        CardAdminMessageId = data.CardAdminMessageId;
        CardPlayerMessageId = data.CardPlayerMessageId;
    }
}