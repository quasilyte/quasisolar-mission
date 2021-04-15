using Godot;
using System;

public class SettingsScreen : Node2D {
    public override void _Ready() {
        var controlMethod = GetNode<OptionButton>("ControlMethod");
        controlMethod.AddItem("Gamepad", 0);
        controlMethod.AddItem("Keyboard", 1);
        controlMethod.Select(GameControls.preferGamepad ? 0 : 1);

        var musicVolume = GetNode<OptionButton>("MusicVolume");
        musicVolume.AddItem("Turned off", 0);
        musicVolume.AddItem("Normal", 1);
        musicVolume.Select(BackgroundMusic.disabled ? 0 : 1);

        GetNode<ButtonNode>("ExitButton").Connect("pressed", this, nameof(OnExitButton));
    }

    private void OnExitButton() {
        GameControls.preferGamepad = GetNode<OptionButton>("ControlMethod").Selected == 0;
        BackgroundMusic.disabled = GetNode<OptionButton>("MusicVolume").Selected == 0;

        var bgMusic = GetNode<BackgroundMusic>("/root/BackgroundMusic");
        if (BackgroundMusic.disabled && bgMusic.Playing) {
            bgMusic.Stop();
        }

        GetTree().ChangeScene("res://scenes/MainMenu.tscn");
    }
}
