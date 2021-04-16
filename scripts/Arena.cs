using Godot;
using System;
using System.Collections.Generic;

/*
TODO:
- Waypoints are not removed if they were reached before they became the current waypoint
  (since they were already entered)
- Where to store AudioStream resources?
- Use move_towards for vessel movement?
- When player dies, sometimes firing does the disposed object access
- Ship size should change vessel mask size
- Point-defense should destroy hostile rockets
- Unused ideas: plasma, cryo, gauss, railgun
- Inconsistent naming: weapons vs weapon (in audio and scripts)
- Code dup in Explosion, DiskExplosion and WarpEffect
- Using GetParent to attach a child; should attach via GetTree()
- Inconsistent f floating suffix for int-like values

Weapon ideas:
- boomerang: electro blade that follows a boomerang pattern
- back rockets: fires 1 homing missile from the back
- confusion cannon: makes ship rotate randomly for some time
- weapon with charges, like melnorme primary weapon in SC2

Special weapon idea:
- afterburner: adds speed, leaves fire trail that can damage foes
- a weapon that follows the crosshair
- a weapon with charges

*/

public class Arena : Node2D {
    private List<Pilot> _pilots;
    private Dictionary<Vessel, Pilot> _pilotByVessel;
    private Dictionary<Pilot, Vessel> _vesselByPilot;
    private Pilot _flagshipPilot;
    private bool _battleIsOver = false;

    private RpgGameState _gameState;

    private float _envHazardTick = 0;

    private void ApplyAllianceColor(Node2D n, int alliance) {
        // 1 normal (bright)
        // 2 red
        // 3 green
        // 4 blue
        // 5 dark
        // 6 yellow
        if (alliance == 2) {
            n.Modulate = Color.Color8(255, 170, 170);
        } else if (alliance == 3) {
            n.Modulate = Color.Color8(170, 255, 170);
        } else if (alliance == 4) {
            n.Modulate = Color.Color8(170, 190, 240);
        } else if (alliance == 5) {
            n.Modulate = Color.Color8(130, 130, 130);
        } else if (alliance == 6) {
            n.Modulate = Color.Color8(255, 230, 70);
        }
    }

    private VesselNode CreateVesselNode(Vessel v, Pilot pilot) {
        var artifacts = new List<IArtifact>();
        foreach (ArtifactDesign a in v.artifacts) {
            if (a != EmptyArtifact.Design) {
                artifacts.Add(ArtifactFactory.New(a));
            }
        }

        var state = new VesselState(v.design, v.energySource);
        foreach (var a in artifacts) {
            a.Upgrade(state);
        }

        var vesselNode = VesselNode.New(pilot, state, v.design.Texture());
        vesselNode.artifacts = artifacts;
        foreach (WeaponDesign w in v.weapons) {
            vesselNode.weapons.Add(WeaponFactory.New(w, pilot));
        }
        if (v.specialWeapon != null) {
            vesselNode.specialWeapon = WeaponFactory.New(v.specialWeapon, pilot);
        }
        vesselNode.shield = ShieldFactory.New(v.shield, pilot);

        ApplyAllianceColor(vesselNode.GetNode<Sprite>("Sprite"), pilot.alliance);

        return vesselNode;
    }

    private void AddBotCombatant(Pilot pilot) {
        var bot = new RookieBot(pilot.Vessel);
        var computer = ComputerNode.New(bot, pilot);
        computer.Connect("Defeated", this, nameof(OnBotDefeated));
        AddChild(computer);
    }

    private void AddHumanCombatant(ArenaViewport v, Pilot pilot, bool isGamepad, int deviceId) {
        // TODO: HumanNode.New().
        var human = (HumanNode)GD.Load<PackedScene>("res://scenes/HumanNode.tscn").Instance();
        human.pilot = pilot;
        human.camera = v.camera;
        human.canvas = v.canvasLayer;
        human.playerInput = new PlayerInput(isGamepad, deviceId);

        if (deviceId == 0) {
            _flagshipPilot = pilot;
            human.Connect("Defeated", this, nameof(OnHumanDefeated));
        }

        var sentinel = AttackSentinelNode.New(human.pilot.Vessel, SentinelDesign.list[0]);
        AddChild(sentinel);

        var waypointLine = (WaypointLineNode)GD.Load<PackedScene>("res://scenes/WaypointLineNode.tscn").Instance();
        waypointLine.ClearPoints();
        waypointLine.camera = v.camera;
        waypointLine.origin = pilot.Vessel;
        v.canvasLayer.AddChild(waypointLine);
        human.pilot.Vessel.waypointLine = waypointLine;

        AddChild(human);
    }

