using DaresGameBot.Configs;
using DaresGameBot.Game.Data.Cards;
using DaresGameBot.Game.Data.Decks;
using DaresGameBot.Game.Matchmaking.ActionCheck;
using DaresGameBot.Game.Matchmaking.Interactions;
using System.Collections.Generic;
using DaresGameBot.Game.Matchmaking;
using DaresGameBot.Game.Data.PlayerListUpdates;

namespace DaresGameBot.Game.Data;

internal sealed class Game
{
    public enum State
    {
        ArrangementPresented,
        CardRevealed
    }

    public string CurrentPlayer => _players.Current;

    public State CurrentState { get; private set; }

    public bool IncludeEn { get; private set; }

    public IReadOnlyList<string> GetPlayers() => _players.GetNames();

    public ushort GetPoints(string name) => _players.GetPoints(name);

    public Game(Config config, DecksProvider decksProvider, PlayerRepository players, Matchmaker matchmaker,
        List<IInteractionSubscriber> interactionSubscribers)
    {
        _config = config;
        _players = players;

        _companionsSelector = new CompanionsSelector(matchmaker, GetPlayers());
        _actionDeck = decksProvider.GetActionDeck(_companionsSelector);
        _questionsDeck = decksProvider.GetQuestionDeck();
        _interactionSubscribers = interactionSubscribers;
    }

    public Action GetAction(ushort id) => _actionDeck.Cards[id];

    public Arrangement? TryDrawArrangement()
    {
        CurrentState = State.ArrangementPresented;
        ArrangementType? arrangementType = _actionDeck.TrySelectArrangement(_players);
        return arrangementType is null
            ? null
            : _companionsSelector.SelectCompanionsFor(_players.Current, arrangementType.Value);
    }

    public ActionInfo DrawAction(Arrangement arrangement, string tag)
    {
        CurrentState = State.CardRevealed;
        ushort id = _actionDeck.SelectCard(arrangement.GetArrangementType(), tag);
        return new ActionInfo(arrangement, id);
    }

    public void RegisterAction(ActionInfo info, ushort points)
    {
        Action action = GetAction(info.ActionId);

        foreach (IInteractionSubscriber subscriber in _interactionSubscribers)
        {
            subscriber.OnInteraction(CurrentPlayer, info.Arrangement.Partners, action.CompatablePartners, points);
        }

        _actionDeck.FoldCard(info.ActionId);

        _players.MoveNext();
    }

    public void RegisterQuestion() => _players.MoveNext();

    public Turn DrawQuestion()
    {
        CurrentState = State.CardRevealed;
        Question question = _questionsDeck.Draw();
        return new Turn(_config.Texts, _config.ImagesFolder, _config.Texts.QuestionsTag, question.Description,
            question.DescriptionEn, CurrentPlayer);
    }

    public void UpdatePlayers(List<PlayerListUpdate> updates) => _players.UpdateList(updates);

    public void ToggleLanguages() => IncludeEn = !IncludeEn;

    private readonly Config _config;
    private readonly ActionDeck _actionDeck;
    private readonly QuestionDeck _questionsDeck;
    private readonly PlayerRepository _players;
    private readonly List<IInteractionSubscriber> _interactionSubscribers;
    private readonly CompanionsSelector _companionsSelector;
}