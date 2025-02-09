﻿using System.Collections.Generic;

namespace DaresGameBot.Game.Matchmaking.Interactions;

internal interface IInteractionSubscriber
{
    public void OnArrangementPurposed(string player, Arrangement arrangement);
    public void OnActionCompleted(string player, ushort id, IReadOnlyList<string> partners, bool fully);
}