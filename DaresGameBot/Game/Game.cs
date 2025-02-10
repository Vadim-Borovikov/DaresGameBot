using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data;
using DaresGameBot.Game.Matchmaking.Interactions;
using DaresGameBot.Operations.Data.PlayerListUpdates;
using GryphonUtilities.Extensions;
using Telegram.Bot.Types;

namespace DaresGameBot.Game;

internal sealed class Game
{
    public enum State
    {
        ArrangementPurposed,
        CardRevealed
    }

    public readonly Players.Repository Players;
    public readonly GameStats Stats;

    public bool IncludeEn { get; private set; }

    public Message? PlayersMessage;
    public bool PlayersMessageShowsPoints;

    public State CurrentState { get; private set; }

    public Game(Deck<ActionData> actionDeck, Deck<CardData> questionsDeck, Players.Repository players, GameStats stats,
        Matchmaker matchmaker)
    {
        _actionDeck = actionDeck;
        _questionsDeck = questionsDeck;
        Players = players;
        Stats = stats;
        _matchmaker = matchmaker;

        _interactionSubscribers = new List<IInteractionSubscriber>
        {
            Stats
        };
    }

    public ActionData GetActionData(ushort id) => _actionDeck.GetCard(id);
    public CardData GetQuestionData(ushort id) => _questionsDeck.GetCard(id);

    public void ToggleEn() => IncludeEn = !IncludeEn;

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
        OnArrangementPurposed(arrangement);

        return arrangement;
    }

    public ushort DrawQuestion()
    {
        ushort id = _questionsDeck.GetRandomId().Denull("No question found!");
        _questionsDeck.Mark(id);

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

    public void CompleteQuestion(ushort id)
    {
        _questionsDeck.Mark(id);

        StartNewTurn();
    }

    public void CompleteAction(ActionInfo info, bool fully)
    {
        _actionDeck.Mark(info.Id);

        OnActionCompleted(info, fully);

        StartNewTurn();
    }

    private void StartNewTurn() => Players.MoveNext();

    private void OnArrangementPurposed(Arrangement arrangement)
    {
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnArrangementPurposed(Players.Current, arrangement);
        }
    }

    private void OnActionCompleted(ActionInfo info, bool fully)
    {
        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnActionCompleted(Players.Current, info.Id, info.Arrangement.Partners, fully);
        }
    }

    public bool UpdatePlayers(List<PlayerListUpdateData> updateDatas) => Stats.UpdateList(updateDatas);

    private readonly Deck<ActionData> _actionDeck;
    private readonly Deck<CardData> _questionsDeck;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly Matchmaker _matchmaker;
}