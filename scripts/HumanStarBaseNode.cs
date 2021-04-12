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

    public override void ProcessDay() {
        base.ProcessDay();

        var vesselProduced = ProcessProduction();
        if (vesselProduced != null) {
            vesselProduced.energySource = EnergySource.Find("None");
            vesselProduced.weapons = new List<WeaponDesign>{
                EmptyWeapon.Design,
                EmptyWeapon.Design,
            };
            vesselProduced.artifacts = new List<ArtifactDesign>{
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
            };
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_completed.wav"));
            var notification = MapNotificationNode.New("Production completed");
            GetParent().AddChild(notification);
            notification.GlobalPosition = GlobalPosition;
        }
    }
}
