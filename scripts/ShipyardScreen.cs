using Godot;
using System;
using System.Collections.Generic;

public class ShipyardScreen : Node2D {
    class Merchandise {
        public Sprite sprite;
        public VesselDesign item;
    }

    private bool _lockControls = false;
    private PopupNode _exodusPopup;

    private RpgGameState _gameState;

    private Merchandise _selectedMerchandise = new Merchandise { };
    private List<VesselDesign> _vesselSelection;

    private Vessel[] _fleetSlots = new Vessel[SpaceUnit.maxFleetSize];
    private Vessel[] _garrisonSlots = new Vessel[StarBase.maxGarrisonSize];

    private StarBase _starBase;

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _starBase = RpgGameState.enteredBase;
        _vesselSelection = VesselSelection();
        SetupUI();
        UpdateUI();
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

    private void UpdateUI() {
        GetNode<Label>("Status/CreditsValue").Text = _gameState.credits.ToString();

        GetNode<ButtonNode>("Status/ArkButton").Disabled = !CanTurnIntoArk();

        GetNode<Button>("VesselProduction/StartProduction").Disabled = _selectedMerchandise.sprite == null ||
            ItemInfo.BuyingPrice(_selectedMerchandise.item) > _gameState.credits ||
            _starBase.productionQueue.Count >= 4;

        {
            int i = 0;
            var productionQueue = GetNode<Panel>("ProductionQueue");
            foreach (var vesselDesignName in _starBase.productionQueue) {
                var order = productionQueue.GetNode<Sprite>($"Order{i}");
                var sprite = order.GetNode<Sprite>("Sprite");
                sprite.Texture = ItemInfo.Texture(VesselDesign.Find(vesselDesignName));
                i++;
            }
        }
    }

    private List<VesselDesign> VesselSelection() {
        var selection = new List<VesselDesign>();
        foreach (var design in VesselDesign.list) {
            if (_starBase.level < ItemInfo.MinStarBaseLevel(design)) {
                continue;
            }
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
        _lockControls = true;
        _exodusPopup.PopupCentered();
    }

    private void OnArkCancelButton() {
        _lockControls = false;
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

        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }

    private void SetupUI() {
        _exodusPopup = GetNode<PopupNode>("ExodusPopup");
        _exodusPopup.GetNode<ButtonNode>("CancelButton").Connect("pressed", this, nameof(OnArkCancelButton));
        _exodusPopup.GetNode<ButtonNode>("AcceptButton").Connect("pressed", this, nameof(OnArkAcceptButton));

        GetNode<ButtonNode>("Status/ArkButton").Connect("pressed", this, nameof(OnArkButton));

        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));

        GetNode<Button>("VesselProduction/StartProduction").Connect("pressed", this, nameof(OnStartProductionButton));

