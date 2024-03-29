using Godot;
using System;

public class GaussDefenseNode : Node2D {
    private Pilot _target;

    private Pilot _owner;
    private int _burst = 3;
    private float _burstCooldown = 0;

    private static PackedScene _scene = null;
    public static GaussDefenseNode New(Pilot owner, Pilot target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/GaussDefenseNode.tscn");
        }
        var o = (GaussDefenseNode)_scene.Instance();
        o._owner = owner;
        o._target = target;
        return o;
    }

    public override void _Process(float delta) {
        if (!IsInstanceValid(_target.Vessel)) {
            QueueFree();
            return;
        }

        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }

        if ( _burstCooldown == 0) {
            _burst--;
            _burstCooldown += 0.5f;

            var projectile = Projectile.New(NeedleGunWeapon.TurretDesign, _owner);
            var predictedPos = QMath.CalculateSnipeShot(NeedleGunWeapon.TurretDesign, Position, _target.Vessel.Position, _target.Vessel.State.velocity);
            var cursor = QMath.RandomizedLocation(predictedPos, 14);
            projectile.Position = Position + new Vector2(0, QRandom.FloatRange(-15, 15));
            projectile.Rotation = (cursor - Position).Normalized().Angle();
            GetParent().AddChild(projectile);
        }

        if (_burst <= 0) {
            QueueFree();
        }
    }
}
