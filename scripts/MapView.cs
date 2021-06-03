using Godot;
using System;
using System.Collections.Generic;
using CheatCommandKind = MapViewCheatMenuPopup.CommandKind;

public class MapView : Node2D {
    const float MAP_WIDTH = (1080 * 3) + 220;

    private RandomEvent _randomEvent;
    private RandomEventContext _randomEventContext;

    private GameMenuNode _menuNode;

    private RpgGameState _gameState;
    private SpaceUnit _humanUnit;

    private float _dayCounter = 0;

    private MapHumanNode _human;

    private StarSystemNode _currentSystem;
    private StarSystemNode _dstSystem;

    private List<StarSystemNode> _starSystemNodes = new List<StarSystemNode>();
    private Dictionary<StarSystem, StarSystemNode> _starSystemNodeByStarSystem = new Dictionary<StarSystem, StarSystemNode>();

    private TextureButton _movementToggle;

    private Camera2D _camera;
    private Vector2 _cameraSpeed;

    private bool _lockControls = false;
    private Popup _fleetAttackPopup;
    private Popup _miningPopup;
    private PopupNode _starSystemMenu;
    private PopupNode _randomEventPopup;
    private PopupNode _randomEventResolvedPopup;
    private PopupNode _battleResult;
    private PopupNode _researchCompletedPopup;
    private PopupNode _patrolReachesBasePopup;
    private MapViewCheatMenuPopup _cheatsPopup;

    private RandomEvent.EffectKind _randomEventResolutionPostEffect;

    private SpaceUnitNode _eventUnit;
    private PopupNode _starBaseAttackPopup;
    private PopupNode _scavengersEventPopup;
    private PopupNode _krigiaPatrolPopup;
    private PopupNode _krigiaTaskForcePopup;

    private HashSet<SpaceUnitNode> _spaceUnits = new HashSet<SpaceUnitNode>();

    private List<UnitMemberNode> _unitMembers = new List<UnitMemberNode>();

    private TextureButton _modeToggled;
    private Dictionary<UnitMode, TextureButton> _modeToggles;

    private void SwitchButtonTextures(TextureButton b) {
        var tmp = b.TextureNormal;
        b.TextureNormal = b.TexturePressed;
        b.TexturePressed = tmp;
    }

    private void SetUnitMode(UnitMode newMode) {
        var pressed = _modeToggles[newMode];
        if (_modeToggled == pressed) {
            return;
        }
        SwitchButtonTextures(pressed);
        SwitchButtonTextures(_modeToggled);
        _gameState.mapState.mode = newMode;
        _modeToggled = pressed;
    }

    private void OnModeTogglePressed(UnitMode newMode) {
        SetUnitMode(newMode);
    }

    private bool IsRandomEventFaction(Faction faction) {
        return faction == Faction.RandomEventHostile ||
            faction == Faction.RandomEventHostile2 ||
            faction == Faction.RandomEventAlly;
    }

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _humanUnit = _gameState.humanUnit.Get();
        QRandom.SetRandomNumberGenerator(RpgGameState.rng);
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayMapMusic();

