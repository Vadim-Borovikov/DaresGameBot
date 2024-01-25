using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Configs.MessageTemplates;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class NewCommand : DaresGameCommand
{
    protected override byte Order => 3;

    protected override string Alias => _bot.Config.Texts.NewGameCaption;

    public NewCommand(Bot bot) : base(bot, "new", bot.Config.Texts.NewGameCaption.ToLowerInvariant()) => _bot = bot;

    protected override Task ExecuteAsync(Chat chat, int _)
    {
        MessageTemplateText message = _bot.Config.Texts.NewGame;
        message.KeyboardProvider = KeyboardProvider.Remove;
        return message.SendAsync(_bot, chat);
    }

    private readonly Bot _bot;
}