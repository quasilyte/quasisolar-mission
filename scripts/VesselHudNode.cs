using Godot;
using System;

public class VesselHudNode : Node2D {
    public ArenaCameraNode camera;

    private TextureProgress _energy;
    private TextureProgress _backupEnergy;

    private static PackedScene _scene = null;
    public static VesselHudNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/VesselHudNode.tscn");
        }
        var o = (VesselHudNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        _energy = GetNode<TextureProgress>("Energy");
        _backupEnergy = GetNode<TextureProgress>("BackupEnergy");
    }

    public void UpdateEnergyPercentage(float energy) {
        _energy.Value = energy;
    }

    public void UpdateBackupEnergyPercentage(float backupEnergy) {
        _backupEnergy.Value = backupEnergy;
    }
}
