public abstract class AbstractDiplomacyTQuest: AbstractTQuest {
    protected Faction _faction;
    protected RpgGameState _gameState;

    public AbstractDiplomacyTQuest(Faction faction) {
        _faction = faction;
        _gameState = RpgGameState.instance;
    }

    protected string GetCurrencyName() { return RpgGameState.alienCurrencyNames[_faction]; }
    protected int GetCurrency() { return _gameState.alienCurrency[_faction]; }
    protected int GetReputation() { return _gameState.reputations[_faction]; }
    protected DiplomaticStatus GetStatus() { return _gameState.diplomaticStatuses[_faction]; }

    protected string DiplomaticStatusString(DiplomaticStatus status) {
        if (status == DiplomaticStatus.War) {
            return "at war";
        }
        if (status == DiplomaticStatus.Alliance) {
            return "allies";
        }
        if (status == DiplomaticStatus.NonAttackPact) {
            return "non-aggression pact";
        }
        return "unspecified";
    }

    protected void DoChangeReputation(int delta) { _gameState.reputations[Faction.Phaa] += delta; }
    protected void DoDeclareWar() { _gameState.diplomaticStatuses[_faction] = DiplomaticStatus.War; }
    protected void DoSignNonAttackPact() { _gameState.diplomaticStatuses[_faction] = DiplomaticStatus.NonAttackPact; }
}
