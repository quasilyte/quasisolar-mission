using Godot;
using System.Collections.Generic;

public class WaypointLineNode : Line2D {
    public Node2D origin;
    public ArenaCameraNode camera;

    private Queue<Vector2> _globalPositions = new Queue<Vector2>();

    public override void _Ready() {
        // AddPoint(TranslatePos(origin.GlobalPosition));
    }

    public override void _Process(float delta) {
        // SetPointPosition(0, TranslatePos(origin.GlobalPosition + origin.Transform.x * 32));
        // int i = 1;
        // foreach (var pos in _globalPositions) {
        //     SetPointPosition(i, TranslatePos(pos));
        //     i++;
        // }

        int i = 0;
        foreach (var pos in _globalPositions) {
            SetPointPosition(i, QMath.TranslateViewportPos(camera, pos));
            i++;
        }
    }

    public void Clear() {
        ClearPoints();
        _globalPositions.Clear();
    }

    public void Enqueue(Vector2 pos) {
        if (GetPointCount() == 0) {
            _globalPositions.Enqueue(origin.GlobalPosition);
            AddPoint(origin.GlobalPosition);
        }
        _globalPositions.Enqueue(pos);
        AddPoint(pos);
    }

    public void Dequeue() {
        if (GetPointCount() == 2) {
            _globalPositions.Dequeue();
            _globalPositions.Dequeue();
            RemovePoint(1);
            RemovePoint(0);
        } else {
            _globalPositions.Dequeue();
            RemovePoint(1);
        }
    }
}
