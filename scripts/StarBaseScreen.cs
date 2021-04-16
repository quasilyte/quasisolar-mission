using Godot;
using System;

public class StarBaseScreen : Node2D {
    public override void _Ready() {
        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayShipyardMusic();

        GetNode<Button>("EquipmentShop").Connect("pressed", this, nameof(OnEquipmentShopButton));
        GetNode<Button>("Shipyard").Connect("pressed", this, nameof(OnShipyardButton));
        GetNode<Button>("SkillTree").Connect("pressed", this, nameof(OnSkillTreeButton));
        GetNode<Button>("LeaveBase").Connect("pressed", this, nameof(OnLeaveBaseButton));
    }

    private void OnEquipmentShopButton() {
        GetTree().ChangeScene("res://scenes/EquipmentShopScreen.tscn");
    }

    private void OnShipyardButton() {
        GetTree().ChangeScene("res://scenes/ShipyardScreen.tscn");
    }

    private void OnSkillTreeButton() {
        GetTree().ChangeScene("res://scenes/SkillsScreen.tscn");
    }

    private void OnLeaveBaseButton() {
        RpgGameState.instance.transition = RpgGameState.MapTransition.ExitStarBase;
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }
}
