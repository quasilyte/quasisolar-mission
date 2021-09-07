using Godot;
using System;

public class MissileDefenseNode : Node2D {
    private Pilot _target;

    private Pilot _owner;
    private int _burst = 4;
    private float _burstCooldown = 0;

    private static PackedScene _scene = null;
    public static MissileDefenseNode New(Pilot owner, Pilot target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MissileDefenseNode.tscn");
        }
        var o = (MissileDefenseNode)_scene.Instance();
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
            _burstCooldown += 0.15f;

            var cursor = _target.Vessel.Position;
            
            {
                var rocket = Rocket.New(_owner, RocketLauncherWeapon.TurretDesign);
                rocket.Position = Position;
                rocket.Position += rocket.Transform.y * ((4 - _burst) * -25);
                rocket.Rotation += (cursor - Position).Normalized().Angle();
                rocket.Start(_target.Vessel);
                GetParent().AddChild(rocket);
            }
            {
                var rocket = Rocket.New(_owner, RocketLauncherWeapon.TurretDesign);
                rocket.Position = Position;
                rocket.Position += rocket.Transform.y * ((4 - _burst) * 25);
                rocket.Rotation += (cursor - Position).Normalized().Angle();
                rocket.Start(_target.Vessel);
                GetParent().AddChild(rocket);
            }
        }

        if (_burst <= 0) {
            QueueFree();
        }
    }
}