    public void Run(List<ArenaViewport> viewports) {
        _pilotByVessel = new Dictionary<Vessel, Pilot>();
        _vesselByPilot = new Dictionary<Pilot, Vessel>();
        _pilots = new List<Pilot>();
        for (int i = 0; i < ArenaSettings.combatants.Count; i++) {
            var vessel = ArenaSettings.combatants[i];
            var pilot = new Pilot{
                name = vessel.pilotName,
                alliance = vessel.player.Alliance,
            };
            _pilots.Add(pilot);
            _pilotByVessel[vessel] = pilot;
            _vesselByPilot[pilot] = vessel;
        }

        VesselNode humanVessel = null;

        for (int i = 0; i < _pilots.Count; i++) {
            var pilot = _pilots[i];
            var combatant = ArenaSettings.combatants[i];
            
            var vesselNode = CreateVesselNode(combatant, pilot);
            vesselNode.State.hp = combatant.hp;
            vesselNode.State.backupEnergy = combatant.energy;
            pilot.Vessel = vesselNode;
            AddChild(vesselNode);
            vesselNode.AddToGroup("affectedByEnvHazard");
            var centerPos = new Vector2(GetTree().Root.Size.x / 2, GetTree().Root.Size.y / 2);
            vesselNode.GlobalPosition = combatant.spawnPos;
            vesselNode.Rotation = centerPos.AngleToPoint(vesselNode.GlobalPosition);
            
            if (combatant.isBot) {
                AddBotCombatant(pilot);
            } else {
                var v = viewports[combatant.deviceId];
                v.used = true;
                AddHumanCombatant(v, pilot, combatant.isGamepad, combatant.deviceId);
                if (humanVessel == null) {
                    humanVessel = vesselNode;
                }
            }
        }

        foreach (Pilot x in _pilots) {
            foreach (Pilot y in _pilots) {
                if (x == y) {
                    continue;
                }
                if (x.alliance != y.alliance) {
                    x.Enemies.Add(y);
                } else {
                    x.Allies.Add(y);
                }
            }
        }

        if (humanVessel != null) {
            GetNode<DebugUi>("CanvasLayer/DebugUi").ObserveVessel(humanVessel);
        } else {
            viewports[0].used = true;
            GetNode<DebugUi>("CanvasLayer/DebugUi").ObserveVessel(_pilots[0].Vessel);
        }
    }

    public override void _Ready() {
        ArenaState.Reset();

        _gameState = RpgGameState.instance;

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        QRandom.SetRandomNumberGenerator(rng);

        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayBattleMusic();

        if (ArenaSettings.speed != ArenaSettings.BattleSpeed.Normal) {
            if (ArenaSettings.speed == ArenaSettings.BattleSpeed.Slow) {
                Engine.TimeScale = 0.75f;
            } else if (ArenaSettings.speed == ArenaSettings.BattleSpeed.Fast) {
                Engine.TimeScale = 1.25f;
            } else {
                Engine.TimeScale = 2.00f;
            }
        }

        switch (ArenaSettings.envDanger) {
        case ArenaSettings.EnvDanger.Star: {
                var starHarard = StarHazardNode.New(ArenaSettings.starColor);
                AddChild(starHarard);
                starHarard.GlobalPosition = RandomScreenPosCentered();
                ArenaState.starHazard = starHarard;
                break;
            }
        case ArenaSettings.EnvDanger.PurpleNebula: {
                var nebula = PurpleNebulaNode.New();
                AddChild(nebula);
                nebula.GlobalPosition = RandomScreenPos();
                break;
            }
        case ArenaSettings.EnvDanger.BlueNebula: {
                var nebula = BlueNebulaNode.New();
                AddChild(nebula);
                nebula.GlobalPosition = RandomScreenPos();
                break;
            }
        }
    }

