using Godot;
using System;

public class DefeatScreen : Node2D {
    public override void _Ready() {
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