        if (RpgGameState.transition == RpgGameState.MapTransition.EnemyBaseAttackRepelled) {
            ProcessUnitCasualties(_humanUnit);
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase);
        } else if (RpgGameState.transition == RpgGameState.MapTransition.EnemyUnitDestroyed) {
            ProcessUnitCasualties(_humanUnit);
            ProcessUnitCasualties(RpgGameState.arenaUnit1);
            ProcessUnitCasualties(RpgGameState.arenaUnit2);
        } else if (RpgGameState.transition == RpgGameState.MapTransition.BaseAttackSimulation) {
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase);
            ProcessUnitCasualties(RpgGameState.arenaUnit1);
            if (RpgGameState.arenaUnit1.owner == Faction.Krigia && !RpgGameState.arenaUnit1.deleted) {
                MarkStarBaseAsDiscovered(RpgGameState.garrisonStarBase);
            }
        }
        if (RpgGameState.arenaUnit1 != null && IsRandomEventFaction(RpgGameState.arenaUnit1.owner)) {
            RpgGameState.arenaUnit1.deleted = true;
        }
        if (RpgGameState.arenaUnit2 != null && IsRandomEventFaction(RpgGameState.arenaUnit1.owner)) {
            RpgGameState.arenaUnit2.deleted = true;
        }
        RpgGameState.garrisonStarBase = null;
        RpgGameState.arenaUnit1 = null;
        RpgGameState.arenaUnit2 = null;
        _gameState.CollectGarbage();

        RenderMap();

        _movementToggle = GetNode<TextureButton>("UI/MovementToggle");
        _movementToggle.Connect("pressed", this, nameof(OnMovementTogglePressed));

        var modeIdleToggle = GetNode<TextureButton>("UI/Modes/IdleModeToggle");
        modeIdleToggle.Connect("pressed", this, nameof(OnModeTogglePressed),
            new Godot.Collections.Array { UnitMode.Idle });

        var modeAttackToggle = GetNode<TextureButton>("UI/Modes/AttackModeToggle");
        modeAttackToggle.Connect("pressed", this, nameof(OnModeTogglePressed),
            new Godot.Collections.Array { UnitMode.Attack });

        var modeSearchToggle = GetNode<TextureButton>("UI/Modes/SearchModeToggle");
        modeSearchToggle.Connect("pressed", this, nameof(OnModeTogglePressed),
            new Godot.Collections.Array { UnitMode.Search });

        _modeToggles = new Dictionary<UnitMode, TextureButton>{
            {UnitMode.Idle, modeIdleToggle},
            {UnitMode.Attack, modeAttackToggle},
            {UnitMode.Search, modeSearchToggle},
        };

        _modeToggled = _modeToggles[_gameState.mapState.mode];
        SwitchButtonTextures(_modeToggled);

        AddUnitMembers();

        GetNode<TextureButton>("UI/EnterBaseButton").Connect("pressed", this, nameof(OnEnterBaseButton));
        GetNode<TextureButton>("UI/MiningButton").Connect("pressed", this, nameof(OnMiningButton));
        GetNode<TextureButton>("UI/ActionMenuButton").Connect("pressed", this, nameof(OnActionMenuButton));
        GetNode<TextureButton>("UI/ResearchButton").Connect("pressed", this, nameof(OnResearchButton));

        _patrolReachesBasePopup = GetNode<PopupNode>("UI/PatrolReachesBasePopup");
        _patrolReachesBasePopup.GetNode<ButtonNode>("AttackButton").Connect("pressed", this, nameof(OnPatrolReachesBaseAttackButton));
        _patrolReachesBasePopup.GetNode<ButtonNode>("IgnoreButton").Connect("pressed", this, nameof(OnPatrolReachesBaseIgnoreButton));

        _researchCompletedPopup = GetNode<PopupNode>("UI/ResearchCompletedPopup");
        _researchCompletedPopup.GetNode<ButtonNode>("DoneButton").Connect("pressed", this, nameof(OnResearchCompleteDoneButton));

        _starBaseAttackPopup = GetNode<PopupNode>("UI/BaseUnderAttackPopup");
        _starBaseAttackPopup.GetNode<ButtonNode>("PlayButton").Connect("pressed", this, nameof(OnStarBaseAttackPlayButton));

        _krigiaTaskForcePopup = GetNode<PopupNode>("UI/KrigiaTaskForcePopup");
        _krigiaTaskForcePopup.GetNode<ButtonNode>("FightButton").Connect("pressed", this, nameof(OnFightEventUnit));
        _krigiaTaskForcePopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnKrigiaTaskForceLeaveButton));

        _krigiaPatrolPopup = GetNode<PopupNode>("UI/KrigiaPatrolPopup");
        _krigiaPatrolPopup.GetNode<ButtonNode>("FightButton").Connect("pressed", this, nameof(OnFightEventUnit));
        _krigiaPatrolPopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnKrigiaPatrolLeaveButton));

        _scavengersEventPopup = GetNode<PopupNode>("UI/ScavengersPopup");
        _scavengersEventPopup.GetNode<ButtonNode>("FightButton").Connect("pressed", this, nameof(OnFightEventUnit));
        _scavengersEventPopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnScavengersLeaveButton));

        _battleResult = GetNode<PopupNode>("UI/BattleResultPopup");
        _battleResult.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnBattleResultsDone));

        _cheatsPopup = GetNode<MapViewCheatMenuPopup>("UI/CheatMenuPopup");
        _cheatsPopup.Connect("CommandExecuted", this, nameof(OnCheatCommandExecuted));
        _cheatsPopup.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnCheatsDone));

        _randomEventPopup = GetNode<PopupNode>("UI/RandomEventPopup");
        _randomEventPopup.GetNode<ButtonNode>("Action1").Connect("pressed", this, nameof(OnRandomEventAction), new Godot.Collections.Array { 0 });
        _randomEventPopup.GetNode<ButtonNode>("Action2").Connect("pressed", this, nameof(OnRandomEventAction), new Godot.Collections.Array { 1 });
        _randomEventPopup.GetNode<ButtonNode>("Action3").Connect("pressed", this, nameof(OnRandomEventAction), new Godot.Collections.Array { 2 });
        _randomEventPopup.GetNode<ButtonNode>("Action4").Connect("pressed", this, nameof(OnRandomEventAction), new Godot.Collections.Array { 3 });

        _randomEventResolvedPopup = GetNode<PopupNode>("UI/RandomEventResolvedPopup");
        _randomEventResolvedPopup.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnRandomEventResolvedDone));

        _starSystemMenu = GetNode<PopupNode>("UI/StarSystemMenuPopup");
        _starSystemMenu.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnStarSystemMenuDone));
        _starSystemMenu.GetNode<ButtonNode>("ConvertPower").Connect("pressed", this, nameof(OnConvertPower));
        _starSystemMenu.GetNode<ButtonNode>("BuildNewBase").Connect("pressed", this, nameof(OnBuildNewBase));

        _fleetAttackPopup = GetNode<Popup>("UI/FleetAttackPopup");
        _fleetAttackPopup.GetNode<Button>("FightButton").Connect("pressed", this, nameof(OnFightButton));
        _fleetAttackPopup.GetNode<Button>("RetreatButton").Connect("pressed", this, nameof(OnRetreatButton));

        _miningPopup = GetNode<Popup>("UI/MiningPopup");
        _miningPopup.GetNode<Button>("Done").Connect("pressed", this, nameof(OnMiningDoneButton));
        _miningPopup.GetNode<Button>("LoadMinerals").Connect("pressed", this, nameof(OnMiningLoadMinerals));
        _miningPopup.GetNode<Button>("LoadOrganic").Connect("pressed", this, nameof(OnMiningLoadOrganic));
        _miningPopup.GetNode<Button>("LoadPower").Connect("pressed", this, nameof(OnMiningLoadPower));
        _miningPopup.GetNode<Button>("LoadAll").Connect("pressed", this, nameof(OnMiningLoadAll));
        for (int i = 0; i < 3; i++) {
            var args = new Godot.Collections.Array { i };
            _miningPopup.GetNode<Button>($"Planet{i}/SendRecall").Connect("pressed", this, nameof(OnMiningSendRecallButton), args);
        }

        _camera = GetNode<Camera2D>("Camera");
        _camera.LimitLeft = 0;
        _camera.LimitRight = (int)MAP_WIDTH;
        _camera.Position = new Vector2(_humanUnit.pos.x, GetViewport().Size.y / 2);
        _cameraSpeed = new Vector2(256, 0);

        if (RpgGameState.transition == RpgGameState.MapTransition.EnemyBaseAttackRepelled) {
            ShowBattleResults();
        } else if (RpgGameState.transition == RpgGameState.MapTransition.EnemyUnitDestroyed) {
            ShowBattleResults();
        } else if (RpgGameState.transition == RpgGameState.MapTransition.UnitDestroyed) {
            _lockControls = true;
            _human.node.Visible = false;
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/unit_destroyed.wav"));
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/mission_failed.wav"));
        }

        if (_currentSystem != null) {
            _currentSystem.OnPlayerEnter(Faction.Human);
        }
        UpdateUI();

        _menuNode = GameMenuNode.New();
        GetNode<CanvasLayer>("UI").AddChild(_menuNode);
        _menuNode.Connect("Closed", this, nameof(OnGameMenuClosed));
    }

    private List<Vessel.Ref> RemoveCasualties(List<Vessel.Ref> fleet) {
        var result = new List<Vessel.Ref>();
        foreach (var v in fleet) {
            if (v.Get().hp > 0) {
                result.Add(v);
                continue;
            }
            v.Get().deleted = true;
        }
        return result;
    }

    private void ProcessStarBaseCasualties(StarBase starBase) {
        starBase.garrison = RemoveCasualties(starBase.garrison);
        foreach (var v in starBase.garrison) {
            v.Get().energy = v.Get().GetEnergySource().maxBackupEnergy;
        }
    }

    private void ProcessUnitCasualties(SpaceUnit unit) {
        if (unit == null) {
            return;
        }
        unit.fleet = RemoveCasualties(unit.fleet);
        if (unit.fleet.Count == 0) {
            unit.deleted = true;
        }
    }

    private void OnStarBaseAttackPlayButton() {
        var u = _eventUnit;
        RpgGameState.arenaUnit1 = u.unit;

        // TODO: allow units selection?
        var system = RpgGameState.starSystemByPos[u.unit.pos];
        var starBase = system.starBase.Get();
        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.garrison[i].Get());
        }
        RpgGameState.garrisonStarBase = starBase;

        SetArenaSettings(system, ConvertVesselList(u.unit.fleet), defenders);
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnFightEventUnit() {
        var u = _eventUnit;
        RpgGameState.arenaUnit1 = u.unit;
        SetArenaSettings(_currentSystem.sys, ConvertVesselList(u.unit.fleet), ConvertVesselList(_humanUnit.fleet));
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private float RetreatFuelCost() {
        if (_gameState.skillsLearned.Contains("Escape Tactics")) {
            return 35;
        }
        return 70;
    }

    private void OnScavengersLeaveButton() {
        _lockControls = false;
        _scavengersEventPopup.Hide();

        var u = (ScavengerSpaceUnitNode)_eventUnit;
        if (ScavengersWantToAttack(u)) {
            _gameState.fuel -= RetreatFuelCost();
        } else {
            // Scare them off.
            u.unit.botSystemLeaveDelay = 0;
            u.PickNewWaypoint();
        }

        UpdateUI();
    }

    private void OnKrigiaPatrolLeaveButton() {
        _lockControls = false;
        _krigiaPatrolPopup.Hide();

        MarkStarBaseAsDiscovered(_currentSystem.sys.starBase.Get());

        _gameState.fuel -= RetreatFuelCost();
        UpdateUI();
    }

    private void OnKrigiaTaskForceLeaveButton() {
        _lockControls = false;
        _krigiaTaskForcePopup.Hide();
        _gameState.fuel -= RetreatFuelCost();
        UpdateUI();

        if (_currentSystem.sys.starBase.id != 0) {
            TaskForceAttacksHumanBase(_currentSystem.sys.starBase.Get(), _eventUnit);
        }
    }

    private void TaskForceAttacksHumanBase(StarBase starBase, SpaceUnitNode taskForce) {
        if (starBase.garrison.Count != 0) {
            var roll = QRandom.Float();
            if (roll < 0.25) {
                TriggerBaseAttackEvent(taskForce);
            }
            return;
        }

        var damage = taskForce.unit.fleet.Count;
        starBase.hp -= damage;
        if (starBase.hp <= 0) {
            // TODO: create notification.
            StopMovement();
            RpgGameState.humanBases.Remove(starBase);
            _starSystemNodeByStarSystem[starBase.system.Get()].DestroyStarBase();
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/base_eradicated.wav"));
        }
    }

    private void ShowBattleResults() {
        _lockControls = true;

        var listNode = _battleResult.GetNode<VBoxContainer>("List");
        var doneButton = _battleResult.GetNode<ButtonNode>("Done");

        var lines = new List<string>();

        var result = RpgGameState.lastBattleResult;

        if (result.exp != 0) {
            _gameState.experience += result.exp;
            lines.Add($"+{result.exp} experience");
        }
        if (result.fuel != 0) {
            _gameState.fuel += result.fuel;
            lines.Add($"+{result.fuel} fuel units");
        }

        if (result.genericDebris != 0) {
            _humanUnit.CargoAddDebris(result.genericDebris, Research.Material.None);
            lines.Add($"+{result.genericDebris} debris");
        }
        if (result.krigiaDebris != 0) {
            _humanUnit.CargoAddDebris(result.krigiaDebris, Research.Material.Krigia);
            lines.Add($"+{result.krigiaDebris} Krigia debris");
        }
        if (result.wertuDebris != 0) {
            _humanUnit.CargoAddDebris(result.wertuDebris, Research.Material.Wertu);
            lines.Add($"+{result.wertuDebris} Wertu debris");
        }
        if (result.zythDebris != 0) {
            _humanUnit.CargoAddDebris(result.zythDebris, Research.Material.Zyth);
            lines.Add($"+{result.zythDebris} Zyth debris");
        }

        if (result.minerals != 0) {
            _humanUnit.CargoAddMinerals(result.minerals);
            lines.Add($"+{result.minerals} mineral resouce");
        }
        if (result.organic != 0) {
            _humanUnit.CargoAddOrganic(result.organic);
            lines.Add($"+{result.organic} organic resource");
        }
        if (result.power != 0) {
            _humanUnit.CargoAddPower(result.power);
            lines.Add($"+{result.power} power resource");
        }

        if (!string.IsNullOrEmpty(result.technology)) {
            _gameState.technologiesResearched.Add(result.technology);
            lines.Add($"{result.technology} unlocked");
        }

        var offsetY = 36 * (lines.Count - 1);
        _battleResult.RectSize += new Vector2(0, offsetY);
        listNode.RectSize += new Vector2(0, offsetY); ;
        doneButton.RectPosition += new Vector2(0, offsetY);

        foreach (var l in lines) {
            var label = new Label();
            label.Text = l;
            label.Align = Label.AlignEnum.Center;
            label.Valign = Label.VAlign.Center;
            label.RectMinSize = new Vector2(0, 32);
            listNode.AddChild(label);
        }

        _battleResult.PopupCentered();
    }

    private void OnBattleResultsDone() {
        _lockControls = false;
        _battleResult.Hide();
    }

    private void OpenCheats() {
        _lockControls = true;
        StopMovement();
        _cheatsPopup.PopupCentered();
    }

    private void OnCheatCommandExecuted(MapViewCheatMenuPopup.Command command) {
        switch (command.kind) {
            case CheatCommandKind.ResearchComplete:
                OnCheatsDone();
                ResearchCompleted();
                return;

            case CheatCommandKind.CurrentSystemChange:
                _currentSystem.OnPlayerEnter(Faction.Human);
                UpdateUI();
                return;

            case CheatCommandKind.StatsChange:
                UpdateUI();
                return;

            case CheatCommandKind.CallRandomEvent:
                OnCheatsDone();
                _randomEvent = (RandomEvent)command.value;
                OpenRandomEvent();
                return;
        }
    }

    private RandomEventContext NewRandomEventContext() {
        return new RandomEventContext { roll = QRandom.Float() };
    }

    private void OnCheatsDone() {
        _lockControls = false;
        _cheatsPopup.Hide();
    }

    private void OpenRandomEvent() {
        StopMovement();

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));

        _randomEventPopup.GetNode<Label>("Title").Text = _randomEvent.title;
        _randomEventContext = NewRandomEventContext();

        var text = _randomEvent.text;
        var extraText = _randomEvent.extraText(_randomEventContext);
        if (!extraText.Empty()) {
            text += "\n\n" + extraText;
        }
        _randomEventPopup.GetNode<Label>("Text").Text = text;

        for (int i = 0; i < 4; i++) {
            var button = _randomEventPopup.GetNode<ButtonNode>($"Action{i + 1}");
            if (_randomEvent.actions.Count > i) {
                var a = _randomEvent.actions[i];
                button.Disabled = !a.condition();
                button.Visible = true;

                var buttonText = a.name;
                var hint = a.hint();
                if (hint != "") {
                    buttonText += " " + hint;
                }
                button.Text = buttonText;
            } else {
                button.Disabled = true;
                button.Visible = false;
            }
        }

        _lockControls = true;
        _randomEventPopup.PopupCentered();
    }

    private void OnRandomEventResolvedDone() {
        _lockControls = false;
        _randomEventResolvedPopup.Hide();

        switch (_randomEventResolutionPostEffect) {
            case RandomEvent.EffectKind.EnterArena:
                GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
                return;
        }
    }

    private void OnRandomEventAction(int actionIndex) {
        var eventResult = _randomEvent.actions[actionIndex].apply(_randomEventContext);
        var outcomeText = "<" + _randomEvent.actions[actionIndex].name + ">";
        outcomeText += "\n\n" + eventResult.text;
        outcomeText += $"\n\nGained {_randomEvent.expReward} experience points.";

        _gameState.experience += _randomEvent.expReward;

        foreach (var effect in eventResult.effects) {
            ExecuteEffect(effect);
        }

        _randomEventResolvedPopup.GetNode<Label>("Title").Text = _randomEvent.title;
        _randomEventResolvedPopup.GetNode<Label>("Text").Text = outcomeText;

        _randomEvent = null;

        _randomEventPopup.Hide();
        _randomEventResolvedPopup.PopupCentered();
        UpdateUI();
    }

    private void ExecuteEffect(RandomEvent.Effect effect) {
        switch (effect.kind) {
            case RandomEvent.EffectKind.AddTechnology:
                _gameState.technologiesResearched.Add((string)effect.value);
                return;

            case RandomEvent.EffectKind.AddVesselToFleet:
                AddUnitMember((Vessel)effect.value);
                ReorderUnitMembers();
                _gameState.humanUnit.Get().fleet.Add(((Vessel)effect.value).GetRef());
                return;

            case RandomEvent.EffectKind.AddCredits:
                _gameState.credits += (int)effect.value;
                return;
            case RandomEvent.EffectKind.AddMinerals:
                _humanUnit.CargoAddMinerals((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddOrganic:
                _humanUnit.CargoAddOrganic((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddPower:
                _humanUnit.CargoAddPower((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddFlagshipBackupEnergy:
                _humanUnit.fleet[0].Get().AddEnergy((float)effect.value);
                return;
            case RandomEvent.EffectKind.AddWertuReputation:
                _gameState.wertuReputation += (int)effect.value;
                return;
            case RandomEvent.EffectKind.AddKrigiaReputation:
                _gameState.krigiaReputation += (int)effect.value;
                return;
            case RandomEvent.EffectKind.SpendAnyVesselBackupEnergy:
                foreach (var v in _humanUnit.fleet) {
                    if (v.Get().energy >= (int)effect.value) {
                        v.Get().energy -= (int)effect.value;
                        break;
                    }
                }
                return;
            case RandomEvent.EffectKind.AddFuel:
                RpgGameState.AddFuel((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddFleetBackupEnergyPercentage: {
                    var randRange = (Vector2)effect.value;
                    foreach (var handle in _humanUnit.fleet) {
                        var v = handle.Get();
                        var roll = QRandom.FloatRange(randRange.x, randRange.y);
                        v.energy = QMath.ClampMin(v.energy - v.energy * roll, 0);
                    }
                    return;
                }

            case RandomEvent.EffectKind.AddKrigiaMaterial:
                _gameState.krigiaMaterial += (int)effect.value;
                return;

            case RandomEvent.EffectKind.DamageFlagshipPercentage: {
                var randRange = (Vector2)effect.value;
                var flagship = _humanUnit.fleet[0].Get();
                flagship.hp -= flagship.hp * QRandom.FloatRange(randRange.x, randRange.y);
                return;
            }

            case RandomEvent.EffectKind.DamageFleetPercentage: {
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
            case RandomEvent.EffectKind.TeleportToSystem:
                var targetSys = (StarSystem)effect.value;
                _humanUnit.pos = targetSys.pos;
                _human.GlobalPosition = _humanUnit.pos;
                _human.node.GlobalPosition = _humanUnit.pos;
                EnterSystem(targetSys);
                return;
            case RandomEvent.EffectKind.ApplySlow:
                _gameState.travelSlowPoints += (int)effect.value;
                return;

            case RandomEvent.EffectKind.EnterArena: {
                    var unit = (SpaceUnit)effect.value;
                    RpgGameState.arenaUnit1 = unit;
                    if (effect.value2 != null) {
                        RpgGameState.arenaUnit2 = (SpaceUnit)effect.value2;
                        SetStagedArenaSettings(_currentSystem.sys, unit, RpgGameState.arenaUnit2, _humanUnit);
                    } else {
                        // A normal battle.
                        SetArenaSettings(_currentSystem.sys, ConvertVesselList(unit.fleet), ConvertVesselList(_humanUnit.fleet));
                    }
                    _randomEventResolutionPostEffect = effect.kind;
                    return;
                }
        }
    }

    private void AddUnitMembers() {
        foreach (var handle in _humanUnit.fleet) {
            AddUnitMember(handle.Get());
        }
        ReorderUnitMembers();
    }

    private void AddUnitMember(Vessel v) {
        var unitMembers = GetNode<Label>("UI/UnitMembers");
        var box = unitMembers.GetNode<VBoxContainer>("Box");

        var hpPercentage = QMath.Percantage(v.hp, v.Design().maxHp);
        var energyPercentage = QMath.Percantage(v.energy, v.GetEnergySource().maxBackupEnergy);
        var m = UnitMemberNode.New(v.pilotName, v.Design().Texture(), hpPercentage, energyPercentage);
        _unitMembers.Add(m);
        box.AddChild(m);
    }

    private void ReorderUnitMembers() {
        var offsetY = 80;
        for (int i = 0; i < _unitMembers.Count; i++) {
            _unitMembers[i].Position = new Vector2(64, (offsetY * i) + 24);
        }
    }

    private void OnStarSystemMenuDone() {
        _lockControls = false;
        _starSystemMenu.Hide();
    }

    private void OnConvertPower() {
        if (_humanUnit.cargo.power < 5) {
            return;
        }
        _humanUnit.cargo.power -= 5;
        _gameState.fuel += 20;
        UpdateUI();
    }

    private bool CanBuildStarBase() {
        return _currentSystem != null && _currentSystem.sys.starBase.id == 0;
    }

    private void OnBuildNewBase() {
        var arkIndex = ArkVesselIndex();
        if (arkIndex == -1 || !CanBuildStarBase()) {
            return;
        }

        _humanUnit.fleet.RemoveAt(arkIndex);
        _unitMembers[arkIndex].QueueFree();
        _unitMembers.RemoveAt(arkIndex);
        ReorderUnitMembers();

        var starBase = _gameState.starBases.New();
        starBase.owner = Faction.Human;
        starBase.level = 1;
        starBase.mineralsStock = 0;
        starBase.organicStock = 0;
        starBase.powerStock = 0;
        starBase.system = _currentSystem.sys.GetRef();
        RpgGameState.humanBases.Add(starBase);
        _currentSystem.sys.starBase = starBase.GetRef();

        var starBaseNode = NewStarBaseNode(_currentSystem.sys);
        AddChild(starBaseNode);
        _currentSystem.AddStarBase(starBaseNode);
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/construction_completed.wav"));

        UpdateUI();
    }

    private void OnActionMenuButton() {
        _lockControls = true;
        StopMovement();
        _starSystemMenu.PopupCentered();
    }

    private void OnEnterBaseButton() {
        var starBase = _currentSystem.sys.starBase.Get();
        RpgGameState.enteredBase = starBase;
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }

    private void OnResearchButton() {
        StopMovement();
        GetTree().ChangeScene("res://scenes/ResearchScreen.tscn");
    }

    private void OnMiningButton() {
        _lockControls = true;
        StopMovement();
        _miningPopup.PopupCentered();
    }

    private void OnMiningSendRecallButton(int planetIndex) {
        var planetRow = _miningPopup.GetNode<Node2D>($"Planet{planetIndex}");
        var p = _currentSystem.sys.resourcePlanets[planetIndex];
        if (p.hasMine) {
            p.hasMine = false;
            _gameState.drones++;
            RpgGameState.planetsWithMines.Remove(p);
        } else {
            p.hasMine = true;
            _gameState.drones--;
            RpgGameState.planetsWithMines.Add(p);
        }
        UpdateUI();
    }

    private void OnMiningDoneButton() {
        _lockControls = false;
        _miningPopup.Hide();
    }

    private void MiningLoadMinerals() {
        var freeSpace = _humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.mineralsCollected ? p.mineralsCollected : freeSpace;
            p.mineralsCollected -= loadAmount;
            _humanUnit.cargo.minerals += loadAmount;
            freeSpace -= loadAmount;
        }
    }

    private void MiningLoadOrganic() {
        var freeSpace = _humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.organicCollected ? p.organicCollected : freeSpace;
            p.organicCollected -= loadAmount;
            _humanUnit.cargo.organic += loadAmount;
            freeSpace -= loadAmount;
        }
    }

    private void MiningLoadPower() {
        var freeSpace = _humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.powerCollected ? p.powerCollected : freeSpace;
            p.powerCollected -= loadAmount;
            _humanUnit.cargo.power += loadAmount;
            freeSpace -= loadAmount;
        }
    }

    private void OnMiningLoadMinerals() {
        MiningLoadMinerals();
        UpdateUI();
    }

    private void OnMiningLoadOrganic() {
        MiningLoadOrganic();
        UpdateUI();
    }

    private void OnMiningLoadPower() {
        MiningLoadPower();
        UpdateUI();
    }

    private void OnMiningLoadAll() {
        MiningLoadPower();
        MiningLoadOrganic();
        MiningLoadMinerals();
        UpdateUI();
    }

    private void OnFightButton() {
        _lockControls = false;
        _fleetAttackPopup.Hide();
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnRetreatButton() {
        _lockControls = false;
        _fleetAttackPopup.Hide();
        _gameState.fuel -= RetreatFuelCost();
        UpdateUI();
    }

    private void OnMovementTogglePressed() {
        if (_lockControls) {
            return;
        }
        _gameState.mapState.movementEnabled = !_gameState.mapState.movementEnabled;
    }

    private void StopMovement() {
        _movementToggle.Pressed = false;
        _gameState.mapState.movementEnabled = false;
    }

    private void PanCamera(float delta) {
        if (_lockControls) {
            return;
        }

        var cameraPos = _camera.Position;

        var viewport = GetViewport();
        var leftMargin = 40;
        var rightMargin = viewport.Size.x - 40;
        var cursor = viewport.GetMousePosition();

        if (cursor.x < leftMargin) {
            if (cursor.x < 8) {
                cameraPos -= _cameraSpeed * delta * 3;
            } else {
                cameraPos -= _cameraSpeed * delta;
            }
        } else if (cursor.x > rightMargin) {
            if (cursor.x > (viewport.Size.x - 8)) {
                cameraPos += _cameraSpeed * delta * 3;
            } else {
                cameraPos += _cameraSpeed * delta;
            }
        }

        if (cameraPos == _camera.Position) {
            return;
        }

        var x = QMath.Clamp(cameraPos.x, viewport.Size.x / 2, MAP_WIDTH - viewport.Size.x / 2);
        var y = cameraPos.y;
        _camera.Position = new Vector2(x, y);
    }

    private void OnGameMenuClosed() {
        _lockControls = false;
    }

    private void OpenGameMenu() {
        _lockControls = true;
        StopMovement();
        _menuNode.Open();
    }

    public override void _Process(float delta) {
        PanCamera(delta);

        if (!_lockControls && Input.IsActionJustPressed("escape")) {
            OpenGameMenu();
        }

        if (!_lockControls && Input.IsActionJustPressed("mapMovementToggle")) {
            _movementToggle.Pressed = !_movementToggle.Pressed;
            _movementToggle.EmitSignal("pressed");
        }

        if (!_lockControls && Input.IsActionJustPressed("openConsole")) {
            OpenCheats();
        }

        if (_gameState.mapState.movementEnabled) {
            if (_currentSystem != null && _human.node.GlobalPosition != _currentSystem.GlobalPosition) {
                _currentSystem = null;
            }
            float daysPerSecond = 5;
            _dayCounter += delta * daysPerSecond;
            if (_dayCounter > 1) {
                _dayCounter -= 1;
                _gameState.day++;
                ProcessDayEvents();
                UpdateUI();
            }
        }
    }

    private void OnStarClicked(StarSystemNode sys) {
        if (_lockControls) {
            return;
        }

        if (_human.node.GlobalPosition == sys.GlobalPosition) {
            _human.UnsetDestination();
            return;
        }

        if (_human.node.GlobalPosition.DistanceTo(sys.GlobalPosition) < ((_gameState.fuel * 2) - 1)) {
            _dstSystem = sys;
            _human.SetDestination(sys.GlobalPosition);
            AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/movement_ok.wav")));
        } else {
            AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/movement_error.wav")));
        }
    }

    private void OnSpaceUnitAttackStarBase(SpaceUnitNode unitNode) {
        if (unitNode.unit.owner != Faction.Krigia) {
            // Other race task forces are not implemented yet.
            throw new Exception("unexpected star base attack from " + unitNode.unit.owner.ToString());
        }

        if (unitNode.unit.pos == _humanUnit.pos) {
            TriggerKrigiaTaskForceEvent(unitNode);
            return;
        }
        var sys = RpgGameState.starSystemByPos[unitNode.unit.pos];
        TaskForceAttacksHumanBase(sys.starBase.Get(), unitNode);
    }

    private void OnSpaceUnitRemoved(SpaceUnitNode unitNode) {
        _spaceUnits.Remove(unitNode);
    }

    private void OnDroneDestroyed(SpaceUnitNode unitNode) {
        _gameState.dronesOwned--;
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapBadNotificationNode.New("Drone destroyed");
        AddChild(notification);
        notification.GlobalPosition = unitNode.GlobalPosition;
    }

    private void MarkStarBaseAsDiscovered(StarBase starBase) {
        if (starBase.discoveredByKrigia != 0) {
            return;
        }
        starBase.discoveredByKrigia = _gameState.day;
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapBadNotificationNode.New("Base detected");
        AddChild(notification);
        notification.GlobalPosition = starBase.system.Get().pos;
    }

    private void OnSearchForStarBase(SpaceUnitNode unitNode) {
        var sys = RpgGameState.starSystemByPos[unitNode.unit.pos];
        var starBase = sys.starBase.Get();
        if (starBase.garrison.Count == 0) {
            MarkStarBaseAsDiscovered(starBase);
        } else {
            TriggerPatrolReachesBaseEvent(unitNode);
        }
    }

    private void AddSpaceUnit(SpaceUnitNode unitNode) {
        var args = new Godot.Collections.Array{unitNode};
        unitNode.Connect("AttackStarBase", this, nameof(OnSpaceUnitAttackStarBase), args);
        unitNode.Connect("Removed", this, nameof(OnSpaceUnitRemoved), args);
        unitNode.Connect("DroneDestroyed", this, nameof(OnDroneDestroyed), args);
        unitNode.Connect("SearchForStarBase", this, nameof(OnSearchForStarBase), args);

        AddChild(unitNode);
        // unitNode.GlobalPosition = unitNode.unit.pos;
        _spaceUnits.Add(unitNode);
    }

    private void OnSpaceUnitCreated(SpaceUnitNode unitNode) {
        AddSpaceUnit(unitNode);
    }

    private StarBaseNode NewStarBaseNode(StarSystem sys) {
        StarBaseNode baseNode;

        var starBase = sys.starBase.Get();
        if (starBase.owner == Faction.Human) {
            baseNode = HumanStarBaseNode.New(starBase);
        } else if (starBase.owner == Faction.Scavenger) {
            baseNode = ScavengerStarBaseNode.New(starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else if (starBase.owner == Faction.Krigia) {
            baseNode = KrigiaStarBaseNode.New(starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else {
            baseNode = StarBaseNode.New(starBase);
        }

        baseNode.Position = sys.pos;
        baseNode.Visible = false;

        return baseNode;
    }

    private void RenderMap() {
        StarSystemNode currentSystem = null;

        foreach (StarSystem sys in _gameState.starSystems.objects.Values) {
            StarBaseNode starBaseNode = null;
            if (sys.starBase.id != 0) {
                // Sync star base units. They could be destroyed.
                sys.starBase.Get().units.RemoveWhere((x) => {
                    return !_gameState.spaceUnits.Contains(x.id) || x.Get().deleted;
                });

                starBaseNode = NewStarBaseNode(sys);
                AddChild(starBaseNode);
            }
            var systemNode = StarSystemNode.New(sys, starBaseNode);
            _starSystemNodes.Add(systemNode);
            _starSystemNodeByStarSystem[sys] = systemNode;
            {
                var args = new Godot.Collections.Array { systemNode };
                systemNode.Connect("Clicked", this, nameof(OnStarClicked), args);
            }
            AddChild(systemNode);
            if (sys.pos == _humanUnit.pos) {
                currentSystem = systemNode;
            }
        }

        var playerUnit = SpaceUnitNode.New(_humanUnit);
        playerUnit.Connect("DestinationReached", this, nameof(OnDestinationReached));
        playerUnit.speed = CalculateFleetSpeed();
        AddChild(playerUnit);
        if (playerUnit.unit.waypoint != Vector2.Zero) {
            _dstSystem = _starSystemNodeByStarSystem[RpgGameState.starSystemByPos[playerUnit.unit.waypoint]];
        }
        _human = MapHumanNode.New(playerUnit);
        _human.GlobalPosition = _humanUnit.pos;
        playerUnit.GlobalPosition = _humanUnit.pos;
        AddChild(_human);

        _currentSystem = currentSystem;

        foreach (var u in _gameState.spaceUnits.objects.Values) {
            if (u.deleted) {
                throw new Exception("trying to add a deleted unit");
            }
            if (u.fleet.Count == 0) {
                throw new Exception("trying to add a unit with empty fleet");
            }
            if (u.owner == Faction.Scavenger) {
                AddSpaceUnit(ScavengerSpaceUnitNode.New(u));
            } else if (u.owner == Faction.Krigia) {
                AddSpaceUnit(KrigiaSpaceUnitNode.New(u));
            } else if (u.owner == Faction.Human) {
                // Handled above.
            } else {
                throw new Exception("unexpected unit owner: " + u.owner.ToString());
            }
        }
    }

    private void EnterSystem(StarSystem sys) {
        _gameState.travelSlowPoints = 0;
        _currentSystem = _starSystemNodeByStarSystem[sys];
        _dstSystem = null;
        _currentSystem.OnPlayerEnter(Faction.Human);
        if (sys.starBase.id != 0) {
            if (sys.starBase.Get().owner == Faction.Human) {
                RecoverFleetEnergy(_humanUnit.fleet);
            }
        }

        if (sys.color == StarColor.Purple) {
            var e = RandomEvent.eventByTitle["Purple System Visitor"];
            if (_gameState.randomEventsAvailable.Contains(e.title)) {
                _randomEvent = e;
                _gameState.randomEventsAvailable.Remove(_randomEvent.title);
                OpenRandomEvent();
            }
        } else if (_gameState.randomEventCooldown == 0 && sys.randomEventCooldown == 0) {
            var roll = QRandom.Float();
            if (roll < 0.5) {
                MaybeTriggerEnterSystemEvent(sys);
                // TODO: should depend on the game settings.
                sys.randomEventCooldown += QRandom.IntRange(250, 450);
                _gameState.randomEventCooldown += QRandom.IntRange(100, 150);
            }
        }
    }

    private void MaybeTriggerEnterSystemEvent(StarSystem sys) {
        if (_gameState.randomEventsAvailable.Count == 0) {
            return;
        }

        var enterSystemEvents = new List<RandomEvent>();
        foreach (var eventTitle in _gameState.randomEventsAvailable) {
            var e = RandomEvent.eventByTitle[eventTitle];
            if (e.trigger != RandomEvent.TriggerKind.OnSystemEntered) {
                continue;
            }
            if (!e.condition()) {
                continue;
            }
            enterSystemEvents.Add(e);
        }
        if (enterSystemEvents.Count == 0) {
            return;
        }

        _randomEvent = QRandom.Element(enterSystemEvents);
        _gameState.randomEventsAvailable.Remove(_randomEvent.title);
        OpenRandomEvent();
    }

    private void OnDestinationReached() {
        StopMovement();
        EnterSystem(_dstSystem.sys);
        UpdateUI();
    }

    private void UpdateUI() {
        GetNode<Label>("UI/FuelValue/Max").Text = "/" + ((int)RpgGameState.MaxFuel()).ToString();

        var enterBase = GetNode<TextureButton>("UI/EnterBaseButton");
        enterBase.Disabled = true;
        var mining = GetNode<TextureButton>("UI/MiningButton");
        mining.Disabled = true;

        _starSystemMenu.GetNode<ButtonNode>("ConvertPower").Disabled = _humanUnit.cargo.power < 5;
        _starSystemMenu.GetNode<ButtonNode>("BuildNewBase").Disabled = ArkVesselIndex() == -1 || !CanBuildStarBase();

        for (int i = 0; i < _unitMembers.Count; i++) {
            var p = _humanUnit.fleet[i].Get();
            var hpPercentage = QMath.Percantage(p.hp, p.Design().maxHp);
            var energyPercentage = QMath.Percantage(p.energy, p.GetEnergySource().maxBackupEnergy);
            var unit = _unitMembers[i];
            unit.UpdateStatus(hpPercentage, energyPercentage);
        }

        foreach (var u in _spaceUnits) {
            u.UpdateColor();
        }

        UpdateDayValue();
        UpdateFuelValue();
        UpdateExpValue();
        UpdateCreditsValue();
        UpdateCargoValue();
        UpdateDronesValue();
        if (_currentSystem != null) {
            GetNode<Label>("UI/LocationValue").Text = _currentSystem.sys.name;
            if (_currentSystem.sys.starBase.id != 0 && _currentSystem.sys.starBase.Get().owner == Faction.Human) {
                enterBase.Disabled = false;
            }
            // Can mine only in own or neutral systems.
            if (_currentSystem.sys.starBase.id == 0 || _currentSystem.sys.starBase.Get().owner == Faction.Human) {
                mining.Disabled = false;
            }

            var planets = _currentSystem.sys.resourcePlanets;
            for (int i = 0; i < 3; i++) {
                var planetRow = _miningPopup.GetNode<Node2D>($"Planet{i}");
                planetRow.Visible = i < planets.Count;
                if (i >= planets.Count) {
                    continue;
                }
                var p = planets[i];
                planetRow.GetNode<Label>("Name").Text = p.name;
                planetRow.GetNode<Label>("Minerals").Text = $"{p.mineralsPerDay}/day ({p.mineralsCollected})";
                planetRow.GetNode<Label>("Organic").Text = $"{p.organicPerDay}/day ({p.organicCollected})";
                planetRow.GetNode<Label>("Power").Text = $"{p.powerPerDay}/day ({p.powerCollected})";
                var sendrecall = planetRow.GetNode<Button>("SendRecall");
                if (p.hasMine) {
                    sendrecall.Text = "Recall Drone";
                    sendrecall.Disabled = false;
                } else {
                    sendrecall.Text = "Send Drone";
                    sendrecall.Disabled = _gameState.drones <= 0;
                }
            }
        } else {
            GetNode<Label>("UI/LocationValue").Text = "Interstellar Space";
        }

        _human.Update();
    }

    private void UpdateCargoValue() {
        int max = _humanUnit.CargoCapacity();
        var current = _humanUnit.CargoSize();
        GetNode<Label>("UI/CargoValue").Text = $"{current}/{max}";
    }

    private void UpdateExpValue() {
        GetNode<Label>("UI/ExpValue").Text = _gameState.experience.ToString();
    }

    private void UpdateCreditsValue() {
        GetNode<Label>("UI/CreditsValue").Text = _gameState.credits.ToString();
    }

    private void UpdateDayValue() {
        GetNode<Label>("UI/DayValue").Text = _gameState.day.ToString();
    }

    private void UpdateFuelValue() {
        GetNode<Label>("UI/FuelValue").Text = ((int)_gameState.fuel).ToString();
    }

    private void UpdateDronesValue() {
        var carrying = _gameState.drones;
        var limit = RpgGameState.MaxDrones();
        var text = $"{carrying} ({_gameState.dronesOwned}/{limit})";
        GetNode<Label>("UI/DronesValue").Text = text;
    }

    private void RecoverFleetEnergy(List<Vessel.Ref> fleet) {
        foreach (var handle in fleet) {
            var v = handle.Get();
            v.energy = v.GetEnergySource().maxBackupEnergy;
        }
    }

    private float CalculateFleetSpeed() {
        var travelSpeed = _gameState.travelSpeed;
        if (_gameState.travelSlowPoints > 0) {
            travelSpeed /= 2;
        }
        if (_gameState.skillsLearned.Contains("Navigation III")) {
            travelSpeed += travelSpeed * 0.25f;
        } else if (_gameState.skillsLearned.Contains("Navigation II")) {
            travelSpeed += travelSpeed * 0.2f;
        } else if (_gameState.skillsLearned.Contains("Navigation I")) {
            travelSpeed += travelSpeed * 0.15f;
        }
        return travelSpeed;
    }

    private void ProcessDayEvents() {
        _human.node.speed = CalculateFleetSpeed();

        _gameState.travelSlowPoints = QMath.ClampMin(_gameState.travelSlowPoints - 1, 0);
        _gameState.randomEventCooldown = QMath.ClampMin(_gameState.randomEventCooldown - 1, 0);

        ProcessStarSystems();
        ProcessMines();
        ProcessUnits();
        ProcessResearch();

        if (_currentSystem == null) {
            ProcessInterstellarDay();
        } else {
            ProcessStarSystemDay();
        }

        ProcessKrigiaActions();

        if (_gameState.day == _gameState.missionDeadline) {
            SpawnKrigiaFinalAttack(new Vector2(512, 224), RpgGameState.StartingSystem().pos);
        }
    }

    private void ProcessResearch() {
        if (_gameState.currentResearch == "") {
            return;
        }

        var research = Research.Find(_gameState.currentResearch);

        _gameState.researchProgress += RpgGameState.ResearchRate();

        if (research.material == Research.Material.Krigia) {
            _gameState.krigiaMaterial = QMath.ClampMin(_gameState.krigiaMaterial - 1, 0);
        } else if (research.material == Research.Material.Wertu) {
            _gameState.wertuMaterial = QMath.ClampMin(_gameState.wertuMaterial - 1, 0);
        } else if (research.material == Research.Material.Zyth) {
            _gameState.zythMaterial = QMath.ClampMin(_gameState.zythMaterial - 1, 0);
        }

        if (_gameState.scienceFunds > 0) {
            var roll = QRandom.IntRange(5, 20);
            _gameState.scienceFunds = QMath.ClampMin(_gameState.scienceFunds - roll, 0);
        }

        var researchTime = research.researchTime;
        if (_gameState.skillsLearned.Contains("Scholar")) {
            researchTime -= 10;
        }

        if ((int)_gameState.researchProgress >= researchTime) {
            ResearchCompleted();
        }
    }

    private void OnPatrolReachesBaseAttackButton() {
        var u = _eventUnit;
        RpgGameState.arenaUnit1 = u.unit;

        // TODO: allow units selection?
        // FIXME: code is duplicated from OnStarBaseAttackPlayButton().
        var system = RpgGameState.starSystemByPos[u.unit.pos];
        var starBase = system.starBase.Get();
        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.garrison[i].Get());
        }
        RpgGameState.garrisonStarBase = starBase;

        SetArenaSettings(system, ConvertVesselList(u.unit.fleet), defenders);
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnPatrolReachesBaseIgnoreButton() {
        _lockControls = false;
        _patrolReachesBasePopup.Hide();

        var system = RpgGameState.starSystemByPos[_eventUnit.unit.pos];
        var starBase = system.starBase.Get();
        MarkStarBaseAsDiscovered(starBase);
    }

    private void OnResearchCompleteDoneButton() {
        _lockControls = false;
        _researchCompletedPopup.Hide();
    }

    private void ResearchCompleted() {
        var research = Research.Find(_gameState.currentResearch);

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/research_completed.wav"));

        Func<Research, bool> researchAvailable = r => Research.IsAvailable(_gameState.technologiesResearched, r.dependencies);
        var availableBefore = new HashSet<Research>(Research.list.FindAll(r => researchAvailable(r)));
        _gameState.technologiesResearched.Add(research.name);
        var availableAfter = new HashSet<Research>(Research.list.FindAll(r => researchAvailable(r)));
        availableAfter.ExceptWith(availableBefore);

        if (research.category == Research.Category.NewArtifact) {
            var artifact = ArtifactDesign.Find(research.name);
            _gameState.PutItemToStorage(artifact);
        }

        _gameState.researchProgress = 0;
        _gameState.currentResearch = "";

        StopMovement();
        UpdateUI();

        _researchCompletedPopup.GetNode<Label>("SubTitle").Text = research.name;
        var text = "";
        if (research.category == Research.Category.NewArtifact) {
            text += "New artifact is available for production.\n\n";
        } else if (research.category == Research.Category.NewEnergySource) {
            text += "New energy source is available for production.\n\n";
        } else if (research.category == Research.Category.NewShield) {
            text += "New shield is available for production.\n\n";
        } else if (research.category == Research.Category.NewSpecialWeapon) {
            text += "New special weapon is available for production.\n\n";
        } else if (research.category == Research.Category.NewVesselDesign) {
            text += "New vessel is available for production.\n\n";
        } else if (research.category == Research.Category.NewWeapon) {
            text += "New weapon is available for production.\n\n";
        } else if (research.category == Research.Category.NewSentinel) {
            text += "New sentinel is available for production.\n\n";
        } else if (research.category == Research.Category.Upgrade) {
            text += "Upgrade is now active.\n\n";
        }
        if (availableAfter.Count != 0) {
            text += "New research projects available:\n\n";
            foreach (var r in availableAfter) {
                text += r.name + "\n";
            }
        } else {
            text += "No new research projects available.";
        }

        _researchCompletedPopup.GetNode<Label>("Panel/Text").Text = text;

        _lockControls = true;
        _researchCompletedPopup.PopupCentered();
    }

    private void ProcessInterstellarDay() {
        // The space is empty.
    }

    private void ProcessStarSystemDay() {
        ProcessUnitMode();

        var starBase = _currentSystem.sys.starBase;
        if (starBase.id != 0) {
            if (starBase.Get().owner == Faction.Human) {
                RecoverFleetEnergy(_humanUnit.fleet);
            }
        }

        _currentSystem.UpdateInfo();
        _currentSystem.RenderKnownInfo();

        foreach (var u in _spaceUnits) {
            if (u.unit.pos == _currentSystem.sys.pos) {
                if (RollUnitAttack(u)) {
                    return;
                }
            }
        }

        if (starBase.id != 0) {
            if (_gameState.FactionsAtWar(starBase.Get().owner, Faction.Human)) {
                RollFleetAttack();
            }
        }
    }

    private bool RollUnitAttack(SpaceUnitNode u) {
        if (u.unit.owner == Faction.Scavenger) {
            TriggerScavengersEvent(u);
            return true;
        }
        if (u.unit.owner == Faction.Krigia) {
            if (u.unit.botProgram == SpaceUnit.Program.KrigiaPatrol) {
                TriggerKrigiaPatrolEvent(u);
            }
            return true;
        }

        return false;
    }

    private bool ScavengersWantToAttack(SpaceUnitNode u) {
        if (u.unit.CargoFree() == 0) {
            return false;
        }
        var scavengersForce = u.unit.FleetCost();
        var humanForce = _humanUnit.FleetCost();
        return scavengersForce * 2 > humanForce;
    }

    private void TriggerKrigiaPatrolEvent(SpaceUnitNode u) {
        _krigiaPatrolPopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RetreatFuelCost();

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _krigiaPatrolPopup.PopupCentered();
    }

    private void TriggerBaseAttackEvent(SpaceUnitNode u) {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/base_under_attack.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _starBaseAttackPopup.PopupCentered();
    }

    private void TriggerPatrolReachesBaseEvent(SpaceUnitNode u) {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _patrolReachesBasePopup.PopupCentered();
    }

    private void TriggerKrigiaTaskForceEvent(SpaceUnitNode u) {
        _krigiaTaskForcePopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RetreatFuelCost();

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _krigiaTaskForcePopup.PopupCentered();
    }

    private void TriggerScavengersEvent(SpaceUnitNode u) {
        var pluralSuffix = u.unit.fleet.Count == 1 ? "" : "s";
        var text = "Short-range radars detect the scavengers raid unit. ";
        text += "They have " + u.unit.fleet.Count + " vessel" + pluralSuffix + ".\n\n";
        if (ScavengersWantToAttack(u)) {
            text += "Based on the fact that it's moving towards your direction, ";
            text += "the battle is imminent, unless you sacrifice some fuel and warp away.";
            _scavengersEventPopup.GetNode<ButtonNode>("FightButton").Text = "Prepare for battle";
            _scavengersEventPopup.GetNode<ButtonNode>("LeaveButton").Text = "Retreat";
            _scavengersEventPopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RetreatFuelCost();
        } else {
            text += "It looks like they're not looking for a fight.";
            _scavengersEventPopup.GetNode<ButtonNode>("FightButton").Text = "Attack them";
            _scavengersEventPopup.GetNode<ButtonNode>("LeaveButton").Text = "Ignore them";
        }
        _scavengersEventPopup.GetNode<Label>("Panel/Text").Text = text;

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _scavengersEventPopup.PopupCentered();
    }

    private void ProcessUnits() {
        foreach (var u in _spaceUnits) {
            u.ProcessDay();

            // TODO: pass a star system node as an argument?
            // if (!_gameState.starSystemByPos.ContainsKey(u.unit.pos)) {
            //     u.ProcessDay(null);
            // } else {
            //     var sys = _gameState.starSystemByPos[u.unit.pos];
            //     var sysNode = _starSystems[sys.id];
            //     u.ProcessDay(sysNode);
            // }
        }
    }

    private List<Vessel> ConvertVesselList(List<Vessel.Ref> list) {
        var result = new List<Vessel>();
        foreach (var v in list) {
            result.Add(v.Get());
        }
        return result;
    }

    private void ProcessStarSystems() {
        foreach (StarSystemNode starSystem in _starSystemNodes) {
            starSystem.sys.randomEventCooldown = QMath.ClampMin(starSystem.sys.randomEventCooldown - 1, 0);
            starSystem.ProcessDay();
        }
    }

    private void ProcessMines() {
        foreach (ResourcePlanet p in RpgGameState.planetsWithMines) {
            p.mineralsCollected = QMath.ClampMax(p.mineralsCollected + p.mineralsPerDay, _gameState.limits.droneCapacity);
            p.organicCollected = QMath.ClampMax(p.organicCollected + p.organicPerDay, _gameState.limits.droneCapacity);
            p.powerCollected = QMath.ClampMax(p.powerCollected + p.powerPerDay, _gameState.limits.droneCapacity);
        }
    }

    private void ProcessUnitMode() {
        if (_gameState.mapState.mode == UnitMode.Idle) {
            var toAdd = _gameState.technologiesResearched.Contains("Recycling") ? 2 : 1;
            _gameState.fuel = QMath.ClampMax(_gameState.fuel + toAdd, RpgGameState.MaxFuel());
            return;
        }

        var starBaseRef = _currentSystem.sys.starBase;

        if (_gameState.mapState.mode == UnitMode.Attack) {
            if (_gameState.fuel < 1) {
                SetUnitMode(UnitMode.Idle);
                StopMovement();
                return;
            }
            if (starBaseRef.id == 0) {
                return;
            }
            var starBase = starBaseRef.Get();
            if (!_gameState.FactionsAtWar(starBase.owner, Faction.Human)) {
                return;
            }
            
            _gameState.fuel -= 1;
            if (starBase.garrison.Count != 0 && !_gameState.skillsLearned.Contains("Siege Mastery")) {
                return;
            }
            var damage = _humanUnit.fleet.Count;
            if (_gameState.skillsLearned.Contains("Siege Mastery")) {
                damage *= 2;
            }
            starBase.hp -= damage;
            if (starBase.hp <= 0) {
                SetUnitMode(UnitMode.Idle);
                StopMovement();
                _currentSystem.DestroyStarBase();
                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/enemy_base_eradicated.wav"));
            }
            return;
        }

        if (_gameState.mapState.mode == UnitMode.Search) {
            if (_gameState.fuel < 3) {
                SetUnitMode(UnitMode.Idle);
                StopMovement();
                return;
            }
            if (_currentSystem.sys.artifact == null) {
                return;
            }
            if (starBaseRef.id != 0 && _gameState.FactionsAtWar(starBaseRef.Get().owner, Faction.Human)) {
                _currentSystem.sys.artifactRecoveryDelay -= 1;
            } else {
                _currentSystem.sys.artifactRecoveryDelay -= 2;
            }
            if (_currentSystem.sys.artifactRecoveryDelay <= 0) {
                _gameState.artifactsRecovered.Add(_currentSystem.sys.artifact);

                _currentSystem.sys.artifactRecoveryDelay = 0;
                _currentSystem.sys.artifact = null;

                var notification = MapNotificationNode.New("Artifact recovered");
                _currentSystem.AddChild(notification);

                _gameState.credits += 3000;

                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_recovered.wav"));

                SetUnitMode(UnitMode.Idle);
                StopMovement();

                _currentSystem.UpdateInfo();
                _currentSystem.RenderKnownInfo();
            }
            _gameState.fuel -= 3;
            return;
        }
    }

    private int ArkVesselIndex() {
        // Start from 1, since flagship can't be used to build a base;
        // even if it's Ark.
        for (int i = 1; i < _humanUnit.fleet.Count; i++) {
            var v = _humanUnit.fleet[i].Get();
            if (v.Design().name == "Ark") {
                return i;
            }
        }
        return -1;
    }

    private Dictionary<Vessel, Vector2> DefaultSpawnMap(List<Vessel> enemyFleet, List<Vessel> alliedFleet) {
        var m = new Dictionary<Vessel, Vector2>();

        for (int i = 0; i < enemyFleet.Count; i++) {
            var v = enemyFleet[i];
            m[v] = QMath.RandomizedLocation(new Vector2(1568, 288 + (i * 192)), 40);
        }
        for (int i = 0; i < alliedFleet.Count; i++) {
            var v = alliedFleet[i];
            m[v] = QMath.RandomizedLocation(new Vector2(224, 288 + (i * 192)), 40);
        }

        return m;
    }

    private void SetArenaSettings(StarSystem location, List<Vessel> vessels, Dictionary<Vessel, Vector2> spawnMap, Dictionary<Vessel, int> alliances) {
        ArenaSettings.Reset();
        ArenaSettings.isQuickBattle = false;
        ArenaSettings.alliances = alliances;

        // TODO: respect the game settings here.
        ArenaSettings.numAsteroids = QRandom.IntRange(0, 3);
        // 30% - none
        // 20% - purple nebula
        // 20% - blue nebula
        // 30% - star
        var envHazardRoll = QRandom.Float();
        if (envHazardRoll < 0.3) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.None;
        } else if (envHazardRoll < 0.5) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.PurpleNebula;
        } else if (envHazardRoll < 0.7) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.BlueNebula;
        } else {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.Star;
        }
        ArenaSettings.starColor = location.color;

        foreach (var v in vessels) {
            v.spawnPos = spawnMap[v];
            ArenaSettings.combatants.Add(v);
            if (!v.isBot) {
                v.isGamepad = GameControls.preferGamepad;
            }
        }
    }

    private void SetArenaSettings(StarSystem location, List<Vessel> enemyFleet, List<Vessel> alliedFleet) {
        var spawnMap = DefaultSpawnMap(enemyFleet, alliedFleet);
        var vessels = new List<Vessel>();
        vessels.AddRange(enemyFleet);
        vessels.AddRange(alliedFleet);
        var alliances = new Dictionary<Vessel, int>();
        foreach (var v in vessels) {
            alliances[v] = _gameState.FactionsAtWar(Faction.Human, v.faction) ? 1 : 0;
        }
        SetArenaSettings(location, vessels, spawnMap, alliances);
    }

    private void SetStagedArenaSettings(StarSystem location, SpaceUnit bot1, SpaceUnit bot2, SpaceUnit human) {
        Func<Faction, int> factionToAlliance = (Faction f) => {
            if (f == Faction.RandomEventAlly) {
                return 0;
            }
            if (f == Faction.RandomEventHostile) {
                return 1;
            }
            return 2;
        };
        var alliances = new Dictionary<Vessel, int>();
        foreach (var vref in human.fleet) {
            alliances[vref.Get()] = 0;
        }
        foreach (var vref in bot1.fleet) {
            alliances[vref.Get()] = factionToAlliance(bot1.owner);
        }
        foreach (var vref in bot2.fleet) {
            alliances[vref.Get()] = factionToAlliance(bot2.owner);
        }

        var spawnMap = new Dictionary<Vessel, Vector2>();
        var vessels = new List<Vessel>();
        for (int i = 0; i < human.fleet.Count; i++) {
            var v = human.fleet[i].Get();
            spawnMap[v] = QMath.RandomizedLocation(new Vector2(224, 288 + (i * 192)), 40);
            vessels.Add(v);
        }
        foreach (var vref in bot1.fleet) {
            var v = vref.Get();
            spawnMap[v] = v.spawnPos;
            vessels.Add(v);
        }
        foreach (var vref in bot2.fleet) {
            var v = vref.Get();
            spawnMap[v] = v.spawnPos;
            vessels.Add(v);
        }

        SetArenaSettings(location, vessels, spawnMap, alliances);
    }

    private void RollFleetAttack() {
        var starBase = _currentSystem.sys.starBase.Get();

        if (starBase.garrison.Count == 0) {
            return;
        }

        if (_gameState.mapState.mode != UnitMode.Attack) {
            var roll = QRandom.Float();
            if (roll >= 0.25) {
                return;
            }
        }

        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.garrison[i].Get());
        }
        RpgGameState.garrisonStarBase = starBase;

        SetArenaSettings(_currentSystem.sys, defenders, ConvertVesselList(_humanUnit.fleet));

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/unit_under_attack.wav"));
        StopMovement();
        _lockControls = true;
        var pluralSuffix = numDefenders == 1 ? "" : "s";
        _fleetAttackPopup.GetNode<Label>("Attackers").Text = $"Attackers: {numDefenders} {starBase.owner.ToString()} ship" + pluralSuffix;
        _fleetAttackPopup.GetNode<Button>("RetreatButton").Disabled = _gameState.fuel < RetreatFuelCost();
        _fleetAttackPopup.PopupCentered();
    }

    private void KrigiaBaseRequestReinforcements(StarBase starBase) {
        if (starBase.botReinforcementsDelay > 0) {
            return;
        }
        starBase.botReinforcementsDelay = QRandom.IntRange(100, 200);

        var connectedSystems = RpgGameState.starSystemConnections[starBase.system.Get()];
        StarBase alliedBase = null;
        foreach (var sys in connectedSystems) {
            if (sys.starBase.id == 0 || sys.starBase.Get().owner != starBase.owner) {
                continue;
            }
            if (sys.starBase.Get().garrison.Count <= starBase.garrison.Count) {
                continue;
            }
            alliedBase = sys.starBase.Get();
            break;
        }
        if (alliedBase == null) {
            return;
        }

        var reinforcementsFleet = new List<Vessel.Ref>();
        var groupSize = QRandom.IntRange(2, 4);
        var keptInGarrison = alliedBase.garrison.FindAll(v => {
            if (v.Get().Design().level <= 2) {
                return true;
            }
            if (reinforcementsFleet.Count == groupSize) {
                return true;
            }
            reinforcementsFleet.Add(v);
            return false;
        });

        if (reinforcementsFleet.Count < groupSize) {
            return;
        }

        alliedBase.garrison = keptInGarrison;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Krigia;
        spaceUnit.pos = alliedBase.system.Get().pos;
        spaceUnit.waypoint = starBase.system.Get().pos;
        spaceUnit.botProgram = SpaceUnit.Program.KrigiaReinforcements;
        spaceUnit.botOrigin = alliedBase.GetRef();
        spaceUnit.fleet = reinforcementsFleet;

        alliedBase.units.Add(spaceUnit.GetRef());

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }

    private void SpawnKrigiaFinalAttack(Vector2 pos, Vector2 firstWaypoint) {
        var notification = MapBadNotificationNode.New("Krigia Flagship Arrives");
        AddChild(notification);
        notification.GlobalPosition = pos;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Krigia;
        spaceUnit.pos = pos;
        spaceUnit.waypoint = firstWaypoint;
        spaceUnit.botProgram = SpaceUnit.Program.KrigiaFinalAttack;

        spaceUnit.fleet = new List<Vessel.Ref>{
            VesselFactory.NewVessel(Faction.Krigia, "Ashes").GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns").GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns").GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns").GetRef(),
        };

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }

    private void ProcessKrigiaActions() {
        // 1 Choose a star base to attack
        // 2 If nearest Krigia star base has enough vessels, attack from there
        // 2.1 Otherwise send a reinforcement unit there from somewhere
        // 2.2 If it's impossible to get enough vessels to do an attack, drop the idea
        // 3 Send a task force unit from that Krigia star base

        _gameState.krigiaPlans.taskForceDelay = QMath.ClampMin(_gameState.krigiaPlans.taskForceDelay - 1, 0);

        if (_gameState.day < 450) {
            return;
        }

        if (_gameState.krigiaPlans.taskForceDelay != 0) {
            return;
        }

        var potentialTargets = new List<StarBase>();
        foreach (var starBase in RpgGameState.humanBases) {
            if (starBase.discoveredByKrigia == 0) {
                continue;
            }
            if (_gameState.day - starBase.discoveredByKrigia < 100) {
                continue;
            }
            potentialTargets.Add(starBase);
        }
        if (potentialTargets.Count == 0) {
            return;
        }

        var targetBase = QRandom.Element(potentialTargets);

        StarBase nearestStarBase = null;
        var connectedSystems = RpgGameState.starSystemConnections[targetBase.system.Get()];
        foreach (var sys in connectedSystems) {
            if (sys.starBase.id == 0) {
                continue;
            }
            var starBase = sys.starBase.Get();
            if (starBase.owner != Faction.Krigia) {
                continue;
            }
            if (nearestStarBase == null) {
                nearestStarBase = starBase;
            } else if (sys.pos.DistanceTo(targetBase.system.Get().pos) < nearestStarBase.system.Get().pos.DistanceTo(targetBase.system.Get().pos)) {
                nearestStarBase = starBase;
            }
        }
        if (nearestStarBase == null) {
            return; // Target system is out of reach
        }

        var taskForceFleet = new List<Vessel.Ref>();
        var groupSize = QRandom.IntRange(2, 4);
        var keptInGarrison = nearestStarBase.garrison.FindAll(v => {
            if (v.Get().Design().level <= 2) {
                return true;
            }
            if (taskForceFleet.Count == groupSize) {
                return true;
            }
            taskForceFleet.Add(v);
            return false;
        });

        if (taskForceFleet.Count < groupSize) {
            // Can't assemble a task force from this base.
            KrigiaBaseRequestReinforcements(nearestStarBase);
            _gameState.krigiaPlans.taskForceDelay = QRandom.IntRange(60, 90);
            return;
        }

        nearestStarBase.garrison = keptInGarrison;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Krigia;
        spaceUnit.pos = nearestStarBase.system.Get().pos;
        spaceUnit.waypoint = targetBase.system.Get().pos;
        spaceUnit.botProgram = SpaceUnit.Program.KrigiaTaskForce;
        spaceUnit.botOrigin = nearestStarBase.GetRef();
        spaceUnit.fleet = taskForceFleet;

        nearestStarBase.units.Add(spaceUnit.GetRef());
        _gameState.krigiaPlans.taskForceDelay = QRandom.IntRange(200, 300);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }
}