    private Vector2 RandomScreenPosCentered() {
        var screenSize = GetTree().Root.Size;
        var x = QRandom.FloatRange(640, screenSize.x - 640);
        var y = QRandom.FloatRange(96, screenSize.y - 96);
        return new Vector2(x, y);
    }

    private Vector2 RandomScreenPos() {
        var screenSize = GetTree().Root.Size;
        var x = QRandom.FloatRange(128, screenSize.x - 128);
        var y = QRandom.FloatRange(128, screenSize.y - 128);
        return new Vector2(x, y);
    }

    public override void _Process(float delta) {
        _envHazardTick += delta;
        if (_envHazardTick >= 1) {
            var nodes = GetTree().GetNodesInGroup("affectedByEnvHazard");
            foreach (var n in nodes) {
                if (n is Asteroid asteroid) {
                    asteroid.OnEnvHazardTick();
                } else if (n is VesselNode vesselNode) {
                    vesselNode.OnEnvHazardTick();
                }
            }
            _envHazardTick = 0;
        }

        if (GetTree().GetNodesInGroup("asteroids").Count < ArenaSettings.numAsteroids) {
            spawnAsteroid();
        }
    }

    public override void _Input(InputEvent e) {
        if (e is InputEventKey keyEvent) {
            if (keyEvent.Scancode == (uint)Godot.KeyList.Escape && !keyEvent.Pressed) {
                ChangeScene("QuickBattleMenu");
            }
        }
    }

    private void OnBotDefeated() {
        CheckStatus();
    }

    private void OnHumanDefeated() {
        if (!ArenaSettings.isQuickBattle) {
            TriggerDefeat();
        }
    }

    private void TriggerDefeat() {
        _gameState.transition = RpgGameState.MapTransition.UnitDestroyed;
        _gameState.humanUnit.fleet = new List<Vessel>{_gameState.humanUnit.fleet[0]};
        _gameState.humanUnit.fleet[0].hp = 0;
        _gameState.humanUnit.fleet[0].energy = 0;
        ChangeSceneAfterDelay("MapView");
    }

    private void CheckStatus() {
        if (ArenaSettings.isQuickBattle) {
            return;
        }
        if (_battleIsOver) {
            return;
        }

        var alliances = new HashSet<int>();
        foreach (Pilot p in _pilots) {
            if (p.Active) {
                alliances.Add(p.alliance);
                if (alliances.Count > 1) {
                    return;
                }
            }
        }

        _battleIsOver = true;

        var result = new BattleResult();
        var survivors = new List<Pilot>();
        foreach (Pilot p in _pilots) {
            var vessel = _vesselByPilot[p];
            if (p.Active) {
                var hpLoss = vessel.hp - p.Vessel.State.hp;
                if (vessel.player == _gameState.humanPlayer) {
                    if (_gameState.skillsLearned.Contains("Repair II")) {
                        hpLoss *= 0.8f;
                    } else if (_gameState.skillsLearned.Contains("Repair I")) {
                        hpLoss *= 0.9f;
                    }
                }
                var energyLoss = vessel.energy - p.Vessel.State.backupEnergy;
                if (vessel.player == _gameState.humanPlayer) {
                    if (_gameState.skillsLearned.Contains("Repair II")) {
                        energyLoss *= 0.75f;
                    }
                }
                vessel.hp -= hpLoss;
                vessel.energy -= energyLoss;
                survivors.Add(p);
            } else {
                vessel.hp = 0;
                var roll = QRandom.FloatRange(0.8f, 1.2f);
                var debris = (int)((float)p.Vessel.State.debris * roll);
                if (vessel.design.affiliation == "Krigia") {
                    result.krigiaDebris += debris;
                    _gameState.metKrigia = true;
                } else if (vessel.design.affiliation == "Wertu") {
                    result.wertuDebris += debris;
                    _gameState.metWertu = true;
                } else if (vessel.design.affiliation == "Zyth") {
                    result.zythDebris += debris;
                    _gameState.metZyth = true;
                } else {
                    result.genericDebris += debris;
                }
                if (p.alliance != _gameState.humanPlayer.Alliance) {
                    result.exp += p.Vessel.State.vesselLevel * 3;
                }
            }
        }

        // alliances should contain only 1 element,
        // the victorious team.
        foreach (int alliance in alliances) {
            HandleVictory(survivors, alliance, result);
            break;
        }
    }

