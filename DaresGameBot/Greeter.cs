using AbstractBot.Interfaces.Modules;
using AbstractBot.Interfaces.Operations.Commands.Start;
using AbstractBot.Models.MessageTemplates;
using DaresGameBot.Configs;
using DaresGameBot.Game.States;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations;
using DaresGameBot.Operations.Data;
using GryphonUtilities.Save;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DaresGameBot;

[PublicAPI]
internal sealed class Greeter: IGreeter<StartData>
{
    public Greeter(IUpdateSender updateSender, ITextsProvider<Texts> textsProvider, IGuidProvider guidProvider,
        Config config, BotState state, SaveManager<BotState, BotData> saveManager)
    {
        _updateSender = updateSender;
        _textsProvider = textsProvider;
        _guidProvider = guidProvider;
        _config = config;
        _state = state;
        _saveManager = saveManager;
    }

    public async Task GreetAsync(Message message, User from, StartData data)
    {
        Chat chat = message.Chat;
        Texts texts = _textsProvider.GetTextsFor(from.Id);

        if (data.Id is null)
        {
            await texts.StartFormat.SendAsync(_updateSender, chat);
            return;
        }

        if (data.Id != _guidProvider.Guid)
        {
            await texts.WrongGuid.SendAsync(_updateSender, chat);
            return;
        }

        if (!_state.UserStates.ContainsKey(from.Id) && _state.UserStates[from.Id].InfoMessageId is not null)
        {
            await _updateSender.UnpinChatMessageAsync(chat, _state.UserStates[from.Id].InfoMessageId);
            _state.UserStates[from.Id].InfoMessageId = null;
            _saveManager.Save(_state);
        }

        await texts.NewPlayerGreeting.SendAsync(_updateSender, chat);
        await ReportAndPinPlayerAsync(texts, from.Id, chat);
    }

    public Task EditPlayerNameAsync(Chat chat, long id)
    {
        if (!_state.UserStates.ContainsKey(id))
        {
            return Task.CompletedTask;
        }

        _state.UserStates[id].State = UserState.StateType.EnteringName;
        _saveManager.Save(_state);

        Texts texts = _textsProvider.GetTextsFor(id);
        return texts.UpdatePlayerName.SendAsync(_updateSender, chat);
    }

    public async Task AcceptPlayerNameAsync(Chat chat, long id, string name)
    {
        if (_state.Game is null || !_state.UserStates.ContainsKey(id)
                                || (_state.UserStates[id].State != UserState.StateType.EnteringName))
        {
            return;
        }

        Texts texts = _textsProvider.GetTextsFor(id);

        if (!_state.Game.Players.IsNameVacant(name, id))
        {
            await texts.NameIsPresent.SendAsync(_updateSender, chat);
            return;
        }

        _state.UserStates[id].State = UserState.StateType.Default;

        PlayerInfo info = _state.Game.Players.GetOrAddInfo(id);
        if (info.Name == name)
        {
            _saveManager.Save(_state);
        }
        else
        {
            info.Name = name;
            await ReportAndPinPlayerAsync(texts, id, chat);
        }
    }

    private async Task ReportAndPinPlayerAsync(Texts texts, long id, Chat chat)
    {
        if (_state.Game is null)
        {
            return;
        }

        PlayerInfo info = _state.Game.Players.GetOrAddInfo(id);

        string name = string.IsNullOrWhiteSpace(info.Name) ? texts.Unknown : info.Name;
        string gender = string.IsNullOrWhiteSpace(info.GroupInfo.Group) ? texts.Unknown : info.Name;

        List<string> partnersGendersList = _config.Genders
                                                  .Where(info.GroupInfo.CompatableGroups.Contains)
                                                  .ToList();
        string partnersGenders = partnersGendersList.Any() ? string.Join(", ", partnersGendersList) : texts.Unknown;

        MessageTemplateText messageText = texts.PlayerInfoFormat.Format(name, gender, partnersGenders);
        messageText.KeyboardProvider = CreateInfoKeyboard(texts, !string.IsNullOrWhiteSpace(info.Name));

        if (!_state.UserStates.ContainsKey(id))
        {
            _state.UserStates[id] = new UserState();
        }
        UserState userState = _state.UserStates[id];

        if (userState.InfoMessageId is null)
        {
            Message message = await messageText.SendAsync(_updateSender, chat);
            userState.InfoMessageId = message.MessageId;
            await _updateSender.PinChatMessageAsync(chat, userState.InfoMessageId.Value);
        }
        else
        {
            await messageText.EditMessageWithSelfAsync(_updateSender, chat, userState.InfoMessageId.Value);
        }

        _saveManager.Save(_state);
    }

