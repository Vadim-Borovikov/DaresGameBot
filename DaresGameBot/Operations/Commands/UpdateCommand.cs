using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class UpdateCommand : CommandSimple
{
    protected override byte Order => 5;

    public UpdateCommand(Bot bot) : base(bot, "update", bot.Config.Texts.UpdateCommandDescription) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender) => _bot.UpdateDecksAsync(message.Chat);

    private readonly Bot _bot;
}