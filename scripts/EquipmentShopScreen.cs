using Godot;
using System;
using System.Collections.Generic;

public class EquipmentShopScreen : Node2D {
    class Merchandise {
        public Sprite sprite;
        public IItem item;
    }

    private bool _lockControls = false;

    private Popup _refuelPopup;
    private Popup _repairPopup;
    private Popup _cargoPopup;
    private Popup _dronesPopup;
    private Popup _sellEquipmentPopup;

    private ItemSlotNode _sellItemSlot;
    private ItemSlotNode _sellItemFallbackSlot;
    private DraggableItemNode _sellItemNode;

    private OptionButton _shopCategorySelect;

    private Vessel _selectedVessel;

    private List<Sprite> _equipmentSlots = new List<Sprite> { };
    private Merchandise _selectedMerchandise = new Merchandise { };

    private static string[] _equipmentCategories = new string[]{
        "Weapon",
        "Special Weapon",
        "Energy Source",
        "Shield",
        "Artifact",
    };

    public override void _Ready() {
        SetupUI();

        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeavePressed));
        GetNode<Button>("Status/RefuelButton").Connect("pressed", this, nameof(OnRefuelButton));
        GetNode<Button>("Status/RepairButton").Connect("pressed", this, nameof(OnRepairButton));
        GetNode<Button>("Status/CargoButton").Connect("pressed", this, nameof(OnCargoButton));
        GetNode<Button>("Status/DronesButton").Connect("pressed", this, nameof(OnDronesButton));

        _shopCategorySelect = GetNode<OptionButton>("EquipmentShop/CategorySelect");
        for (int i = 0; i < _equipmentCategories.Length; i++) {
            _shopCategorySelect.AddItem(_equipmentCategories[i], i);
        }
        _shopCategorySelect.Connect("item_selected", this, nameof(OnShopCategorySelected));

        _sellEquipmentPopup = GetNode<Popup>("SellEquipmentPopup");
        _sellEquipmentPopup.GetNode<Button>("Confirm").Connect("pressed", this, nameof(OnSellEquipmentConfirmButton));
        _sellEquipmentPopup.GetNode<Button>("Cancel").Connect("pressed", this, nameof(OnSellEquipmentCancelButton));

        _refuelPopup = GetNode<Popup>("RefuelPopup");
        _refuelPopup.GetNode<Button>("BuyFull").Connect("pressed", this, nameof(OnBuyFullFuelButton));
        _refuelPopup.GetNode<Button>("BuyMinor").Connect("pressed", this, nameof(OnBuyMinorFuelButton));
        _refuelPopup.GetNode<Button>("Done").Connect("pressed", this, nameof(OnRefuelDoneButton));
        _refuelPopup.GetNode<Label>("BuyMinor/Label").Text = (RpgGameState.fuelPrice * 50).ToString() + " cr";

        _repairPopup = GetNode<Popup>("RepairPopup");
        _repairPopup.GetNode<Button>("RepairFull").Connect("pressed", this, nameof(OnFullRepairButton));
        _repairPopup.GetNode<Button>("RepairMinor").Connect("pressed", this, nameof(OnMinorRepairButton));
        _repairPopup.GetNode<Button>("Cancel").Connect("pressed", this, nameof(OnRepairCancelButton));

        _cargoPopup = GetNode<Popup>("CargoPopup");
        _cargoPopup.GetNode<Button>("Done").Connect("pressed", this, nameof(OnCargoDoneButton));
        _cargoPopup.GetNode<Button>("SellDebris").Connect("pressed", this, nameof(OnCargoSellDebrisButton));
        _cargoPopup.GetNode<Button>("SellMinerals").Connect("pressed", this, nameof(OnCargoSellMineralsButton));
        _cargoPopup.GetNode<Button>("SellOrganic").Connect("pressed", this, nameof(OnCargoSellOrganicButton));
        _cargoPopup.GetNode<Button>("SellPower").Connect("pressed", this, nameof(OnCargoSellPowerButton));

        _dronesPopup = GetNode<Popup>("DronesPopup");
        _dronesPopup.GetNode<Button>("Done").Connect("pressed", this, nameof(OnDronesDoneButton));
        _dronesPopup.GetNode<Button>("BuyDrone").Connect("pressed", this, nameof(OnDronesBuyButton));

        if (RpgGameState.fuel == RpgGameState.MaxFuel()) {
            GetNode<Button>("Status/RefuelButton").Disabled = true;
        }

        for (int i = 0; i < RpgGameState.humanUnit.fleet.Count; i++) {
            var u = RpgGameState.humanUnit.fleet[i];
            if (u == null) {
                break;
            }
            var panel = GetNode<Sprite>($"UnitMenu/Unit{i}");
            var button = new TextureButton();
            button.RectSize = new Vector2(64, 64);
            button.Expand = true;
            button.StretchMode = TextureButton.StretchModeEnum.KeepCentered;
            button.TextureNormal = u.design.Texture();
            panel.AddChild(button);
            button.Connect("pressed", this, nameof(OnMemberSelected), new Godot.Collections.Array { i });
        }

        SelectMember(0);
        SelectShopCategory("Weapon");

        UpdateUI();
    }

    private void SetupUI() {
        var panel = GetNode<Panel>("UnitMenu");

        for (int i = 0; i < 2; i++) {
            var weaponPanel = panel.GetNode<Sprite>($"Weapon{i}");
            var weaponSlot = ItemSlotNode.New(i, ItemKind.Weapon);
            weaponSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.weapons[index] = itemNode != null ? (WeaponDesign)itemNode.item : EmptyWeapon.Design;
                return true;
            });
            weaponSlot.Name = "Slot";
            weaponPanel.AddChild(weaponSlot);
            var args = new Godot.Collections.Array { i };
            weaponSlot.GetNode<Area2D>("Area2D").Connect("mouse_entered", this, nameof(OnWeaponSlotHover), args);
        }
        {
            var weaponPanel = panel.GetNode<Sprite>("SpecialWeapon");
            var weaponSlot = ItemSlotNode.New(0, ItemKind.SpecialWeapon);
            weaponSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.specialWeapon = itemNode != null ? (WeaponDesign)itemNode.item : EmptyWeapon.Design;
                return true;
            });
            weaponSlot.Name = "Slot";
            weaponPanel.AddChild(weaponSlot);
        }

        {
            var energySourcePanel = panel.GetNode<Sprite>("EnergySource");
            var energySourceSlot = ItemSlotNode.New(0, ItemKind.EnergySource);
            energySourceSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.energySource = itemNode != null ? (EnergySource)itemNode.item : EnergySource.Find("None");
                return true;
            });
            energySourceSlot.Name = "Slot";
            energySourcePanel.AddChild(energySourceSlot);
        }

        {
            var shieldPanel = panel.GetNode<Sprite>("Shield");
            var shieldSlot = ItemSlotNode.New(0, ItemKind.Shield);
            shieldSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.shield = itemNode != null ? (ShieldDesign)itemNode.item : EmptyShield.Design;
                return true;
            });
            shieldSlot.Name = "Slot";
            shieldPanel.AddChild(shieldSlot);
        }

        for (int i = 0; i < 5; i++) {
            var artifactPanel = panel.GetNode<Sprite>($"Artifact{i}");
            var artifactSlot = ItemSlotNode.New(i, ItemKind.Artifact);
            artifactSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.artifacts[index] = itemNode != null ? (ArtifactDesign)itemNode.item : EmptyArtifact.Design;
                return true;
            });
            artifactSlot.Name = "Slot";
            artifactPanel.AddChild(artifactSlot);
        }

        const int numRows = 2;
        const int numCols = 7;
        for (int row = 0; row < numRows; row++) {
            for (int col = 0; col < numCols; col++) {
                var i = col + (row * numCols);
                var storagePanel = GetNode<Sprite>($"Storage/Item{i}");
                var itemSlot = ItemSlotNode.New(i, ItemKind.Storage);
                itemSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                    RpgGameState.storage[index] = itemNode != null ? itemNode.item : null;
                    return true;
                });
                itemSlot.Reset(null, true);
                itemSlot.Name = "Slot";
                storagePanel.AddChild(itemSlot);

                if (RpgGameState.storage[i] != null) {
                    var itemNode = DraggableItemNode.New(itemSlot, RpgGameState.storage[i]);
                    itemSlot.ApplyItem(null, itemNode);
                    GetTree().CurrentScene.AddChild(itemNode);
                    itemNode.GlobalPosition = storagePanel.GlobalPosition;
                }
            }
        }

        {
            var sellPanel = GetNode<Sprite>("EquipmentShop/SellSlot");
            _sellItemSlot = ItemSlotNode.New(0, ItemKind.Sell);
            _sellItemSlot.Connect("ItemApplied", this, nameof(OnSellEquipmentItemDragged));
            _sellItemSlot.Reset(null, true);
            sellPanel.AddChild(_sellItemSlot);
        }

        const int shopMumRows = 4;
        const int shopNumCols = 8;
        for (int i = 0; i < shopMumRows * shopNumCols; i++) {
            _equipmentSlots.Add(GetNode<Sprite>($"EquipmentShop/Item{i}"));
        }

        GetNode<Button>("EquipmentShop/Buy").Connect("pressed", this, nameof(OnShopBuyButton));
    }

    private void SelectMember(int vesselIndex) {
        var panel = GetNode<Panel>("UnitMenu");

        for (int i = 0; i < RpgGameState.humanUnit.fleet.Count; i++) {
            GetNode<Sprite>($"UnitMenu/Unit{i}").Frame = 1;
        }

        var unitPanel = GetNode<Sprite>($"UnitMenu/Unit{vesselIndex}");
        unitPanel.Frame = 2;

        var u = RpgGameState.humanUnit.fleet[vesselIndex];
        _selectedVessel = u;

        panel.GetNode<Sprite>("VesselDesign/Sprite").Texture = u.design.Texture();

        panel.GetNode<TextureProgress>("HealthBar").Value = QMath.Percantage(u.hp, u.design.maxHp);

        {
            var energySourcePanel = panel.GetNode<Sprite>("EnergySource");
            var energySourceSlot = energySourcePanel.GetNode<ItemSlotNode>("Slot");
            energySourceSlot.Reset(u, true);
            if (u.energySource != EnergySource.Find("None")) {
                var itemNode = DraggableItemNode.New(energySourceSlot, u.energySource);
                energySourceSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = energySourcePanel.GlobalPosition;
            }
        }

        {
            var shieldPanel = panel.GetNode<Sprite>("Shield");
            var shieldSlot = shieldPanel.GetNode<ItemSlotNode>("Slot");
            shieldSlot.Reset(u, true);
            if (u.shield != EmptyShield.Design) {
                var itemNode = DraggableItemNode.New(shieldSlot, u.shield);
                shieldSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = shieldPanel.GlobalPosition;
            }
        }

        {
            var specialWeaponPanel = panel.GetNode<Sprite>("SpecialWeapon");
            var specialWeaponSlot = specialWeaponPanel.GetNode<ItemSlotNode>("Slot");
            specialWeaponPanel.Frame = u.design.specialSlot ? 1 : 0;
            specialWeaponSlot.Reset(u, u.design.specialSlot);
            if (u.design.specialSlot && u.specialWeapon != EmptyWeapon.Design) {
                var itemNode = DraggableItemNode.New(specialWeaponSlot, u.specialWeapon);
                specialWeaponSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = specialWeaponPanel.GlobalPosition;
            }
        }

        for (int j = 0; j < u.weapons.Count; j++) {
            bool slotAvailable = j < u.design.weaponSlots;
            var weaponPanel = panel.GetNode<Sprite>($"Weapon{j}");
            var weaponSlot = weaponPanel.GetNode<ItemSlotNode>("Slot");
            var w = u.weapons[j];
            weaponPanel.Frame = slotAvailable ? 1 : 0;
            weaponSlot.Reset(u, slotAvailable);
            if (!slotAvailable) {
                continue;
            }
            if (w == EmptyWeapon.Design) {
                continue;
            }

            var itemNode = DraggableItemNode.New(weaponSlot, w);
            weaponSlot.ApplyItem(null, itemNode);
            GetTree().CurrentScene.AddChild(itemNode);
            itemNode.GlobalPosition = weaponPanel.GlobalPosition;
        }

        for (int j = 0; j < u.artifacts.Count; j++) {
            bool slotAvailable = j < u.design.artifactSlots;
            var artifactPanel = panel.GetNode<Sprite>($"Artifact{j}");
            var artifactSlot = artifactPanel.GetNode<ItemSlotNode>("Slot");
            var art = u.artifacts[j];
            artifactPanel.Frame = slotAvailable ? 1 : 0;
            artifactSlot.Reset(u, slotAvailable);

            if (!slotAvailable) {
                continue;
            }
            if (art == EmptyArtifact.Design) {
                continue;
            }

            var itemNode = DraggableItemNode.New(artifactSlot, art);
            artifactSlot.ApplyItem(null, itemNode);
            GetTree().CurrentScene.AddChild(itemNode);
            itemNode.GlobalPosition = artifactPanel.GlobalPosition;
        }
    }

    private void SelectShopCategory(string category) {
        foreach (var slot in _equipmentSlots) {
            if (slot.HasNode("Merchandise")) {
                // Used Free() instead of QueueFree() here on purpose.
                // We're adding a new node with the same name below,
                // so we need it removed right now.
                slot.GetNode<MerchandiseItemNode>("Merchandise").Free();
            }
        }
        if (_selectedMerchandise.sprite != null) {
            _selectedMerchandise.sprite.Frame = 1;
            _selectedMerchandise.sprite = null;
            _selectedMerchandise.item = null;
        }

        int i = 0;
        category = category.Replace(" ", ""); // "Special Weapon" -> "SpecialWeapon"
        for (int itemIndex = 0; itemIndex < RpgGameState.enteredBase.shopSelection.Count; itemIndex++) {
            var item = RpgGameState.enteredBase.shopSelection[itemIndex];
            if (item.Kind().ToString() != category) {
                continue;
            }
            var shopPanel = _equipmentSlots[i];
            var itemNode = MerchandiseItemNode.New(item);
            var args = new Godot.Collections.Array { i, itemIndex };
            itemNode.Connect("Clicked", this, nameof(OnMerchandiseClicked), args);
            itemNode.Name = "Merchandise";
            shopPanel.AddChild(itemNode);
            itemNode.GlobalPosition = shopPanel.GlobalPosition;
            i++;
        }
    }

    private void OnMerchandiseClicked(int merchIndex, int itemIndex) {
        if (_selectedMerchandise.sprite != null) {
            _selectedMerchandise.sprite.Frame = 1;
        }
        var item = RpgGameState.enteredBase.shopSelection[itemIndex];
        _selectedMerchandise.sprite = _equipmentSlots[merchIndex];
        _selectedMerchandise.item = item;
        _selectedMerchandise.sprite.Frame = 2;

        GetNode<Label>("EquipmentInfo/InfoBoxMerchandise/Body").Text = item.RenderHelp();

        UpdateUI();
    }

    private void OnShopBuyButton() {
        var item = _selectedMerchandise.item;
        var price = ItemInfo.BuyingPrice(item);
        if (price > RpgGameState.credits) {
            return;
        }
        RpgGameState.credits -= price;
        PlayMoneySound();
        for (int i = 0; i < RpgGameState.storage.Length; i++) {
            if (RpgGameState.storage[i] == null) {
                RpgGameState.storage[i] = item;

                var storagePanel = GetNode<Sprite>($"Storage/Item{i}");
                var itemSlot = storagePanel.GetNode<ItemSlotNode>("Slot");
                itemSlot.Reset(null, true);
                var itemNode = DraggableItemNode.New(itemSlot, RpgGameState.storage[i]);
                itemSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = storagePanel.GlobalPosition;
                break;
            }
        }
        UpdateUI();
    }

    private void OnShopCategorySelected(int id) {
        SelectShopCategory(_equipmentCategories[id]);
        UpdateUI();
    }

    private void OnMemberSelected(int i) {
        SelectMember(i);
        UpdateUI();
    }

    private void OnWeaponSlotHover(int weaponIndex) {
        GetNode<Label>("EquipmentInfo/InfoBoxOwn/Body").Text = _selectedVessel.weapons[weaponIndex].RenderHelp();
    }

    private void UpdateUI() {
        var starBase = RpgGameState.enteredBase;

        GetNode<Label>("Status/CreditsValue").Text = RpgGameState.credits.ToString();
        GetNode<Label>("Status/FuelValue").Text = ((int)RpgGameState.fuel).ToString() + "/" + RpgGameState.MaxFuel().ToString();
        GetNode<Label>("Status/CargoValue").Text = RpgGameState.humanUnit.CargoSize() + "/" + RpgGameState.humanUnit.CargoCapacity();

        GetNode<TextureProgress>("UnitMenu/HealthBar").Value = QMath.Percantage(_selectedVessel.hp, _selectedVessel.design.maxHp);

        GetNode<Button>("Status/RepairButton").Disabled = _selectedVessel.hp == _selectedVessel.design.maxHp;

        var missingFuel = (int)(RpgGameState.MaxFuel() - RpgGameState.fuel);
        _refuelPopup.GetNode<Button>("BuyMinor").Disabled = RpgGameState.fuel + 50 > RpgGameState.MaxFuel();
        _refuelPopup.GetNode<Label>("BuyFull/Label").Text = (RpgGameState.fuelPrice * missingFuel).ToString() + " cr";

        _dronesPopup.GetNode<Label>("BuyDrone/Value").Text = RpgGameState.drones.ToString();
        _dronesPopup.GetNode<Button>("BuyDrone").Disabled = RpgGameState.credits < RpgGameState.dronePrice;

        _cargoPopup.GetNode<Label>("SellDebris/Value").Text = RpgGameState.humanUnit.DebrisCount().ToString();
        _cargoPopup.GetNode<Label>("SellDebris/Price").Text = RpgGameState.DebrisSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Value").Text = RpgGameState.humanUnit.cargo.minerals.ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Price").Text = RpgGameState.MineralsSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Stock").Text = starBase.mineralsStock.ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Value").Text = RpgGameState.humanUnit.cargo.organic.ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Price").Text = RpgGameState.OrganicSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Stock").Text = starBase.organicStock.ToString();
        _cargoPopup.GetNode<Label>("SellPower/Value").Text = RpgGameState.humanUnit.cargo.power.ToString();
        _cargoPopup.GetNode<Label>("SellPower/Price").Text = RpgGameState.PowerSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellPower/Stock").Text = starBase.powerStock.ToString();

        GetNode<Button>("EquipmentShop/Buy").Disabled = _selectedMerchandise.sprite == null ||
            ItemInfo.BuyingPrice(_selectedMerchandise.item) > RpgGameState.credits;
    }

    private void OnSellEquipmentItemDragged(ItemSlotNode fromSlot, DraggableItemNode dragged) {
        if (_lockControls) {
            fromSlot.ApplyItem(_sellItemSlot, dragged);
            return;
        }

        _sellItemFallbackSlot = fromSlot;
        _sellItemNode = dragged;

        var itemName = ItemInfo.Name(dragged.item);
        _sellEquipmentPopup.GetNode<Label>("Title").Text = "Sell " + itemName + "?";
        var sellingPrice = ItemInfo.SellingPrice(dragged.item) / 2;
        _sellEquipmentPopup.GetNode<Label>("SellingPrice").Text = $"{sellingPrice} cr";
        
        _lockControls = true;
        _sellEquipmentPopup.PopupCentered();
    }

    private void PlayMoneySound() {
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
    }

    private void OnSellEquipmentConfirmButton() {
        _sellItemNode.QueueFree();
        _sellItemSlot.MakeEmpty();

        var sellingPrice = ItemInfo.SellingPrice(_sellItemNode.item) / 2;
        RpgGameState.credits += sellingPrice;
        UpdateUI();

        PlayMoneySound();

        _sellEquipmentPopup.Hide();
        _lockControls = false;
    }

    private void OnSellEquipmentCancelButton() {
        _sellItemFallbackSlot.ApplyItem(_sellItemSlot, _sellItemNode);

        _sellEquipmentPopup.Hide();
        _lockControls = false;
    }

    private void OnDronesButton() {
        if (_lockControls) {
            return;
        }

        _dronesPopup.GetNode<Label>("BuyDrone/Value").Text = RpgGameState.drones.ToString();
        _dronesPopup.GetNode<Label>("BuyDrone/Price").Text = "(" + RpgGameState.dronePrice + " cr/unit)";

        _lockControls = true;
        _dronesPopup.PopupCentered();
    }

    private void OnDronesBuyButton() {
        if (RpgGameState.credits < RpgGameState.dronePrice) {
            return;
        }
        RpgGameState.drones++;
        RpgGameState.credits -= RpgGameState.dronePrice;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnDronesDoneButton() {
        _dronesPopup.Hide();
        _lockControls = false;
    }

    private void OnCargoSellDebrisButton() {
        var cargo = RpgGameState.humanUnit.cargo;

        RpgGameState.credits += RpgGameState.DebrisSellPrice() * RpgGameState.humanUnit.DebrisCount();
        RpgGameState.krigiaMaterial += cargo.krigiaDeris;
        RpgGameState.wertuMaterial += cargo.wertuDebris;
        RpgGameState.zythMaterial += cargo.zythDebris;

        cargo.genericDebris = 0;
        cargo.krigiaDeris = 0;
        cargo.wertuDebris = 0;
        cargo.zythDebris = 0;

        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellMineralsButton() {
        RpgGameState.credits += RpgGameState.MineralsSellPrice() * RpgGameState.humanUnit.cargo.minerals;
        RpgGameState.enteredBase.mineralsStock += RpgGameState.humanUnit.cargo.minerals;
        RpgGameState.humanUnit.cargo.minerals = 0;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellOrganicButton() {
        RpgGameState.credits += RpgGameState.OrganicSellPrice() * RpgGameState.humanUnit.cargo.organic;
        RpgGameState.enteredBase.organicStock += RpgGameState.humanUnit.cargo.organic;
        RpgGameState.humanUnit.cargo.organic = 0;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellPowerButton() {
        RpgGameState.credits += RpgGameState.PowerSellPrice() * RpgGameState.humanUnit.cargo.power;
        RpgGameState.enteredBase.powerStock += RpgGameState.humanUnit.cargo.power;
        RpgGameState.humanUnit.cargo.power = 0;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoButton() {
        if (_lockControls) {
            return;
        }

        _lockControls = true;
        _cargoPopup.PopupCentered();
    }

    private void OnCargoDoneButton() {
        _cargoPopup.Hide();
        _lockControls = false;
    }

    private void OnRepairCancelButton() {
        _repairPopup.Hide();
        _lockControls = false;
    }

    private void OnRepairButton() {
        if (_lockControls) {
            return;
        }

        var missingHp = (int)(_selectedVessel.design.maxHp - _selectedVessel.hp);
        _repairPopup.GetNode<Label>("RepairFull/Label").Text = (RpgGameState.repairPrice * missingHp).ToString() + " cr";
        _repairPopup.GetNode<Button>("RepairMinor").Disabled = _selectedVessel.hp + 25 > _selectedVessel.design.maxHp;
        _repairPopup.GetNode<Label>("RepairMinor/Label").Text = (RpgGameState.repairPrice * 25).ToString() + " cr";

        _lockControls = true;
        _repairPopup.PopupCentered();
    }

    private void OnFullRepairButton() {
        _repairPopup.Hide();
        _lockControls = false;
        var missingHp = (int)(_selectedVessel.design.maxHp - _selectedVessel.hp);
        var price = RpgGameState.repairPrice * missingHp;
        if (RpgGameState.credits < price) {
            return;
        }
        _selectedVessel.hp = _selectedVessel.design.maxHp;
        RpgGameState.credits -= price;
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/repair.wav")));
        UpdateUI();
    }

    private void OnMinorRepairButton() {
        _repairPopup.Hide();
        _lockControls = false;
        var price = RpgGameState.repairPrice * 25;
        if (RpgGameState.credits < price) {
            return;
        }
        _selectedVessel.hp += 25;
        RpgGameState.credits -= price;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnRefuelDoneButton() {
        _refuelPopup.Hide();
        _lockControls = false;
    }

    private void OnRefuelButton() {
        if (_lockControls) {
            return;
        }

        _lockControls = true;
        _refuelPopup.PopupCentered();
    }

    private void OnBuyFullFuelButton() {
        var missingFuel = (int)(RpgGameState.MaxFuel() - RpgGameState.fuel);
        var price = RpgGameState.fuelPrice * missingFuel;
        if (RpgGameState.credits < price) {
            return;
        }
        RpgGameState.fuel = RpgGameState.MaxFuel();
        RpgGameState.credits -= price;
        PlayMoneySound();
        UpdateUI();

        _refuelPopup.Hide();
        _lockControls = false;
    }

    private void OnBuyMinorFuelButton() {
        var price = RpgGameState.fuelPrice * 50;
        if (RpgGameState.credits < price) {
            return;
        }
        RpgGameState.fuel += 50;
        RpgGameState.credits -= price;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnLeavePressed() {
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