    private InlineKeyboardMarkup CreateInfoKeyboard(Texts texts, bool nameReady)
    {
        List<List<InlineKeyboardButton>> rows = new()
        {
            Bot.CreateOneButtonRow<EditPlayerName>(nameReady ? texts.EditPlayerName : texts.EnterPlayerName)
        };
        /*if (showReady)
        {
            // rows.Add(Bot.CreateOneButtonRow<AcceptPartnersGenders>(texts.Continue);
        }*/
        return new InlineKeyboardMarkup(rows);
    }

    /*public Task SelectGenderForAsync(Chat chat, long id, string gender)
    {
        if (!_state.UserStates.ContainsKey(id))
        {
            _state.UserStates[id] = new UserState();
        }
        _state.UserStates[id].Gender = gender;
        _state.Save();

        Texts texts = _textsProvider.GetTextsFor(id);
        if (!_state.UserStates[id].PartnersGenders.Any())
        {
            texts.SelectPartnersGenders.KeyboardProvider =
                CreatePartnersGendersKeyboard(texts, _state.UserStates[id].PartnersGenders);
            return texts.SelectPartnersGenders.SendAsync(_updateSender, chat);
        }

        throw new NotImplementedException($"Player with id {id} has no compatable groups");
    }

    public Task TogglePartnersGenderForAsync(long userId, long messageId, string gender)
    {
        if (!_state.UserStates.ContainsKey(id))
        {
            _state.UserStates[id] = new UserState();
        }

        if (_state.UserStates[id].PartnersGenders.Contains(gender))
        {
            _state.UserStates[id].PartnersGenders.Remove(gender);
        }
        else
        {
            _state.UserStates[id].PartnersGenders.Add(gender);
        }
        _state.Save();

        Texts texts = _textsProvider.GetTextsFor(id);
    }

    private InlineKeyboardMarkup CreateGenderKeyboard(Texts texts)
    {
        return new InlineKeyboardMarkup(_config.Genders.Select(g => CreateGenderRow(texts, g)));
    }

    private InlineKeyboardMarkup CreatePartnersGendersKeyboard(Texts texts, HashSet<string> partnersGenders)
    {
        List<List<InlineKeyboardButton>> rows = _config.Genders
                                                       .Select(g => CreatePartnersGenderRow(texts, g, partnersGenders))
                                                       .ToList();
        if (partnersGenders.Any())
        {
            rows.Add(Bot.CreateOneButtonRow<AcceptPartnersGenders>(texts.Continue));
        }
        return new InlineKeyboardMarkup(rows);
    }

    private List<InlineKeyboardButton> CreateGenderRow(Texts texts, string gender)
    {
        return Bot.CreateOneButtonRow<SelectGender>(texts.Genders[gender], gender);
    }

    private List<InlineKeyboardButton> CreatePartnersGenderRow(Texts texts, string gender,
        HashSet<string> partnersGenders)
    {
        bool selected = partnersGenders.Contains(gender);
        string format = selected ? texts.ActivePlayerFormat : texts.InactivePlayerFormat;
        return Bot.CreateOneButtonRow<TogglePartnersGender>(string.Format(format, texts.PartnersGenders[gender]),
            gender);
    }*/

    private readonly IUpdateSender _updateSender;
    private readonly ITextsProvider<Texts> _textsProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly Config _config;
    private readonly BotState _state;
    private readonly SaveManager<BotState, BotData> _saveManager;
}