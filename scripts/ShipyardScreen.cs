using Godot;
using System.Collections.Generic;

public class ShipyardScreen : Node2D {
    class Merchandise {
        public Sprite sprite;
        public VesselDesign item;
    }

    private Merchandise _selectedMerchandise = new Merchandise { };
    private List<VesselDesign> _vesselSelection;

    private Vessel[] _fleetSlots = new Vessel[SpaceUnit.maxFleetSize];
    private Vessel[] _garrisonSlots = new Vessel[StarBase.maxGarrisonSize];

    private StarBase _starBase;

    public override void _Ready() {
        _starBase = RpgGameState.enteredBase;
        _vesselSelection = VesselSelection();
        SetupUI();
        UpdateUI();
    }

    private void UpdateUI() {
        GetNode<Label>("Status/CreditsValue").Text = RpgGameState.credits.ToString();

        GetNode<Button>("VesselProduction/StartProduction").Disabled = _selectedMerchandise.sprite == null ||
            ItemInfo.BuyingPrice(_selectedMerchandise.item) > RpgGameState.credits ||
            _starBase.productionQueue.Count >= 4;

        for (int i = 0; i < _fleetSlots.Length; i++) {
            var panel = GetNode<Sprite>($"ActiveFleet/Vessel{i}");
            panel.GetNode<Label>("Name").Text = "";
            if (RpgGameState.humanUnit.fleet.Count <= i) {
                continue;
            }
            var vessel = RpgGameState.humanUnit.fleet[i];
            panel.GetNode<Label>("Name").Text = vessel.pilotName;
            var slot = panel.GetNode<ItemSlotNode>("Slot");
            var itemNode = DraggableItemNode.New(slot, vessel);
            slot.ApplyItem(null, itemNode);
            GetTree().CurrentScene.AddChild(itemNode);
            itemNode.GlobalPosition = panel.GlobalPosition;
        }

        {
            int i = 0;
            var productionQueue = GetNode<Panel>("ProductionQueue");
            foreach (var vesselDesign in _starBase.productionQueue) {
                var order = productionQueue.GetNode<Sprite>($"Order{i}");
                var sprite = order.GetNode<Sprite>("Sprite");
                sprite.Texture = ItemInfo.Texture(vesselDesign);
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
            // TODO: check for the technology requirements instead.
            if (design.affiliation != "Earthling") {
                continue;
            }
            selection.Add(design);
        }
        return selection;
    }

    private void SetupUI() {
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
                var vesselDesign = _starBase.productionQueue.Peek();
                progressValue = QMath.Percantage(_starBase.productionProgress, vesselDesign.productionTime);
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
                    var itemNode = DraggableItemNode.New(itemSlot, _starBase.garrison[i]);
                    itemSlot.ApplyItem(null, itemNode);
                    GetTree().CurrentScene.AddChild(itemNode);
                    itemNode.GlobalPosition = storagePanel.GlobalPosition;
                }
            }
        }
    }

    private void OnStartProductionButton() {
        if (RpgGameState.credits < ItemInfo.BuyingPrice(_selectedMerchandise.item)) {
            return;
        }
        var starBase = RpgGameState.enteredBase;
        if (starBase.productionQueue.Count >= 4) {
            return;
        }
        if (starBase.garrison.Count == StarBase.maxGarrisonSize) {
            return;
        }

        RpgGameState.credits -= ItemInfo.BuyingPrice(_selectedMerchandise.item);
        if (starBase.productionQueue.Count == 0) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/production_started.wav"));
        }
        starBase.productionQueue.Enqueue(_selectedMerchandise.item);

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
            dragged.GetSlotNode().GetParent().GetNode<Label>("Name").Text = vessel.pilotName;
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
        list[0].isBot = false;
        list[0].isGamepad = true;
        RpgGameState.humanUnit.fleet = list;
    }

    private List<Vessel> VesselArrayToList(Vessel[] arr) {
        var fleetList = new List<Vessel>();
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i] != null) {
                var vessel = arr[i];
                vessel.isBot = true;
                fleetList.Add(vessel);
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