    private void HandleVictory(List<Pilot> survivors, int alliance, BattleResult result) {
        if (alliance != _gameState.humanPlayer.Alliance) {
            if (_flagshipPilot != null) {
                TriggerDefeat();
                return;
            }
            _gameState.transition = RpgGameState.MapTransition.BaseAttackSimulation;
            ChangeSceneAfterDelay("MapView");
            return;
        }

        if (_gameState.enemyAttackerUnit != null) {
            var unit = _gameState.enemyAttackerUnit;
            if (_gameState.skillsLearned.Contains("Salvaging")) {
                // Collect 80% of cargo instead of 50%.
                result.minerals += QMath.IntAdjust(unit.cargo.minerals, 0.8);
                result.organic += QMath.IntAdjust(unit.cargo.organic, 0.8);
                result.power += QMath.IntAdjust(unit.cargo.power, 0.8);
            } else {
                result.minerals += unit.cargo.minerals / 2;
                result.organic += unit.cargo.organic / 2;
                result.power += unit.cargo.power / 2;
            }

            if (_flagshipPilot == null) {
                _gameState.transition = RpgGameState.MapTransition.BaseAttackSimulation;
            } else {
                _gameState.transition = RpgGameState.MapTransition.EnemyUnitDestroyed;
            }
        } else {
            _gameState.transition = RpgGameState.MapTransition.EnemyBaseAttackRepelled;
        }

        if (_gameState.skillsLearned.Contains("Salvaging")) {
            // +5% debris.
            result.genericDebris = QMath.IntAdjust(result.genericDebris, 1.05);
            result.krigiaDebris = QMath.IntAdjust(result.krigiaDebris, 1.05);
            result.wertuDebris = QMath.IntAdjust(result.wertuDebris, 1.05);
            result.zythDebris = QMath.IntAdjust(result.zythDebris, 1.05);
        }

        if (_flagshipPilot != null) {
            if (_gameState.skillsLearned.Contains("Fighter")) {
                result.exp = QMath.IntAdjust(result.exp, 1.33);
            }
            // TODO: check for the cargo overflow.
            _gameState.lastBattleResult = result;
        }

        ChangeSceneAfterDelay("MapView");
    }

    private void ChangeScene(string name) {
        Input.SetMouseMode(Input.MouseMode.Visible);
        GetTree().ChangeScene("res://scenes/" + name + ".tscn");
    }

    private void ChangeSceneAfterDelay(string name) {
        var timer = new Timer();
        timer.WaitTime = 2;
        var args = new Godot.Collections.Array { name };
        timer.Connect("timeout", this, nameof(ChangeScene), args);
        AddChild(timer);
        timer.Start();
    }

    private void spawnAsteroid() {
        var screenWidth = GetTree().Root.Size.x;
        var screenHeight = GetTree().Root.Size.y;
        var asteroid = Asteroid.New();
        int side = QRandom.IntRange(0, 3);
        if (side == 0) { // Right
            asteroid.Position = new Vector2(screenWidth - 20, QRandom.Float() * screenHeight);
            asteroid.RotationDegrees = 160 + QRandom.Float() * 40;
        } else if (side == 1) { // Up
            asteroid.Position = new Vector2(QRandom.Float() * screenWidth, 20);
            asteroid.RotationDegrees = 70 + QRandom.Float() * 40;
        } else if (side == 2) { // Left
            asteroid.Position = new Vector2(20, QRandom.Float() * screenHeight);
            asteroid.RotationDegrees = -20 + QRandom.Float() * 40;
        } else { // Down
            asteroid.Position = new Vector2(QRandom.Float() * screenWidth, screenHeight - 20);
            asteroid.RotationDegrees = 250 + QRandom.Float() * 40;
        }
        asteroid.AddToGroup("asteroids");
        asteroid.AddToGroup("affectedByEnvHazard");
        AddChild(asteroid);
    }
}
