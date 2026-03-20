using System;
using System.Collections.Generic;
using DaresGameBot.Configs;

namespace DaresGameBot.Game.States;

internal sealed class PlayersMessageState
{
    public enum Type
    {
        Activity,
        Selection,
        FastMovement,
        Movement
    }

    private PlayersMessageState(Type next, byte minActivePlayers)
    {
        _next = next;
        _minActivePlayers = minActivePlayers;
    }

    public static string GetLabel(Type state, Texts texts)
    {
        return state switch
        {
            Type.Activity      => texts.PlayersMessageStateActivity,
            Type.Selection     => texts.PlayersMessageStateSelection,
            Type.FastMovement  => texts.PlayersMessageStateFastMovement,
            Type.Movement      => texts.PlayersMessageStateMovement,
            _                  => throw new ArgumentOutOfRangeException()
        };
    }

    public Type GetNext(int activePlayers)
    {
        PlayersMessageState state = this;
        Type type;
        do
        {
            type = state._next;
            state = States[type];
        }
        while (activePlayers < state._minActivePlayers);
        return type;
    }

    public static readonly IReadOnlyDictionary<Type, PlayersMessageState> States =
        new Dictionary<Type, PlayersMessageState>
        {
            { Type.Activity, new PlayersMessageState(Type.Selection, 0) },
            { Type.Selection, new PlayersMessageState(Type.FastMovement, 2) },
            { Type.FastMovement, new PlayersMessageState(Type.Movement, 3) },
            { Type.Movement, new PlayersMessageState(Type.Activity, 2) }
        };

    private readonly Type _next;
    private readonly byte _minActivePlayers;
}
