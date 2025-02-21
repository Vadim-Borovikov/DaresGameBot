using System;
using System.Threading.Tasks;
using AbstractBot.Bots;
using AbstractBot.Operations.Commands;
using Telegram.Bot.Types;

namespace DaresGameBot.Operations.Commands;

internal sealed class LangCommand : CommandSimple
{
    protected override byte Order => 5;

    public override Enum AccessRequired => Bot.AccessType.Admin;

    public LangCommand(Bot bot)
        : base(bot.Config.Texts.CommandDescriptionFormat, "lang", bot.Config.Texts.LangCommandDescription)
    {
        _bot = bot;
    }

    protected override Task ExecuteAsync(BotBasic bot, Message message, User sender)
    {
        return _bot.OnToggleLanguagesAsync(message.Chat);
    }

    private readonly Bot _bot;
}