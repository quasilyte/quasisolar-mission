using Godot;
using System;

public class AfterburnerFlameNode : Node2D, IProjectile {
    private float _hp;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return AfterburnerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static AfterburnerFlameNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/AfterburnerFlameNode.tscn");
        }
        var o = (AfterburnerFlameNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = AfterburnerWeapon.Design.duration;
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        Rotation -= delta * 4;
        _hp -= delta;
    }

    public void OnImpact() {
        QueueFree();
    }
}
