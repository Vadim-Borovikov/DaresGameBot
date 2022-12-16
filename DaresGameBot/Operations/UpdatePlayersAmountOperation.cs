using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Operations;
using DaresGameBot.Game;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdatePlayersAmountOperation : Operation, IBotProvider<Bot>
{
    protected override int Priority => 5;

    protected override string Title => "целое число";

    protected override string Description => "изменить количество игроков";

    public UpdatePlayersAmountOperation(Bot bot) : base(bot) => Bot = bot;

    protected override async Task<bool> TryExecuteAsync(Message message, Chat sender)
    {
        if (!IsAccessSuffice(sender.Id) || (message.Type != MessageType.Text))
        {
            return false;
        }

        bool parsed = ushort.TryParse(message.Text, out ushort playersAmount);
        if (!parsed)
        {
            return false;
        }

        Chat chat = BotBase.GetReplyChatFor(message, sender);
        return await Manager.UpdatePlayersAmountAsync(playersAmount, Bot, chat);
    }

    public Bot Bot { get; }
}