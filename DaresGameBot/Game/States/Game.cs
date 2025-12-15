using DaresGameBot.Configs;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using DaresGameBot.Utilities;
using DaresGameBot.Utilities.Extensions;
using GryphonUtilities.Save;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaresGameBot.Game.States;

internal sealed class Game : IStateful<GameData>
{
    public enum State
    {
        Fresh,
        ArrangementPurposed,
        CardRevealed
    }

    public readonly PlayersRepository Players;
    public readonly GameStats Stats;

    public State? CurrentState { get; private set; }

    public Arrangement? CurrentArrangement { get; private set; }

    public Game(Dictionary<string, Option> actionOptions, ushort? questionPoints, string actionsVersion,
        string questionsVersion, SheetInfo sheetInfo)
    {
        _actionDeck = new Deck<ActionData>(sheetInfo.Actions);
        _questionDeck = new Deck<QuestionData>(sheetInfo.Questions);
        _actionsVersion = actionsVersion;
        _questionsVersion = questionsVersion;
        Players = new PlayersRepository();

        GameStatsStateCore gameStatsStateCore = new(actionOptions, questionPoints, Players);
        Stats = new GameStats(gameStatsStateCore);
        GroupCompatibility compatibility = new();
        _matchmaker = new DistributedMatchmaker(Players, Stats, compatibility);

        _interactionSubscribers = new List<IInteractionSubscriber>
        {
            Stats
        };

        CurrentState = State.Fresh;
        CurrentArrangement = null;
        _currentActionId = null;
        _currentQuestionId = null;
    }

    public Game(Deck<ActionData> actionsDeck, Deck<QuestionData> questionsDeck, string actionsVersion,
        string questionsVersion, PlayersRepository players, GameStats stats, Matchmaker matchmaker,
        State? currentState = State.Fresh)
    {
        _actionDeck = actionsDeck;
        _questionDeck = questionsDeck;
        _actionsVersion = actionsVersion;
        _questionsVersion = questionsVersion;
        Players = players;
        Stats = stats;
        _matchmaker = matchmaker;

        _interactionSubscribers = new List<IInteractionSubscriber>
        {
            Stats
        };

        CurrentState = currentState;
    }

    public ActionData GetActionData() => _actionDeck.GetCard(_currentActionId!.Value);
    public QuestionData GetQuestionData() => _questionDeck.GetCard(_currentQuestionId!.Value);

    public bool IsCurrentArrangementValid()
    {
        return CurrentArrangement is not null && _matchmaker.CanBePlayed(CurrentArrangement);
    }

    public void DrawQuestion()
    {
        if (_currentQuestionId is null || !_questionDeck.CheckCard(_currentQuestionId.Value))
        {
            _currentQuestionId = _questionDeck.FilterMinUses().RandomItem();
        }

        CurrentState = State.CardRevealed;
    }

    public void ProcessCardRevealed(string tag)
    {
        _currentCardTag = tag;
        CurrentState = State.CardRevealed;
    }

    public void ProcessCardUnrevealed()
    {
        _currentCardTag = null;
        CurrentState = State.ArrangementPurposed;
    }

    public void CompleteQuestion()
    {
        _questionDeck.Mark(_currentQuestionId!.Value);

        OnQuestionCompleted();

        StartNewTurn();
    }

    public void CompleteAction(bool fully)
    {
        _actionDeck.Mark(_currentActionId!.Value);

        OnActionCompleted(fully);

        StartNewTurn();
    }

    public bool UpdatePlayers(List<AddOrUpdatePlayerData> updateDatas, string handlerSeparator)
    {
        return Stats.UpdateList(updateDatas, handlerSeparator);
    }

    public GameData Save()
    {
        return new GameData
        {
            ActionUses = _actionDeck.Save(),
            QuestionUses = _questionDeck.Save(),
            ActionsVersion = _actionsVersion,
            QuestionsVersion = _questionsVersion,
            PlayersRepositoryData = Players.Save(),
            GameStatsData = Stats.Save(),
            CurrentState = CurrentState?.ToString(),
            CurrentArrangementData = CurrentArrangement?.Save(),
            CurrentCardTag = _currentCardTag,
            CurrentActionId = _currentActionId,
            CurrentQuestionId = _currentQuestionId
        };
    }

    public void LoadFrom(GameData? data)
    {
        if (data is null)
        {
            return;
        }

        if ((_actionsVersion != data.ActionsVersion) || (_questionsVersion != data.QuestionsVersion))
        {
            return;
        }

        _actionDeck.LoadFrom(data.ActionUses);
        _questionDeck.LoadFrom(data.QuestionUses);

        Players.LoadFrom(data.PlayersRepositoryData);

        Stats.LoadFrom(data.GameStatsData);

        CurrentState = data.CurrentState?.ToState();

        if (data.CurrentArrangementData is null)
        {
            CurrentArrangement = null;
        }

        CurrentArrangement = new Arrangement();
        CurrentArrangement.LoadFrom(data.CurrentArrangementData);

        _currentCardTag = data.CurrentCardTag;
        _currentActionId = data.CurrentActionId;
        _currentQuestionId = data.CurrentQuestionId;
    }

    private void StartNewTurn()
    {
        _currentActionId = null;
        _currentQuestionId = null;
        Players.MoveNext();
    }

    public void DrawAction()
    {
        List<ushort> playableIds = _actionDeck.GetIds(c => _matchmaker.CanPlay(c.ArrangementType)).ToList();
        _currentActionId = playableIds.Count == 0 ? null : _actionDeck.FilterMinUses(playableIds).RandomItem();
        if (_currentActionId is null)
        {
            return;
        }

        ActionData data = GetActionData();
        CurrentArrangement = _matchmaker.SelectCompanionsFor(data.ArrangementType);
        CurrentState = State.ArrangementPurposed;
    }

    private void OnQuestionCompleted()
    {
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnQuestionCompleted(Players.Current, CurrentArrangement);
        }
    }

    private void OnActionCompleted(bool fully)
    {
        if (CurrentArrangement is null)
        {
            throw new NullReferenceException("Current arrangement is null");
        }

        if (_currentCardTag is null)
        {
            throw new NullReferenceException("Current tag is null");
        }
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnActionCompleted(Players.Current, CurrentArrangement, _currentCardTag, fully);
        }
    }

    private readonly Deck<ActionData> _actionDeck;
    private readonly Deck<QuestionData> _questionDeck;
    private readonly string _actionsVersion;
    private readonly string _questionsVersion;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly Matchmaker _matchmaker;
    private string? _currentCardTag;
    private ushort? _currentActionId;
    private ushort? _currentQuestionId;
}