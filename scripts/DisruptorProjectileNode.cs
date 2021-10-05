using Godot;
using System;

public class DisruptorProjectileNode : Node2D, IProjectile {
    private float _hp;
    private Node2D _target;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return DisruptorWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static DisruptorProjectileNode New(Pilot owner, Node2D target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DisruptorProjectileNode.tscn");
        }
        var o = (DisruptorProjectileNode)_scene.Instance();
        o._firedBy = owner;
        o._target = target;
        return o;
    }

    public override void _Ready() {
        _hp = DisruptorWeapon.Design.range;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Disruptor.wav"), -3);
        GetParent().AddChild(sfx);
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        if (!IsInstanceValid(_target)) {
            _target = null;
        }
        if (_target != null) {
            var dstRotation = _target.Position.AngleToPoint(Position);
            var rotationDiff = QMath.RotationDiff(dstRotation, Rotation);
            var rotationAmount = 0.5f * delta;
            if (Math.Abs(rotationDiff) >= rotationAmount) {
                if (rotationDiff < 0) {
                    Rotation += rotationAmount;
                } else {
                    Rotation -= rotationAmount;
                }
            } else {
                Rotation = dstRotation;
            }
        }

        float traveled = DisruptorWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
        QueueFree();
    }
}
