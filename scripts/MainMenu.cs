using Godot;
using System;
using System.Collections.Generic;

public class MainMenu : Node2D {
    private static bool _needInit = true;

    public override void _Ready() {
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayMenuMusic();

        GetNode<Button>("NewAdventureButton").Connect("pressed", this, nameof(OnNewAdventureButtonPressed));
        GetNode<Button>("LoadGameButton").Connect("pressed", this, nameof(OnLoadGameButtonPressed));        
        GetNode<Button>("QuickBattleButton").Connect("pressed", this, nameof(OnQuickBattleButtonPressed));
        GetNode<Button>("SettingsButton").Connect("pressed", this, nameof(OnSettingsButtonPressed));
        GetNode<Button>("ExitButton").Connect("pressed", this, nameof(OnExitButtonPressed));

        if (_needInit) {
            _needInit = false;

            GameControls.InitInputMap();

            ShieldDesign.InitLists();
            WeaponDesign.InitLists();
            VesselDesign.InitLists();
            ArtifactDesign.InitLists();
            SentinelDesign.InitLists();
            Research.InitLists();
            MapEventRegistry.InitLists();
            VesselStatus.InitLists();
        }

        QuickBattleState.Reset();
    }
    
    private void OnSettingsButtonPressed() {
        SettingsScreen.fromMainMenu = true;
        GetTree().ChangeScene("res://scenes/SettingsScreen.tscn");
    }

    private void OnLoadGameButtonPressed() {
        GetTree().ChangeScene("res://scenes/LoadGameScreen.tscn");
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
