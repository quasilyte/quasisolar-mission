using Godot;
using System;

public class EnergyBoltNode : Node2D, IProjectile {
    private float _hp;

    private Pilot _firedBy;

    public int chargeLevel;

    public WeaponDesign GetWeaponDesign() { return DisintegratorWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static EnergyBoltNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/EnergyBoltNode.tscn");
        }
        var o = (EnergyBoltNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = DisintegratorWeapon.Design.range;

        // _sprite = GetNode<Sprite>("Sprite");
        // _sprite.Texture = GD.Load<Texture>("res://images/ammo/harpoon.png");

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Disintegrator.wav"), -5);
        GetParent().AddChild(sfx);
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        float traveled = DisintegratorWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
        QueueFree();
    }
}
