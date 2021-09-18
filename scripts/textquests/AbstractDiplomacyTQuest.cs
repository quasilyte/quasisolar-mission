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
    protected SpaceUnit GetPlayerSpaceUnit() { return _gameState.humanUnit.Get(); }
    protected StarSystem GetCurrentStarSystem() { return RpgGameState.starSystemByPos[GetPlayerSpaceUnit().pos]; }

    protected string DiplomaticStatusString(DiplomaticStatus status) {
        return Utils.DiplomaticStatusString(status);
    }

    protected void DoChangeReputation(int delta) { _gameState.reputations[_faction] += delta; }
    protected void DoDeclareWar() { _gameState.diplomaticStatuses[_faction] = DiplomaticStatus.War; }
    protected void DoSignNonAttackPact() { _gameState.diplomaticStatuses[_faction] = DiplomaticStatus.NonAttackPact; }
}
