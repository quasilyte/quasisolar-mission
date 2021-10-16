using Godot;
using System;

public class DefeatScreen : Node2D {
    public override void _Ready() {
        GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/mission_failed.wav"));

        GetNode<ButtonNode>("LoadGame").Connect("pressed", this, nameof(OnLoadGame));
        GetNode<ButtonNode>("MainMenu").Connect("pressed", this, nameof(OnMainMenu));
    }

    private void OnLoadGame() {
        GetTree().ChangeScene("res://scenes/LoadGameScreen.tscn");
    }

    private void OnMainMenu() {
        GetTree().ChangeScene("res://scenes/MainMenu.tscn");
    }
}
