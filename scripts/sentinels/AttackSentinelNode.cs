using Godot;
using System;

public class AttackSentinelNode : SentinelNode {
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

    protected override void ProcessAttack() {
        var enemy = QMath.NearestEnemy(_sprite.GlobalPosition, pilot);
        if (enemy == null) {
            return;
        }
        if (enemy.Vessel.Position.DistanceTo(_sprite.GlobalPosition) >= _design.weapon.range) {
            return;
        }

        var projectile = Projectile.New(_design.weapon, pilot);
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = _sprite.GlobalPosition;
        var target = QMath.RandomizedLocation(enemy.Vessel.Position, 8);
        projectile.Rotation = (target - _sprite.GlobalPosition).Normalized().Angle();

        if (_design.weapon == PhotonBurstCannonWeapon.Design) {
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Burst_Cannon.wav"), -4);
            GetParent().AddChild(sfx);
        }

        _attackCooldown = _design.attackCooldown;
    }
}
