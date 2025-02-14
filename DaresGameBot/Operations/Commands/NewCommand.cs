using AbstractBot.Operations.Commands;
using System.Threading.Tasks;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : CommandSimple
{
    protected override byte Order => 2;

    public NewCommand(Bot bot) : base(bot, "new", bot.Config.Texts.NewGameCaption) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnEndGameRequesedAsync(message.Chat, sender, ConfirmEndData.ActionAfterGameEnds.StartNewGame);
    }

    private readonly Bot _bot;
}