using Godot;
using System.Collections.Generic;
using System;

public class RandomMapEventPopupBuilder : AbstractMapPopupBuilder {
    private AbstractMapEvent _mapEvent;
    private IMapViewContext _context;
    private RandomEventContext _eventContext;

    private AbstractMapEvent.EffectKind _randomEventResolutionPostEffect;
    private Action _randomEventResolutionAction;
    private RpgGameState _gameState;
    private SpaceUnit _humanUnit;
    private StarSystem _currentSystem;

    public void SetMapEventContext(RandomEventContext ctx) { _eventContext = ctx; }
    public void SetMapViewContext(IMapViewContext ctx) { _context = ctx; }
    public void SetMapEvent(AbstractMapEvent e) { _mapEvent = e; }

    public MapEventPopupNode Build() {
        _gameState = RpgGameState.instance;
        _humanUnit = _gameState.humanUnit.Get();
        _randomEventResolutionPostEffect = AbstractMapEvent.EffectKind.None;
        _currentSystem = RpgGameState.starSystemByPos[_humanUnit.pos];

        var title = _mapEvent.title;
        var text = _mapEvent.text;

        var options = new List<MapEventPopupNode.Option>();
        foreach (var a in _mapEvent.actions) {
            var buttonText = a.name;
            var buttonHint = a.hint();
            if (buttonHint != "") {
                buttonText += " " + buttonHint;
            }
            options.Add(new MapEventPopupNode.Option {
                text = buttonText,
                disabled = !a.condition(),
                apply = () => OnActionClicked(a),
            });
        }

        return MapEventPopupNode.New(title, text, options);
    }

    private void OnActionClicked(AbstractMapEvent.Action a) {
        var result = a.apply();
        foreach (var effect in result.effects) {
            ExecuteEffect(effect);
        }

        if (result.skipText) {
            DoResolve();
            return;
        }

        var popupBuilder = new RandomMapEventResolvedPopup();
        popupBuilder.SetOnResolved(() => {
            DoResolve();
        });
        popupBuilder.SetMapEvent(_mapEvent);
        popupBuilder.SetAction(a);
        popupBuilder.SetResult(result);
        var popup = popupBuilder.Build();
        _context.AddUIChild(popup);
        _context.UpdateUI();
        popup.PopupCentered();
    }

    private void DoResolve() {
        _context.UpdateUI();
        Resolve();
        RunEventResolutionPostEffect();
    }

