using Godot;
using System;
using System.Collections.Generic;
using CheatCommandKind = MapViewCheatMenuPopup.CommandKind;

public class MapView : Node2D {
    const float MAP_WIDTH = (1080 * 3) + 220;

    private AbstractMapEvent _randomEventProto;
    private AbstractMapEvent _randomEventInstance;
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
    private PlanetsMenuPopupNode _planetsPopup;
    private PopupNode _starSystemMenu;
    private PopupNode _randomEventPopup;
    private PopupNode _randomEventResolvedPopup;
    private PopupNode _battleResult;
    private PopupNode _researchCompletedPopup;
    private PopupNode _patrolReachesBasePopup;
    private MapViewCheatMenuPopup _cheatsPopup;

    private AbstractMapEvent.EffectKind _randomEventResolutionPostEffect;

    private SpaceUnitNode _eventUnit;
    private PopupNode _starBaseAttackPopup;
    private PopupNode _draklidsEventPopup;
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

        var music = GetNode<BackgroundMusic>("/root/BackgroundMusic");
        if (!music.PlayingMapMusic()) {
            if (QRandom.Bool()) {
                music.PlayMapMusic();
            } else {
                music.PlayMapMusic2();
            }
        }

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
        } else if (RpgGameState.transition == RpgGameState.MapTransition.EnemyUnitRetreats) {
            var unit = RpgGameState.arenaUnit1;
            unit.waypoint = unit.botOrigin.Get().system.Get().pos;
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

        MapItemInfoNode.instance = MapItemInfoNode.New();
        AddChild(MapItemInfoNode.instance);
        // GetNode<CanvasLayer>("UI").AddChild(MapItemInfoNode.instance);

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
        GetNode<TextureButton>("UI/MiningButton").Connect("pressed", this, nameof(OnPlanetsButton));
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
        _krigiaPatrolPopup.GetNode<ButtonNode>("CommunicateButton").Connect("pressed", this, nameof(OnKrigiaPatrolCommunicateButton));
        _krigiaPatrolPopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnKrigiaPatrolLeaveButton));

        _draklidsEventPopup = GetNode<PopupNode>("UI/DraklidsPopup");
        _draklidsEventPopup.GetNode<ButtonNode>("FightButton").Connect("pressed", this, nameof(OnFightEventUnit));
        _draklidsEventPopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnDraklidsLeaveButton));

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

        _planetsPopup = PlanetsMenuPopupNode.New();
        _planetsPopup.GetNode<ButtonNode>("DoneButton").Connect("pressed", this, nameof(OnPlanetsDoneButton));
        GetNode<CanvasLayer>("UI").AddChild(_planetsPopup);
        for (int i = 0; i < 3; i++) {
            var planet = _planetsPopup.GetNode<Label>($"Planet{i}");
            var args = new Godot.Collections.Array { i };
            planet.Connect("mouse_entered", this, nameof(OnPlanetMouseEnter), args);
            planet.GetNode<ButtonNode>("SendDroneButton").Connect("pressed", this, nameof(OnPlanetSendDroneButton), args);
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
            _currentSystem.OnPlayerEnter(Faction.Earthling);
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
            v.Get().energy = v.Get().MaxBackupEnergy();
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

    private void OnDraklidsLeaveButton() {
        _lockControls = false;
        _draklidsEventPopup.Hide();

        var u = (DraklidSpaceUnitNode)_eventUnit;
        if (DraklidsWantToAttack(u)) {
            _gameState.fuel -= RetreatFuelCost();
        } else {
            // Scare them off.
            u.unit.botSystemLeaveDelay = 0;
            u.PickNewWaypoint();
        }

        UpdateUI();
    }

    private void OnKrigiaPatrolCommunicateButton() {
        var u = _eventUnit;
        RpgGameState.arenaUnit1 = u.unit;
        SetArenaSettings(_currentSystem.sys, ConvertVesselList(u.unit.fleet), ConvertVesselList(_humanUnit.fleet));
        RpgGameState.selectedTextQuest = new KrigiaPatrolTQuest();
        GetTree().ChangeScene("res://scenes/TextQuestScreen.tscn");
    }

    private void OnKrigiaPatrolLeaveButton() {
        _lockControls = false;
        _krigiaPatrolPopup.Hide();

        if (_currentSystem.sys.starBase.id != 0) {
            MarkStarBaseAsDiscovered(_currentSystem.sys.starBase.Get());
        }

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
            RpgGameState.AddFuel(result.fuel);
            lines.Add($"+{result.fuel} fuel units");
        }

        if (result.debris.other != 0) {
            _humanUnit.CargoAddDebris(result.debris.other, Faction.Neutral);
            lines.Add($"+{result.debris.other} debris");
        }
        if (result.debris.krigia != 0) {
            _humanUnit.CargoAddDebris(result.debris.krigia, Faction.Krigia);
            lines.Add($"+{result.debris.krigia} Krigia debris");
        }
        if (result.debris.wertu != 0) {
            _humanUnit.CargoAddDebris(result.debris.wertu, Faction.Wertu);
            lines.Add($"+{result.debris.wertu} Wertu debris");
        }
        if (result.debris.zyth != 0) {
            _humanUnit.CargoAddDebris(result.debris.zyth, Faction.Zyth);
            lines.Add($"+{result.debris.zyth} Zyth debris");
        }
        if (result.debris.phaa != 0) {
            _humanUnit.CargoAddDebris(result.debris.phaa, Faction.Phaa);
            lines.Add($"+{result.debris.phaa} Phaa debris");
        }
        if (result.debris.draklid != 0) {
            _humanUnit.CargoAddDebris(result.debris.draklid, Faction.Draklid);
            lines.Add($"+{result.debris.draklid} Draklid debris");
        }
        if (result.debris.vespion != 0) {
            _humanUnit.CargoAddDebris(result.debris.phaa, Faction.Vespion);
            lines.Add($"+{result.debris.vespion} Vespion debris");
        }
        if (result.debris.rarilou != 0) {
            _humanUnit.CargoAddDebris(result.debris.rarilou, Faction.Rarilou);
            lines.Add($"+{result.debris.rarilou} Rarilou debris");
        }

        if (result.power != 0) {
            _humanUnit.CargoAddPower(result.power);
            lines.Add($"+{result.power} power resource");
        }
        if (result.organic != 0) {
            _humanUnit.CargoAddOrganic(result.organic);
            lines.Add($"+{result.organic} organic resource");
        }
        if (result.minerals != 0) {
            _humanUnit.CargoAddMinerals(result.minerals);
            lines.Add($"+{result.minerals} mineral resouce");
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
            case CheatCommandKind.RevealMap:
                foreach (var sys in _starSystemNodes) {
                    sys.UpdateInfo();
                    sys.ShowStarBase();
                }
                return;

            case CheatCommandKind.ResearchComplete:
                OnCheatsDone();
                ResearchCompleted();
                return;

            case CheatCommandKind.CurrentSystemChange:
                _currentSystem.OnPlayerEnter(Faction.Earthling);
                UpdateUI();
                return;

            case CheatCommandKind.StatsChange:
                UpdateUI();
                return;

            case CheatCommandKind.CallRandomEvent:
                OnCheatsDone();
                _randomEventProto = (AbstractMapEvent)command.value;
                OpenRandomEvent(NewRandomEventContext());
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

    private void OpenRandomEvent(RandomEventContext ctx) {
        StopMovement();

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));

        _randomEventInstance = _randomEventProto.Create(ctx);

        _randomEventPopup.GetNode<Label>("Title").Text = _randomEventInstance.title;
        _randomEventContext = ctx;

        var text = _randomEventInstance.text;
        // var extraText = _randomEvent.extraText(_randomEventContext);
        // if (!extraText.Empty()) {
        //     text += "\n\n" + extraText;
        // }
        _randomEventPopup.GetNode<Label>("Text").Text = text;

        for (int i = 0; i < 4; i++) {
            var button = _randomEventPopup.GetNode<ButtonNode>($"Action{i + 1}");
            if (_randomEventInstance.actions.Count > i) {
                var a = _randomEventInstance.actions[i];
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

        RunEventResolutionPostEffect();
    }

    private void RunEventResolutionPostEffect() {
        switch (_randomEventResolutionPostEffect) {
            case AbstractMapEvent.EffectKind.EnterArena:
                GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
                return;
            case AbstractMapEvent.EffectKind.EnterTextQuest:
                GetTree().ChangeScene("res://scenes/TextQuestScreen.tscn");
                return;
        }
    }

    private void OnRandomEventAction(int actionIndex) {
        var eventResult = _randomEventInstance.actions[actionIndex].apply();

        foreach (var effect in eventResult.effects) {
            ExecuteEffect(effect);
        }

        if (eventResult.skipText) {
            _randomEventPopup.Hide();
            _lockControls = false;
            RunEventResolutionPostEffect();
        } else {
            var outcomeText = "<" + _randomEventInstance.actions[actionIndex].name + ">";
            outcomeText += "\n\n" + eventResult.text;

            if (eventResult.expReward != 0) {
                outcomeText += $"\n\nGained {eventResult.expReward} experience points.";
                _gameState.experience += eventResult.expReward;
            }

            _randomEventResolvedPopup.GetNode<Label>("Title").Text = _randomEventInstance.title;
            _randomEventResolvedPopup.GetNode<Label>("Text").Text = outcomeText;

            _randomEventPopup.Hide();
            _randomEventResolvedPopup.PopupCentered();
        }

        _randomEventInstance = null;
        _randomEventProto = null;
        UpdateUI();
    }

    private void ExecuteEffect(AbstractMapEvent.Effect effect) {
        switch (effect.kind) {
            case AbstractMapEvent.EffectKind.AddTechnology:
                _gameState.technologiesResearched.Add((string)effect.value);
                return;

            case AbstractMapEvent.EffectKind.AddVesselToFleet:
                AddUnitMember((Vessel)effect.value);
                ReorderUnitMembers();
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
                RemoveHumanVessel((int)effect.value);
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
                RpgGameState.AddFuel((int)effect.value);
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
                _humanUnit.pos = targetSys.pos;
                _human.GlobalPosition = _humanUnit.pos;
                _human.node.GlobalPosition = _humanUnit.pos;
                EnterSystem(targetSys);
                return;
            case AbstractMapEvent.EffectKind.ApplySlow:
                _gameState.travelSlowPoints += (int)effect.value;
                return;

            case AbstractMapEvent.EffectKind.EnterTextQuest:
                RpgGameState.selectedTextQuest = (AbstractTQuest)effect.value;
                _randomEventResolutionPostEffect = effect.kind;
                return;

            case AbstractMapEvent.EffectKind.PrepareArenaSettings:
                SetArenaSettings(_currentSystem.sys, ConvertVesselList(_randomEventContext.spaceUnit.fleet), ConvertVesselList(_humanUnit.fleet));
                return;

            case AbstractMapEvent.EffectKind.EnterArena: {
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

        var hpPercentage = QMath.Percantage(v.hp, v.MaxHp());
        var energyPercentage = QMath.Percantage(v.energy, v.MaxBackupEnergy());
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

    private void RemoveHumanVessel(int index) {
        _humanUnit.fleet[index].Get().deleted = true;
        _humanUnit.fleet.RemoveAt(index);
        _unitMembers[index].QueueFree();
        _unitMembers.RemoveAt(index);
        ReorderUnitMembers();
    }

    private void OnBuildNewBase() {
        var arkIndex = ArkVesselIndex();
        if (arkIndex == -1 || !CanBuildStarBase()) {
            return;
        }

        RemoveHumanVessel(arkIndex);

        var starBase = _gameState.starBases.New();
        starBase.owner = Faction.Earthling;
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
        starBaseNode.Visible = true;
        _currentSystem.UpdateInfo();
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

    private void OnPlanetSendDroneButton(int i) {
        var selectedOption = _planetsPopup.GetNode<OptionButton>($"Planet{i}/DroneSelect").Selected;
        if (selectedOption <= 0) {
            return;
        }
        var droneName = (string)_planetsPopup.GetNode<OptionButton>($"Planet{i}/DroneSelect").GetItemText(selectedOption);
        int index = _gameState.explorationDrones.FindIndex((x) => x == droneName);
        _gameState.explorationDrones.RemoveAt(index);

        var p = _currentSystem.sys.resourcePlanets[i];
        p.activeDrone = droneName;

        AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Scythe.wav"), -6));

        UpdatePlanetsMenu();
        UpdateDronesValue();
    }

    private void OnPlanetMouseEnter(int i) {
        var p = _currentSystem.sys.resourcePlanets[i];

        var infoLines = new List<string>();
        infoLines.Add(p.name);
        infoLines.Add("");
        infoLines.Add("Type: " + (p.gasGiant ? "gas giant" : "rocky"));
        infoLines.Add("Temperature: " + p.temperature);
        infoLines.Add("Radius: " + (p.explorationUnits * 50) + " km");

        var surfaceDescription = "";
        if (p.explorationBonus < 6000) {
            surfaceDescription = "very poor";
        } else if (p.explorationBonus < 9000) {
            surfaceDescription = "poor";
        } else if (p.explorationBonus < 12000) {
            surfaceDescription = "normal";
        } else if (p.explorationBonus < 15000) {
            surfaceDescription = "rich";
        } else {
            surfaceDescription = "very rich";
        }

        infoLines.Add("Surface resources: " + surfaceDescription);

        infoLines.Add("Explored: " + QMath.Percantage(p.explored, p.explorationUnits) + "%");

        _planetsPopup.GetNode<Label>("PlanetInfo/BasicInfo").Text = string.Join("\n", infoLines);

        _planetsPopup.GetNode<AnimatedPlanetNode>("PlanetModel").Visible = true;
        _planetsPopup.GetNode<AnimatedPlanetNode>("PlanetModel").SetSprite(p.textureName);
    }

    private void OnPlanetsDoneButton() {
        _lockControls = false;
        _planetsPopup.Hide();
    }

    private void OnPlanetsButton() {
        _lockControls = true;
        StopMovement();
        UpdatePlanetsMenu();
        _planetsPopup.PopupCentered();
    }

    private void OnMiningButton() {
        _lockControls = true;
        StopMovement();
        _miningPopup.PopupCentered();
    }

    private void OnMiningDoneButton() {
        _lockControls = false;
        _miningPopup.Hide();
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
                LeaveSystem();
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

    private void LeaveSystem() {
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            p.activeDrone = "";
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
        unitNode.Connect("SearchForStarBase", this, nameof(OnSearchForStarBase), args);
        unitNode.Connect("MovePhaaStarBase", this, nameof(OnMovePhaaStarBase), args);

        AddChild(unitNode);
        // unitNode.GlobalPosition = unitNode.unit.pos;
        _spaceUnits.Add(unitNode);
    }

    private void OnMovePhaaStarBase(SpaceUnitNode unitNode) {
        var oldSystem = RpgGameState.phaaBase.system.Get();

        var sys = RpgGameState.starSystemByPos[unitNode.unit.pos];
        sys.starBase = RpgGameState.phaaBase.GetRef();
        var systemNode = _starSystemNodeByStarSystem[sys];

        var oldSystemNode = _starSystemNodeByStarSystem[oldSystem];
        oldSystemNode.DetachStarBase();

        RpgGameState.phaaBase.system = sys.GetRef();
        var starBaseNode = NewStarBaseNode(sys);
        AddChild(starBaseNode);
        systemNode.AddStarBase(starBaseNode);
    }

    private void OnSpaceUnitCreated(SpaceUnitNode unitNode) {
        AddSpaceUnit(unitNode);
    }

    private StarBaseNode NewStarBaseNode(StarSystem sys) {
        StarBaseNode baseNode;

        var starBase = sys.starBase.Get();
        if (starBase.owner == Faction.Earthling) {
            baseNode = HumanStarBaseNode.New(starBase);
        } else if (starBase.owner == Faction.Draklid) {
            baseNode = DraklidStarBaseNode.New(starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else if (starBase.owner == Faction.Krigia) {
            baseNode = KrigiaStarBaseNode.New(starBase);
            baseNode.Connect("SpaceUnitCreated", this, nameof(OnSpaceUnitCreated));
        } else if (starBase.owner == Faction.Phaa) {
            baseNode = PhaaStarBaseNode.New(starBase);
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
            SpaceUnitNode node = null;
            if (u.owner == Faction.Draklid) {
                node = DraklidSpaceUnitNode.New(u); 
            } else if (u.owner == Faction.Phaa) {
                node = PhaaSpaceUnitNode.New(u);
            } else if (u.owner == Faction.Krigia) {
                node = KrigiaSpaceUnitNode.New(u);
            } else if (u.owner == Faction.Rarilou) {
                node = RarilouSpaceUnitNode.New(u);
            } else if (u.owner == Faction.Earthling) {
                // Handled above.
            } else {
                throw new Exception("unexpected unit owner: " + u.owner.ToString());
            }
            if (node != null) {
                AddSpaceUnit(node);
            }
        }
    }

    private void EnterSystem(StarSystem sys) {
        _gameState.travelSlowPoints = 0;
        _currentSystem = _starSystemNodeByStarSystem[sys];
        _dstSystem = null;
        _currentSystem.OnPlayerEnter(Faction.Earthling);
        if (sys.starBase.id != 0) {
            if (sys.starBase.Get().owner == Faction.Earthling) {
                RecoverFleetEnergy(_humanUnit.fleet);
            }
        }

        if (sys.color == StarColor.Purple) {
            var e = MapEventRegistry.eventByTitle["Purple System Visitor"];
            if (_gameState.randomEventsAvailable.Contains(e.title)) {
                _randomEventProto = e;
                _gameState.randomEventsAvailable.Remove(_randomEventProto.title);
                OpenRandomEvent(NewRandomEventContext());
                return;
            }
        }

        if (sys.starBase.id == 0) {
            SpaceUnit rarilouUnit = null;
            foreach (var x in _gameState.spaceUnits.objects.Values) {
                if (x.pos == sys.pos && x.owner == Faction.Rarilou) {
                    rarilouUnit = x;
                    break;
                }
            }
            if (rarilouUnit != null) {
                _randomEventProto = new RarilouEncounterMapEvent();
                var ctx = NewRandomEventContext();
                ctx.spaceUnit = rarilouUnit;
                RpgGameState.arenaUnit1 = rarilouUnit;
                OpenRandomEvent(ctx);
                return;
            }
        }

        if (_gameState.randomEventCooldown == 0 && sys.randomEventCooldown == 0) {
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

        var enterSystemEvents = new List<AbstractMapEvent>();
        foreach (var eventTitle in _gameState.randomEventsAvailable) {
            var e = MapEventRegistry.eventByTitle[eventTitle];
            if (e.triggerKind != AbstractMapEvent.TriggerKind.OnSystemEntered) {
                continue;
            }
            if (!e.Condition()) {
                continue;
            }
            enterSystemEvents.Add(e);
        }
        if (enterSystemEvents.Count == 0) {
            return;
        }

        _randomEventProto = QRandom.Element(enterSystemEvents);
        _gameState.randomEventsAvailable.Remove(_randomEventProto.title);
        OpenRandomEvent(NewRandomEventContext());
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
            var hpPercentage = QMath.Percantage(p.hp, p.MaxHp());
            var energyPercentage = QMath.Percantage(p.energy, p.MaxBackupEnergy());
            var unit = _unitMembers[i];
            unit.UpdateStatus(hpPercentage, energyPercentage);
        }

        foreach (var u in _spaceUnits) {
            u.UpdateColor();
        }

        UpdateDronesValue();
        UpdateDayValue();
        UpdateFuelValue();
        UpdateExpValue();
        UpdateCreditsValue();
        UpdateCargoValue();
        if (_currentSystem != null) {
            GetNode<Label>("UI/LocationValue").Text = _currentSystem.sys.name;
            if (_currentSystem.sys.starBase.id != 0 && _currentSystem.sys.starBase.Get().owner == Faction.Earthling) {
                enterBase.Disabled = false;
            }
            // Can mine only in own or neutral systems.
            if (_currentSystem.sys.starBase.id == 0 || _currentSystem.sys.starBase.Get().owner == Faction.Earthling) {
                mining.Disabled = false;
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

    private void UpdateDronesValue() {
        GetNode<Label>("UI/DronesValue").Text = _gameState.explorationDrones.Count.ToString();
    }

    private void UpdateDayValue() {
        GetNode<Label>("UI/DayValue").Text = _gameState.day.ToString();
    }

    private void UpdateFuelValue() {
        GetNode<Label>("UI/FuelValue").Text = ((int)_gameState.fuel).ToString();
    }

    private void RecoverFleetEnergy(List<Vessel.Ref> fleet) {
        foreach (var handle in fleet) {
            var v = handle.Get();
            v.energy = v.MaxBackupEnergy();
        }
    }

    private float CalculateFleetSpeed() {
        float multiplier = 1.0f;
        if (_gameState.travelSlowPoints > 0) {
            multiplier -= 0.5f;
        }
        if (_gameState.technologiesResearched.Contains("Aligned Jumping")) {
            multiplier += 0.1f;
        }
        if (_gameState.technologiesResearched.Contains("Rarilou Warping")) {
            multiplier += 0.15f;
        }
        return _gameState.travelSpeed * multiplier;
    }

    private void ProcessDayEvents() {
        _human.node.speed = CalculateFleetSpeed();

        _gameState.travelSlowPoints = QMath.ClampMin(_gameState.travelSlowPoints - 1, 0);
        _gameState.randomEventCooldown = QMath.ClampMin(_gameState.randomEventCooldown - 1, 0);

        ProcessStarSystems();
        ProcessUnits();
        ProcessResearch();

        if (_currentSystem == null) {
            ProcessInterstellarDay();
        } else {
            ProcessStarSystemDay();
        }

        ProcessKrigiaActions();
        ProcessPhaaActions();
        ProcessRarilouActions();

        if (_gameState.day == _gameState.missionDeadline) {
            SpawnKrigiaFinalAttack(new Vector2(512, 224), RpgGameState.StartingSystem().pos);
        }
    }

    private void UpdatePlanetsMenu() {
        _planetsPopup.GetNode<AnimatedPlanetNode>("PlanetModel").Visible = false;
        _planetsPopup.GetNode<Label>("PlanetInfo/BasicInfo").Text = "";

        var planets = _currentSystem.sys.resourcePlanets;
        for (int i = 0; i < 3; i++) {
            var planetRow = _planetsPopup.GetNode<Label>($"Planet{i}");
            planetRow.Visible = i < planets.Count;
            if (i >= planets.Count) {
                continue;
            }
            var p = planets[i];
            planetRow.Text = p.name;
            var droneSelect = planetRow.GetNode<OptionButton>("DroneSelect");
            droneSelect.Clear();
            if (p.explored == p.explorationUnits) {
                planetRow.GetNode<Label>("Minerals").Text = p.mineralsPerDay.ToString();
                planetRow.GetNode<Label>("Organic").Text = p.organicPerDay.ToString();
                planetRow.GetNode<Label>("Power").Text = p.powerPerDay.ToString();
                planetRow.GetNode<ButtonNode>("SendDroneButton").Disabled = true;
                droneSelect.Disabled = true;
            } else {
                planetRow.GetNode<Label>("Minerals").Text = "?";
                planetRow.GetNode<Label>("Organic").Text = "?";
                planetRow.GetNode<Label>("Power").Text = "?";
                planetRow.GetNode<ButtonNode>("SendDroneButton").Disabled = p.IsExplored() || p.activeDrone != "" || _gameState.explorationDrones.Count == 0;
                droneSelect.Disabled = p.activeDrone != "" || p.IsExplored() || _gameState.explorationDrones.Count == 0;
            }

            if (!droneSelect.Disabled) {
                droneSelect.AddItem("No drone deployed");
                var droneSet = new HashSet<string>();
                foreach (var drone in _gameState.explorationDrones) {
                    var d = ExplorationDrone.Find(drone);
                    if (d.maxTemp < p.temperature) {
                        continue;
                    }
                    if (p.gasGiant && !d.canExploreGasGiants) {
                        continue;
                    }
                    if (droneSet.Contains(drone)) {
                        continue;
                    }
                    droneSet.Add(drone);
                    droneSelect.AddItem(drone);
                }
            } else {
                if (p.activeDrone != "") {
                    droneSelect.AddItem(p.activeDrone);
                } else {
                    droneSelect.AddItem("No drone deployed");
                }
            }
        }
    }

    private void ProcessResearch() {
        if (_gameState.currentResearch == "") {
            return;
        }

        var research = Research.Find(_gameState.currentResearch);

        _gameState.researchProgress += RpgGameState.ResearchRate();

        if (research.material != Faction.Neutral) {
            _gameState.researchMaterial.Add(-1, research.material);
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
        } else if (research.category == Research.Category.NewExplorationDrone) {
            text += "New exploration drone is available for production\n\n";
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

    private void ProcessDrones() {
        foreach (var p in _currentSystem.sys.resourcePlanets) {
            if (p.activeDrone == "") {
                continue;
            }
            var drone = ExplorationDrone.Find(p.activeDrone);
            var exploredBefore = p.explored;
            p.explored = QMath.ClampMax(p.explored + drone.explorationRate, p.explorationUnits);
            var exploredToday = p.explored - exploredBefore;
            int bountyPerUnit = p.explorationBonus / p.explorationUnits;
            _gameState.credits += bountyPerUnit * exploredToday;

            if (p.artifact != "" && p.explored > p.explorationUnits/2) {
                _gameState.artifactsRecovered.Add(p.artifact);
                p.artifact = "";
                var notification = MapNotificationNode.New("Artifact recovered");
                _currentSystem.AddChild(notification);
                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_recovered.wav"));
                _currentSystem.UpdateInfo();
                StopMovement();
            }

            if (p.IsExplored()) {
                p.activeDrone = "";
                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
                var notification = MapNotificationNode.New(p.name + " explored");
                _currentSystem.AddChild(notification);
                StopMovement();
            }
        }
    }

    private void ProcessStarSystemDay() {
        ProcessUnitMode();
        ProcessDrones();

        var starBase = _currentSystem.sys.starBase;
        if (starBase.id != 0) {
            if (starBase.Get().owner == Faction.Earthling) {
                RecoverFleetEnergy(_humanUnit.fleet);
            }
        }

        _currentSystem.UpdateInfo();

        foreach (var u in _spaceUnits) {
            if (u.unit.pos == _currentSystem.sys.pos) {
                if (RollUnitAttack(u)) {
                    return;
                }
            }
        }

        if (starBase.id != 0) {
            if (_gameState.FactionsAtWar(starBase.Get().owner, Faction.Earthling)) {
                RollFleetAttack();
            }
        }
    }

    private bool RollUnitAttack(SpaceUnitNode u) {
        if (u.unit.owner == Faction.Draklid) {
            TriggerDraklidEvent(u);
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

    private bool DraklidsWantToAttack(SpaceUnitNode u) {
        if (u.unit.CargoFree() == 0) {
            return false;
        }
        var draklidForce = u.unit.FleetCost();
        var humanForce = _humanUnit.FleetCost();
        return draklidForce * 2 > humanForce;
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

    private void TriggerDraklidEvent(SpaceUnitNode u) {
        var pluralSuffix = u.unit.fleet.Count == 1 ? "" : "s";
        var text = "Short-range radars detect a Draklid raid unit. ";
        text += "They have " + u.unit.fleet.Count + " vessel" + pluralSuffix + ".\n\n";
        if (DraklidsWantToAttack(u)) {
            text += "Based on the fact that it's moving towards your direction, ";
            text += "the battle is imminent, unless you sacrifice some fuel and warp away.";
            _draklidsEventPopup.GetNode<ButtonNode>("FightButton").Text = "Prepare for battle";
            _draklidsEventPopup.GetNode<ButtonNode>("LeaveButton").Text = "Retreat";
            _draklidsEventPopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RetreatFuelCost();
        } else {
            text += "It looks like they're not looking for a fight.";
            _draklidsEventPopup.GetNode<ButtonNode>("FightButton").Text = "Attack them";
            _draklidsEventPopup.GetNode<ButtonNode>("LeaveButton").Text = "Ignore them";
        }
        _draklidsEventPopup.GetNode<Label>("Panel/Text").Text = text;

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _draklidsEventPopup.PopupCentered();
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
            if (!_gameState.FactionsAtWar(starBase.owner, Faction.Earthling)) {
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
            // if (_gameState.fuel < 3) {
            //     SetUnitMode(UnitMode.Idle);
            //     StopMovement();
            //     return;
            // }
            // if (_currentSystem.sys.artifact == null) {
            //     return;
            // }
            // if (starBaseRef.id != 0 && _gameState.FactionsAtWar(starBaseRef.Get().owner, Faction.Earthling)) {
            //     _currentSystem.sys.artifactRecoveryDelay -= 1;
            // } else {
            //     _currentSystem.sys.artifactRecoveryDelay -= 2;
            // }
            // if (_currentSystem.sys.artifactRecoveryDelay <= 0) {
            //     _gameState.artifactsRecovered.Add(_currentSystem.sys.artifact);

            //     _currentSystem.sys.artifactRecoveryDelay = 0;
            //     _currentSystem.sys.artifact = null;

            //     var notification = MapNotificationNode.New("Artifact recovered");
            //     _currentSystem.AddChild(notification);

            //     _gameState.credits += 3000;

            //     GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_recovered.wav"));

            //     SetUnitMode(UnitMode.Idle);
            //     StopMovement();

            //     _currentSystem.UpdateInfo();
            // }
            // _gameState.fuel -= 3;
            // return;
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
            if (v.id == _humanUnit.fleet[0].id) {
                ArenaSettings.flagship = v;
            }

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
            alliances[v] = _gameState.FactionsAtWar(Faction.Earthling, v.faction) ? 1 : 0;
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

    private void ProcessRarilouActions() {
        _gameState.rarilouPlans.unitSpawnDelay = QMath.ClampMin(_gameState.rarilouPlans.unitSpawnDelay - 1, 0);

        if (_gameState.rarilouPlans.unitSpawnDelay != 0) {
            return;
        }

        var numUnits = 0;
        foreach (var u in _gameState.spaceUnits.objects.Values) {
            if (u.owner == Faction.Rarilou) {
                numUnits++;
            }
        }
        if (numUnits >= 2) {
            return;
        }

        _gameState.rarilouPlans.unitSpawnDelay = QRandom.IntRange(400, 900);

        var fleetSize = QRandom.IntRange(1, 3);
        var fleet = new List<Vessel.Ref>();
        for (int i = 0; i < fleetSize; i++) {
            var rank = QRandom.IntRange(1, 2);
            if (_gameState.day > 1400) {
                rank++;
            }
            var vessel = VesselFactory.NewVessel(Faction.Rarilou, "Leviathan", rank);
            fleet.Add(vessel.GetRef());
        }

        var candidateSystems = new List<StarSystem>();
        foreach (var sys in _gameState.starSystems.objects.Values) {
            if (sys.starBase.id != 0) {
                continue;
            }
            if (sys.color == StarColor.Purple) {
                continue;
            }
            candidateSystems.Add(sys);
        }
        var system = QRandom.Element(candidateSystems);

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Rarilou;
        spaceUnit.pos = QMath.RandomizedLocation(system.pos, 60);
        spaceUnit.waypoint = system.pos;
        spaceUnit.fleet = fleet;

        if (_gameState.humanUnit.Get().pos.DistanceTo(spaceUnit.pos) <= RpgGameState.RadarRange()) {
            var notification = MapNotificationNode.New("Unit Materialized");
            AddChild(notification);
            notification.GlobalPosition = spaceUnit.pos;
        }

        var unitNode = RarilouSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
    }

    private void ProcessPhaaActions() {
        _gameState.phaaPlans.transitionDelay = QMath.ClampMin(_gameState.phaaPlans.transitionDelay - 1, 0);

        if (_gameState.phaaPlans.transitionDelay != 0) {
            return;
        }

        if (RpgGameState.phaaBase == null) {
            return;
        }

        var candidateSystems = new List<StarSystem>();
        var currentSystem = RpgGameState.phaaBase.system.Get();
        foreach (var sys in _gameState.starSystems.objects.Values) {
            if (sys.starBase.id != 0) {
                continue;
            }
            if (sys.color == StarColor.Purple) {
                continue;
            }
            if (currentSystem.pos.DistanceTo(sys.pos) < 500) {
                candidateSystems.Add(sys);
            }
        }

        if (candidateSystems.Count == 0) {
            _gameState.phaaPlans.transitionDelay = QRandom.IntRange(10, 100);
            return;
        }

        var targetSystem = QRandom.Element(candidateSystems);

        var alliedBase = currentSystem.starBase.Get();
        var arkGroup = new List<Vessel.Ref>();
        var groupSize = 4;
        var keptInGarrison = alliedBase.garrison.FindAll(v => {
            if (arkGroup.Count == groupSize) {
                return true;
            }
            arkGroup.Add(v);
            return false;
        });

        if (arkGroup.Count < groupSize) {
            return;
        }

        alliedBase.garrison = keptInGarrison;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Phaa;
        spaceUnit.pos = alliedBase.system.Get().pos;
        spaceUnit.waypoint = targetSystem.pos;
        spaceUnit.botProgram = SpaceUnit.Program.PhaaArk;
        spaceUnit.botOrigin = alliedBase.GetRef();
        spaceUnit.fleet = arkGroup;

        alliedBase.units.Add(spaceUnit.GetRef());

        var unitNode = PhaaSpaceUnitNode.New(spaceUnit);
        AddSpaceUnit(unitNode);
        _gameState.phaaPlans.transitionDelay = QRandom.IntRange(150, 250);
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
            VesselFactory.NewVessel(Faction.Krigia, "Ashes", 3).GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns", 3).GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns", 3).GetRef(),
            VesselFactory.NewVessel(Faction.Krigia, "Horns", 3).GetRef(),
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
