using System.Text.Json;
using DaresGameBot.Web.Models;
using TL;
using WTelegram;

namespace DaresGameBot.WebTests;

[TestClass]
public sealed class IntegrationTests
{
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext _)
    {
        _config = Config.Load();

        _httpLocal = new HttpClient { BaseAddress = _config.UriLocal };
        _httpAzure = new HttpClient { BaseAddress = _config.UriAzure };

        _messages = new AsyncMessageStream();

        WTelegram.Helpers.Log = (_, _) => { /* do nothing */ };
        _client = new Client(_config.Get);

        _user = await _client.LoginUserIfNeeded();
        Contacts_ResolvedPeer? botTest = await _client.Contacts_ResolveUsername(_config.BotUsernameTest);
        _botTest = botTest.User;

        Contacts_ResolvedPeer? botProd = await _client.Contacts_ResolveUsername(_config.BotUsernameProd);
        _botProd = botProd.User;
    }

    [ClassCleanup]
    public static async Task Cleanup() => await _client.DisposeAsync();

    [TestMethod]
    public async Task TestTurns01() => await MeasureTestScenario(1);

    [TestMethod]
    public async Task TestTurns05() => await MeasureTestScenario(5);

    [TestMethod]
    public async Task TestTurns10() => await MeasureTestScenario(10);

    [TestMethod]
    public async Task TestTurns50() => await MeasureTestScenario(50);

    private static async Task MeasureTestScenario(byte turns)
    {
        await RunTestScenario(true, turns);
        await GetSnapshot(true, turns);

        // await RunTestScenario(false, turns);
        // await GetSnapshot(false, turns);
    }

    private static async Task RunTestScenario(bool test, byte turns)
    {
        Func<UpdatesBase, Task> handler = test ? HandleUpdatesTestAsync : HandleUpdatesProdAsync;
        User bot = test ? _botTest : _botProd;
        _client.OnUpdates += handler;

        await _client.SendMessageAsync(bot, _config.New);

        Message message;
        while (true)
        {
            message = await GetNextMessage();
            if (message.message.StartsWith(_config.FirstMessageStart, StringComparison.Ordinal))
            {
                await PressButtonAsync(_client, _user, message, 0);
            }
            else if (message.message.StartsWith(_config.SecondMessageStart, StringComparison.Ordinal))
            {
                break;
            }
        }

        await _client.SendMessageAsync(bot, _config.Players);

        await _messages.SkipAsync(2);

        int button = 0;

        for (byte t = 0; t < turns; t++)
        {
            message = await GetNextMessage();
            await PressButtonAsync(_client, bot, message, 1 + button);
            button = (button + 1) % 3;

            message = await GetNextMessage();
            await PressButtonAsync(_client, bot, message);
        }

        await _messages.SkipAsync(1);

        await _client.SendMessageAsync(bot, _config.Rates);

        await _messages.SkipAsync(1);

        _client.OnUpdates -= handler;
    }

    private static async Task<Message> GetNextMessage()
    {
        Message? message = await _messages.ReadNextAsync();
        Assert.IsNotNull(message);
        return message;
    }

    private static async Task PressButtonAsync(Client client, User user, Message message, int? row = null)
    {
        ReplyInlineMarkup? replyMarkup = message.reply_markup as ReplyInlineMarkup;
        Assert.IsNotNull(replyMarkup);
        if (row is not null)
        {
            Assert.IsTrue(replyMarkup.rows.Length > row);
        }
        else
        {
            row = replyMarkup.rows.Length - 1;
        }
        Assert.IsTrue(replyMarkup.rows[row.Value].buttons.Length > 0);
        KeyboardButtonBase? button = replyMarkup.rows[row.Value].buttons[0];
        Assert.IsNotNull(button);
        KeyboardButtonCallback? callback = button as KeyboardButtonCallback;
        Assert.IsNotNull(callback);

#pragma warning disable CS4014
        client.Messages_GetBotCallbackAnswer(user, message.id, callback.data);
        await Task.CompletedTask;
#pragma warning restore CS4014
        /*try
        {
            await client.Messages_GetBotCallbackAnswer(user, message.id, callback.data);
        }
        catch (RpcException ex) when ((ex.Code == 400) && ex.Message.Contains("BOT_RESPONSE_TIMEOUT"))
        {
            // Bot didn't respond to callback — ignore if you're not expecting AnswerCallbackQuery
        }*/
    }

    private static Task HandleUpdatesTestAsync(UpdatesBase updates) => HandleUpdatesAsync(_botTest, updates);
    private static Task HandleUpdatesProdAsync(UpdatesBase updates) => HandleUpdatesAsync(_botProd, updates);

    private static Task HandleUpdatesAsync(IPeerInfo bot, UpdatesBase updates)
    {
        foreach (Update? update in updates.UpdateList)
        {
            Message? message = update switch
            {
                UpdateNewMessage unm  => unm.message as Message,
                UpdateEditMessage uem => uem.message as Message,
                _                     => null
            };

            if (message?.peer_id is not PeerUser peer || (peer.user_id != bot?.ID))
            {
                continue;
            }

            _messages.Write(message);
        }

        return Task.CompletedTask;
    }

    private static async Task GetSnapshot(bool local, byte turns)
    {
        HttpClient http = local ? _httpLocal : _httpAzure;
        CpuMeasureResult result = await GetSnapshot(http);
        string httpLabel = local ? "Local" : "Azure";

        Console.WriteLine($"{httpLabel} CPU Snapshot for {turns} turns:");
        Console.WriteLine($"  Web - Total:  {result.WebApp.MillisecondsTotal:F2} ms");
        Console.WriteLine($"  Web - Delta:  {result.WebApp.MillisecondsSinceLastSnapshot:F2} ms");
        Console.WriteLine($"  Bot - Total:  {result.Bot.MillisecondsTotal:F2} ms");
        Console.WriteLine($"  Bot - Delta:  {result.Bot.MillisecondsSinceLastSnapshot:F2} ms");
    }

    private static async Task<CpuMeasureResult> GetSnapshot(HttpClient http)
    {
        HttpResponseMessage response = await http.GetAsync("measure/snapshot");
        string json = await response.Content.ReadAsStringAsync();
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        CpuMeasureResult? result = JsonSerializer.Deserialize<CpuMeasureResult>(json, options);
        Assert.IsNotNull(result);
        return result;
    }

    private static User _user = null!;
    private static User _botTest = null!;
    private static User _botProd = null!;
    private static AsyncMessageStream _messages = null!;
    private static Config _config = null!;
    private static HttpClient _httpLocal = null!;
    private static HttpClient _httpAzure = null!;
    private static Client _client = null!;
}