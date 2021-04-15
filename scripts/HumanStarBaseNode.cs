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

    public override void ProcessDay() {
        base.ProcessDay();

        ProcessLevelProgression();

        var vesselProduced = ProcessProduction();
        if (vesselProduced != null) {
            vesselProduced.pilotName = PilotNames.UniqHumanName(RpgGameState.usedNames);
            VesselFactory.PadEquipment(vesselProduced);
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_completed.wav"));
            var notification = MapNotificationNode.New("Production completed");
            GetParent().AddChild(notification);
            notification.GlobalPosition = GlobalPosition;
        }
    }
}
