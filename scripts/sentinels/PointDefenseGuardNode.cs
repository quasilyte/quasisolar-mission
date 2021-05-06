using Godot;
using System;

public class PointDefenseGuardNode : SentinelNode {
    private static PackedScene _scene = null;
    public static new PointDefenseGuardNode New(VesselNode vessel, SentinelDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PointDefenseGuardNode.tscn");
        }
        var o = (PointDefenseGuardNode)_scene.Instance();
        o._vessel = vessel;
        o._design = design;
        o.pilot = vessel.pilot;
        return o;
    }

    private void FireAt(Vector2 target) {
        var color = Color.Color8(180, 255, 180);
        var beam = Beam.New(_sprite.GlobalPosition, target, color, 1);
        beam.weapon = _design.weapon;
        GetParent().AddChild(beam);
        _attackCooldown = _design.attackCooldown;
        
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/point_laser.wav"));
        GetParent().AddChild(sfx);
    }

    protected override void ProcessAttack() {
        foreach (var n in GetTree().GetNodesInGroup("rockets")) {
            if (n is Rocket rocket) {
                if (rocket.GetWeaponDesign() == ShieldBreakerWeapon.Design) {
                    continue;
                }
                if (rocket.FiredBy().alliance == pilot.alliance) {
                    continue;
                }
                if (rocket.Position.DistanceTo(_sprite.GlobalPosition) > 100) {
                    continue;
                }
                FireAt(rocket.Position);
                rocket.Explode();
                return;
            }
            if (n is TorpedoNode torpedo) {
                if (torpedo.FiredBy().alliance == pilot.alliance) {
                    continue;
                }
                if (torpedo.Position.DistanceTo(_sprite.GlobalPosition) > 100) {
                    continue;
                }
                FireAt(torpedo.Position);
                torpedo.ApplyDamage(_design.weapon.damage);
                return;
            }
        }
    }
}
