using Godot;
using System;

public class AttackSentinelNode : SentinelNode {
    private float _attackCooldown = 0;

    private static PackedScene _scene = null;
    public static new AttackSentinelNode New(VesselNode vessel, SentinelDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/AttackSentinelNode.tscn");
        }
        var o = (AttackSentinelNode)_scene.Instance();
        o._vessel = vessel;
        o._design = design;
        o.pilot = vessel.pilot;
        return o;
    }

    public override void _Ready() {
        base._Ready();
    }

    public override void _Process(float delta) {
        base._Process(delta);
        if (_vessel == null) { // Vessel was destroyed
            return;
        }

        if (_attackCooldown == 0) {
            MaybeAttackEnemy();
        }

        _attackCooldown = QMath.ClampMin(_attackCooldown - delta, 0);
    }

    private void MaybeAttackEnemy() {
        var enemy = QMath.NearestEnemy(_vessel.Position, pilot);
        if (enemy == null) {
            return;
        }
        if (enemy.Vessel.Position.DistanceTo(_vessel.Position) >= _design.weapon.range) {
            return;
        }

        var projectile = Projectile.New(_design.weapon, pilot);
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = _sprite.GlobalPosition;
        var target = QMath.RandomizedLocation(enemy.Vessel.Position, 8);
        projectile.Rotation = (target - _sprite.GlobalPosition).Normalized().Angle();

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Burst_Cannon.wav"), -4);
        GetParent().AddChild(sfx);

        _attackCooldown = _design.attackCooldown;
    }
}
