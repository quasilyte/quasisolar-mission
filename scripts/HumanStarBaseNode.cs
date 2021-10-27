using Godot;
using System;
using System.Collections.Generic;

public class HumanStarBaseNode : StarBaseNode {
    private static PackedScene _scene = null;
    public static new HumanStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/HumanStarBaseNode.tscn");
        }
        var o = (HumanStarBaseNode)_scene.Instance();
        o.starBase = starBase;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("LevelUpgraded", this, nameof(OnLevelUpgraded));
    }

    private void OnLevelUpgraded() {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/interface/generic_notification.wav"));
        var notification = MapNotificationNode.New("Star base level up");
        GetParent().AddChild(notification);
        notification.GlobalPosition = GlobalPosition;
    }

    private void ProcessLevelProgression() {
        var pointsGained = 1f;
        if (starBase.mineralsStock >= 200) {
            pointsGained++;
            if (QRandom.Float() < 0.5) {
                starBase.mineralsStock--;
            }
        }
        if (starBase.organicStock >= 100) {
            pointsGained++;
            if (QRandom.Float() < 0.5) {
                starBase.organicStock--;
            }
        }
        if (starBase.powerStock >= 150) {
            pointsGained++;
            if (QRandom.Float() < 0.5) {
                starBase.powerStock--;
            }
        }

        starBase.levelProgression += pointsGained;
        if (starBase.level < StarBase.maxBaseLevel) {
            var upgradeCost = starBase.LevelUpgradeCost();
            if (starBase.levelProgression >= upgradeCost) {
                starBase.levelProgression = 0;
                starBase.level++;
                EmitSignal(nameof(LevelUpgraded));
            }
        }
    }

    protected override void GatherResources() {
        var roll = QRandom.Float();

        switch (starBase.mode) {
        case StarBase.Mode.ProduceMinerals:
            if (roll < 0.4) {
                starBase.mineralsStock++;
            }
            break;

        case StarBase.Mode.ProducePower:
            if (roll < 0.2) {
                starBase.powerStock++;
            }
            break;

        case StarBase.Mode.ProduceRU:
            _gameState.credits += 4 + (starBase.level * 3);
            break;
        }
    }

    public override void ProcessDay() {
        base.ProcessDay();

        ProcessLevelProgression();

        var vesselProduced = ProcessProduction();
        if (vesselProduced != null) {
            vesselProduced.pilotName = PilotNames.UniqHumanName(RpgGameState.instance.usedNames);
            VesselFactory.PadEquipment(vesselProduced);
            if (vesselProduced.Design().weaponSlots >= 1) {
                vesselProduced.weapons[0] = SpreadGunWeapon.Design.name;
            }
            VesselFactory.InitStats(vesselProduced);
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_completed.wav"));
            var notification = MapNotificationNode.New("Production completed");
            GetParent().AddChild(notification);
            notification.GlobalPosition = GlobalPosition;
        }

        ProcessConstruction();
        ProcessModules();
    }

    private void ProcessModules() {
        var sys = starBase.system.Get();
        foreach (var moduleName in starBase.modules) {
            if (moduleName == "Minerals Refinery") {
                if (starBase.mineralsStock > 200) {
                    _gameState.credits += 5 * 15;
                    starBase.mineralsStock -= 5;
                }
            } else if (moduleName == "Organic Refinery") {
                if (starBase.organicStock > 100) {
                    _gameState.credits += 5 * 22;
                    starBase.organicStock -= 5;
                }
            } else if (moduleName == "Power Refinery") {
                if (starBase.powerStock > 150) {
                    _gameState.credits += 5 * 25;
                    starBase.powerStock -= 5;
                }
            } else if (moduleName == "Minerals Collector") {
                foreach (var planet in sys.resourcePlanets) {
                    if (!planet.IsExplored()) {
                        continue;
                    }
                    starBase.AddMinerals(planet.mineralsPerDay);
                }
            } else if (moduleName == "Organic Collector") {
                foreach (var planet in sys.resourcePlanets) {
                    if (!planet.IsExplored()) {
                        continue;
                    }
                    starBase.AddOrganic(planet.organicPerDay);
                }
            } else if (moduleName == "Power Collector") {
                foreach (var planet in sys.resourcePlanets) {
                    if (!planet.IsExplored()) {
                        continue;
                    }
                    starBase.AddPower(planet.powerPerDay);
                }
            }
        }
    }

    private void ProcessConstruction() {
        if (starBase.constructionTarget == "") {
            return;
        }

        var module = StarBaseModule.Find(starBase.constructionTarget);

        if ((starBase.constructionProgress + 1) >= module.buildTime) {
            starBase.constructionProgress = 0;
            starBase.constructionTarget = "";
            starBase.modules.Add(module.name);

            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/construction_completed.wav"));
            var notification = MapNotificationNode.New("Construction completed");
            GetParent().AddChild(notification);
            notification.GlobalPosition = GlobalPosition;
        } else {
            starBase.constructionProgress++;
        }
    }
}