        for (int i = 0; i < _vesselSelection.Count; i++) {
            var item = _vesselSelection[i];
            var shopPanel = GetNode<Sprite>($"VesselProduction/Vessel{i}");
            var itemNode = MerchandiseItemNode.New(item);
            var args = new Godot.Collections.Array { i };
            itemNode.Connect("Clicked", this, nameof(OnMerchandiseClicked), args);
            itemNode.Name = "Merchandise";
            shopPanel.AddChild(itemNode);
            itemNode.GlobalPosition = shopPanel.GlobalPosition;
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
            var panel = GetNode<Sprite>($"ActiveFleet/Vessel{i}");
            var vesselSlot = ItemSlotNode.New(i, ItemKind.Vessel);
            vesselSlot.Name = "Slot";
            vesselSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _fleetSlots[index] = itemNode != null ? (Vessel)itemNode.item : null;
                return true;
            });
            panel.AddChild(vesselSlot);
            var args = new Godot.Collections.Array { i };
            vesselSlot.GetNode<Area2D>("Area2D").Connect("mouse_entered", this, nameof(OnVesselSlotHover), args);
            vesselSlot.Reset(null, true);
            vesselSlot.Connect("ItemApplied", this, nameof(OnShipApplied));
        }

        const int numRows = 4;
        const int numCols = 6;
        for (int row = 0; row < numRows; row++) {
            for (int col = 0; col < numCols; col++) {
                var i = col + (row * numCols);
                var storagePanel = GetNode<Sprite>($"Garrison/Vessel{i}");
                var itemSlot = ItemSlotNode.New(i, ItemKind.GarrisonVessel);
                itemSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                    _garrisonSlots[index] = itemNode != null ? (Vessel)itemNode.item : null;
                    return true;
                });
                itemSlot.Reset(null, true);
                itemSlot.Name = "Slot";
                storagePanel.AddChild(itemSlot);

                if (_starBase.garrison.Count > i) {
                    var itemNode = DraggableItemNode.New(itemSlot, _starBase.garrison[i].Get());
                    itemSlot.ApplyItem(null, itemNode);
                    GetTree().CurrentScene.AddChild(itemNode);
                    itemNode.GlobalPosition = storagePanel.GlobalPosition;
                }

                itemSlot.Connect("ItemApplied", this, nameof(OnShipApplied));
            }
        }

        for (int i = 0; i < _fleetSlots.Length; i++) {
            var panel = GetNode<Sprite>($"ActiveFleet/Vessel{i}");
            panel.GetNode<Label>("Name").Text = "";
            if (_gameState.humanUnit.Get().fleet.Count <= i) {
                continue;
            }
            var vessel = _gameState.humanUnit.Get().fleet[i].Get();
            panel.GetNode<Label>("Name").Text = vessel.pilotName;
            var slot = panel.GetNode<ItemSlotNode>("Slot");
            var itemNode = DraggableItemNode.New(slot, vessel);
            slot.ApplyItem(null, itemNode);
            GetTree().CurrentScene.AddChild(itemNode);
            itemNode.GlobalPosition = panel.GlobalPosition;
        }
    }

    private void OnStartProductionButton() {
        if (_gameState.credits < ItemInfo.BuyingPrice(_selectedMerchandise.item)) {
            return;
        }
        var starBase = RpgGameState.enteredBase;
        if (starBase.productionQueue.Count >= 4) {
            return;
        }
        if (starBase.garrison.Count == StarBase.maxGarrisonSize) {
            return;
        }

        _gameState.credits -= ItemInfo.BuyingPrice(_selectedMerchandise.item);
        if (starBase.productionQueue.Count == 0) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_started.wav"));
        }
        starBase.productionQueue.Enqueue(_selectedMerchandise.item.name);

        UpdateUI();
    }

    private void OnMerchandiseClicked(int index) {
        if (_selectedMerchandise.sprite != null) {
            _selectedMerchandise.sprite.Frame = 1;
        }
        var item = _vesselSelection[index];
        _selectedMerchandise.sprite = GetNode<Sprite>($"VesselProduction/Vessel{index}");
        _selectedMerchandise.item = item;
        _selectedMerchandise.sprite.Frame = 2;

        GetNode<Label>("VesselInfo/InfoBox/Body").Text = item.RenderHelp();

        UpdateUI();
    }

    private void OnShipApplied(ItemSlotNode fromSlot, DraggableItemNode dragged) {
        if (fromSlot != null) {
            var vessel = (Vessel)dragged.item;
            if (fromSlot.GetParent().HasNode("Name")) {
                fromSlot.GetParent().GetNode<Label>("Name").Text = "";
            }
            if (dragged.GetSlotNode().GetParent().HasNode("Name")) {
                dragged.GetSlotNode().GetParent().GetNode<Label>("Name").Text = vessel.pilotName;
            }
        }

        // _sellItemFallbackSlot = fromSlot;
        // _sellItemNode = dragged;

        // var itemName = ItemInfo.Name(dragged.item);
        // _sellEquipmentPopup.GetNode<Label>("Title").Text = "Sell " + itemName + "?";
        // var sellingPrice = ItemInfo.SellingPrice(dragged.item) / 2;
        // _sellEquipmentPopup.GetNode<Label>("SellingPrice").Text = $"{sellingPrice} cr";
        
        // _lockControls = true;
        // _sellEquipmentPopup.PopupCentered();
    }

    private void OnVesselSlotHover(int vesselIndex) {

    }

    private void PlayMoneySound() {
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
    }

    private void UpdateGarrison() {
        var list = VesselArrayToList(_garrisonSlots);
        RpgGameState.enteredBase.garrison = list;
    }

    public void UpdateFleet() {
        var list = VesselArrayToList(_fleetSlots);
        list[0].Get().isBot = false;
        list[0].Get().isGamepad = GameControls.preferGamepad;
        _gameState.humanUnit.Get().fleet = list;
    }

    private List<Vessel.Ref> VesselArrayToList(Vessel[] arr) {
        var fleetList = new List<Vessel.Ref>();
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i] != null) {
                var vessel = arr[i];
                vessel.isBot = true;
                fleetList.Add(vessel.GetRef());
            }
        }
        return fleetList;
    }

    private void OnLeaveButton() {
        UpdateFleet();
        UpdateGarrison();
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
