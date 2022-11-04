using System.Threading.Tasks;
using AbstractBot.Commands;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class NewCommand : CommandBase<Bot, Config>
{
    protected override string Alias => Game.Game.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", Game.Game.NewGameCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        return Manager.StartNewGameAsync(Bot, message.Chat);
    }
}