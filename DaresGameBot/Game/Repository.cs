using System.Collections.Concurrent;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Repository
{
    public Repository(Manager manager) => _manager = manager;

    public bool CheckGame(Chat chat) => _games.ContainsKey(chat.Id) && _games[chat.Id].IsActive();

    public Task StartNewGameAsync(Chat chat)
    {
        _games[chat.Id] = _manager.StartNewGame();
        return _manager.RepotNewGameAsync(chat, _games[chat.Id]);
    }

    public async Task UpdatePlayersAmountAsync(Chat chat, byte playersAmount)
    {
        Data.Game game = await GetOrAddGameAsync(chat);
        await _manager.UpdatePlayersAmountAsync(chat, game, playersAmount);
    }

    public async Task UpdateChoiceChanceAsync(Chat chat, decimal choiceChance)
    {
        Data.Game game = await GetOrAddGameAsync(chat);
        await _manager.UpdateChoiceChanceAsync(chat, game, choiceChance);
    }

    public async Task DrawAsync(Chat chat, int replyToMessageId, bool action = true)
    {
        Data.Game game = await GetOrAddGameAsync(chat);
        Data.Turn? turn = _manager.Draw(game, action);
        if (turn is null)
        {
            await StartNewGameAsync(chat);
        }
        else
        {
            await _manager.RepotTurnAsync(chat, game, turn, replyToMessageId);
        }
    }

    private async Task<Data.Game> GetOrAddGameAsync(Chat chat)
    {
        if (!_games.ContainsKey(chat.Id))
        {
            await StartNewGameAsync(chat);
        }

        return _games[chat.Id];
    }

    private readonly ConcurrentDictionary<long, Data.Game> _games = new();
    private readonly Manager _manager;
}