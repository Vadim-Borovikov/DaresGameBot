using System.Threading.Tasks;
using AbstractBot.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : CommandSimple
{
    protected override byte Order => 6;

    public LangCommand(Bot bot) : base(bot, "lang", bot.Config.Texts.LangCommandDescription) => _bot = bot;

    protected override Task ExecuteAsync(Message message, User sender) => _bot.OnToggleLanguagesAsync(message.Chat);

    private readonly Bot _bot;
}