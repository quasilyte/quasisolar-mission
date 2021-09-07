using Godot;
using System;

public class LancerProjectile : Node2D, IProjectile {
    private Pilot _firedBy;
    private float _hp;

    private LancerLine _line;

    private static PackedScene _scene = null;
    public static LancerProjectile New(LancerLine line, Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/LancerProjectile.tscn");
        }
        var o = (LancerProjectile)_scene.Instance();
        o._firedBy = owner;
        o._line = line;
        return o;
    }

    public WeaponDesign GetWeaponDesign() { return LancerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    public void OnImpact() {
    }

    public override void _Ready() {
        _hp = LancerWeapon.Design.range;
        var newPos = Position + Transform.x.Normalized() * 48;
        _line.SetPointPosition(0, Position);
        _line.SetPointPosition(1, newPos);
        Position = newPos;
    }

    private void Destroy() {
        QueueFree();
        _line.StartFading();
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            Destroy();
            return;
        }

        float traveled = LancerWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;

        _line.SetPointPosition(1, Position);
    }
}
