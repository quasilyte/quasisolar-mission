using Godot;
using System;
using System.Collections.Generic;

public class VesselStatusScreen : Node2D {
    class UpgradeNode {
        public VesselStatus value;
        public Label label;
        public ButtonNode button;
    }

    private RpgGameState _gameState;
    private SpaceUnit _humanUnit;

    private Vessel _selectedVessel;

    private List<UpgradeNode> _upgradesSelection;

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _humanUnit = _gameState.humanUnit.Get();

        SetupUI();

        SelectMember(0);

        UpdateUI();
    }

    private void UpdateUI() {
        GetNode<Label>("Status/ExperienceValue").Text = _selectedVessel.exp.ToString();

        var upgradesAvailable = new List<VesselStatus>();
        foreach (var status in VesselStatus.list) {
            if (_selectedVessel.statusList.Contains(status.name)) {
                continue;
            }
            if (status.kind == VesselStatus.Kind.Unique) {
                continue;
            }
            if (status.kind == VesselStatus.Kind.Basic) {
                upgradesAvailable.Add(status);
                continue;
            }
            if (status.kind == VesselStatus.Kind.Rolled) {
                if (_selectedVessel.rolledUpgrades.Contains(status.name)) {
                    upgradesAvailable.Add(status);
                }
                continue;
            }
        }

        var upgradesPanel = GetNode<VBoxContainer>("UpgradesList/ScrollContainer/List");
        foreach (var child in upgradesPanel.GetChildren()) {
            ((Node)child).QueueFree();
        }
        _upgradesSelection = new List<UpgradeNode>();
        for (int i = 0; i < upgradesAvailable.Count; i++) {
            var status = upgradesAvailable[i];
            var upgradeNode = VesselUpgradeNode.New();

            var label = upgradeNode.GetNode<Label>("GridContainer/Name");

            label.Text = status.name;
            upgradesPanel.AddChild(upgradeNode);
            var button = upgradeNode.GetNode<ButtonNode>("GridContainer/Install");

            _upgradesSelection.Add(new UpgradeNode {
                value = status,
                label = label,
                button = button,
            });

            if (_selectedVessel.exp < status.expCost) {
                button.Disabled = true;
            } else {
                button.Connect("pressed", this, nameof(OnUpgradeInstallPressed), new Godot.Collections.Array { i });
            }
        }

        var statusListPanel = GetNode<VBoxContainer>("VesselStatus/ScrollContainer/List");
        foreach (var child in statusListPanel.GetChildren()) {
            ((Node)child).QueueFree();
        }
        foreach (var statusName in _selectedVessel.statusList) {
            var label = new Label();
            label.Text = statusName;
            label.RectSize = new Vector2(336, 48);
            label.Valign = Label.VAlign.Center;
            label.RectPosition = new Vector2(16, 16);
            label.MouseFilter = Control.MouseFilterEnum.Stop;
            statusListPanel.AddChild(label);
        }
    }

    private void OnUpgradeInstallPressed(int i) {
        var status = _upgradesSelection[i].value;
        _selectedVessel.statusList.Add(status.name);
        _selectedVessel.exp -= status.expCost;
        UpdateUI();
    }

    private void SetupUI() {
        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeavePressed));

        for (int i = 0; i < 4; i++) {
            var panel = GetNode<Sprite>($"Vessels/Vessel{i}");

            if (i >= _humanUnit.fleet.Count) {
                panel.GetNode<Label>("Name").Text = "";
            } else {
                var vessel = _humanUnit.fleet[i].Get();
                panel.GetNode<Label>("Name").Text = vessel.pilotName;

                var button = new TextureButton();
                button.RectSize = new Vector2(64, 64);
                button.Expand = true;
                button.StretchMode = TextureButton.StretchModeEnum.KeepCentered;
                button.TextureNormal = vessel.Design().Texture();
                panel.AddChild(button);
                button.Connect("pressed", this, nameof(OnMemberSelected), new Godot.Collections.Array { i });
            }
        }
    }

    private void OnLeavePressed() {
        GetTree().ChangeScene("res://scenes/screens/StarBaseScreen.tscn");
    }

    private void OnMemberSelected(int id) {
        SelectMember(id);
        UpdateUI();
    }

    private void SelectMember(int vesselIndex) {
        for (int i = 0; i < _humanUnit.fleet.Count; i++) {
            GetNode<Sprite>($"Vessels/Vessel{i}").Frame = 1;
        }

        var panel = GetNode<Sprite>($"Vessels/Vessel{vesselIndex}");
        panel.Frame = 2;

        _selectedVessel = _humanUnit.fleet[vesselIndex].Get();

        // var u = _humanUnit.fleet[vesselIndex].Get();
        // _selectedVessel = u;

        // panel.GetNode<Sprite>("VesselDesign/Sprite").Texture = u.Design().Texture();

        // panel.GetNode<TextureProgress>("HealthBar").Value = QMath.Percantage(u.hp, u.MaxHp());

        // {
        //     var energySourcePanel = panel.GetNode<Sprite>("EnergySource");
        //     var energySourceSlot = energySourcePanel.GetNode<ItemSlotNode>("Slot");
        //     energySourceSlot.Reset(u, true);
        //     if (u.energySourceName != "None") {
        //         var itemNode = DraggableItemNode.New(energySourceSlot, u.GetEnergySource());
        //         energySourceSlot.ApplyItem(null, itemNode);
        //         GetTree().CurrentScene.AddChild(itemNode);
        //         itemNode.GlobalPosition = energySourcePanel.GlobalPosition;
        //     }
        // }

        // {
        //     var shieldPanel = panel.GetNode<Sprite>("Shield");
        //     var shieldSlot = shieldPanel.GetNode<ItemSlotNode>("Slot");
        //     shieldSlot.Reset(u, true);
        //     if (u.Shield() != EmptyShield.Design) {
        //         var itemNode = DraggableItemNode.New(shieldSlot, u.Shield());
        //         shieldSlot.ApplyItem(null, itemNode);
        //         GetTree().CurrentScene.AddChild(itemNode);
        //         itemNode.GlobalPosition = shieldPanel.GlobalPosition;
        //     }
        // }

        // {
        //     var sentinelPanel = panel.GetNode<Sprite>("Sentinel");
        //     var sentinelSlot = sentinelPanel.GetNode<ItemSlotNode>("Slot");
        //     sentinelPanel.Frame = u.Design().sentinelSlot ? 1 : 0;
        //     sentinelSlot.Reset(u, u.Design().sentinelSlot);
        //     if (u.Design().sentinelSlot && u.sentinelName != "Empty") {
        //         var itemNode = DraggableItemNode.New(sentinelSlot, u.Sentinel());
        //         sentinelSlot.ApplyItem(null, itemNode);
        //         GetTree().CurrentScene.AddChild(itemNode);
        //         itemNode.GlobalPosition = sentinelPanel.GlobalPosition;
        //     }
        // }

        // {
        //     var specialWeaponPanel = panel.GetNode<Sprite>("SpecialWeapon");
        //     var specialWeaponSlot = specialWeaponPanel.GetNode<ItemSlotNode>("Slot");
        //     specialWeaponPanel.Frame = u.Design().specialSlot ? 1 : 0;
        //     specialWeaponSlot.Reset(u, u.Design().specialSlot);
        //     if (u.Design().specialSlot && u.SpecialWeapon() != EmptyWeapon.Design) {
        //         var itemNode = DraggableItemNode.New(specialWeaponSlot, u.SpecialWeapon());
        //         specialWeaponSlot.ApplyItem(null, itemNode);
        //         GetTree().CurrentScene.AddChild(itemNode);
        //         itemNode.GlobalPosition = specialWeaponPanel.GlobalPosition;
        //     }
        // }

        // for (int j = 0; j < u.weapons.Count; j++) {
        //     bool slotAvailable = j < u.Design().weaponSlots;
        //     var weaponPanel = panel.GetNode<Sprite>($"Weapon{j}");
        //     var weaponSlot = weaponPanel.GetNode<ItemSlotNode>("Slot");
        //     var w = u.weapons[j];
        //     weaponPanel.Frame = slotAvailable ? 1 : 0;
        //     weaponSlot.Reset(u, slotAvailable);
        //     if (!slotAvailable) {
        //         continue;
        //     }
        //     if (w == EmptyWeapon.Design.name) {
        //         continue;
        //     }

        //     var itemNode = DraggableItemNode.New(weaponSlot, WeaponDesign.Find(w));
        //     weaponSlot.ApplyItem(null, itemNode);
        //     GetTree().CurrentScene.AddChild(itemNode);
        //     itemNode.GlobalPosition = weaponPanel.GlobalPosition;
        // }

        // for (int j = 0; j < u.artifacts.Count; j++) {
        //     bool slotAvailable = j < u.Design().artifactSlots;
        //     var artifactPanel = panel.GetNode<Sprite>($"Artifact{j}");
        //     var artifactSlot = artifactPanel.GetNode<ItemSlotNode>("Slot");
        //     var art = u.artifacts[j];
        //     artifactPanel.Frame = slotAvailable ? 1 : 0;
        //     artifactSlot.Reset(u, slotAvailable);

        //     if (!slotAvailable) {
        //         continue;
        //     }
        //     if (art == EmptyArtifact.Design.name) {
        //         continue;
        //     }

        //     var itemNode = DraggableItemNode.New(artifactSlot, ArtifactDesign.Find(art));
        //     artifactSlot.ApplyItem(null, itemNode);
        //     GetTree().CurrentScene.AddChild(itemNode);
        //     itemNode.GlobalPosition = artifactPanel.GlobalPosition;
        // }
    }
}
