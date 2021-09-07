using Godot;
using System;

public class TempestAuraNode : Node2D {
    private Pilot _firedBy;

    private float _hp;

    private static PackedScene _scene = null;
    public static TempestAuraNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ammo/TempestAuraNode.tscn");
        }
        var o = (TempestAuraNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = TempestWeapon.Design.duration;
        for (int i = 0; i < 15; i++) {
            var projectile = TempestProjectileNode.New(_firedBy);
            projectile.Position = Position;
            projectile.RotationDegrees = i * 24;
            AddChild(projectile);
        }
    }

    private void Destroy() {
        foreach (var n in GetChildren()) {
            var projectile = (TempestProjectileNode)n;
            projectile.Explode();
        }
        QueueFree();
    }

    public override void _Process(float delta) {
        if (!IsInstanceValid(_firedBy.Vessel)) {
            Destroy();
            return;
        }

        _hp -= delta;
        if (_hp < 0) {
            Destroy();
            return;
        }

        Rotation += delta;
        Position = _firedBy.Vessel.Position;
    }
}
