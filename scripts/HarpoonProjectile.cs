using Godot;
using System;

public class HarpoonProjectile : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;

    private static Texture _harpoonTexture;
    private static AudioStream _harpoonAudioStream;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return HarpoonWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static HarpoonProjectile New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/HarpoonProjectile.tscn");
        }
        var o = (HarpoonProjectile)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = HarpoonWeapon.Design.range;
        if (_harpoonTexture == null) {
            _harpoonTexture = GD.Load<Texture>("res://images/ammo/harpoon.png");
        }
        if (_harpoonAudioStream == null) {
            _harpoonAudioStream = GD.Load<AudioStream>("res://audio/weapon/Harpoon.wav");
        }
        
        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Texture = _harpoonTexture;

        var sfx = SoundEffectNode.New(_harpoonAudioStream, -5);
        GetParent().AddChild(sfx);
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        float traveled = HarpoonWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
        QueueFree();
    }
}
