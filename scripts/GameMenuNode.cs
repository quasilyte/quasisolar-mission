using Godot;
using System;

public class GameMenuNode : Node2D {
    [Signal]
    public delegate void Closed();

    private static PackedScene _scene = null;
    public static GameMenuNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/GameMenuNode.tscn");
        }
        var o = (GameMenuNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        var popup = GetNode<PopupNode>("PopupNode");
        popup.GetNode<ButtonNode>("SaveGame").Connect("pressed", this, nameof(OnSaveGame));
        popup.GetNode<ButtonNode>("LoadGame").Connect("pressed", this, nameof(OnLoadGame));
        popup.GetNode<ButtonNode>("Settings").Connect("pressed", this, nameof(OnSettings));
        popup.GetNode<ButtonNode>("CloseMenu").Connect("pressed", this, nameof(OnCloseMenu));
        popup.GetNode<ButtonNode>("MainMenu").Connect("pressed", this, nameof(OnMainMenu));

        popup.GetNode<ButtonNode>("Settings").Disabled = GetTree().CurrentScene.Name != "MapView";
    }

    public void Open() {
        GetNode<PopupNode>("PopupNode").PopupCentered();
    }

    private void OnSaveGame() {
        RpgGameState.instance.CollectGarbage();
        GameStateSerializer.Encode(RpgGameState.instance);
    }

    private void OnLoadGame() {
        GetTree().ChangeScene("res://scenes/LoadGameScreen.tscn");
    }

    private void OnSettings() {
        SettingsScreen.fromMainMenu = false;
        GetTree().ChangeScene("res://scenes/SettingsScreen.tscn");
    }

    private void OnMainMenu() {
        GetTree().ChangeScene("res://scenes/MainMenu.tscn");
    }

    private void OnCloseMenu() {
        GetNode<PopupNode>("PopupNode").Hide();
        EmitSignal(nameof(Closed));
    }
}
