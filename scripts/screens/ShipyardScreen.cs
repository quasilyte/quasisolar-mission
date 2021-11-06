using Godot;
using System;
using System.Collections.Generic;

public class ShipyardScreen : Node2D {
    private PopupNode _exodusPopup;

    private Button _productionButton;

    private RpgGameState _gameState;

    private ItemSlotController _itemSlotController = new ItemSlotController();
    private List<VesselDesign> _vesselSelection;

    private Vessel[] _fleetSlots = new Vessel[SpaceUnit.maxFleetSize];
    private Vessel[] _garrisonSlots = new Vessel[StarBase.maxGarrisonSize];

    private StarBase _starBase;

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _starBase = RpgGameState.enteredBase;
        _vesselSelection = VesselSelection();
        _productionButton = GetNode<Button>("VesselProduction/StartProduction");
        SetupUI();
        UpdateUI();
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            OnLeaveButton();
            return;
        }
    }

    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("escape")) {
            OnLeaveButton();
        }
    }

    private int FleetEmptySlots() {
        var n = 0;
        for (int i = 0; i < _garrisonSlots.Length; i++) {
            if (_garrisonSlots[i] == null) {
                n++;
            }
        }
        return n;
    }

    private bool CanTurnIntoArk() {
        return _gameState.technologiesResearched.Contains("Ark Exodus") &&
            FleetEmptySlots() > 0 &&
            _gameState.credits > _gameState.exodusPrice;
    }

    private bool CanLeave() {
        return VesselArrayToList(_fleetSlots).Count >= 1;
    }

    private void UpdateUI() {
        GetNode<TextureButton>("Status/LeaveButton").Disabled = !CanLeave();

        GetNode<Label>("Status/CreditsValue").Text = _gameState.credits.ToString();

        GetNode<ButtonNode>("Status/ArkButton").Disabled = !CanTurnIntoArk();

        var selected = _itemSlotController.selected;
        _productionButton.Disabled = selected == null ||
            selected.GetItemKind() != ItemKind.Shop ||
            _starBase.VesselProductionPrice((VesselDesign)selected.GetItem()) > _gameState.credits ||
            _starBase.level < ItemInfo.MinStarBaseLevel(selected.GetItem()) ||
            _starBase.productionQueue.Count >= 4;

        // GetNode<Button>("VesselProduction/StartProduction").Disabled = _selectedMerchandise.sprite == null ||
        //     _starBase.VesselProductionPrice(_selectedMerchandise.item) > _gameState.credits ||
        //     _starBase.productionQueue.Count >= 4;

        {
            int i = 0;
            var productionQueue = GetNode<Panel>("ProductionQueue");
            foreach (var vesselDesignName in _starBase.productionQueue) {
                var order = productionQueue.GetNode<Sprite>($"Order{i}");
                var sprite = order.GetNode<Sprite>("Sprite");
                sprite.Scale = new Vector2(1.5f, 1.5f);
                sprite.Texture = ItemInfo.Texture(VesselDesign.Find(vesselDesignName));
                i++;
            }
        }
    }

    private List<VesselDesign> VesselSelection() {
        var selection = new List<VesselDesign>();
        foreach (var design in VesselDesign.list) {
            switch (design.availability) {
            case VesselDesign.ProductionAvailability.Never:
                continue;
            case VesselDesign.ProductionAvailability.Always:
                break;
            case VesselDesign.ProductionAvailability.ResearchRequired:
                if (!_gameState.technologiesResearched.Contains(design.name)) {
                    continue;
                }
                break;
            default:
                throw new Exception("unexpected availability: " + design.availability.ToString());
            }
            selection.Add(design);
        }
        return selection;
    }

    private void OnArkButton() {
        _exodusPopup.PopupCentered();
    }

    private void OnArkCancelButton() {
        _exodusPopup.Hide();
    }

    private void OnArkAcceptButton() {
        if (!CanTurnIntoArk()) {
            return;
        }

        _gameState.credits -= _gameState.exodusPrice;

        var system = RpgGameState.enteredBase.system.Get();
        system.starBase.id = 0;
        RpgGameState.humanBases.Remove(_starBase);

        UpdateFleet();
        UpdateGarrison();

        var ark = _gameState.NewVessel(Faction.Earthling, VesselDesign.Find("Ark"));
        ark.pilotName = PilotNames.UniqHumanName(_gameState.usedNames);
        VesselFactory.PadEquipment(ark);
        VesselFactory.InitStats(ark);
        _gameState.humanUnit.Get().fleet.Add(ark.GetRef());

        GetTree().ChangeScene("res://scenes/screens/MapView.tscn");
    }

    private void SetupUI() {
        _exodusPopup = GetNode<PopupNode>("ExodusPopup");
        _exodusPopup.GetNode<ButtonNode>("CancelButton").Connect("pressed", this, nameof(OnArkCancelButton));
        _exodusPopup.GetNode<ButtonNode>("AcceptButton").Connect("pressed", this, nameof(OnArkAcceptButton));

        GetNode<ButtonNode>("Status/ArkButton").Connect("pressed", this, nameof(OnArkButton));

        GetNode<TextureButton>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));

        GetNode<Button>("VesselProduction/StartProduction").Connect("pressed", this, nameof(OnStartProductionButton));

        var productionWarning = GetNode<Label>("ProductionQueue/WarningText");
        productionWarning.AddColorOverride("font_color", Color.Color8(0x5d, 0x60, 0x79));
        if (_starBase.productionQueue.Count != 0 && _starBase.mineralsStock == 0) {
            productionWarning.Text = "(no minerals)";
            productionWarning.Visible = true;
        } else if (_starBase.productionQueue.Count != 0 && _starBase.powerStock == 0) {
            productionWarning.Text = "(no power)";
            productionWarning.Visible = true;
        } else {
            productionWarning.Visible = false;
        }

        var productionGrid = GetNode<GridContainer>("VesselProduction/ScrollContainer/GridContainer");
        for (int i = 0; i < 5 * 5; i++) {
            var slot = _itemSlotController.NewSlot(0, ItemKind.Shop);
            productionGrid.AddChild(slot);
            slot.SetItemScaling(1.5f);
            var args = new Godot.Collections.Array { i };
            if (_vesselSelection.Count > i) {
                slot.Reset(null, true);
                slot.AssignItem(_vesselSelection[i]);
                if (_starBase.level < ItemInfo.MinStarBaseLevel(_vesselSelection[i])) {
                    slot.SetItemSpriteAlpha(0.5f);
                }
            } else {
                slot.Reset(null, false);
            }
            slot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{slot});
        }

        for (int i = 0; i < 4; i++) {
            var sprite = new Sprite();
            sprite.Name = "Sprite";
            var order = GetNode<Sprite>($"ProductionQueue/Order{i}");
            order.AddChild(sprite);
            sprite.GlobalPosition = order.GlobalPosition;
        }

        {
            var productionQueue = GetNode<Panel>("ProductionQueue");
            var progressValue = 0;
            if (_starBase.productionQueue.Count != 0) {
                var vesselDesign = VesselDesign.Find(_starBase.productionQueue.Peek());
                progressValue = QMath.Percantage((int)_starBase.productionProgress, vesselDesign.productionTime);
            }
            productionQueue.GetNode<Label>("ProgressValue").Text = progressValue + "%";
        }

        for (int i = 0; i < _fleetSlots.Length; i++) {
            var vesselSlot = GetNode<ItemSlotNode>($"ActiveFleet/Vessel{i}");
            _itemSlotController.AddSlot(vesselSlot);
            vesselSlot.SetAssignItemCallback((int index, IItem item) => {
                _fleetSlots[index] = item != null ? (Vessel)item : null;
                if (item == null) {
                    vesselSlot.GetNode<Label>("Name").Text = "";
                } else {
                    vesselSlot.GetNode<Label>("Name").Text = ((Vessel)item).pilotName;
                }
            });
            vesselSlot.Reset(null, true);
            vesselSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{vesselSlot});
        }

        var garrisonGrid = GetNode<GridContainer>("Garrison/ScrollContainer/GridContainer");
        for (int i = 0; i < StarBase.maxGarrisonSize; i++) {
            var itemSlot = _itemSlotController.NewSlot(i, ItemKind.GarrisonVessel);
            garrisonGrid.AddChild(itemSlot);
            itemSlot.SetItemScaling(1.5f);
            itemSlot.SetAssignItemCallback((int index, IItem item) => {
                _garrisonSlots[index] = item != null ? (Vessel)item : null;
            });
            itemSlot.Reset(null, true);
            itemSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{itemSlot});
            if (_starBase.garrison.Count > i) {
                itemSlot.ApplyItem(_starBase.garrison[i].Get());
            }
        }

        for (int i = 0; i < _fleetSlots.Length; i++) {
            var slot = GetNode<ItemSlotNode>($"ActiveFleet/Vessel{i}");
            slot.SetItemScaling(1.5f);
            if (_gameState.humanUnit.Get().fleet.Count <= i) {
                slot.GetNode<Label>("Name").Text = "";
                continue;
            }
            var vessel = _gameState.humanUnit.Get().fleet[i].Get();
            slot.GetNode<Label>("Name").Text = vessel.pilotName;
            slot.ApplyItem(vessel);
        }
    }

    private void OnItemClicked(ItemSlotNode itemSlot) {
        _itemSlotController.OnItemClicked(itemSlot);

        var infoBox = GetNode<Label>("VesselInfo/InfoBox/Body");
        if (_itemSlotController.selected == null) {
            infoBox.Text = "";
        } else if (_itemSlotController.selected.GetItem() is VesselDesign vesselDesign) {
            var text = "";
            if (_starBase.level < ItemInfo.MinStarBaseLevel(vesselDesign)) {
                text += "[!] Can't produce: star base level is too low.\n\n";
            }
            text += vesselDesign.RenderHelp(_starBase.VesselProductionPrice(vesselDesign));
            infoBox.Text = text;
        } else {
            infoBox.Text = ItemInfo.RenderHelp(_itemSlotController.selected.GetItem());
        }

        UpdateUI();
    }

    private void OnStartProductionButton() {
        var selected = _itemSlotController.selected;
        var vesselDesign = (VesselDesign)selected.GetItem();
        if (_gameState.credits < _starBase.VesselProductionPrice(vesselDesign)) {
            return;
        }
        var starBase = RpgGameState.enteredBase;
        if (starBase.productionQueue.Count >= 4) {
            return;
        }
        if (starBase.garrison.Count == StarBase.maxGarrisonSize) {
            return;
        }

        _gameState.credits -= _starBase.VesselProductionPrice(vesselDesign);
        if (starBase.productionQueue.Count == 0) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_started.wav"));
        }
        starBase.productionQueue.Enqueue(vesselDesign.name);

        UpdateUI();
    }

    private void OnVesselSlotHover(int vesselIndex) {

    }

    private void PlayMoneySound() {
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
    }

    private void UpdateGarrison() {
        var list = VesselArrayToList(_garrisonSlots);
        foreach (var v in list) {
            v.Get().RecalculateStats();
        }
        RpgGameState.enteredBase.garrison = list;
    }

    public void UpdateFleet() {
        var list = VesselArrayToList(_fleetSlots);
        list[0].Get().isBot = false;
        list[0].Get().isFlagship = true;
        list[0].Get().isGamepad = GameControls.preferGamepad;
        foreach (var v in list) {
            v.Get().RecalculateStats();
        }
        _gameState.humanUnit.Get().fleet = list;
    }

    private List<Vessel.Ref> VesselArrayToList(Vessel[] arr) {
        var fleetList = new List<Vessel.Ref>();
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i] != null) {
                var vessel = arr[i];
                vessel.isBot = true;
                vessel.isFlagship = false;
                fleetList.Add(vessel.GetRef());
            }
        }
        return fleetList;
    }

    private void OnLeaveButton() {
        UpdateFleet();
        UpdateGarrison();
        GetTree().ChangeScene("res://scenes/screens/StarBaseScreen.tscn");
    }
}
