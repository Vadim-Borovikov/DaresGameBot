using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : CommandSimple
{
    protected override byte Order => 7;

    public LangCommand(Bot bot) : base(bot, "lang", bot.Config.Texts.LangCommandDescription) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender)
    {
        return _bot.OnToggleLanguagesAsync(message.Chat, sender);
    }

    private readonly Bot _bot;
}