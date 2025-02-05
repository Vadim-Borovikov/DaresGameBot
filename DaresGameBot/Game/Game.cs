using DaresGameBot.Configs;
using DaresGameBot.Game.Matchmaking.Interactions;
using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Players;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using GryphonUtilities.Extensions;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Game
{
    public enum State
    {
        ArrangementPresented,
        CardRevealed
    }

    public Message? PlayersMessage;
    public bool PlayersMessageShowsPoints;

    public string CurrentPlayer => _players.Current;

    public State CurrentState { get; private set; }

    public bool IncludeEn { get; private set; }

    public IReadOnlyList<string> GetPlayers() => _players.GetNames();

    public ushort GetPoints(string name) => _players.GetPoints(name);

    public Game(Config config, Deck<ActionData> actionDeck, Deck<CardData> questionsDeck, Repository players,
        PointsManager pointsManager, Matchmaker matchmaker)
    {
        _config = config;
        _actionDeck = actionDeck;
        _questionsDeck = questionsDeck;
        _players = players;


        _companionsSelector = new CompanionsSelector(matchmaker, GetPlayers());
        _interactionSubscribers = new List<IInteractionSubscriber>
        {
            matchmaker,
            pointsManager
        };
    }

    public ActionData GetActionData(ushort id) => _actionDeck.GetCard(id);

    public Arrangement? TryDrawArrangement()
    {
        CurrentState = State.ArrangementPresented;

        ArrangementType? arrangementType = TrySelectArrangement();
        return arrangementType is null ? null : _companionsSelector.SelectCompanionsFor(arrangementType.Value);
    }

    public ActionInfo DrawAction(Arrangement arrangement, string tag)
    {
        CurrentState = State.CardRevealed;
        ushort id =
            _actionDeck.GetRandomId(c => (c.Tag == tag) && (c.ArrangementType == arrangement.GetArrangementType()))
                       .Denull("No suitable cards found");
        return new ActionInfo(id, arrangement);
    }

    public void OnActionPurposed(Arrangement arrangement)
    {
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnInteractionPurposed(CurrentPlayer, arrangement);
        }
    }

    public void OnActionCompleted(ActionInfo info, bool completedFully)
    {
        ActionData actionData = GetActionData(info.Id);

        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnInteractionCompleted(CurrentPlayer, info.Arrangement, actionData.Tag, completedFully);
        }

        _actionDeck.Mark(info.Id);

        _players.MoveNext();
    }

    public void RegisterQuestion() => _players.MoveNext();

    public Turn DrawQuestion()
    {
        CurrentState = State.CardRevealed;
        CardData questionData = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, questionData, CurrentPlayer);
    }

    public bool UpdatePlayers(List<PlayerListUpdateData> updateDatas) => _players.UpdateList(updateDatas);

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private ArrangementType? TrySelectArrangement()
    {
        ushort? id = _actionDeck.GetRandomId(c => _companionsSelector.CanPlay(c.ArrangementType));
        if (id is null)
        {
            return null;
        }

        ActionData card = _actionDeck.GetCard(id.Value);
        return card.ArrangementType;
    }

    private readonly Config _config;
    private readonly Deck<ActionData> _actionDeck;
    private readonly Deck<CardData> _questionsDeck;
    private readonly Repository _players;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly CompanionsSelector _companionsSelector;
}