using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using DaresGameBot.Operations.Data.GameButtons;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class UpdateCommand : CommandSimple
{
    protected override byte Order => 4;

    public UpdateCommand(Bot bot) : base(bot, "update", bot.Config.Texts.UpdateCommandDescription) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnEndGameRequesedAsync(message.Chat, sender, ConfirmEndData.ActionAfterGameEnds.UpdateCards);
    }

    private readonly Bot _bot;
}