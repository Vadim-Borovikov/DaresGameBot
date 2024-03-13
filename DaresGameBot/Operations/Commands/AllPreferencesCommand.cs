using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class AllPreferencesCommand : DaresGameCommand
{
    protected override byte Order => 3;

    public AllPreferencesCommand(Bot bot)
        : base(bot, "all_preferences", bot.Config.Texts.AllPreferencesCommandDescription)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Chat chat, User sender, int replyToMessageId)
    {
        return _bot.UpdateAllPreferencesAsync(chat);
    }

    private readonly Bot _bot;
}