using System;
using AbstractBot.Interfaces.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Data;

internal sealed class StartData : ICommandData<StartData>
{
    public Guid? Id { get; private init; }

    public static StartData? From(Message message, User from, string[] parameters)
    {
        Guid? id = null;
        if (parameters.Length == 1)
        {
            id = Guid.Parse(parameters[0]);
        }

        return new StartData
        {
            Id = id
        };
    }
}