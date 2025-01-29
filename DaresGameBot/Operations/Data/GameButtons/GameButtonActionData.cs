﻿using DaresGameBot.Game;

namespace DaresGameBot.Operations.Data.GameButtons;

internal sealed class GameButtonActionData : GameButtonData
{
    public readonly ActionInfo ActionInfo;
    public readonly bool CompletedFully;

    public GameButtonActionData(ActionInfo actionInfo, bool completedFully)
    {
        ActionInfo = actionInfo;
        CompletedFully = completedFully;
    }
}