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

*/

public class Arena : Node2D {
    private List<Pilot> _pilots;
    private Dictionary<Vessel, Pilot> _pilotByVessel;

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

        ApplyAllianceColor(vesselNode.GetNode<Sprite>("Sprite"), pilot.player.Alliance);

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
        human.Connect("Defeated", this, nameof(OnHumanDefeated));

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
        _pilots = new List<Pilot>();
        for (int i = 0; i < ArenaSettings.combatants.Count; i++) {
            var combatant = ArenaSettings.combatants[i];
            var pilot = new Pilot{
                PilotName = combatant.player.PlayerName, // FIXME: this is incorrect
                player = combatant.player,
            };
            _pilots.Add(pilot);
            _pilotByVessel[combatant] = pilot;
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
                if (x.player.Alliance != y.player.Alliance) {
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
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayBattleMusic();

        GD.Randomize();

        if (ArenaSettings.speed != ArenaSettings.BattleSpeed.Normal) {
            if (ArenaSettings.speed == ArenaSettings.BattleSpeed.Slow) {
                Engine.TimeScale = 0.75f;
            } else if (ArenaSettings.speed == ArenaSettings.BattleSpeed.Fast) {
                Engine.TimeScale = 1.25f;
            } else {
                Engine.TimeScale = 2.00f;
            }
        }
    }

    public override void _Process(float delta) {
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
        RpgGameState.transition = RpgGameState.MapTransition.UnitDestroyed;
        RpgGameState.humanUnit.fleet = new List<Vessel>{RpgGameState.humanUnit.fleet[0]};
        RpgGameState.humanUnit.fleet[0].hp = 0;
        RpgGameState.humanUnit.fleet[0].energy = 0;
        ChangeSceneAfterDelay("MapView");
    }

    private void CheckStatus() {
        if (ArenaSettings.isQuickBattle) {
            return;
        }

        var result = new BattleResult();
        var alliances = new HashSet<int>();
        foreach (Pilot p in _pilots) {
            if (p.Active) {
                alliances.Add(p.player.Alliance);
                if (alliances.Count > 1) {
                    return;
                }
            } else {
                var roll = RpgGameState.rng.RandfRange(0.8f, 1.2f);
                var debris = (int)((float)p.Vessel.State.debris * roll);
                if (p.player == RpgGameState.krigiaPlayer) {
                    result.krigiaDebris += debris;
                    RpgGameState.metKrigia = true;
                } else if (p.player == RpgGameState.wertuPlayer) {
                    result.wertuDebris += debris;
                    RpgGameState.metWertu = true;
                } else if (p.player == RpgGameState.zythPlayer) {
                    result.zythDebris += debris;
                    RpgGameState.metZyth = true;
                } else {
                    result.genericDebris += debris;
                }
                if (p.player.Alliance != RpgGameState.humanPlayer.Alliance) {
                    result.exp += p.Vessel.State.vesselLevel * 3;
                }
            }
        }

        // alliances should contain only 1 element,
        // the victorious team.
        foreach (int alliance in alliances) {
            HandleVictory(alliance, result);
        }
    }

    private void HandleVictory(int alliance, BattleResult result) {
        if (alliance != RpgGameState.humanPlayer.Alliance) {
            TriggerDefeat();
            return;
        }

        // TODO: if allies are possible, this mapping won't work.
        var survivors = new List<Vessel>(RpgGameState.humanUnit.fleet.Count);
        foreach (var vessel in RpgGameState.humanUnit.fleet) {
            var pilot = _pilotByVessel[vessel];
            if (!pilot.Active) {
                continue;
            }
            vessel.hp = pilot.Vessel.State.hp;
            vessel.energy = pilot.Vessel.State.backupEnergy;
            survivors.Add(vessel);
        }
        RpgGameState.humanUnit.fleet = survivors;

        if (RpgGameState.skillsLearned.Contains("Fighter")) {
            result.exp += result.exp / 4;
        }

        // TODO: check for the cargo overflow.
        RpgGameState.lastBattleResult = result;

        if (RpgGameState.enemyAttackerUnit != null) {
            var unit = RpgGameState.enemyAttackerUnit;
            result.minerals += unit.cargo.minerals / 2;
            result.organic += unit.cargo.organic / 2;
            result.power += unit.cargo.power / 2;
    
            RpgGameState.spaceUnits.Remove(RpgGameState.enemyAttackerUnit);
            RpgGameState.enemyAttackerUnit = null;
            RpgGameState.transition = RpgGameState.MapTransition.EnemyUnitDestroyed;
        } else {
            RpgGameState.transition = RpgGameState.MapTransition.EnemyBaseAttackRepelled;
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
        uint side = GD.Randi() % 4;
        if (side == 0) { // Right
            asteroid.Position = new Vector2(screenWidth - 20, GD.Randf() * screenHeight);
            asteroid.RotationDegrees = 160 + GD.Randf() * 40;
        } else if (side == 1) { // Up
            asteroid.Position = new Vector2(GD.Randf() * screenWidth, 20);
            asteroid.RotationDegrees = 70 + GD.Randf() * 40;
        } else if (side == 2) { // Left
            asteroid.Position = new Vector2(20, GD.Randf() * screenHeight);
            asteroid.RotationDegrees = -20 + GD.Randf() * 40;
        } else { // Down
            asteroid.Position = new Vector2(GD.Randf() * screenWidth, screenHeight - 20);
            asteroid.RotationDegrees = 250 + GD.Randf() * 40;
        }
        asteroid.AddToGroup("asteroids");
        AddChild(asteroid);
    }
}
