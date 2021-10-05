using Godot;
using System;

public class GamepadCursorNode : Node2D {
    private Pilot _pilot;
    private TargetLockNode _anchor;
    private Sprite _sprite;

    private bool _attackCursor = false;
    private Vector2 _fallbackPos;

    private static PackedScene _scene = null;
    public static GamepadCursorNode New(Pilot pilot) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/GamepadCursorNode.tscn");
        }
        var o = (GamepadCursorNode)_scene.Instance();
        o._pilot = pilot;
        return o;
    }

    public override void _Ready() {
        _fallbackPos = Position;
        _sprite = GetNode<Sprite>("Sprite");
    }

    public void MoveMainCursor(float offsetX, float offsetY) {
        if (_attackCursor) {
            _attackCursor = false;
            var pos = _fallbackPos;
            _fallbackPos = Position;
            Position = pos;
            _sprite.Frame = 1;
        }
        MoveCursor(offsetX, offsetY);
    }

    public void MoveAttackCursor(float offsetX, float offsetY) {
        if (!_attackCursor) {
            _attackCursor = true;
            var pos = _fallbackPos;
            _fallbackPos = Position;
            if (_anchor != null && _anchor.HasTarget()) {
                pos = _anchor.Position;
            }
            Position = pos;
            _sprite.Frame = 0;
        }
        MoveCursor(offsetX, offsetY);
    }

    // public void MoveCursor(float offsetX, float offsetY) {
    //     if (_anchor != null && _anchor.HasTarget() && !Visible) {
    //         Position = _anchor.Position;
    //     }

    //     var screenWidth = GetViewport().Size.x;
    //     var screenHeight = GetViewport().Size.y;
    //     var newX = QMath.Clamp(Position.x + offsetX, 0, screenWidth);
    //     var newY = QMath.Clamp(Position.y + offsetY, 0, screenHeight);

    //     Position = new Vector2(newX, newY);
    //     _visibilityTime = 2;
    // }

    public void SetAnchor(TargetLockNode anchor) {
        _anchor = anchor;
    }

    private void MoveCursor(float offsetX, float offsetY) {
        var screenWidth = GetViewport().GetVisibleRect().Size.x;
        var screenHeight = GetViewport().GetVisibleRect().Size.y;
        var newX = QMath.Clamp(Position.x + offsetX, 0, screenWidth);
        var newY = QMath.Clamp(Position.y + offsetY, 0, screenHeight);

        Position = new Vector2(newX, newY);
    }
}
