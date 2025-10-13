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

        CurrentState = null;
        CurrentArrangement = null;
        _currentCardId = null;
    }

    public Game(Deck<ActionData> actionsDeck, Deck<QuestionData> questionsDeck, string actionsVersion,
        string questionsVersion, PlayersRepository players, GameStats stats, Matchmaker matchmaker,
        State? currentState = null)
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

    public ActionData GetActionData() => _actionDeck.GetCard(_currentCardId!.Value);
    public QuestionData GetQuestionData() => _questionDeck.GetCard(_currentCardId!.Value);

    public bool IsCurrentArrangementValid()
    {
        return CurrentArrangement is not null && _matchmaker.CanBePlayed(CurrentArrangement);
    }

    public void DrawQuestion()
    {
        if (_revealedQuestionId is null || !_questionDeck.CheckCard(_revealedQuestionId.Value))
        {
            _revealedQuestionId = _questionDeck.FilterMinUses().RandomItem();
        }
        _currentCardId = _revealedQuestionId;

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
        _questionDeck.Mark(_currentCardId!.Value);

        OnQuestionCompleted();

        StartNewTurn();
    }

    public void CompleteAction(bool fully)
    {
        _actionDeck.Mark(_currentCardId!.Value);

        OnActionCompleted(fully);

        StartNewTurn();
    }

    public bool UpdatePlayers(List<PlayerListUpdateData> updateDatas) => Stats.UpdateList(updateDatas);

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
            CurrentCardId = _currentCardId,
            CurrentCardTag = _currentCardTag,
            RevealedQuestionId = _revealedQuestionId
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

        _currentCardId = data.CurrentCardId;
        _currentCardTag = data.CurrentCardTag;
        _revealedQuestionId = data.RevealedQuestionId;
    }

    private void StartNewTurn()
    {
        _revealedQuestionId = null;
        Players.MoveNext();
    }

    public void DrawAction()
    {
        List<ushort> playableIds = _actionDeck.GetIds(c => _matchmaker.CanPlay(c.ArrangementType)).ToList();
        _currentCardId = playableIds.Count == 0 ? null : _actionDeck.FilterMinUses(playableIds).RandomItem();
        if (_currentCardId is null)
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
    private ushort? _currentCardId;
    private string? _currentCardTag;
    private ushort? _revealedQuestionId;
}