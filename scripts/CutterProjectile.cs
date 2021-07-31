using Godot;
using System;

public class CutterProjectile : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;
    private CollisionShape2D _mask = null;

    private static AudioStream _audioStream;

    private WeaponDesign _design;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return _design; }
    public Pilot FiredBy() { return _firedBy; }

    private static PackedScene _scene = null;
    public static CutterProjectile New(Pilot owner, WeaponDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/CutterProjectile.tscn");
        }
        var o = (CutterProjectile)_scene.Instance();
        o._firedBy = owner;
        o._design = design;
        return o;
    }

    public override void _Ready() {
        _hp = _design.range;
        if (_audioStream == null) {
            _audioStream = GD.Load<AudioStream>("res://audio/weapon/Cutter.wav");
        }

        _sprite = GetNode<Sprite>("Sprite");
        _mask = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");

        if (_design == HyperCutterWeapon.Design) {
            _sprite.Texture = GD.Load<Texture>("res://images/ammo/Hyper_Cutter.png");
            GetNode<Area2D>("Area2D").Connect("area_entered", this, nameof(OnCollision));
        }

        GetParent().AddChild(SoundEffectNode.New(_audioStream, -7));
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        if (_sprite.Scale.x < 1) {
            _sprite.Scale = new Vector2(_sprite.Scale.x + delta, _sprite.Scale.y + delta);
            _mask.Scale = new Vector2(_mask.Scale.x, _mask.Scale.y + delta);
        } else {
            var m = _sprite.Modulate;
            _sprite.Modulate = new Color(m.r, m.g, m.b, m.a - 0.04f);
        }

        float traveled = _design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
    }

    private void OnCollision(Area2D other) {
        if (other.GetParent() is IProjectile projectile) {
            var design = projectile.GetWeaponDesign();
            if (design.ignoresAsteroids) {
                return;
            }
            if (CanDestroyProjectile(design)) {
                projectile.OnImpact();
                return;
            }
        }
    }

    private bool CanDestroyProjectile(WeaponDesign w) {
        return w == PulseLaserWeapon.Design ||
            w == IonCannonWeapon.Design ||
            w == AssaultLaserWeapon.Design ||
            w == StingerWeapon.Design ||
            w == HarpoonWeapon.Design ||
            w == DisruptorWeapon.Design;
    }
}
