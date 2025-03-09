using System.Collections.Generic;
using AbstractBot;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using GryphonUtilities.Extensions;
using DaresGameBot.Game;
using DaresGameBot.Save;
using DaresGameBot.Context.Meta;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Compatibility;
using DaresGameBot.Helpers;

namespace DaresGameBot.Context;

internal sealed class Game : IContext<Game, GameData, MetaContext>
{
    public enum State
    {
        ArrangementPurposed,
        CardRevealed
    }

    public readonly PlayersRepository Players;
    public readonly GameStats Stats;

    public State? CurrentState { get; private set; }

    public Game(Deck<ActionData> actionsDeck, Deck<CardData> questionsDeck, string actionsVersion,
        string questionsVersion, PlayersRepository players, GameStats stats, Matchmaker matchmaker,
        State? currentState = null)
    {
        _actionDeck = actionsDeck;
        _questionsDeck = questionsDeck;
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
    public CardData GetQuestionData(ushort id) => _questionsDeck.GetCard(id);

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
        ushort id = _questionsDeck.GetRandomId().Denull("No question found!");

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
        _questionsDeck.Mark(id);

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
            QuestionUses = _questionsDeck.Save(),
            ActionsVersion = _actionsVersion,
            QuestionsVersion = _questionsVersion,
            PlayersRepositoryData = Players.Save(),
            GameStatsData = Stats.Save(),
            CurrentState = CurrentState?.ToString()
        };
    }

    public static Game? Load(GameData data, MetaContext? meta)
    {
        if (meta is null)
        {
            return null;
        }

        if ((meta.ActionsVersion != data.ActionsVersion) || (meta.QuestionsVersion != data.QuestionsVersion))
        {
            return null;
        }

        foreach (ActionData action in meta.Actions.Values)
        {
            action.ArrangementType = new ArrangementType(action.Partners, action.CompatablePartners);
        }
        Deck<ActionData>? actionDeck = Deck<ActionData>.Load(data.ActionUses, meta.Actions);
        if (actionDeck is null)
        {
            return null;
        }

        Deck<CardData>? questionDeck = Deck<CardData>.Load(data.QuestionUses, meta.Questions);
        if (questionDeck is null)
        {
            return null;
        }

        PlayersRepository playersRepository = PlayersRepository.Load(data.PlayersRepositoryData, meta);
        GameStatsMetaContext gameStatsMeta = new(meta, playersRepository);

        GameStats? gameStats = GameStats.Load(data.GameStatsData, gameStatsMeta);
        if (gameStats is null)
        {
            return null;
        }

        State? currentState = data.CurrentState?.ToState();

        GroupCompatibility compatibility = new();
        DistributedMatchmaker matchmaker = new(playersRepository, gameStats, compatibility);
        return new Game(actionDeck, questionDeck, data.ActionsVersion, data.QuestionsVersion, playersRepository,
            gameStats, matchmaker, currentState);
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
    private readonly Deck<CardData> _questionsDeck;
    private readonly string _actionsVersion;
    private readonly string _questionsVersion;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly Matchmaker _matchmaker;
}