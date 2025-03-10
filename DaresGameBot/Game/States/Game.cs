using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using GryphonUtilities.Extensions;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using GryphonUtilities.Save;
using DaresGameBot.Utilities.Extensions;
using DaresGameBot.Configs;
using DaresGameBot.Game.States.Cores;
using DaresGameBot.Game.States.Data;

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

    public ActionData GetActionData(ushort id) => _actionDeck.GetCard(id);
    public CardData GetQuestionData(ushort id) => _questionDeck.GetCard(id);

    public Arrangement? TryDrawArrangement()
    {
        ushort? id = _actionDeck.GetRandomId(c => _matchmaker.CanPlay(c.ArrangementType));
        if (id is null)
        {
            return null;
        }

        ActionData card = _actionDeck.GetCard(id.Value);
        Arrangement arrangement = _matchmaker.SelectCompanionsFor(card.ArrangementType);

        CurrentState = State.ArrangementPurposed;

        return arrangement;
    }

    public ushort DrawQuestion()
    {
        ushort id = _questionDeck.GetRandomId().Denull("No question found!");

        CurrentState = State.CardRevealed;

        return id;
    }

    public ActionInfo DrawAction(Arrangement arrangement, string tag)
    {
        ushort id =
            _actionDeck.GetRandomId(c => (c.Tag == tag) && (c.ArrangementType == arrangement.GetArrangementType()))
                       .Denull("No suitable cards found");
        ActionInfo info = new(id, arrangement);

        CurrentState = State.CardRevealed;

        return info;
    }

    public void ProcessCardUnrevealed() => CurrentState = State.ArrangementPurposed;

    public void CompleteQuestion(ushort id, Arrangement? declinedArrangement)
    {
        _questionDeck.Mark(id);

        OnQuestionCompleted(declinedArrangement);

        StartNewTurn();
    }

    public void CompleteAction(ActionInfo info, bool fully)
    {
        _actionDeck.Mark(info.Id);

        OnActionCompleted(info, fully);

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
            CurrentState = CurrentState?.ToString()
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
    }

    private void StartNewTurn() => Players.MoveNext();

    private void OnQuestionCompleted(Arrangement? declinedArrangement)
    {
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnQuestionCompleted(Players.Current, declinedArrangement);
        }
    }

    private void OnActionCompleted(ActionInfo info, bool fully)
    {
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
}