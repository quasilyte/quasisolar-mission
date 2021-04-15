using Godot;
using System;

public class EnvHazardNode : Node2D {
    public override void _Ready() {
        GetNode<Sprite>("Sprite").Rotation = QRandom.FloatRange(-2, 2);
    }

    protected void RandomizeScaling() {
        var scaling = QRandom.FloatRange(1, 1.4f);

        GetNode<Sprite>("Sprite").Scale = new Vector2(scaling, scaling);
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Scale = new Vector2(scaling, scaling);
    }
}
