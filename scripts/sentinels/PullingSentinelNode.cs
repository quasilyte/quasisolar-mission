using Godot;
using System;

public class PullingSentinelNode : SentinelNode {
    private float _idleTime = 0;

    private static PackedScene _scene = null;
    public static PullingSentinelNode New(VesselNode vessel) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PullingSentinelNode.tscn");
        }
        var o = (PullingSentinelNode)_scene.Instance();
        o._vessel = vessel;
        o._design = SentinelDesign.Find("Pulling Fighter");
        o.pilot = vessel.pilot;
        return o;
    }

    private void FireAt(VesselNode target) {
        target.ApplyEnergyDamage(PullingBeamerWeapon.Design.energyDamage);

        target.State.velocity += target.Position.DirectionTo(_sprite.GlobalPosition) * 30;

        var effect = PullingBeamerEffectNode.New(_sprite.GlobalPosition, target.Position);
        _vessel.GetParent().AddChild(effect);
        
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Pulling_Beamer.wav"));
        _vessel.GetParent().AddChild(sfx);

        AddCooldown();
    }

    protected override void ProcessFrame(float delta) {
        _idleTime = QMath.ClampMin(_idleTime - delta, 0);
    }

    protected override void ProcessAttack() {
        if (_idleTime != 0) {
            return;
        }

        var enemy = QRandom.Element(pilot.Enemies);
        if (enemy == null || !enemy.Active) {
            _idleTime = QRandom.FloatRange(0.3f, 1);
            return;
        }
        if (_sprite.GlobalPosition.DistanceTo(enemy.Vessel.Position) > PullingBeamerWeapon.Design.range) {
            return;
        }
        FireAt(enemy.Vessel);
        _idleTime = QRandom.FloatRange(0.1f, 2.5f);
    }
}
