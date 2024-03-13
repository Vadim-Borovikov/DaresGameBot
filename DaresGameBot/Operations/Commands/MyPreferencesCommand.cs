using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class MyPreferencesCommand : DaresGameCommand
{
    protected override byte Order => 3;

    public MyPreferencesCommand(Bot bot)
        : base(bot, "my_preferences", bot.Config.Texts.MyPreferencesCommandDescription)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(Chat chat, User sender, int replyToMessageId)
    {
        return _bot.UpdatePreferencesAsync(chat);
    }

    private readonly Bot _bot;
}