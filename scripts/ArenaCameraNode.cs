using Godot;
using System;

public class ArenaCameraNode : Camera2D {
    public Node2D _target;

    private float _cameraSpeed = 100;

    public override void _Ready() {
        var screenWidth = GetTree().Root.Size.x;
        var screenHeight = GetTree().Root.Size.y;
        LimitLeft = 0;
        LimitTop = 0;
        LimitRight = (int)screenWidth;
        LimitBottom = (int)screenHeight;
    }

    public void SetTarget(Node2D target) {
        _target = target;
        Position = target.Position;
    }

    public override void _Process(float delta) {
        if (_target == null || !IsInstanceValid(_target)) {
            return;
        }
        
        Position = Position.MoveToward(_target.Position, _cameraSpeed * delta);
    }
}
