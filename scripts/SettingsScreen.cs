using Godot;
using System;

public class SettingsScreen : Node2D {
    public static bool fromMainMenu = true;

    public override void _Ready() {
        var controlMethod = GetNode<OptionButton>("ControlMethod");
        controlMethod.AddItem("Gamepad", 0);
        controlMethod.AddItem("Keyboard", 1);
        controlMethod.Select(GameControls.preferGamepad ? 0 : 1);

        var musicVolume = GetNode<OptionButton>("MusicVolume");
        musicVolume.AddItem("Turned off", 0);
        musicVolume.AddItem("Quiet", 1);
        musicVolume.AddItem("Normal", 2);
        musicVolume.AddItem("Loud", 3);
        musicVolume.Select(BackgroundMusic.volumeSetting);

        GetNode<ButtonNode>("ExitButton").Connect("pressed", this, nameof(OnExitButton));
    }

    private void OnExitButton() {
        var oldMusicVolume = BackgroundMusic.volumeSetting;

        GameControls.preferGamepad = GetNode<OptionButton>("ControlMethod").Selected == 0;
        BackgroundMusic.volumeSetting = GetNode<OptionButton>("MusicVolume").Selected;

        var bgMusic = GetNode<BackgroundMusic>("/root/BackgroundMusic");
        if ((oldMusicVolume != BackgroundMusic.volumeSetting || BackgroundMusic.volumeSetting == 0) && bgMusic.Playing) {
            bgMusic.Stop();
        }

        if (fromMainMenu) {
            GetTree().ChangeScene("res://scenes/MainMenu.tscn");
        } else {
            GetTree().ChangeScene("res://scenes/MapView.tscn");
        }
    }
}
