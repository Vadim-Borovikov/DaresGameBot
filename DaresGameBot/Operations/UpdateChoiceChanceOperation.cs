using System.Globalization;
using System.Threading.Tasks;
using AbstractBot;
using AbstractBot.Operations;
using DaresGameBot.Game;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdateChoiceChanceOperation : Operation, IBotProvider<Bot>
{
    protected override int Priority => 6;

    protected override string Title => "дробное число от 0.0 до 1.0";

    protected override string Description => "изменить шанс на 🤩";

    public UpdateChoiceChanceOperation(Bot bot) : base(bot) => Bot = bot;

    protected override async Task<bool> TryExecuteAsync(Message message, Chat sender)
    {
        if (!IsAccessSuffice(sender.Id) || (message.Type != MessageType.Text))
        {
            return false;
        }

        bool parsed =
            float.TryParse(message.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float choiceChance);
        if (!parsed)
        {
            return false;
        }
        Chat chat = BotBase.GetReplyChatFor(message, sender);
        return await Manager.UpdateChoiceChanceAsync(choiceChance, Bot, chat);
    }

    public Bot Bot { get; }
}