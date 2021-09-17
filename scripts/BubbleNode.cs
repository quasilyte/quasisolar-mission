using Godot;
using System;

public class BubbleNode : Node2D, IProjectile {
    private float _hp;

    private Pilot _firedBy;
    private WeaponDesign _weapon;

    private Vector2 _velocity;
    private int _velocityTicks;

    public WeaponDesign GetWeaponDesign() { return BubbleGunWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static BubbleNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/BubbleNode.tscn");
        }
        var o = (BubbleNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = GetWeaponDesign().duration;
        _velocity = RandomVelocity();
        Rotation = QRandom.Angle();
        _velocityTicks = 2;
        // var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Disruptor.wav"), -3);
        // GetParent().AddChild(sfx);
    }

    private void UpdateVelocity() {
        if (_velocityTicks > 0) {
            _velocityTicks--;
            return;
        }
        _velocityTicks = 5;
        var roll = QRandom.Float();
        if (roll < 0.3) {
            return;
        }
        if (roll < 0.5) {
            _velocity = RandomVelocity();
            return;
        }
        var enemy = QMath.NearestEnemy(GlobalPosition, _firedBy);
        if (enemy == null) {
            return;
        }
        if (enemy.Vessel.GlobalPosition.DistanceTo(GlobalPosition) > 96) {
            return;
        }
        var speed = GetWeaponDesign().projectileSpeed * QRandom.FloatRange(0.5f, 1);
        _velocity = GlobalPosition.MoveToward(enemy.Vessel.GlobalPosition, speed) - GlobalPosition; 
    }

    private Vector2 RandomVelocity() {
        var speed = GetWeaponDesign().projectileSpeed;
        return new Vector2(
            QRandom.FloatRange(-speed, speed),
            QRandom.FloatRange(-speed, speed)
        );
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        _hp -= delta;

        UpdateVelocity();
        Rotation += 0.1f;
        Position += _velocity * delta;
    }

    public void OnImpact() {
        QueueFree();
    }
}
