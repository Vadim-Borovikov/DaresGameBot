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

    public Game(Dictionary<string, Option> actionOptions, string actionsVersion, string questionsVersion,
        SheetInfo sheetInfo)
    {
        _actionDeck = new Deck<ActionData>(sheetInfo.Actions);
        _questionDeck = new Deck<CardData>(sheetInfo.Questions);
        _actionsVersion = actionsVersion;
        _questionsVersion = questionsVersion;
        Players = new PlayersRepository();

        GameStatsStateCore gameStatsStateCore = new(actionOptions, sheetInfo.Actions, Players);
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

    public Game(Deck<ActionData> actionsDeck, Deck<CardData> questionsDeck, string actionsVersion,
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
    public CardData GetQuestionData() => _questionDeck.GetCard(_currentCardId!.Value);

    public bool IsCurrentArrangementValid()
    {
        return CurrentArrangement is not null && _matchmaker.CanBePlayed(CurrentArrangement);
    }

    public void DrawArrangement()
    {
        ArrangementType? arrangementType = SelectArrangementType();
        if (arrangementType is null)
        {
            return;
        }
        CurrentArrangement = _matchmaker.SelectCompanionsFor(arrangementType.Value);
        CurrentState = State.ArrangementPurposed;
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

    public void DrawAction(string tag)
    {
        List<ushort> ids = _actionDeck.GetIds(c => (c.Tag == tag)
                                                   && (c.ArrangementType == CurrentArrangement!.GetArrangementType()))
                                      .ToList();
        if (!ids.Any())
        {
            throw new Exception("No suitable cards found");
        }

        if (!_revealedActionIds.ContainsKey(tag) || !ids.Contains(_revealedActionIds[tag]))
        {
            _revealedActionIds[tag] = _actionDeck.FilterMinUses(ids).RandomItem();
        }
        _currentCardId = _revealedActionIds[tag];

        CurrentState = State.CardRevealed;
    }

    public void ProcessCardUnrevealed() => CurrentState = State.ArrangementPurposed;

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
            RevealedQuestionId = _revealedQuestionId,
            RevealedActionIds = _revealedActionIds
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
        _revealedQuestionId = data.RevealedQuestionId;

        _revealedActionIds.Clear();
        _revealedActionIds.AddAll(data.RevealedActionIds);
    }

    private void StartNewTurn()
    {
        _revealedQuestionId = null;
        _revealedActionIds.Clear();
        Players.MoveNext();
    }

    private ArrangementType? SelectArrangementType()
    {
        IEnumerable<ushort> playableIds = _actionDeck.GetIds(c => _matchmaker.CanPlay(c.ArrangementType));

        Dictionary<ArrangementType, HashSet<string>> types = new();
        foreach (IGrouping<uint, ushort> group in _actionDeck.GroupByUses(playableIds))
        {
            foreach (ActionData actionData in group.Select(_actionDeck.GetCard))
            {
                if (!types.ContainsKey(actionData.ArrangementType))
                {
                    types[actionData.ArrangementType] = new HashSet<string>();
                }

                types[actionData.ArrangementType].Add(actionData.Tag);
            }

            List<ArrangementType> fullTypes = types.Keys
                                                   .Where(t => types[t].SetEquals(Stats.ActionTags))
                                                   .ToList();
            if (fullTypes.Any())
            {
                return fullTypes.RandomItem();
            }
        }
        return null;
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
        ActionInfo info = new(_currentCardId!.Value, CurrentArrangement);

        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnActionCompleted(Players.Current, info, fully);
        }
    }

    private readonly Deck<ActionData> _actionDeck;
    private readonly Deck<CardData> _questionDeck;
    private readonly string _actionsVersion;
    private readonly string _questionsVersion;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly Matchmaker _matchmaker;
    private ushort? _currentCardId;
    private ushort? _revealedQuestionId;

    private readonly Dictionary<string, ushort> _revealedActionIds = new();
}