    private void ExecuteEffect(AbstractMapEvent.Effect effect) {
        switch (effect.kind) {
            case AbstractMapEvent.EffectKind.AddTechnology:
                _gameState.technologiesResearched.Add((string)effect.value);
                return;

            case AbstractMapEvent.EffectKind.AddVesselToFleet:
                _context.AddPlayerUnitMember((Vessel)effect.value);
                _gameState.humanUnit.Get().fleet.Add(((Vessel)effect.value).GetRef());
                return;

            case AbstractMapEvent.EffectKind.DeclareWar:
                if (_gameState.diplomaticStatuses[(Faction)effect.value] != DiplomaticStatus.War) {
                    _gameState.reputations[(Faction)effect.value] -= 5;
                }
                _gameState.diplomaticStatuses[(Faction)effect.value] = DiplomaticStatus.War;
                return;

            case AbstractMapEvent.EffectKind.AddDrone:
                _gameState.explorationDrones.Add((string)effect.value);
                return;

            case AbstractMapEvent.EffectKind.AddVesselStatus:
                _gameState.humanUnit.Get().fleet[(int)effect.value].Get().statusList.Add((string)effect.value2);
                return;

            case AbstractMapEvent.EffectKind.AddItem:
                _gameState.PutItemToStorage((IItem)effect.value);
                return;

            case AbstractMapEvent.EffectKind.AddCredits:
                _gameState.credits += (int)effect.value;
                return;
            case AbstractMapEvent.EffectKind.AddMinerals:
                _humanUnit.CargoAddMinerals((int)effect.value);
                return;
            case AbstractMapEvent.EffectKind.AddOrganic:
                _humanUnit.CargoAddOrganic((int)effect.value);
                return;
            case AbstractMapEvent.EffectKind.AddPower:
                _humanUnit.CargoAddPower((int)effect.value);
                return;
            case AbstractMapEvent.EffectKind.AddFlagshipBackupEnergy:
                _humanUnit.fleet[0].Get().AddEnergy((float)effect.value);
                return;

            case AbstractMapEvent.EffectKind.AddReputation:
                _gameState.reputations[(Faction)effect.value2] += (int)effect.value2;
                return;

            case AbstractMapEvent.EffectKind.DestroyVessel:
                _context.RemovePlayerUnitMember((int)effect.value);
                return;

            case AbstractMapEvent.EffectKind.SpendAnyVesselBackupEnergy:
                foreach (var v in _humanUnit.fleet) {
                    if (v.Get().energy >= (int)effect.value) {
                        v.Get().energy -= (int)effect.value;
                        break;
                    }
                }
                return;
            case AbstractMapEvent.EffectKind.AddFuel:
                RpgGameState.AddFuel((float)effect.value);
                return;
            case AbstractMapEvent.EffectKind.AddFleetBackupEnergyPercentage: {
                    var randRange = (Vector2)effect.value;
                    foreach (var handle in _humanUnit.fleet) {
                        var v = handle.Get();
                        var roll = QRandom.FloatRange(randRange.x, randRange.y);
                        v.energy = QMath.ClampMin(v.energy - v.energy * roll, 0);
                    }
                    return;
                }

            case AbstractMapEvent.EffectKind.AddKrigiaMaterial:
                _gameState.researchMaterial.Add((int)effect.value, Faction.Krigia);
                return;

            case AbstractMapEvent.EffectKind.DamageFlagshipPercentage: {
                    var randRange = (Vector2)effect.value;
                    var flagship = _humanUnit.fleet[0].Get();
                    flagship.hp -= flagship.hp * QRandom.FloatRange(randRange.x, randRange.y);
                    return;
                }

            case AbstractMapEvent.EffectKind.DamageFleetPercentage: {
                    var randRange = (Vector2)effect.value;
                    foreach (var handle in _humanUnit.fleet) {
                        var v = handle.Get();
                        if (v.hp < 2) {
                            continue;
                        }
                        var damageRoll = QRandom.FloatRange(randRange.x, randRange.y);
                        v.hp -= v.hp * damageRoll;
                    }
                    return;
                }
            case AbstractMapEvent.EffectKind.TeleportToSystem:
                var targetSys = (StarSystem)effect.value;
                _context.EnterSystem(targetSys);
                return;
            case AbstractMapEvent.EffectKind.ApplySlow:
                _gameState.travelSlowPoints += (int)effect.value;
                return;

            case AbstractMapEvent.EffectKind.EnterTextQuest:
                RpgGameState.selectedTextQuest = (AbstractTQuest)effect.value;
                _randomEventResolutionPostEffect = effect.kind;
                return;

            case AbstractMapEvent.EffectKind.PrepareArenaSettings:
                ArenaManager.SetArenaSettings(_currentSystem, _eventContext.spaceUnit.fleet, _humanUnit.fleet);
                return;

            case AbstractMapEvent.EffectKind.EnterDuelArena: {
                    var unit = (SpaceUnit)effect.value;
                    RpgGameState.arenaUnit1 = unit;
                    var flagship = new List<Vessel.Ref> { _humanUnit.fleet[0] };
                    ArenaManager.SetArenaSettings(_currentSystem, unit.fleet, flagship);
                    _randomEventResolutionPostEffect = AbstractMapEvent.EffectKind.EnterArena;
                    _randomEventResolutionAction = effect.fn;
                    return;
                }

            case AbstractMapEvent.EffectKind.EnterArena: {
                    var unit = (SpaceUnit)effect.value;
                    RpgGameState.arenaUnit1 = unit;
                    if (effect.value2 != null) {
                        RpgGameState.arenaUnit2 = (SpaceUnit)effect.value2;
                        ArenaManager.SetStagedArenaSettings(_currentSystem, unit, RpgGameState.arenaUnit2, _humanUnit);
                    } else {
                        // A normal battle.
                        ArenaManager.SetArenaSettings(_currentSystem, unit.fleet, _humanUnit.fleet);
                    }
                    _randomEventResolutionPostEffect = effect.kind;
                    _randomEventResolutionAction = effect.fn;
                    return;
                }

            case AbstractMapEvent.EffectKind.SpaceUnitRetreat: {
                    var unit = (SpaceUnit)effect.value;
                    var destinationOptions = RpgGameState.starSystemConnections[_currentSystem];
                    var nextSystem = QRandom.Element(destinationOptions);
                    unit.waypoint = nextSystem.pos;
                    unit.botSystemLeaveDelay = 0;
                    return;
                }

            case AbstractMapEvent.EffectKind.KrigiaDetectsStarBase: {
                    var starSystem = (StarSystem)effect.value;
                    if (starSystem.starBase.id != 0) {
                        var starBase = starSystem.starBase.Get();
                        if (starBase.owner == Faction.Earthling && starBase.discoveredByKrigia == 0) {
                            starBase.discoveredByKrigia = _gameState.day;
                            _context.CreateBadNotification(starBase.system.Get().pos, "Base detected");
                        }
                    }
                    return;
                }

        }
    }

    private void RunEventResolutionPostEffect() {
        switch (_randomEventResolutionPostEffect) {
            case AbstractMapEvent.EffectKind.EnterArena:
                _randomEventResolutionAction();
                _context.GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
                return;
            case AbstractMapEvent.EffectKind.EnterTextQuest:
                _context.GetTree().ChangeScene("res://scenes/TextQuestScreen.tscn");
                return;
        }
    }
}
