using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : DaresGameCommand
{
    protected override byte Order => 6;

    protected override string Alias => _bot.Config.Texts.NewGameCaption;

    public LangCommand(Bot bot) : base(bot, "lang", bot.Config.Texts.LangCommandDescription) => _bot = bot;

    protected override Task ExecuteAsync(Chat chat, int _) => _bot.OnToggleLanguagesAsync(chat);

    private readonly Bot _bot;
}