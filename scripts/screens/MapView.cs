using Godot;
using System;
using System.Collections.Generic;
using CheatCommandKind = MapViewCheatMenuPopupNode.CommandKind;

public class MapView : Node2D, IMapViewContext {
    const float MAP_WIDTH = (1080 * 3) + 220;

    private AbstractMapEvent _randomEventProto;

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
    private PlanetsMenuPopupNode _planetsPopup;
    private PopupNode _starSystemMenu;
    private PopupNode _researchCompletedPopup;
    private PopupNode _patrolReachesBasePopup;
    private MapViewCheatMenuPopupNode _cheatsPopup;

    private SpaceUnitNode _eventUnit;
    private PopupNode _starBaseAttackPopup;
    private PopupNode _krigiaTaskForcePopup;

    private HashSet<SpaceUnitNode> _spaceUnits = new HashSet<SpaceUnitNode>();

    private List<UnitMemberNode> _unitMembers = new List<UnitMemberNode>();

    public void AddPlayerUnitMember(Vessel v) {
        DoAddUnitMember(v);
        ReorderUnitMembers();
    }

    public void UpdateUI() { DoUpdateUI(); }
    public void AddUIChild(Node n) { GetNode<CanvasLayer>("UI").AddChild(n); }
    public void RemovePlayerUnitMember(int index) { DoRemovePlayerVessel(index); }

    public void EnterSystem(StarSystem sys) {
        _humanUnit.pos = sys.pos;
        _human.GlobalPosition = _humanUnit.pos;
        _human.node.GlobalPosition = _humanUnit.pos;
        DoEnterSystem(sys);
    }

    public void CreateNotification(Vector2 pos, string text) {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapNotificationNode.New(text);
        AddChild(notification);
        notification.GlobalPosition = pos;
    }

    public void CreateBadNotification(Vector2 pos, string text) {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapBadNotificationNode.New(text);
        AddChild(notification);
        notification.GlobalPosition = pos;
    }

