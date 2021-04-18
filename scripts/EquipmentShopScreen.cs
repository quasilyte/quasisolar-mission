using Godot;
using System;
using System.Collections.Generic;

public class EquipmentShopScreen : Node2D {
    class Merchandise {
        public Sprite sprite;
        public AbstractItem item;
    }

    private bool _lockControls = false;

    private Popup _refuelPopup;
    private Popup _repairPopup;
    private Popup _cargoPopup;
    private Popup _dronesPopup;
    private Popup _sellEquipmentPopup;

    private RpgGameState _gameState;

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
        _gameState = RpgGameState.instance;

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
        _refuelPopup.GetNode<Label>("BuyMinor/Label").Text = (_gameState.fuelPrice * 50).ToString() + " cr";

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

        if (_gameState.fuel == RpgGameState.MaxFuel()) {
            GetNode<Button>("Status/RefuelButton").Disabled = true;
        }

        for (int i = 0; i < _gameState.humanUnit.fleet.Count; i++) {
            var u = _gameState.humanUnit.fleet[i];
            if (u == null) {
                break;
            }
            var panel = GetNode<Sprite>($"UnitMenu/Unit{i}");
            var button = new TextureButton();
            button.RectSize = new Vector2(64, 64);
            button.Expand = true;
            button.StretchMode = TextureButton.StretchModeEnum.KeepCentered;
            button.TextureNormal = u.Design().Texture();
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
                _selectedVessel.weapons[index] = itemNode != null ? ((WeaponDesign)itemNode.item).name : EmptyWeapon.Design.name;
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
                _selectedVessel.specialWeaponName = itemNode != null ? ((WeaponDesign)itemNode.item).name : EmptyWeapon.Design.name;
                return true;
            });
            weaponSlot.Name = "Slot";
            weaponPanel.AddChild(weaponSlot);
        }

        {
            var energySourcePanel = panel.GetNode<Sprite>("EnergySource");
            var energySourceSlot = ItemSlotNode.New(0, ItemKind.EnergySource);
            energySourceSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.energySourceName = itemNode != null ? ((EnergySource)itemNode.item).name : "None";
                return true;
            });
            energySourceSlot.Name = "Slot";
            energySourcePanel.AddChild(energySourceSlot);
        }

        {
            var shieldPanel = panel.GetNode<Sprite>("Shield");
            var shieldSlot = ItemSlotNode.New(0, ItemKind.Shield);
            shieldSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.shieldName = itemNode != null ? ((ShieldDesign)itemNode.item).name : EmptyShield.Design.name;
                return true;
            });
            shieldSlot.Name = "Slot";
            shieldPanel.AddChild(shieldSlot);
        }

        for (int i = 0; i < 5; i++) {
            var artifactPanel = panel.GetNode<Sprite>($"Artifact{i}");
            var artifactSlot = ItemSlotNode.New(i, ItemKind.Artifact);
            artifactSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.artifacts[index] = itemNode != null ? ((ArtifactDesign)itemNode.item).name : EmptyArtifact.Design.name;
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
                    _gameState.storage[index] = itemNode != null ? itemNode.item : null;
                    return true;
                });
                itemSlot.Reset(null, true);
                itemSlot.Name = "Slot";
                storagePanel.AddChild(itemSlot);

                if (_gameState.storage[i] != null) {
                    var itemNode = DraggableItemNode.New(itemSlot, _gameState.storage[i]);
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

        for (int i = 0; i < _gameState.humanUnit.fleet.Count; i++) {
            GetNode<Sprite>($"UnitMenu/Unit{i}").Frame = 1;
        }

        var unitPanel = GetNode<Sprite>($"UnitMenu/Unit{vesselIndex}");
        unitPanel.Frame = 2;

        var u = _gameState.humanUnit.fleet[vesselIndex];
        _selectedVessel = u;

        panel.GetNode<Sprite>("VesselDesign/Sprite").Texture = u.Design().Texture();

        panel.GetNode<TextureProgress>("HealthBar").Value = QMath.Percantage(u.hp, u.Design().maxHp);

        {
            var energySourcePanel = panel.GetNode<Sprite>("EnergySource");
            var energySourceSlot = energySourcePanel.GetNode<ItemSlotNode>("Slot");
            energySourceSlot.Reset(u, true);
            if (u.energySourceName != "None") {
                var itemNode = DraggableItemNode.New(energySourceSlot, u.GetEnergySource());
                energySourceSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = energySourcePanel.GlobalPosition;
            }
        }

        {
            var shieldPanel = panel.GetNode<Sprite>("Shield");
            var shieldSlot = shieldPanel.GetNode<ItemSlotNode>("Slot");
            shieldSlot.Reset(u, true);
            if (u.Shield() != EmptyShield.Design) {
                var itemNode = DraggableItemNode.New(shieldSlot, u.Shield());
                shieldSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = shieldPanel.GlobalPosition;
            }
        }

        {
            var specialWeaponPanel = panel.GetNode<Sprite>("SpecialWeapon");
            var specialWeaponSlot = specialWeaponPanel.GetNode<ItemSlotNode>("Slot");
            specialWeaponPanel.Frame = u.Design().specialSlot ? 1 : 0;
            specialWeaponSlot.Reset(u, u.Design().specialSlot);
            if (u.Design().specialSlot && u.SpecialWeapon() != EmptyWeapon.Design) {
                var itemNode = DraggableItemNode.New(specialWeaponSlot, u.SpecialWeapon());
                specialWeaponSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = specialWeaponPanel.GlobalPosition;
            }
        }

        for (int j = 0; j < u.weapons.Count; j++) {
            bool slotAvailable = j < u.Design().weaponSlots;
            var weaponPanel = panel.GetNode<Sprite>($"Weapon{j}");
            var weaponSlot = weaponPanel.GetNode<ItemSlotNode>("Slot");
            var w = u.weapons[j];
            weaponPanel.Frame = slotAvailable ? 1 : 0;
            weaponSlot.Reset(u, slotAvailable);
            if (!slotAvailable) {
                continue;
            }
            if (w == EmptyWeapon.Design.name) {
                continue;
            }

            var itemNode = DraggableItemNode.New(weaponSlot, WeaponDesign.Find(w));
            weaponSlot.ApplyItem(null, itemNode);
            GetTree().CurrentScene.AddChild(itemNode);
            itemNode.GlobalPosition = weaponPanel.GlobalPosition;
        }

        for (int j = 0; j < u.artifacts.Count; j++) {
            bool slotAvailable = j < u.Design().artifactSlots;
            var artifactPanel = panel.GetNode<Sprite>($"Artifact{j}");
            var artifactSlot = artifactPanel.GetNode<ItemSlotNode>("Slot");
            var art = u.artifacts[j];
            artifactPanel.Frame = slotAvailable ? 1 : 0;
            artifactSlot.Reset(u, slotAvailable);

            if (!slotAvailable) {
                continue;
            }
            if (art == EmptyArtifact.Design.name) {
                continue;
            }

            var itemNode = DraggableItemNode.New(artifactSlot, ArtifactDesign.Find(art));
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
        for (int itemIndex = 0; itemIndex < _gameState.enteredBase.shopSelection.Count; itemIndex++) {
            var item = _gameState.enteredBase.shopSelection[itemIndex];
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
        var item = _gameState.enteredBase.shopSelection[itemIndex];
        _selectedMerchandise.sprite = _equipmentSlots[merchIndex];
        _selectedMerchandise.item = item;
        _selectedMerchandise.sprite.Frame = 2;

        GetNode<Label>("EquipmentInfo/InfoBoxMerchandise/Body").Text = item.RenderHelp();

        UpdateUI();
    }

    private void OnShopBuyButton() {
        var item = _selectedMerchandise.item;
        var price = ItemInfo.BuyingPrice(item);
        if (price > _gameState.credits) {
            return;
        }
        _gameState.credits -= price;
        PlayMoneySound();
        for (int i = 0; i < _gameState.storage.Length; i++) {
            if (_gameState.storage[i] == null) {
                _gameState.storage[i] = item;

                var storagePanel = GetNode<Sprite>($"Storage/Item{i}");
                var itemSlot = storagePanel.GetNode<ItemSlotNode>("Slot");
                itemSlot.Reset(null, true);
                var itemNode = DraggableItemNode.New(itemSlot, _gameState.storage[i]);
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
        GetNode<Label>("EquipmentInfo/InfoBoxOwn/Body").Text = WeaponDesign.Find(_selectedVessel.weapons[weaponIndex]).RenderHelp();
    }

    private void UpdateUI() {
        var starBase = _gameState.enteredBase;

        _dronesPopup.GetNode<Label>("ControlLimitValue").Text = _gameState.dronwsOwned + "/" + RpgGameState.MaxDrones();

        GetNode<Label>("Status/CreditsValue").Text = _gameState.credits.ToString();
        GetNode<Label>("Status/FuelValue").Text = ((int)_gameState.fuel).ToString() + "/" + RpgGameState.MaxFuel().ToString();
        GetNode<Label>("Status/CargoValue").Text = _gameState.humanUnit.CargoSize() + "/" + _gameState.humanUnit.CargoCapacity();

        GetNode<TextureProgress>("UnitMenu/HealthBar").Value = QMath.Percantage(_selectedVessel.hp, _selectedVessel.Design().maxHp);

        GetNode<Button>("Status/RepairButton").Disabled = _selectedVessel.hp == _selectedVessel.Design().maxHp;

        var missingFuel = (int)(RpgGameState.MaxFuel() - _gameState.fuel);
        _refuelPopup.GetNode<Button>("BuyMinor").Disabled = _gameState.fuel + 50 > RpgGameState.MaxFuel();
        _refuelPopup.GetNode<Label>("BuyFull/Label").Text = (_gameState.fuelPrice * missingFuel).ToString() + " cr";

        _dronesPopup.GetNode<Label>("BuyDrone/Value").Text = _gameState.drones.ToString();
        _dronesPopup.GetNode<Button>("BuyDrone").Disabled = !CanBuyDrone();

        _cargoPopup.GetNode<Label>("SellDebris/Value").Text = _gameState.humanUnit.DebrisCount().ToString();
        _cargoPopup.GetNode<Label>("SellDebris/Price").Text = RpgGameState.DebrisSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Value").Text = _gameState.humanUnit.cargo.minerals.ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Price").Text = RpgGameState.MineralsSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellMinerals/Stock").Text = starBase.mineralsStock.ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Value").Text = _gameState.humanUnit.cargo.organic.ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Price").Text = RpgGameState.OrganicSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellOrganic/Stock").Text = starBase.organicStock.ToString();
        _cargoPopup.GetNode<Label>("SellPower/Value").Text = _gameState.humanUnit.cargo.power.ToString();
        _cargoPopup.GetNode<Label>("SellPower/Price").Text = RpgGameState.PowerSellPrice().ToString();
        _cargoPopup.GetNode<Label>("SellPower/Stock").Text = starBase.powerStock.ToString();

        GetNode<Button>("EquipmentShop/Buy").Disabled = _selectedMerchandise.sprite == null ||
            ItemInfo.BuyingPrice(_selectedMerchandise.item) > _gameState.credits;
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

    private bool CanBuyDrone() {
        return _gameState.credits >= _gameState.dronePrice && _gameState.dronwsOwned < RpgGameState.MaxDrones();
    }

    private void PlayMoneySound() {
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
    }

    private void OnSellEquipmentConfirmButton() {
        _sellItemNode.QueueFree();
        _sellItemSlot.MakeEmpty();

        var sellingPrice = ItemInfo.SellingPrice(_sellItemNode.item) / 2;
        _gameState.credits += sellingPrice;
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

        _dronesPopup.GetNode<Label>("BuyDrone/Value").Text = _gameState.drones.ToString();
        _dronesPopup.GetNode<Label>("BuyDrone/Price").Text = "(" + _gameState.dronePrice + " cr/unit)";

        _lockControls = true;
        _dronesPopup.PopupCentered();
    }

    private void OnDronesBuyButton() {
        if (_gameState.credits < _gameState.dronePrice) {
            return;
        }
        _gameState.drones++;
        _gameState.dronwsOwned++;
        _gameState.credits -= _gameState.dronePrice;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnDronesDoneButton() {
        _dronesPopup.Hide();
        _lockControls = false;
    }

    private void OnCargoSellDebrisButton() {
        var cargo = _gameState.humanUnit.cargo;

        _gameState.credits += RpgGameState.DebrisSellPrice() * _gameState.humanUnit.DebrisCount();
        _gameState.krigiaMaterial += cargo.krigiaDeris;
        _gameState.wertuMaterial += cargo.wertuDebris;
        _gameState.zythMaterial += cargo.zythDebris;

        cargo.genericDebris = 0;
        cargo.krigiaDeris = 0;
        cargo.wertuDebris = 0;
        cargo.zythDebris = 0;

        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellMineralsButton() {
        _gameState.credits += RpgGameState.MineralsSellPrice() * _gameState.humanUnit.cargo.minerals;
        _gameState.enteredBase.mineralsStock += _gameState.humanUnit.cargo.minerals;
        _gameState.humanUnit.cargo.minerals = 0;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellOrganicButton() {
        _gameState.credits += RpgGameState.OrganicSellPrice() * _gameState.humanUnit.cargo.organic;
        _gameState.enteredBase.organicStock += _gameState.humanUnit.cargo.organic;
        _gameState.humanUnit.cargo.organic = 0;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnCargoSellPowerButton() {
        _gameState.credits += RpgGameState.PowerSellPrice() * _gameState.humanUnit.cargo.power;
        _gameState.enteredBase.powerStock += _gameState.humanUnit.cargo.power;
        _gameState.humanUnit.cargo.power = 0;
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

        var missingHp = (int)(_selectedVessel.Design().maxHp - _selectedVessel.hp);
        _repairPopup.GetNode<Label>("RepairFull/Label").Text = (_gameState.repairPrice * missingHp).ToString() + " cr";
        _repairPopup.GetNode<Button>("RepairMinor").Disabled = _selectedVessel.hp + 25 > _selectedVessel.Design().maxHp;
        _repairPopup.GetNode<Label>("RepairMinor/Label").Text = (_gameState.repairPrice * 25).ToString() + " cr";

        _lockControls = true;
        _repairPopup.PopupCentered();
    }

    private void OnFullRepairButton() {
        _repairPopup.Hide();
        _lockControls = false;
        var missingHp = (int)(_selectedVessel.Design().maxHp - _selectedVessel.hp);
        var price = _gameState.repairPrice * missingHp;
        if (_gameState.credits < price) {
            return;
        }
        _selectedVessel.hp = _selectedVessel.Design().maxHp;
        _gameState.credits -= price;
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/repair.wav")));
        UpdateUI();
    }

    private void OnMinorRepairButton() {
        _repairPopup.Hide();
        _lockControls = false;
        var price = _gameState.repairPrice * 25;
        if (_gameState.credits < price) {
            return;
        }
        _selectedVessel.hp += 25;
        _gameState.credits -= price;
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
        var missingFuel = (int)(RpgGameState.MaxFuel() - _gameState.fuel);
        var price = _gameState.fuelPrice * missingFuel;
        if (_gameState.credits < price) {
            return;
        }
        _gameState.fuel = RpgGameState.MaxFuel();
        _gameState.credits -= price;
        PlayMoneySound();
        UpdateUI();

        _refuelPopup.Hide();
        _lockControls = false;
    }

    private void OnBuyMinorFuelButton() {
        var price = _gameState.fuelPrice * 50;
        if (_gameState.credits < price) {
            return;
        }
        _gameState.fuel += 50;
        _gameState.credits -= price;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnLeavePressed() {
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
