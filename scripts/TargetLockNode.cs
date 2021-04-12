using Godot;
using System;

// TODO: rename to cursor lock node.
public class TargetLockNode : Node2D {
    public ArenaCameraNode camera;
    private Node2D _target;

    private static PackedScene _scene = null;
    public static TargetLockNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/TargetLockNode.tscn");
        }
        var o = (TargetLockNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        Visible = false;
    }

    public override void _Process(float delta) {
        if (_target != null && !IsInstanceValid(_target)) {
            _target = null;
        }

        Visible = _target != null;
        if (_target != null) {
            GlobalPosition = QMath.TranslateViewportPos(camera, _target.GlobalPosition);
        }
    }

    public void SetTarget(Node2D target) {
        _target = target;
    }

    public bool HasTarget() {
        return _target != null;
    }
}
