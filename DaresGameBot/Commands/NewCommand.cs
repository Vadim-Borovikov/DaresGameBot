using System.Threading.Tasks;
using DaresGameBot.Game;
using Telegram.Bot.Types;

namespace DaresGameBot.Commands;

internal sealed class NewCommand : CommandWithAlias
{
    protected override string Alias => Game.Game.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", Game.Game.NewGameCaption.ToLowerInvariant()) { }

    public override Task ExecuteAsync(Message message, Chat chat, string? payload)
    {
        return Manager.StartNewGameAsync(Bot, chat);
    }
}