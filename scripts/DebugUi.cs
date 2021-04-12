using Godot;
using System;

public class DebugUi : VBoxContainer {
    private Label _hpLabel;
    private Label _energyLabel;
    private Label _backupEnergyLabel;
    private Label _speedLabel;
    private Label _speedPenaltyLabel;
    private Label _fpsLabel;

    private VesselNode _vessel;

    public void ObserveVessel(VesselNode vessel) {
        _vessel = vessel;

        _hpLabel = GetNode<Label>("Table3/HpValue");
        _energyLabel = GetNode<Label>("Table3/EnergyValue");
        _backupEnergyLabel = GetNode<Label>("Table3/BackupEnergyValue");
        _speedLabel = GetNode<Label>("Table3/SpeedValue");
        _speedPenaltyLabel = GetNode<Label>("Table2/SpeedPenaltyValue");
        _fpsLabel = GetNode<Label>("Table2/FpsValue");

        GetNode<Label>("Table3/HpMaxValue").Text = "/" + _vessel.State.maxHp.ToString();
        GetNode<Label>("Table3/EnergyMaxValue").Text = "/" + _vessel.State.maxEnergy.ToString();
        GetNode<Label>("Table3/BackupEnergyMaxValue").Text = "/" + _vessel.State.maxBackupEnergy.ToString();
        GetNode<Label>("Table3/SpeedMaxValue").Text = "/" + _vessel.State.maxSpeed.ToString();

        var timer = new Timer();
        AddChild(timer);
        timer.Connect("timeout", this, nameof(OnTimeout));
        timer.WaitTime = 0.2f;
        timer.Start();
    }

    public override void _Ready() {
    }

    private void OnTimeout() {
        _hpLabel.Text = ((int)_vessel.State.hp).ToString();
        _energyLabel.Text = ((int)_vessel.State.energy).ToString();
        _backupEnergyLabel.Text = ((int)_vessel.State.backupEnergy).ToString();
        _speedLabel.Text = ((int)_vessel.State.velocity.Length()).ToString();
        _speedPenaltyLabel.Text = (_vessel.State.speedPenalty).ToString("G3");
        _fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
    }
}
