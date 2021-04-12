using Godot;
using System;

public class LightningProjectile : Node2D, IProjectile {
    private float _hp;
    private float _splitDelay;
    private float _drawDelay = 0;
    private Node2D _target;
    private LightningLine _line;

    private const float SPLIT_DELAY = 45;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return StormbringerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }

    private static PackedScene _scene = null;
    public static LightningProjectile New(LightningLine line, Pilot owner, Node2D target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/LightningProjectile.tscn");
        }
        var o = (LightningProjectile)_scene.Instance();
        o._firedBy = owner;
        o._target = target;
        o._line = line;
        return o;
    }

    public override void _Ready() {
        _hp = StormbringerWeapon.Design.range;
        _splitDelay = SPLIT_DELAY;

        _line.AddPoint(Position);
        _line.AddPoint(Position);
    }

    private void Destroy() {
        QueueFree();
        _line.StartFading();
    }

    public void OnImpact() {
        Destroy();
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            Destroy();
            return;
        }

        float traveled = StormbringerWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;

        _drawDelay -= delta;
        if (_drawDelay <= 0) {
            _drawDelay = 0.08f;
            var numPoints = _line.GetPointCount();
            _line.SetPointPosition(numPoints - 1, Position);
        }

        _splitDelay -= traveled;
        if (_splitDelay <= 0) {
            _splitDelay = SPLIT_DELAY;
            if (!IsInstanceValid(_target)) {
                _target = null;
            }
            if (_target != null) {
                var dstRotation = _target.Position.AngleToPoint(Position) + (float)GD.RandRange(-0.5, 0.5);
                var rotationDiff = QMath.RotationDiff(dstRotation, Rotation);
                var rotationAmount = 0.40f;
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

            _line.AddPoint(Position);
            // if (_line.GetPointCount() > 6) {
            //     _line.RemovePoint(0);
            // }
        }
    }
}