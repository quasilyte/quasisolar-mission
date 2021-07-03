using Godot;
using System;

public class MjolnirProjectile : Node2D {
    private float _hp;

    public Pilot FiredBy;

    private Vector2 _target;

    private static AudioStream _audioStream;

    private static PackedScene _scene = null;
    public static MjolnirProjectile New(Pilot owner, Vector2 target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MjolnirProjectile.tscn");
        }
        var o = (MjolnirProjectile)_scene.Instance();
        o.FiredBy = owner;
        o._target = target;
        return o;
    }

    public override void _Ready() {
        AddToGroup("mortar_shells");

        _hp = MortarWeapon.Design.range;

        if (_audioStream == null) {
            _audioStream = GD.Load<AudioStream>("res://audio/weapon/emp.wav");
        }

        GetParent().AddChild(SoundEffectNode.New(_audioStream, -6));
    }

    public override void _Process(float delta) {
        if (_hp <= 0) {
            Explode();
            return;
        }
        if (Position.DistanceTo(_target) < 10) {
            Explode();
        }

        float traveled = MjolnirWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    private void Explode() {
        var explosion = MjolnirExplosionNode.New();
        explosion.Position = Position;
        GetParent().AddChild(explosion);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/emp.wav"), -4);
        GetParent().AddChild(sfx);

        foreach (var enemy in FiredBy.Enemies) {
            if (!enemy.Active) {
                continue;
            }
            if (Position.DistanceTo(enemy.Vessel.Position) < 64) {
                enemy.Vessel.ApplyDamage(MjolnirWeapon.Design.damage, MjolnirWeapon.Design.damageKind);
                enemy.Vessel.ApplyEnergyDamage(MjolnirWeapon.Design.energyDamage);
            }
        }

        QueueFree();
    }
}
