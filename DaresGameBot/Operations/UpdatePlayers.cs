using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayers : Operation<PlayersInfo>
{
    protected override byte Order => 6;

    public UpdatePlayers(Bot bot) : base(bot, bot.Config.Texts.UpdatePlayersOperationDescription)
    {
        _bot = bot;
    }

    protected override bool IsInvokingBy(Message message, User sender, out PlayersInfo? data)
    {
        data = null;
        if (message.Type != MessageType.Text)
        {
            return false;
        }

        List<string>? lines = message.Text?.Split(PlayersSeparator).ToList();

        switch (lines?.Count)
        {
            case null:
            case < 1: return false;
            default:
                Game.Data.Game? game = _bot.TryGetContext<Game.Data.Game>(sender.Id);
                if (game is not null)
                {
                    List<string> allLines = game.PlayerLines.ToList();
                    allLines.AddRange(lines);
                    lines = allLines;
                }
                data = PlayersInfo.From(lines);
                return data is not null;
        }
    }

    protected override Task ExecuteAsync(PlayersInfo data, Message message, User sender)
    {
        return _bot.UpdatePlayersAsync(message.Chat, sender, data);
    }

    private readonly Bot _bot;
    private const string PlayersSeparator = "\n";
}