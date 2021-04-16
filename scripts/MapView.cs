using Godot;
using System;
using System.Collections.Generic;
using CheatCommandKind = MapViewCheatMenuPopup.CommandKind;

public class MapView : Node2D {
    const float MAP_WIDTH = (1080 * 3) + 220;

    private RandomEvent _randomEvent;
    private RandomEventContext _randomEventContext;

    private float _dayCounter = 0;

    private MapHumanNode _human;

    private StarSystemNode _currentSystem;
    private StarSystemNode _dstSystem;

    private List<StarSystemNode> _starSystems = new List<StarSystemNode>();

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
    private MapViewCheatMenuPopup _cheatsPopup;

    private RandomEvent.EffectKind _randomEventResolutionPostEffect;

    private SpaceUnitNode _eventUnit;
    private PopupNode _starBaseAttackPopup;
    private PopupNode _scavengersEventPopup;
    private PopupNode _krigiaPatrolPopup;
    private PopupNode _krigiaTaskForcePopup;

    private HashSet<SpaceUnitNode> _spaceUnits = new HashSet<SpaceUnitNode>();
    private List<StarBaseNode> _starBases = new List<StarBaseNode>();

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
        RpgGameState.mapState.mode = newMode;
        _modeToggled = pressed;
    }

    private void OnModeTogglePressed(UnitMode newMode) {
        SetUnitMode(newMode);
    }

    public override void _Ready() {
        QRandom.SetRandomNumberGenerator(RpgGameState.rng);
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayMapMusic();

        if (RpgGameState.transition == RpgGameState.MapTransition.EnemyBaseAttackRepelled) {
            ProcessUnitCasualties(RpgGameState.humanUnit);
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase);
            RpgGameState.garrisonStarBase = null;
        } else if (RpgGameState.transition == RpgGameState.MapTransition.EnemyUnitDestroyed) {
            ProcessUnitCasualties(RpgGameState.humanUnit);
            ProcessUnitCasualties(RpgGameState.enemyAttackerUnit);
            RpgGameState.enemyAttackerUnit = null;
        } else if (RpgGameState.transition == RpgGameState.MapTransition.BaseAttackSimulation) {
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase);
            ProcessUnitCasualties(RpgGameState.enemyAttackerUnit);
            RpgGameState.enemyAttackerUnit = null;
            RpgGameState.garrisonStarBase = null;
        }

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

        _modeToggled = _modeToggles[RpgGameState.mapState.mode];
        SwitchButtonTextures(_modeToggled);

        AddUnitMembers();

        GetNode<TextureButton>("UI/EnterBaseButton").Connect("pressed", this, nameof(OnEnterBaseButton));
        GetNode<TextureButton>("UI/MiningButton").Connect("pressed", this, nameof(OnMiningButton));
        GetNode<TextureButton>("UI/ActionMenuButton").Connect("pressed", this, nameof(OnActionMenuButton));
        GetNode<TextureButton>("UI/ResearchButton").Connect("pressed", this, nameof(OnResearchButton));

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
        for (int i = 0; i < 3; i++) {
            var args = new Godot.Collections.Array { i };
            _miningPopup.GetNode<Button>($"Planet{i}/SendRecall").Connect("pressed", this, nameof(OnMiningSendRecallButton), args);
        }

        _camera = GetNode<Camera2D>("Camera");
        _camera.LimitLeft = 0;
        _camera.LimitRight = (int)MAP_WIDTH;
        _camera.Position = new Vector2(RpgGameState.humanUnit.pos.x, GetViewport().Size.y / 2);
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
            _currentSystem.OnPlayerEnter(RpgGameState.humanPlayer);
        }
        UpdateUI();
    }

    private List<Vessel> FindSurvivors(List<Vessel> fleet) {
        return fleet.FindAll(v => v.hp > 0);
    }

    private void ProcessStarBaseCasualties(StarBase starBase) {
        starBase.garrison = FindSurvivors(starBase.garrison);
        foreach (var v in starBase.garrison) {
            v.energy = v.energySource.maxBackupEnergy;
        }
    }

    private void ProcessUnitCasualties(SpaceUnit unit) {
        unit.fleet = FindSurvivors(unit.fleet);
        if (unit.fleet.Count == 0) {
            if (unit.botOrigin != null) {
                unit.botOrigin.units.Remove(unit);
            }
            RpgGameState.spaceUnits.Remove(unit);
        }
    }

    private void OnStarBaseAttackPlayButton() {
        var u = _eventUnit;
        RpgGameState.enemyAttackerUnit = u.unit;

        // TODO: allow units selection?
        var system = RpgGameState.starSystemByPos[u.unit.pos];
        var starBase = system.starBase;
        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.garrison[i]);
        }
        RpgGameState.garrisonStarBase = starBase;

        SetArenaSettings(system, u.unit.fleet, defenders);
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnFightEventUnit() {
        var u = _eventUnit;
        RpgGameState.enemyAttackerUnit = u.unit;
        SetArenaSettings(_currentSystem.sys, u.unit.fleet, RpgGameState.humanUnit.fleet);
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private float RetreatFuelCost() {
        if (RpgGameState.skillsLearned.Contains("Escape Tactics")) {
            return 35;
        }
        return 70;
    }

    private void OnScavengersLeaveButton() {
        _lockControls = false;
        _scavengersEventPopup.Hide();

        var u = (ScavengerSpaceUnitNode)_eventUnit;
        if (ScavengersWantToAttack(u)) {
            RpgGameState.fuel -= RetreatFuelCost();
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
        RpgGameState.fuel -= RetreatFuelCost();
        UpdateUI();
    }

    private void OnKrigiaTaskForceLeaveButton() {
        _lockControls = false;
        _krigiaTaskForcePopup.Hide();
        RpgGameState.fuel -= RetreatFuelCost();
        UpdateUI();

        if (_currentSystem.sys.starBase != null) {
            TaskForceAttacksHumanBase(_currentSystem.sys.starBase, _eventUnit);
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
            _starSystems[starBase.system.id].DestroyStarBase();
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
            RpgGameState.experience += result.exp;
            lines.Add($"+{result.exp} experience");
        }
        if (result.fuel != 0) {
            RpgGameState.fuel += result.fuel;
            lines.Add($"+{result.fuel} fuel units");
        }

        if (result.genericDebris != 0) {
            RpgGameState.humanUnit.CargoAddDebris(result.genericDebris, Research.Material.None);
            lines.Add($"+{result.genericDebris} debris");
        }
        if (result.krigiaDebris != 0) {
            RpgGameState.humanUnit.CargoAddDebris(result.krigiaDebris, Research.Material.Krigia);
            lines.Add($"+{result.krigiaDebris} Krigia debris");
        }
        if (result.wertuDebris != 0) {
            RpgGameState.humanUnit.CargoAddDebris(result.wertuDebris, Research.Material.Wertu);
            lines.Add($"+{result.wertuDebris} Wertu debris");
        }
        if (result.zythDebris != 0) {
            RpgGameState.humanUnit.CargoAddDebris(result.zythDebris, Research.Material.Zyth);
            lines.Add($"+{result.zythDebris} Zyth debris");
        }

        if (result.minerals != 0) {
            RpgGameState.humanUnit.CargoAddMinerals(result.minerals);
            lines.Add($"+{result.minerals} mineral resouce");
        }
        if (result.organic != 0) {
            RpgGameState.humanUnit.CargoAddOrganic(result.organic);
            lines.Add($"+{result.organic} organic resource");
        }
        if (result.power != 0) {
            RpgGameState.humanUnit.CargoAddPower(result.power);
            lines.Add($"+{result.power} power resource");
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
                ResearchCompleted();
                return;

            case CheatCommandKind.CurrentSystemChange:
                _currentSystem.OnPlayerEnter(RpgGameState.humanPlayer);
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
                button.Text = a.name;
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

        RpgGameState.experience += _randomEvent.expReward;

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
            case RandomEvent.EffectKind.AddCredits:
                RpgGameState.credits += (int)effect.value;
                return;
            case RandomEvent.EffectKind.AddMinerals:
                RpgGameState.humanUnit.CargoAddMinerals((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddOrganic:
                RpgGameState.humanUnit.CargoAddOrganic((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddPower:
                RpgGameState.humanUnit.CargoAddPower((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddFlagshipBackupEnergy:
                RpgGameState.humanUnit.fleet[0].AddEnergy((float)effect.value);
                return;
            case RandomEvent.EffectKind.AddWertuReputation:
                RpgGameState.wertuReputation += (int)effect.value;
                return;
            case RandomEvent.EffectKind.SpendAnyVesselBackupEnergy:
                foreach (var v in RpgGameState.humanUnit.fleet) {
                    if (v.energy >= (int)effect.value) {
                        v.energy -= (int)effect.value;
                        break;
                    }
                }
                return;
            case RandomEvent.EffectKind.AddFuel:
                RpgGameState.AddFuel((int)effect.value);
                return;
            case RandomEvent.EffectKind.AddFleetBackupEnergyPercentage: {
                    var randRange = (Vector2)effect.value;
                    foreach (var v in RpgGameState.humanUnit.fleet) {
                        var roll = QRandom.FloatRange(randRange.x, randRange.y);
                        v.energy = QMath.ClampMin(v.energy - v.energy * roll, 0);
                    }
                    return;
                }
            case RandomEvent.EffectKind.DamageFleetPercentage: {
                    var randRange = (Vector2)effect.value;
                    foreach (var v in RpgGameState.humanUnit.fleet) {
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
                RpgGameState.humanUnit.pos = targetSys.pos;
                _human.GlobalPosition = RpgGameState.humanUnit.pos;
                _human.node.GlobalPosition = RpgGameState.humanUnit.pos;
                EnterSystem(targetSys);
                return;
            case RandomEvent.EffectKind.ApplySlow:
                RpgGameState.travelSlowPoints += (int)effect.value;
                return;

            case RandomEvent.EffectKind.EnterArena: {
                    var unit = (SpaceUnit)effect.value;
                    RpgGameState.enemyAttackerUnit = unit;
                    SetArenaSettings(_currentSystem.sys, unit.fleet, RpgGameState.humanUnit.fleet);
                    _randomEventResolutionPostEffect = effect.kind;
                    return;
                }
        }
    }

    private void AddUnitMembers() {
        var unitMembers = GetNode<Label>("UI/UnitMembers");
        var box = unitMembers.GetNode<VBoxContainer>("Box");

        foreach (var v in RpgGameState.humanUnit.fleet) {
            var hpPercentage = QMath.Percantage(v.hp, v.design.maxHp);
            var energyPercentage = QMath.Percantage(v.energy, v.energySource.maxBackupEnergy);
            var m = UnitMemberNode.New(v.pilotName, v.design.Texture(), hpPercentage, energyPercentage);
            _unitMembers.Add(m);
            box.AddChild(m);
        }

        ReorderUnitMembers();
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
        if (RpgGameState.humanUnit.cargo.power < 5) {
            return;
        }
        RpgGameState.humanUnit.cargo.power -= 5;
        RpgGameState.fuel += 20;
        UpdateUI();
    }

    private bool CanBuildStarBase() {
        return _currentSystem != null && _currentSystem.sys.starBase == null;
    }

    private void OnBuildNewBase() {
        var arkIndex = ArkVesselIndex();
        if (arkIndex == -1 || !CanBuildStarBase()) {
            return;
        }

        RpgGameState.humanUnit.fleet.RemoveAt(arkIndex);
        _unitMembers[arkIndex].QueueFree();
        _unitMembers.RemoveAt(arkIndex);
        ReorderUnitMembers();

        var starBase = new StarBase(_currentSystem.sys, RpgGameState.humanPlayer);
        starBase.mineralsStock = 0;
        starBase.organicStock = 0;
        starBase.powerStock = 0;
        RpgGameState.humanBases.Add(starBase);
        _currentSystem.sys.starBase = starBase;

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
        _currentSystem.sys.starBase.UpdateShopSelection();
        RpgGameState.enteredBase = _currentSystem.sys.starBase;
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
            RpgGameState.drones++;
            RpgGameState.planetsWithMines.Remove(p);
        } else {
            p.hasMine = true;
            RpgGameState.drones--;
            RpgGameState.planetsWithMines.Add(p);
        }
        UpdateUI();
    }

    private void OnMiningDoneButton() {
        _lockControls = false;
        _miningPopup.Hide();
    }

    private void OnMiningLoadMinerals() {
        var freeSpace = RpgGameState.humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.mineralsCollected ? p.mineralsCollected : freeSpace;
            p.mineralsCollected -= loadAmount;
            RpgGameState.humanUnit.cargo.minerals += loadAmount;
            freeSpace -= loadAmount;
        }
        UpdateUI();
    }

    private void OnMiningLoadOrganic() {
        var freeSpace = RpgGameState.humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.organicCollected ? p.organicCollected : freeSpace;
            p.organicCollected -= loadAmount;
            RpgGameState.humanUnit.cargo.organic += loadAmount;
            freeSpace -= loadAmount;
        }
        UpdateUI();
    }

    private void OnMiningLoadPower() {
        var freeSpace = RpgGameState.humanUnit.CargoFree();
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            var loadAmount = freeSpace > p.powerCollected ? p.powerCollected : freeSpace;
            p.powerCollected -= loadAmount;
            RpgGameState.humanUnit.cargo.power += loadAmount;
            freeSpace -= loadAmount;
        }
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
        RpgGameState.fuel -= RetreatFuelCost();
        UpdateUI();
    }

    private void OnMovementTogglePressed() {
        if (_lockControls) {
            return;
        }
        RpgGameState.mapState.movementEnabled = !RpgGameState.mapState.movementEnabled;
    }

    private void StopMovement() {
        _movementToggle.Pressed = false;
        RpgGameState.mapState.movementEnabled = false;
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

    public override void _Process(float delta) {
        PanCamera(delta);

        if (!_lockControls && Input.IsActionJustPressed("mapMovementToggle")) {
            _movementToggle.Pressed = !_movementToggle.Pressed;
            _movementToggle.EmitSignal("pressed");
        }

        if (!_lockControls && Input.IsActionJustPressed("openConsole")) {
            OpenCheats();
        }

        if (RpgGameState.mapState.movementEnabled) {
            if (_currentSystem != null && _human.node.GlobalPosition != _currentSystem.GlobalPosition) {
                _currentSystem = null;
            }
            float daysPerSecond = 5;
            _dayCounter += delta * daysPerSecond;
            if (_dayCounter > 1) {
                _dayCounter -= 1;
                RpgGameState.day++;
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

        if (_human.node.GlobalPosition.DistanceTo(sys.GlobalPosition) < ((RpgGameState.fuel * 2) - 1)) {
            _dstSystem = sys;
            _human.SetDestination(sys.GlobalPosition);
            AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/movement_ok.wav")));
        } else {
            AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/movement_error.wav")));
        }
    }

    private void OnSpaceUnitAttackStarBase(SpaceUnitNode unitNode) {
        if (unitNode.unit.owner != RpgGameState.krigiaPlayer) {
            // Other race task forces are not implemented yet.
            throw new Exception("unexpected star base attack from " + unitNode.unit.owner.PlayerName);
        }

        if (unitNode.unit.pos == RpgGameState.humanUnit.pos) {
            TriggerKrigiaTaskForceEvent(unitNode);
            return;
        }
        var sys = RpgGameState.starSystemByPos[unitNode.unit.pos];
        TaskForceAttacksHumanBase(sys.starBase, unitNode);
    }

    private void OnSpaceUnitRemoved(SpaceUnitNode unitNode) {
        _spaceUnits.Remove(unitNode);
    }

    private void OnDroneDestroyed(SpaceUnitNode unitNode) {
        RpgGameState.dronwsOwned--;
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapBadNotificationNode.New("Drone destroyed");
        AddChild(notification);
        notification.GlobalPosition = unitNode.GlobalPosition;
    }

    private void OnBaseDetected(SpaceUnitNode unitNode) {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapBadNotificationNode.New("Base detected");
        AddChild(notification);
        notification.GlobalPosition = unitNode.GlobalPosition;
    }

    private void AddSpaceUnit(SpaceUnitNode unitNode) {
        var args = new Godot.Collections.Array{unitNode};
        unitNode.Connect("AttackStarBase", this, nameof(OnSpaceUnitAttackStarBase), args);
        unitNode.Connect("Removed", this, nameof(OnSpaceUnitRemoved), args);
        unitNode.Connect("DroneDestroyed", this, nameof(OnDroneDestroyed), args);
        unitNode.Connect("BaseDetected", this, nameof(OnBaseDetected), args);

        AddChild(unitNode);
        // unitNode.GlobalPosition = unitNode.unit.pos;
        _spaceUnits.Add(unitNode);
    }

    private void OnSpaceUnitCreated(SpaceUnitNode unitNode) {
        AddSpaceUnit(unitNode);
    }

    private StarBaseNode NewStarBaseNode(StarSystem sys) {
        StarBaseNode baseNode;

        if (sys.starBase.owner == RpgGameState.humanPlayer) {
            baseNode = HumanStarBaseNode.New(sys.starBase);
        } else if (sys.starBase.owner == RpgGameState.scavengerPlayer) {
            baseNode = ScavengerStarBaseNode.New(sys.starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else if (sys.starBase.owner == RpgGameState.krigiaPlayer) {
            baseNode = KrigiaStarBaseNode.New(sys.starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else {
            baseNode = StarBaseNode.New(sys.starBase);
        }

        baseNode.Position = sys.pos;
        baseNode.Visible = false;
        _starBases.Add(baseNode);

        return baseNode;
    }

    private void RenderMap() {
        StarSystemNode currentSystem = null;

        foreach (StarSystem sys in RpgGameState.starSystems) {
            StarBaseNode starBaseNode = null;
            if (sys.starBase != null) {
                // Sync star base units. They could be destroyed.
                sys.starBase.units.RemoveWhere((x) => !RpgGameState.spaceUnits.Contains(x));

                starBaseNode = NewStarBaseNode(sys);
                AddChild(starBaseNode);
            }
            var systemNode = StarSystemNode.New(sys, starBaseNode);
            _starSystems.Add(systemNode);
            {
                var args = new Godot.Collections.Array { systemNode };
                systemNode.Connect("Clicked", this, nameof(OnStarClicked), args);
            }
            AddChild(systemNode);
            if (sys.pos == RpgGameState.humanUnit.pos) {
                currentSystem = systemNode;
            }
        }

        var playerUnit = SpaceUnitNode.New(RpgGameState.humanUnit);
        playerUnit.Connect("DestinationReached", this, nameof(OnDestinationReached));
        playerUnit.speed = CalculateFleetSpeed();
        AddChild(playerUnit);
        if (playerUnit.unit.waypoint != Vector2.Zero) {
            _dstSystem = _starSystems[RpgGameState.starSystemByPos[playerUnit.unit.waypoint].id];
        }
        _human = MapHumanNode.New(playerUnit);
        _human.player = RpgGameState.humanPlayer;
        _human.GlobalPosition = RpgGameState.humanUnit.pos;
        playerUnit.GlobalPosition = RpgGameState.humanUnit.pos;
        AddChild(_human);

        _currentSystem = currentSystem;

        foreach (var u in RpgGameState.spaceUnits) {
            SpaceUnitNode node;
            if (u.owner == RpgGameState.scavengerPlayer) {
                node = ScavengerSpaceUnitNode.New(u);
            } else if (u.owner == RpgGameState.krigiaPlayer) {
                node = KrigiaSpaceUnitNode.New(u);
            } else {
                throw new Exception("unexpected unit owner: " + u.owner.PlayerName);
            }
            AddSpaceUnit(node);
        }
    }

    private void EnterSystem(StarSystem sys) {
        RpgGameState.travelSlowPoints = 0;
        _currentSystem = _starSystems[sys.id];
        _dstSystem = null;
        _currentSystem.OnPlayerEnter(_human.player);
        if (sys.starBase != null) {
            if (sys.starBase.owner == _human.player) {
                RecoverFleetEnergy(RpgGameState.humanUnit.fleet);
            }
        }

        if (RpgGameState.randomEventCooldown == 0 && sys.randomEventCooldown == 0) {
            var roll = QRandom.Float();
            if (roll < 0.5) {
                MaybeTriggerEnterSystemEvent(sys);
                // TODO: should depend on the game settings.
                sys.randomEventCooldown += QRandom.IntRange(250, 450);
                RpgGameState.randomEventCooldown += QRandom.IntRange(100, 150);
            }
        }
    }

    private void MaybeTriggerEnterSystemEvent(StarSystem sys) {
        if (RpgGameState.randomEventsAvailable.Count == 0) {
            return;
        }

        var enterSystemEvents = new List<RandomEvent>();
        foreach (var e in RpgGameState.randomEventsAvailable) {
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
        RpgGameState.randomEventsAvailable.Remove(_randomEvent);
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

        _starSystemMenu.GetNode<ButtonNode>("ConvertPower").Disabled = RpgGameState.humanUnit.cargo.power < 5;
        _starSystemMenu.GetNode<ButtonNode>("BuildNewBase").Disabled = ArkVesselIndex() == -1 || !CanBuildStarBase();

        for (int i = 0; i < _unitMembers.Count; i++) {
            var p = RpgGameState.humanUnit.fleet[i];
            var hpPercentage = QMath.Percantage(p.hp, p.design.maxHp);
            var energyPercentage = QMath.Percantage(p.energy, p.energySource.maxBackupEnergy);
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
            if (_currentSystem.sys.starBase != null && _currentSystem.sys.starBase.owner == _human.player) {
                enterBase.Disabled = false;
            }
            // Can mine only in own or neutral systems.
            if (_currentSystem.sys.starBase == null || _currentSystem.sys.starBase.owner == _human.player) {
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
                    sendrecall.Disabled = RpgGameState.drones <= 0;
                }
            }
        } else {
            GetNode<Label>("UI/LocationValue").Text = "Interstellar Space";
        }

        _human.Update();
    }

    private void UpdateCargoValue() {
        int max = RpgGameState.humanUnit.CargoCapacity();
        var current = RpgGameState.humanUnit.CargoSize();
        GetNode<Label>("UI/CargoValue").Text = $"{current}/{max}";
    }

    private void UpdateExpValue() {
        GetNode<Label>("UI/ExpValue").Text = RpgGameState.experience.ToString();
    }

    private void UpdateCreditsValue() {
        GetNode<Label>("UI/CreditsValue").Text = RpgGameState.credits.ToString();
    }

    private void UpdateDayValue() {
        GetNode<Label>("UI/DayValue").Text = RpgGameState.day.ToString();
    }

    private void UpdateFuelValue() {
        GetNode<Label>("UI/FuelValue").Text = ((int)RpgGameState.fuel).ToString();
    }

    private void UpdateDronesValue() {
        GetNode<Label>("UI/DronesValue").Text = RpgGameState.drones.ToString();
    }

    private void RecoverFleetEnergy(List<Vessel> fleet) {
        foreach (var v in fleet) {
            v.energy = v.energySource.maxBackupEnergy;
        }
    }

    private float CalculateFleetSpeed() {
        var travelSpeed = RpgGameState.travelSpeed;
        if (RpgGameState.travelSlowPoints > 0) {
            travelSpeed /= 2;
        }
        if (RpgGameState.skillsLearned.Contains("Navigation III")) {
            travelSpeed += travelSpeed * 0.25f;
        } else if (RpgGameState.skillsLearned.Contains("Navigation II")) {
            travelSpeed += travelSpeed * 0.2f;
        } else if (RpgGameState.skillsLearned.Contains("Navigation I")) {
            travelSpeed += travelSpeed * 0.15f;
        }
        return travelSpeed;
    }

    private void ProcessDayEvents() {
        _human.node.speed = CalculateFleetSpeed();

        RpgGameState.travelSlowPoints = QMath.ClampMin(RpgGameState.travelSlowPoints - 1, 0);
        RpgGameState.randomEventCooldown = QMath.ClampMin(RpgGameState.randomEventCooldown - 1, 0);

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
    }

    private void ProcessResearch() {
        if (RpgGameState.currentResearch == null) {
            return;
        }

        var research = RpgGameState.currentResearch;

        RpgGameState.researchProgress += RpgGameState.ResearchRate();

        if (research.material == Research.Material.Krigia) {
            RpgGameState.krigiaMaterial = QMath.ClampMin(RpgGameState.krigiaMaterial - 1, 0);
        } else if (research.material == Research.Material.Wertu) {
            RpgGameState.wertuMaterial = QMath.ClampMin(RpgGameState.wertuMaterial - 1, 0);
        } else if (research.material == Research.Material.Zyth) {
            RpgGameState.zythMaterial = QMath.ClampMin(RpgGameState.zythMaterial - 1, 0);
        }

        if (RpgGameState.scienceFunds > 0) {
            var roll = QRandom.IntRange(5, 20);
            RpgGameState.scienceFunds = QMath.ClampMin(RpgGameState.scienceFunds - roll, 0);
        }

        var researchTime = research.researchTime;
        if (RpgGameState.skillsLearned.Contains("Scholar")) {
            researchTime -= 10;
        }

        if ((int)RpgGameState.researchProgress >= researchTime) {
            ResearchCompleted();
        }
    }

    private void ResearchCompleted() {
        var research = RpgGameState.currentResearch;

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/research_completed.wav"));

        var button = GetNode<TextureButton>("UI/ResearchButton");
        var notification = MapNotificationNode.New("Research completed");
        button.AddChild(notification);
        notification.Position += button.RectSize / 2;

        RpgGameState.technologiesResearched.Add(research.name);

        if (research.category == Research.Category.NewArtifact) {
            var artifact = ArtifactDesign.Find(research.name);
            RpgGameState.PutItemToStorage(artifact);
        }

        RpgGameState.researchProgress = 0;
        RpgGameState.currentResearch = null;

        StopMovement();
        UpdateUI();
    }

    private void ProcessInterstellarDay() {
        // The space is empty.
    }

    private void ProcessStarSystemDay() {
        ProcessUnitMode();

        var starBase = _currentSystem.sys.starBase;
        if (starBase != null) {
            if (starBase.owner == _human.player) {
                RecoverFleetEnergy(RpgGameState.humanUnit.fleet);
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

        if (starBase != null) {
            if (starBase.owner.Alliance != _human.player.Alliance) {
                RollFleetAttack();
            }
        }
    }

    private bool RollUnitAttack(SpaceUnitNode u) {
        if (u.unit.owner == RpgGameState.scavengerPlayer) {
            TriggerScavengersEvent(u);
            return true;
        }
        if (u.unit.owner == RpgGameState.krigiaPlayer) {
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
        var humanForce = RpgGameState.humanUnit.FleetCost();
        return scavengersForce * 2 > humanForce;
    }

    private void TriggerKrigiaPatrolEvent(SpaceUnitNode u) {
        _krigiaPatrolPopup.GetNode<ButtonNode>("LeaveButton").Disabled = RpgGameState.fuel < RetreatFuelCost();

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

    private void TriggerKrigiaTaskForceEvent(SpaceUnitNode u) {
        _krigiaTaskForcePopup.GetNode<ButtonNode>("LeaveButton").Disabled = RpgGameState.fuel < RetreatFuelCost();

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
            _scavengersEventPopup.GetNode<ButtonNode>("LeaveButton").Disabled = RpgGameState.fuel < RetreatFuelCost();
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
            // if (!RpgGameState.starSystemByPos.ContainsKey(u.unit.pos)) {
            //     u.ProcessDay(null);
            // } else {
            //     var sys = RpgGameState.starSystemByPos[u.unit.pos];
            //     var sysNode = _starSystems[sys.id];
            //     u.ProcessDay(sysNode);
            // }
        }
    }

    private void ProcessStarSystems() {
        foreach (var starBase in _starBases) {
            starBase.ProcessDay();
        }

        foreach (StarSystemNode starSystem in _starSystems) {
            starSystem.sys.randomEventCooldown = QMath.ClampMin(starSystem.sys.randomEventCooldown - 1, 0);
            starSystem.ProcessDay();
        }
    }

    private void ProcessMines() {
        foreach (ResourcePlanet p in RpgGameState.planetsWithMines) {
            p.mineralsCollected = QMath.ClampMax(p.mineralsCollected + p.mineralsPerDay, RpgGameState.limits.droneCapacity);
            p.organicCollected = QMath.ClampMax(p.organicCollected + p.organicPerDay, RpgGameState.limits.droneCapacity);
            p.powerCollected = QMath.ClampMax(p.powerCollected + p.powerPerDay, RpgGameState.limits.droneCapacity);
        }
    }

    private void ProcessUnitMode() {
        var starBase = _currentSystem.sys.starBase;

        if (RpgGameState.mapState.mode == UnitMode.Idle) {
            RpgGameState.fuel = QMath.ClampMax(RpgGameState.fuel + 1, RpgGameState.MaxFuel());
            return;
        }

        if (RpgGameState.mapState.mode == UnitMode.Attack) {
            if (RpgGameState.fuel < 1) {
                SetUnitMode(UnitMode.Idle);
                StopMovement();
                return;
            }
            if (starBase.owner.Alliance == RpgGameState.humanPlayer.Alliance) {
                return;
            }
            if (starBase == null) {
                return;
            }
            
            RpgGameState.fuel -= 1;
            if (starBase.garrison.Count != 0 && !RpgGameState.skillsLearned.Contains("Siege Mastery")) {
                return;
            }
            var damage = RpgGameState.humanUnit.fleet.Count;
            if (RpgGameState.skillsLearned.Contains("Siege Mastery")) {
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

        if (RpgGameState.mapState.mode == UnitMode.Search) {
            if (RpgGameState.fuel < 3) {
                SetUnitMode(UnitMode.Idle);
                StopMovement();
                return;
            }
            if (_currentSystem.sys.artifact == null) {
                return;
            }
            if (starBase != null && starBase.owner.Alliance != _human.player.Alliance) {
                _currentSystem.sys.artifactRecoveryDelay -= 1;
            } else {
                _currentSystem.sys.artifactRecoveryDelay -= 2;
            }
            if (_currentSystem.sys.artifactRecoveryDelay <= 0) {
                RpgGameState.artifactsRecovered.Add(_currentSystem.sys.artifact);

                _currentSystem.sys.artifactRecoveryDelay = 0;
                _currentSystem.sys.artifact = null;

                var notification = MapNotificationNode.New("Artifact recovered");
                _currentSystem.AddChild(notification);

                RpgGameState.credits += 2000;

                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_recovered.wav"));

                SetUnitMode(UnitMode.Idle);
                StopMovement();

                _currentSystem.UpdateInfo();
                _currentSystem.RenderKnownInfo();
            }
            RpgGameState.fuel -= 3;
            return;
        }
    }

    private int ArkVesselIndex() {
        // Start from 1, since flagship can't be used to build a base;
        // even if it's Ark.
        for (int i = 1; i < RpgGameState.humanUnit.fleet.Count; i++) {
            var v = RpgGameState.humanUnit.fleet[i];
            if (v.design.name == "Ark") {
                return i;
            }
        }
        return -1;
    }

    private void SetArenaSettings(StarSystem location, List<Vessel> enemyFleet, List<Vessel> alliedFleet) {
        ArenaSettings.Reset();
        ArenaSettings.isQuickBattle = false;

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

        for (int i = 0; i < enemyFleet.Count; i++) {
            var pos = new Vector2(1568, 288 + (i * 192));
            var v = enemyFleet[i];
            v.spawnPos = QMath.RandomizedLocation(pos, 40);
            ArenaSettings.combatants.Add(v);
        }

        for (int i = 0; i < alliedFleet.Count; i++) {
            var pos = new Vector2(224, 288 + (i * 192));
            var v = alliedFleet[i];
            v.spawnPos = QMath.RandomizedLocation(pos, 40);
            ArenaSettings.combatants.Add(v);
        }
    }

    private void RollFleetAttack() {
        var starBase = _currentSystem.sys.starBase;

        if (starBase.garrison.Count == 0) {
            return;
        }

        if (RpgGameState.mapState.mode != UnitMode.Attack) {
            var roll = QRandom.Float();
            if (roll >= 0.25) {
                return;
            }
        }

        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.garrison[i]);
        }
        RpgGameState.garrisonStarBase = starBase;

        SetArenaSettings(_currentSystem.sys, defenders, RpgGameState.humanUnit.fleet);

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/unit_under_attack.wav"));
        StopMovement();
        _lockControls = true;
        var pluralSuffix = numDefenders == 1 ? "" : "s";
        _fleetAttackPopup.GetNode<Label>("Attackers").Text = $"Attackers: {numDefenders} {starBase.owner.PlayerName} ship" + pluralSuffix;
        _fleetAttackPopup.GetNode<Button>("RetreatButton").Disabled = RpgGameState.fuel < RetreatFuelCost();
        _fleetAttackPopup.PopupCentered();
    }

    private void KrigiaBaseRequestReinforcements(StarBase starBase) {
        if (starBase.botReinforcementsDelay > 0) {
            return;
        }
        starBase.botReinforcementsDelay = QRandom.IntRange(100, 200);

        var connectedSystems = RpgGameState.starSystemConnections[starBase.system];
        StarBase alliedBase = null;
        foreach (var sys in connectedSystems) {
            if (sys.starBase == null || sys.starBase.owner != starBase.owner) {
                continue;
            }
            if (sys.starBase.garrison.Count <= starBase.garrison.Count) {
                continue;
            }
            alliedBase = sys.starBase;
            break;
        }
        if (alliedBase == null) {
            return;
        }

        var reinforcementsFleet = new List<Vessel>();
        var groupSize = QRandom.IntRange(2, 4);
        var keptInGarrison = alliedBase.garrison.FindAll(v => {
            if (v.design.level <= 2) {
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

        var spaceUnit = new SpaceUnit {
            owner = RpgGameState.krigiaPlayer,
            pos = alliedBase.system.pos,
            waypoint = starBase.system.pos,
            botOrigin = alliedBase,
            botProgram = SpaceUnit.Program.KrigiaReinforcements,
            fleet = reinforcementsFleet,
        };
        RpgGameState.spaceUnits.Add(spaceUnit);
        alliedBase.units.Add(spaceUnit);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }

    private void ProcessKrigiaActions() {
        // 1 Choose a star base to attack
        // 2 If nearest Krigia star base has enough vessels, attack from there
        // 2.1 Otherwise send a reinforcement unit there from somewhere
        // 2.2 If it's impossible to get enough vessels to do an attack, drop the idea
        // 3 Send a task force unit from that Krigia star base

        RpgGameState.krigiaPlans.taskForceDelay = QMath.ClampMin(RpgGameState.krigiaPlans.taskForceDelay - 1, 0);

        if (RpgGameState.day < 450) {
            return;
        }

        if (RpgGameState.krigiaPlans.taskForceDelay != 0) {
            return;
        }

        var potentialTargets = new List<StarBase>();
        foreach (var starBase in RpgGameState.humanBases) {
            if (starBase.discoveredByKrigia == 0) {
                continue;
            }
            if (RpgGameState.day - starBase.discoveredByKrigia < 100) {
                continue;
            }
            potentialTargets.Add(starBase);
        }
        if (potentialTargets.Count == 0) {
            return;
        }

        var targetBase = QRandom.Element(potentialTargets);

        StarBase nearestStarBase = null;
        var connectedSystems = RpgGameState.starSystemConnections[targetBase.system];
        foreach (var sys in connectedSystems) {
            if (sys.starBase == null || sys.starBase.owner != RpgGameState.krigiaPlayer) {
                continue;
            }
            if (nearestStarBase == null) {
                nearestStarBase = sys.starBase;
            } else if (sys.pos.DistanceTo(targetBase.system.pos) < nearestStarBase.system.pos.DistanceTo(targetBase.system.pos)) {
                nearestStarBase = sys.starBase;
            }
        }
        if (nearestStarBase == null) {
            return; // Target system is out of reach
        }

        var taskForceFleet = new List<Vessel>();
        var groupSize = QRandom.IntRange(2, 4);
        var keptInGarrison = nearestStarBase.garrison.FindAll(v => {
            if (v.design.level <= 2) {
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
            RpgGameState.krigiaPlans.taskForceDelay = QRandom.IntRange(60, 90);
            return;
        }

        nearestStarBase.garrison = keptInGarrison;

        var spaceUnit = new SpaceUnit {
            owner = RpgGameState.krigiaPlayer,
            pos = nearestStarBase.system.pos,
            waypoint = targetBase.system.pos,
            botOrigin = nearestStarBase,
            botProgram = SpaceUnit.Program.KrigiaTaskForce,
            fleet = taskForceFleet,
        };
        RpgGameState.spaceUnits.Add(spaceUnit);
        nearestStarBase.units.Add(spaceUnit);
        RpgGameState.krigiaPlans.taskForceDelay = QRandom.IntRange(200, 300);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }
}