    private void SwitchButtonTextures(TextureButton b) {
        var tmp = b.TextureNormal;
        b.TextureNormal = b.TexturePressed;
        b.TexturePressed = tmp;
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
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase, RpgGameState.arenaUnit1);
        } else if (RpgGameState.transition == RpgGameState.MapTransition.EnemyUnitDestroyed) {
            ProcessUnitCasualties(_humanUnit);
            ProcessUnitCasualties(RpgGameState.arenaUnit1);
            ProcessUnitCasualties(RpgGameState.arenaUnit2);
        } else if (RpgGameState.transition == RpgGameState.MapTransition.BaseAttackSimulation) {
            ProcessStarBaseCasualties(RpgGameState.garrisonStarBase, RpgGameState.arenaUnit2);
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

        RenderMap();

        _movementToggle = GetNode<TextureButton>("UI/MovementToggle");
        _movementToggle.Connect("pressed", this, nameof(OnMovementTogglePressed));

        AddUnitMembers();

        GetNode<TextureButton>("UI/EnterBaseButton").Connect("pressed", this, nameof(OnEnterBaseButton));
        GetNode<TextureButton>("UI/MiningButton").Connect("pressed", this, nameof(OnPlanetsButton));
        GetNode<TextureButton>("UI/ActionMenuButton").Connect("pressed", this, nameof(OnActionMenuButton));
        GetNode<TextureButton>("UI/ResearchButton").Connect("pressed", this, nameof(OnResearchButton));
        GetNode<TextureButton>("UI/QuestLogButton").Connect("pressed", this, nameof(OnQuestLogButton));

        _patrolReachesBasePopup = GetNode<PopupNode>("UI/PatrolReachesBasePopup");
        _patrolReachesBasePopup.GetNode<ButtonNode>("AttackButton").Connect("pressed", this, nameof(OnPatrolReachesBaseAttackButton));
        _patrolReachesBasePopup.GetNode<ButtonNode>("IgnoreButton").Connect("pressed", this, nameof(OnPatrolReachesBaseIgnoreButton));

        _researchCompletedPopup = GetNode<PopupNode>("UI/ResearchCompletedPopup");
        _researchCompletedPopup.GetNode<ButtonNode>("DoneButton").Connect("pressed", this, nameof(OnResearchCompleteDoneButton));
        _researchCompletedPopup.GetNode<ButtonNode>("OpenResearchScreen").Connect("pressed", this, nameof(OnResearchCompleteOpenResearchScreenButton));

        _starBaseAttackPopup = GetNode<PopupNode>("UI/BaseUnderAttackPopup");
        _starBaseAttackPopup.GetNode<ButtonNode>("PlayButton").Connect("pressed", this, nameof(OnStarBaseAttackPlayButton));

        _krigiaTaskForcePopup = GetNode<PopupNode>("UI/KrigiaTaskForcePopup");
        _krigiaTaskForcePopup.GetNode<ButtonNode>("FightButton").Connect("pressed", this, nameof(OnFightEventUnit));
        _krigiaTaskForcePopup.GetNode<ButtonNode>("LeaveButton").Connect("pressed", this, nameof(OnKrigiaTaskForceLeaveButton));

        _cheatsPopup = GetNode<MapViewCheatMenuPopupNode>("UI/CheatMenuPopup");
        _cheatsPopup.Connect("CommandExecuted", this, nameof(OnCheatCommandExecuted));
        _cheatsPopup.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnCheatsDone));

        _starSystemMenu = GetNode<PopupNode>("UI/StarSystemMenuPopup");
        _starSystemMenu.GetNode<ButtonNode>("Done").Connect("pressed", this, nameof(OnStarSystemMenuDone));
        _starSystemMenu.GetNode<ButtonNode>("Attack").Connect("pressed", this, nameof(OnAttackBaseButton));
        _starSystemMenu.GetNode<ButtonNode>("ConvertPower").Connect("pressed", this, nameof(OnConvertPower));
        _starSystemMenu.GetNode<ButtonNode>("BuildNewBase").Connect("pressed", this, nameof(OnBuildNewBase));

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
        _camera.Position = new Vector2(_humanUnit.pos.x, GetViewport().GetVisibleRect().Size.y / 2);
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

    private void ProcessStarBaseCasualties(StarBase starBase, SpaceUnit defenders) {
        var survivors = RemoveCasualties(defenders.fleet);
        defenders.deleted = true;
        foreach (var v in survivors) {
            v.Get().energy = v.Get().MaxBackupEnergy();
            starBase.garrison.Add(v);
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
        var defenders = new List<Vessel.Ref>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.PopVessel());
        }
        RpgGameState.garrisonStarBase = starBase;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = starBase.owner;
        spaceUnit.pos = starBase.system.Get().pos;
        spaceUnit.fleet = defenders;

        ArenaManager.SetArenaSettings(system, u.unit.fleet, defenders);
        ArenaSettings.isStarBaseBattle = true;
        RpgGameState.arenaUnit1 = u.unit;
        RpgGameState.arenaUnit2 = spaceUnit;
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnFightEventUnit() {
        var u = _eventUnit;
        RpgGameState.arenaUnit1 = u.unit;
        ArenaManager.SetArenaSettings(_currentSystem.sys, ConvertVesselList(u.unit.fleet), ConvertVesselList(_humanUnit.fleet));
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnKrigiaTaskForceLeaveButton() {
        _lockControls = false;
        _krigiaTaskForcePopup.Hide();
        _gameState.fuel -= RpgGameState.RetreatFuelCost();
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

        if (taskForce.unit.HasBomber()) {
            StopMovement();
            RpgGameState.humanBases.Remove(starBase);
            _starSystemNodeByStarSystem[starBase.system.Get()].DestroyStarBase();
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/base_eradicated.wav"));
        }
    }

    private void ShowBattleResults() {
        _lockControls = true;

        var popupBuilder = new BattleResultPopup();
        popupBuilder.SetOnResolved(() => { _lockControls = false; });
        popupBuilder.SetBattleResult(RpgGameState.lastBattleResult);
        
        var popup = popupBuilder.Build();
        GetNode<CanvasLayer>("UI").AddChild(popup);
        popup.PopupCentered();
    }

    private void OpenCheats() {
        _lockControls = true;
        StopMovement();
        _cheatsPopup.PopupCentered();
    }

    private void OnCheatCommandExecuted(MapViewCheatMenuPopupNode.Command command) {
        switch (command.kind) {
            case CheatCommandKind.RevealMap:
                foreach (var sys in _starSystemNodes) {
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

        var popupBuilder = new RandomMapEventPopupBuilder();
        popupBuilder.SetOnResolved(() => { _lockControls = false; });
        popupBuilder.SetMapEvent(_randomEventProto.Create(ctx));
        popupBuilder.SetMapViewContext(this);
        popupBuilder.SetMapEventContext(ctx);

        _lockControls = true;
        var popup = popupBuilder.Build();
        GetNode<CanvasLayer>("UI").AddChild(popup);
        popup.PopupCentered();
    }

    private void AddUnitMembers() {
        foreach (var handle in _humanUnit.fleet) {
            DoAddUnitMember(handle.Get());
        }
        ReorderUnitMembers();
    }

    private void DoAddUnitMember(Vessel v) {
        var unitMembers = GetNode<Label>("UI/UnitMembers");
        var box = unitMembers.GetNode<VBoxContainer>("Box");

        var hpPercentage = QMath.Percantage(v.hp, v.MaxHp());
        var energyPercentage = QMath.Percantage(v.energy, v.MaxBackupEnergy());
        var m = UnitMemberNode.New(v.pilotName, v.Design().Texture(), hpPercentage, energyPercentage);
        _unitMembers.Add(m);
        box.AddChild(m);
    }

    private void ReorderUnitMembers() {
        var offsetY = 128;
        for (int i = 0; i < _unitMembers.Count; i++) {
            _unitMembers[i].Position = new Vector2(16, (offsetY * i) + 8);
        }
    }

    private void OnStarSystemMenuDone() {
        _lockControls = false;
        _starSystemMenu.Hide();
    }

    private void OnAttackBaseButton() {
        _currentSystem.DestroyStarBase();
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/enemy_base_eradicated.wav"));
    }

    private void OnConvertPower() {
        if (_humanUnit.cargo.power < 5) {
            return;
        }
        _humanUnit.cargo.power -= 5;
        if (_gameState.technologiesResearched.Contains("Improved Power Conversion")) {
            _gameState.fuel += 20;
        } else {
            _gameState.fuel += 15;
        }
        UpdateUI();
    }

    private bool CanBuildStarBase() {
        return _currentSystem != null && _currentSystem.sys.starBase.id == 0;
    }

    private void DoRemovePlayerVessel(int index) {
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

        DoRemovePlayerVessel(arkIndex);

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
        if (starBase.owner == Faction.Earthling) {
            RpgGameState.enteredBase = starBase;
            GetTree().ChangeScene("res://scenes/screens/StarBaseScreen.tscn");
        } else if (starBase.owner == Faction.Phaa) {
            _randomEventProto = new PhaaBaseMapEvent();
            OpenRandomEvent(NewRandomEventContext());
        }
    }

    private void OnResearchButton() {
        StopMovement();
        GetTree().ChangeScene("res://scenes/screens/ResearchScreen.tscn");
    }

    private void OnQuestLogButton() {
        StopMovement();
        GetTree().ChangeScene("res://scenes/screens/QuestLogScreen.tscn");
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
    }

    private void OnPlanetMouseEnter(int i) {
        var p = _currentSystem.sys.resourcePlanets[i];

        var infoLines = new List<string>();
        infoLines.Add(p.name + " (" + QMath.Percantage(p.explored, p.explorationUnits) + "% explored)");
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
        var viewportRect = viewport.GetVisibleRect();
        var leftMargin = 56;
        var rightMargin = viewportRect.Size.x - 56;
        var cursor = viewport.GetMousePosition();

        if (cursor.x < leftMargin) {
            if (cursor.x < 16) {
                cameraPos -= _cameraSpeed * delta * 3;
            } else {
                cameraPos -= _cameraSpeed * delta;
            }
        } else if (cursor.x > rightMargin) {
            if (cursor.x > (viewportRect.Size.x - 16)) {
                cameraPos += _cameraSpeed * delta * 3;
            } else {
                cameraPos += _cameraSpeed * delta;
            }
        }

        if (cameraPos == _camera.Position) {
            return;
        }

        var x = QMath.Clamp(cameraPos.x, viewportRect.Size.x / 2, MAP_WIDTH - viewportRect.Size.x / 2);
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

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            OpenGameMenu();
            return;
        }
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


        _human.node.ProcessTick(delta);
        foreach (var u in _spaceUnits) {
            u.ProcessTick(delta);
        }
        _spaceUnits.RemoveWhere((x) => x.unit.deleted || !IsInstanceValid(x));

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
        unitNode.unit.deleted = true;
    }

    private void MarkStarBaseAsDiscovered(StarBase starBase) {
        if (starBase.discoveredByKrigia != 0) {
            return;
        }
        starBase.discoveredByKrigia = _gameState.day;
        CreateBadNotification(starBase.system.Get().pos, "Base detected");
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

    private void DoEnterSystem(StarSystem sys) {
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
                _gameState.eventsResolved.Add(_randomEventProto.title);
                OpenRandomEvent(NewRandomEventContext());
                return;
            }
        }

        if (sys.starBase.id == 0) {
            SpaceUnit rarilouUnit = null;
            foreach (var x in _spaceUnits) {
                if (x.unit.pos == sys.pos && x.unit.owner == Faction.Rarilou) {
                    rarilouUnit = x.unit;
                    break;
                }
            }
            if (rarilouUnit != null) {
                _randomEventProto = new RarilouEncounterMapEvent();
                var ctx = NewRandomEventContext();
                ctx.spaceUnit = rarilouUnit;
                RpgGameState.arenaUnit1 = rarilouUnit;
                rarilouUnit.botProgram = SpaceUnit.Program.RarilouFlee;
                OpenRandomEvent(ctx);
                return;
            }
        }

        foreach (var q in _gameState.activeQuests) {
            if (q.name == "Phaa Rebels" && !_gameState.eventsResolved.Contains(q.name)) {
                _randomEventProto = new PhaaRebelsMapEvent();
                if (_randomEventProto.Condition()) {
                    _gameState.eventsResolved.Add(q.name);
                    OpenRandomEvent(NewRandomEventContext());
                }
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
        _gameState.eventsResolved.Add(_randomEventProto.title);
        OpenRandomEvent(NewRandomEventContext());
    }

    private void OnDestinationReached() {
        StopMovement();
        DoEnterSystem(_dstSystem.sys);
        UpdateUI();
    }

    private void DoUpdateUI() {
        var enterBase = GetNode<TextureButton>("UI/EnterBaseButton");
        enterBase.Disabled = true;
        var mining = GetNode<TextureButton>("UI/MiningButton");
        mining.Disabled = true;

        bool hasBomber = _humanUnit.HasBomber();

        bool isAtEnemyBase = _currentSystem != null &&
                             _currentSystem.sys.starBase.id != 0 &&
                             _currentSystem.sys.starBase.Get().owner != Faction.Earthling &&
                             _gameState.diplomaticStatuses[_currentSystem.sys.starBase.Get().owner] >= DiplomaticStatus.War;

        bool canDestroyEnemyBase = isAtEnemyBase && hasBomber && _currentSystem.sys.starBase.Get().garrison.Count == 0;

        _starSystemMenu.GetNode<ButtonNode>("ConvertPower").Disabled = _humanUnit.cargo.power < 5;
        _starSystemMenu.GetNode<ButtonNode>("Attack").Disabled = !canDestroyEnemyBase;
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

        UpdateDayValue();
        UpdateFuelValue();
        UpdateCreditsValue();
        UpdateCargoValue();
        if (_currentSystem != null) {
            GetNode<Label>("UI/LocationValue").Text = _currentSystem.sys.name;
            if (_currentSystem.sys.starBase.id != 0) {
                enterBase.Disabled = false;
            }
            // Can mine only in non-hostile systems.
            var canMine = _currentSystem.sys.starBase.id == 0 ||
                          _currentSystem.sys.starBase.Get().owner == Faction.Earthling ||
                          _gameState.diplomaticStatuses[_currentSystem.sys.starBase.Get().owner] >= DiplomaticStatus.NonAttackPact;
            if (canMine) {
                mining.Disabled = false;
            }
        } else {
            GetNode<Label>("UI/LocationValue").Text = "Undefined";
        }

        _human.Update();
    }

    private void UpdateCargoValue() {
        int max = _humanUnit.CargoCapacity();
        var current = _humanUnit.CargoSize();
        GetNode<Label>("UI/CargoValue").Text = $"{current}/{max}";
    }

    private void UpdateCreditsValue() {
        GetNode<Label>("UI/CreditsValue").Text = _gameState.credits.ToString();
    }

    private void UpdateDayValue() {
        GetNode<Label>("UI/DayValue").Text = _gameState.day.ToString();
    }

    private void UpdateFuelValue() {
        int max = (int)RpgGameState.MaxFuel();
        int current = (int)_gameState.fuel;
        GetNode<Label>("UI/FuelValue").Text = $"{current}/{max}";
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
                planetRow.GetNode<ButtonNode>("SendDroneButton").Visible = false;
                droneSelect.Disabled = true;
                droneSelect.Visible = false;
            } else {
                planetRow.GetNode<Label>("Minerals").Text = "?";
                planetRow.GetNode<Label>("Organic").Text = "?";
                planetRow.GetNode<Label>("Power").Text = "?";
                planetRow.GetNode<ButtonNode>("SendDroneButton").Disabled = p.IsExplored() || p.activeDrone != "" || _gameState.explorationDrones.Count == 0;
                planetRow.GetNode<ButtonNode>("SendDroneButton").Visible = true;
                droneSelect.Disabled = p.activeDrone != "" || p.IsExplored() || _gameState.explorationDrones.Count == 0;
                droneSelect.Visible = true;
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
        if ((int)_gameState.researchProgress >= researchTime) {
            ResearchCompleted();
        }
    }

    private void OnPatrolReachesBaseAttackButton() {
        var u = _eventUnit;

        // TODO: allow units selection?
        // FIXME: code is duplicated from OnStarBaseAttackPlayButton().
        var system = RpgGameState.starSystemByPos[u.unit.pos];
        var starBase = system.starBase.Get();
        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel.Ref>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.PopVessel());
        }
        RpgGameState.garrisonStarBase = starBase;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = starBase.owner;
        spaceUnit.pos = starBase.system.Get().pos;
        spaceUnit.fleet = defenders;

        ArenaManager.SetArenaSettings(system, u.unit.fleet, defenders);
        ArenaSettings.isStarBaseBattle = true;
        RpgGameState.arenaUnit1 = u.unit;
        RpgGameState.arenaUnit2 = spaceUnit;
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnPatrolReachesBaseIgnoreButton() {
        _lockControls = false;
        _patrolReachesBasePopup.Hide();

        var system = RpgGameState.starSystemByPos[_eventUnit.unit.pos];
        var starBase = system.starBase.Get();
        MarkStarBaseAsDiscovered(starBase);
    }

    private void OnResearchCompleteOpenResearchScreenButton() {
        StopMovement();
        GetTree().ChangeScene("res://scenes/screens/ResearchScreen.tscn");
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
        } else if (research.category == Research.Category.NewBaseModule) {
            text += "New base module is available for construction\n\n";
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
                CreateNotification(_currentSystem.GlobalPosition, "Artifact recovered");
                StopMovement();
            }

            if (p.IsExplored()) {
                p.activeDrone = "";
                CreateNotification(_currentSystem.GlobalPosition, p.name + " explored");
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
                foreach (var handle in _humanUnit.fleet) {
                    var v = handle.Get();
                    v.hp = QMath.ClampMax(v.hp + 1, v.MaxHp());
                }
            }
        }

        foreach (var u in _spaceUnits) {
            if (u.unit.pos == _currentSystem.sys.pos) {
                if (RollUnitAttack(u)) {
                    return;
                }
            }
        }

        if (starBase.id != 0 && starBase.Get().owner != Faction.Earthling) {
            if (_gameState.diplomaticStatuses[starBase.Get().owner] <= DiplomaticStatus.Unspecified) {
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

    private void TriggerKrigiaPatrolEvent(SpaceUnitNode u) {
        _randomEventProto = new KrigiaEncounterMapEvent();
        var ctx = NewRandomEventContext();
        ctx.spaceUnit = u.unit;
        RpgGameState.arenaUnit1 = u.unit;
        OpenRandomEvent(ctx);

        // _krigiaPatrolPopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RpgGameState.RetreatFuelCost();

        // GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        // StopMovement();
        // _lockControls = true;
        // _eventUnit = u;
        // _krigiaPatrolPopup.PopupCentered();
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
        _krigiaTaskForcePopup.GetNode<ButtonNode>("LeaveButton").Disabled = _gameState.fuel < RpgGameState.RetreatFuelCost();

        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/random_event.wav"));
        StopMovement();
        _lockControls = true;
        _eventUnit = u;
        _krigiaTaskForcePopup.PopupCentered();
    }

    private void TriggerDraklidEvent(SpaceUnitNode u) {
        _randomEventProto = new DraklidEncounterMapEvent();
        var ctx = NewRandomEventContext();
        ctx.spaceUnit = u.unit;
        RpgGameState.arenaUnit1 = u.unit;
        OpenRandomEvent(ctx);
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
        return ArenaManager.ConvertVesselList(list);
    }

    private void ProcessStarSystems() {
        foreach (StarSystemNode starSystem in _starSystemNodes) {
            starSystem.sys.randomEventCooldown = QMath.ClampMin(starSystem.sys.randomEventCooldown - 1, 0);
            starSystem.ProcessDay();
        }
    }

    private void ProcessUnitMode() {
        var toAdd = _gameState.technologiesResearched.Contains("Recycling") ? 2 : 1;
        _gameState.fuel = QMath.ClampMax(_gameState.fuel + toAdd, RpgGameState.MaxFuel());
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

    private void RollFleetAttack() {
        var starBase = _currentSystem.sys.starBase.Get();

        if (starBase.garrison.Count == 0) {
            return;
        }

        var roll = QRandom.Float();
        if (roll >= 0.25) {
            return;
        }

        var numDefenders = Math.Min(starBase.garrison.Count, 4);
        var defenders = new List<Vessel.Ref>();
        for (int i = 0; i < numDefenders; i++) {
            defenders.Add(starBase.PopVessel());
        }
        RpgGameState.garrisonStarBase = starBase;

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = starBase.owner;
        spaceUnit.pos = starBase.system.Get().pos;
        spaceUnit.fleet = defenders;

        _randomEventProto = new PatrolAttackMapEvent();
        var ctx = NewRandomEventContext();
        ctx.spaceUnit = spaceUnit;
        OpenRandomEvent(ctx);
    }

    private void ProcessRarilouActions() {
        _gameState.rarilouPlans.unitSpawnDelay = QMath.ClampMin(_gameState.rarilouPlans.unitSpawnDelay - 1, 0);

        if (_gameState.rarilouPlans.unitSpawnDelay != 0) {
            return;
        }

        var numUnits = 0;
        foreach (var u in _spaceUnits) {
            if (u.unit.owner == Faction.Rarilou) {
                numUnits++;
            }
        }
        if (numUnits >= 2) {
            return;
        }

        _gameState.rarilouPlans.unitSpawnDelay = QRandom.IntRange(350, 700);

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
            CreateNotification(spaceUnit.pos, "Unit materialized");
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
        starBase.botReinforcementsDelay = QRandom.IntRange(150, 300);

        var connectedSystems = RpgGameState.starSystemConnections[starBase.system.Get()];
        StarBase alliedBase = null;
        foreach (var sys in connectedSystems) {
            if (sys.starBase.id == 0 || sys.starBase.Get().owner != starBase.owner) {
                continue;
            }
            if (sys.starBase.Get().garrison.Count < 8) {
                continue;
            }
            bool hasBomber = false;
            foreach (var v in sys.starBase.Get().garrison) {
                if (v.Get().Design().canDestroyBase) {
                    hasBomber = true;
                    break;
                }
            }
            if (!hasBomber) {
                continue;
            }
            alliedBase = sys.starBase.Get();
            break;
        }
        if (alliedBase == null) {
            return;
        }

        var reinforcementsFleet = AssembleKrigiaTaskForce(alliedBase);
        if (reinforcementsFleet == null) {
            return;
        }

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
        CreateBadNotification(pos, "Krigia flagship arrives");

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

    private List<Vessel.Ref> AssembleKrigiaTaskForce(StarBase starBase) {
        var taskForceFleet = new List<Vessel.Ref>();
        var groupSize = QRandom.IntRange(2, 3);
        bool hasBomber = false;
        var keptInGarrison = starBase.garrison.FindAll(v => {
            if (v.Get().Design().level <= 2) {
                return true;
            }
            if (taskForceFleet.Count == groupSize) {
                if (!hasBomber && v.Get().Design().canDestroyBase) {
                    hasBomber = true;
                    taskForceFleet.Add(v);
                    return false;
                }
                return true;
            }
            if (v.Get().Design().canDestroyBase) {
                hasBomber = true;
            }
            taskForceFleet.Add(v);
            return false;
        });
        if (taskForceFleet.Count < groupSize || !hasBomber) {
            return null;
        }
        starBase.garrison = keptInGarrison;
        return taskForceFleet;
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

        var taskForceFleet = AssembleKrigiaTaskForce(nearestStarBase);
        if (taskForceFleet == null) {
            // Can't assemble a task force from this base.
            KrigiaBaseRequestReinforcements(nearestStarBase);
            _gameState.krigiaPlans.taskForceDelay = QRandom.IntRange(90, 140);
            return;
        }

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
