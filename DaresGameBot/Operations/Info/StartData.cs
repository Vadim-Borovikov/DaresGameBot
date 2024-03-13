using System;
using System.Linq;
using AbstractBot.Operations.Data;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Info;

public sealed class StartData : ICommandData<StartData>
{
    internal readonly Guid GameId;

    private StartData(Guid gameId) => GameId = gameId;

    public static StartData? From(Message message, User sender, string[] parameters)
    {
        return parameters.Length switch
        {
            1 => new StartData(Guid.Parse(parameters.Single())),
            _ => null
        };
    }
}