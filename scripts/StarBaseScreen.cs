using Godot;
using System;

public class StarBaseScreen : Node2D {
    public override void _Ready() {
        // GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayShipyardMusic();

        GetNode<Button>("EquipmentShop").Connect("pressed", this, nameof(OnEquipmentShopButton));
        GetNode<Button>("Shipyard").Connect("pressed", this, nameof(OnShipyardButton));
        GetNode<Button>("StarBaseModules").Connect("pressed", this, nameof(OnStarBaseModulesButton));
        GetNode<Button>("VesselUpgrades").Connect("pressed", this, nameof(OnVesselUpgradesButton));
        GetNode<Button>("LeaveBase").Connect("pressed", this, nameof(OnLeaveBaseButton));
    }

    private void OnEquipmentShopButton() {
        GetTree().ChangeScene("res://scenes/EquipmentShopScreen.tscn");
    }

    private void OnShipyardButton() {
        GetTree().ChangeScene("res://scenes/ShipyardScreen.tscn");
    }

    private void OnStarBaseModulesButton() {
        GetTree().ChangeScene("res://scenes/StarBaseModulesScreen.tscn");
    }

    private void OnVesselUpgradesButton() {
        GetTree().ChangeScene("res://scenes/VesselStatusScreen.tscn");
    }

    private void OnLeaveBaseButton() {
        RpgGameState.transition = RpgGameState.MapTransition.ExitStarBase;
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }
}
