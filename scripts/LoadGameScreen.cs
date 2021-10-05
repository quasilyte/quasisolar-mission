using Godot;
using System;

public class LoadGameScreen : Node2D {
    public override void _Ready() {
        var instance = GameStateSerializer.Decode();

        var rng = new RandomNumberGenerator();
        rng.Seed = instance.seed;
        RpgGameState.rng = rng;

        instance.InitStaticState(false);
        RpgGameState.instance = instance;

        GetTree().ChangeScene("res://scenes/screens/MapView.tscn");
    }
}
