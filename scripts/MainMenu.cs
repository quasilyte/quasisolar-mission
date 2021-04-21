using Godot;
using System;
using System.Collections.Generic;

public class MainMenu : Node2D {
    public bool _needInit = true;

    public override void _Ready() {
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayMenuMusic();

        GetNode<Button>("NewAdventureButton").Connect("pressed", this, nameof(OnNewAdventureButtonPressed));
        GetNode<Button>("QuickBattleButton").Connect("pressed", this, nameof(OnQuickBattleButtonPressed));
        GetNode<Button>("SettingsButton").Connect("pressed", this, nameof(OnSettingsButtonPressed));
        GetNode<Button>("ExitButton").Connect("pressed", this, nameof(OnExitButtonPressed));

        QuickBattleState.Reset();

        if (_needInit) {
            _needInit = false;

            GameControls.InitInputMap();

            ShieldDesign.InitLists();
            WeaponDesign.InitLists();
            RandomEvent.InitLists();
            ArtifactDesign.InitLists();
            VesselDesign.InitLists();
            Research.InitLists();
        }
    }
    
    private void OnSettingsButtonPressed() {
        GetTree().ChangeScene("res://scenes/SettingsScreen.tscn");
    }

    private void OnNewAdventureButtonPressed() {
        GetTree().ChangeScene("res://scenes/NewGameScene.tscn");
    }

    private void OnQuickBattleButtonPressed() {
        GetTree().ChangeScene("res://scenes/QuickBattleMenu.tscn");
    }

    private void OnExitButtonPressed() {
        GetTree().Quit();
    }
}
