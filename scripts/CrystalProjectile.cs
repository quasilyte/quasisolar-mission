using Godot;
using System;

public class CrystalProjectile : Node2D {
    private float _hp;

    public Pilot FiredBy;

    private Vector2 _target;

    private static AudioStream _audioStream;

    private static PackedScene _scene = null;
    public static CrystalProjectile New(Pilot owner, Vector2 target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/CrystalProjectile.tscn");
        }
        var o = (CrystalProjectile)_scene.Instance();
        o.FiredBy = owner;
        o._target = target;
        return o;
    }

    public override void _Ready() {
        _hp = HarpoonWeapon.Design.range;
        if (_audioStream == null) {
            _audioStream = GD.Load<AudioStream>("res://audio/weapon/Crystal_Cannon.wav");
        }
        GetParent().AddChild(SoundEffectNode.New(_audioStream, -7));
    }

    public override void _Process(float delta) {
        if (_hp <= 0) {
            Explode();
            return;
        }
        if (Position.DistanceTo(_target) < 10) {
            Explode();
        }

        float traveled = CrystalCannonWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    private void Explode() {
        for (int i = 0; i < 9; i++) {
            var projectile = Projectile.New(CrystalCannonWeapon.Design, FiredBy);
            projectile.Position = Position;
            projectile.RotationDegrees = (float)i * 40;
            projectile.Position += projectile.Transform.x * 20;
            GetParent().AddChild(projectile);
        }
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Crystal_Cannon_Impact.wav"), -9));
        QueueFree();
    }

    public void OnImpact() {
    }
}
