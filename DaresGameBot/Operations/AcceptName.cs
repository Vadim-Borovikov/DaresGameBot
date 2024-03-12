using AbstractBot.Operations;
using DaresGameBot.Operations.Info;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DaresGameBot.Operations;

internal sealed class UpdateName : Operation<UpdateNameInfo>
{
    protected override byte Order => 7;

    public UpdateName(Bot bot) : base(bot) => _bot = bot;

    protected override bool IsInvokingBy(Message message, User sender, out UpdateNameInfo? data)
    {
        data = null;

        if ((message.Type != MessageType.Text) || string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        Game.Data.Game? context = Bot.TryGetContext<Game.Data.Game>(sender.Id);
        if (context is null)
        {
            return false;
        }
        data = new UpdateNameInfo(context, message.Text);
        return true;
    }

    protected override Task ExecuteAsync(UpdateNameInfo data, Message message, User sender)
    {
        return _bot.AddPlayerAsync(message.Chat, sender, data.Game, data.Name);
    }

    private readonly Bot _bot;
}