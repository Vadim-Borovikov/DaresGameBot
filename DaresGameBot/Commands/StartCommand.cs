using System.Threading.Tasks;
using AbstractBot;
using DaresGameBot.Game;
using GoogleSheetsManager;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Commands;

internal sealed class StartCommand : CommandBase<Bot, Config>
{
    protected override string Name => "start";
    protected override string Description => "инструкции и команды";

    public StartCommand(Bot bot) : base(bot) { }

    public override async Task ExecuteAsync(Message message, bool fromChat, string? payload)
    {
        User user = message.From.GetValue(nameof(message.From));
        await Bot.Client.SendTextMessageAsync(message.Chat.Id, Bot.GetDescriptionFor(user.Id), ParseMode.MarkdownV2);
        if (!Manager.IsGameManagerValid(message.Chat.Id))
        {
            await Manager.StartNewGameAsync(Bot, message.Chat.Id);
        }
    }
}
