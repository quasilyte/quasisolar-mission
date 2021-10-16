using Godot;
using System;
using System.Collections.Generic;

public class EquipmentShopScreen : Node2D {
    private bool _lockControls = false;
    private Popup _activePopup;

    private Popup _repairPopup;
    private CargoMenuPopupNode _cargoPopup;
    private DronesShopPopupNode _dronesPopup;
    private Popup _sellEquipmentPopup;

    private RpgGameState _gameState;

    private SpaceUnit _humanUnit;
    private StarBase _starBase;

    private List<IItem> _shopSelection;
    private List<ItemSlotNode> _shopSlots = new List<ItemSlotNode>();
    private List<ItemSlotNode> _storageSlots = new List<ItemSlotNode>();

    private ItemSlotController _itemSlotController = new ItemSlotController();

    private Button _buySell;

    private OptionButton _shopCategorySelect;

    private Vessel _selectedVessel;

    private static string[] _equipmentCategories = new string[]{
        "Weapon",
        "Special Weapon",
        "Energy Source",
        "Shield",
        "Sentinel",
        "Artifact",
    };

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _humanUnit = RpgGameState.instance.humanUnit.Get();
        _starBase = RpgGameState.enteredBase;

        _shopSelection = RpgGameState.enteredBase.ShopSelection();
        _buySell = GetNode<Button>("EquipmentShop/BuySell");

        SetupUI();

        GetNode<TextureButton>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeavePressed));
        GetNode<Button>("Status/RefuelButton").Connect("pressed", this, nameof(OnRefuelButton));
        GetNode<Button>("Status/RepairAllButton").Connect("pressed", this, nameof(OnRepairAllButton));
        GetNode<Button>("Status/CargoButton").Connect("pressed", this, nameof(OnCargoButton));
        GetNode<Button>("Status/DronesButton").Connect("pressed", this, nameof(OnDronesButton));
        GetNode<Button>("Status/SellDebris").Connect("pressed", this, nameof(OnSellDebrisButton));

        GetNode<Button>("UnitMenu/RepairButton").Connect("pressed", this, nameof(OnRepairButton));

        _shopCategorySelect = GetNode<OptionButton>("EquipmentShop/CategorySelect");
        for (int i = 0; i < _equipmentCategories.Length; i++) {
            _shopCategorySelect.AddItem(_equipmentCategories[i], i);
        }
        _shopCategorySelect.Connect("item_selected", this, nameof(OnShopCategorySelected));

        _sellEquipmentPopup = GetNode<Popup>("SellEquipmentPopup");
        _sellEquipmentPopup.GetNode<Button>("Confirm").Connect("pressed", this, nameof(OnSellEquipmentConfirmButton));
        _sellEquipmentPopup.GetNode<Button>("Cancel").Connect("pressed", this, nameof(OnSellEquipmentCancelButton));

        _repairPopup = GetNode<Popup>("RepairPopup");
        _repairPopup.GetNode<Button>("RepairFull").Connect("pressed", this, nameof(OnFullRepairButton));
        _repairPopup.GetNode<Button>("RepairMinor").Connect("pressed", this, nameof(OnMinorRepairButton));
        _repairPopup.GetNode<Button>("Cancel").Connect("pressed", this, nameof(OnRepairCancelButton));

        SetupCargoPopup();
        SetupDronesPopup();
        
        for (int i = 0; i < _humanUnit.fleet.Count; i++) {
            var u = _humanUnit.fleet[i];
            if (u.id == 0) {
                break;
            }
            var panel = GetNode<Sprite>($"UnitMenu/Unit{i}");
            var button = new TextureButton();
            button.RectSize = new Vector2(112, 112);
            button.Expand = true;
            button.StretchMode = TextureButton.StretchModeEnum.KeepCentered;
            button.TextureNormal = u.Get().Design().Texture();
            panel.AddChild(button);
            button.Connect("pressed", this, nameof(OnMemberSelected), new Godot.Collections.Array { i });
        }

        SelectMember(0);
        SelectShopCategory("Weapon");

        UpdateUI();
    }

    private void GoBackAction() {
        if (_activePopup != null && _lockControls) {
            HidePopup(_activePopup);
            return;
        }
        OnLeavePressed();
    }

    private void HidePopup(Popup p) {
        if (p != _activePopup) {
            return;
        }
        _lockControls = false;
        p.Hide();
        _activePopup = null;
    }

    private void ShowPopup(Popup p) {
        if (_lockControls) {
            return;
        }
        p.PopupCentered();
        _lockControls = true;
        _activePopup = p;
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            GoBackAction();
            return;
        }
    }

    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("escape")) {
            GoBackAction();
        }
    }

    private void SetupCargoPopup() {
        _cargoPopup = CargoMenuPopupNode.New();
        AddChild(_cargoPopup);

        _cargoPopup.GetNode<ButtonNode>("Minerals/LoadButton").Connect("pressed", this, nameof(OnLoadMineralsButton));
        _cargoPopup.GetNode<ButtonNode>("Organic/LoadButton").Connect("pressed", this, nameof(OnLoadOrganicButton));
        _cargoPopup.GetNode<ButtonNode>("Power/LoadButton").Connect("pressed", this, nameof(OnLoadPowerButton));

        _cargoPopup.GetNode<ButtonNode>("Minerals/UnloadButton").Connect("pressed", this, nameof(OnUnloadMineralsButton));
        _cargoPopup.GetNode<ButtonNode>("Organic/UnloadButton").Connect("pressed", this, nameof(OnUnloadOrganicButton));
        _cargoPopup.GetNode<ButtonNode>("Power/UnloadButton").Connect("pressed", this, nameof(OnUnloadPowerButton));

        _cargoPopup.GetNode<ButtonNode>("UnloadAllButton").Connect("pressed", this, nameof(OnCargoUnloadAllButton));

        _cargoPopup.GetNode<Button>("DoneButton").Connect("pressed", this, nameof(OnCargoDoneButton));

        UpdateCargoPopup();
    }

    private void OnCargoUnloadAllButton() {
        UnloadMinerals(999999);
        UnloadOrganic(999999);
        UnloadPower(999999);
        UpdateCargoPopup();
    }

    private void UnloadMinerals(int maxAmount) {
        var amount = Math.Min(maxAmount, _humanUnit.cargo.minerals);
        amount = Math.Min(amount, _starBase.StorageFreeSpace());
        _humanUnit.cargo.minerals -= amount;
        _starBase.mineralsStock += amount;
    }

    private void UnloadOrganic(int maxAmount) {
        var amount = Math.Min(maxAmount, _humanUnit.cargo.organic);
        amount = Math.Min(amount, _starBase.StorageFreeSpace());
        _humanUnit.cargo.organic -= amount;
        _starBase.organicStock += amount;
    }

    private void UnloadPower(int maxAmount) {
        var amount = Math.Min(maxAmount, _humanUnit.cargo.power);
        amount = Math.Min(amount, _starBase.StorageFreeSpace());
        _humanUnit.cargo.power -= amount;
        _starBase.powerStock += amount;
    }

    private void OnUnloadMineralsButton() {
        UnloadMinerals(50);
        UpdateCargoPopup();
    }

    private void OnUnloadOrganicButton() {
        UnloadOrganic(50);
        UpdateCargoPopup();
    }

    private void OnUnloadPowerButton() {
        UnloadPower(50);
        UpdateCargoPopup();
    }

    private void OnLoadMineralsButton() {
        var amount = Math.Min(50, _starBase.mineralsStock);
        amount = Math.Min(amount, _humanUnit.CargoFree());
        _starBase.mineralsStock -= amount;
        _humanUnit.cargo.minerals += amount;
        UpdateCargoPopup();
    }

    private void OnLoadOrganicButton() {
        var amount = Math.Min(50, _starBase.organicStock);
        amount = Math.Min(amount, _humanUnit.CargoFree());
        _starBase.organicStock -= amount;
        _humanUnit.cargo.organic += amount;
        UpdateCargoPopup();
    }

    private void OnLoadPowerButton() {
        var amount = Math.Min(50, _starBase.powerStock);
        amount = Math.Min(amount, _humanUnit.CargoFree());
        _starBase.powerStock -= amount;
        _humanUnit.cargo.power += amount;
        UpdateCargoPopup();
    }

    private void UpdateCargoPopup() {
        _cargoPopup.GetNode<Label>("Minerals/FleetValue").Text = _humanUnit.cargo.minerals.ToString();
        _cargoPopup.GetNode<Label>("Organic/FleetValue").Text = _humanUnit.cargo.organic.ToString();
        _cargoPopup.GetNode<Label>("Power/FleetValue").Text = _humanUnit.cargo.power.ToString();

        _cargoPopup.GetNode<Label>("Minerals/StockValue").Text = _starBase.mineralsStock.ToString();
        _cargoPopup.GetNode<Label>("Organic/StockValue").Text = _starBase.organicStock.ToString();
        _cargoPopup.GetNode<Label>("Power/StockValue").Text = _starBase.powerStock.ToString();

        _cargoPopup.GetNode<Label>("FleetFreeSpaceValue").Text = _humanUnit.CargoFree().ToString();
        _cargoPopup.GetNode<Label>("StockFreeSpaceValue").Text = _starBase.StorageFreeSpace().ToString();
    }

    private void SetupDronesPopup() {
        _dronesPopup = DronesShopPopupNode.New();
        AddChild(_dronesPopup);
        _dronesPopup.GetNode<Button>("DoneButton").Connect("pressed", this, nameof(OnDronesDoneButton));

        DroneInfoBoxClear();

        for (int i = 0; i < 3; i++) {
            var label = _dronesPopup.GetNode<Label>($"FleetDronesBox/Slot{i}");
            label.Connect("mouse_entered", this, nameof(OnFleetDroneMouseEnter), new Godot.Collections.Array { i });
            label.Connect("mouse_exited", this, nameof(DroneInfoBoxClear));
            label.GetNode<ButtonNode>("SellButton").Connect("pressed", this, nameof(OnFleetDroneSellPressed), new Godot.Collections.Array { i });
        }

        var dronesForSale = new List<ExplorationDrone>();
        foreach (var drone in ExplorationDrone.list) {
            if (drone.needsResearch && !_gameState.technologiesResearched.Contains(drone.name)) {
                continue;
            }
            dronesForSale.Add(drone);
        }

        var box = _dronesPopup.GetNode<Panel>("DroneSelectionBox");
        var offsetX = 96;
        var offsetY = 32;
        foreach (var drone in dronesForSale) {
            var droneLabel = new Label();
            droneLabel.Text = drone.name;
            droneLabel.RectSize = new Vector2(256, 32);
            droneLabel.Valign = Label.VAlign.Center;
            droneLabel.RectPosition = new Vector2(offsetX, offsetY);
            droneLabel.MouseFilter = Control.MouseFilterEnum.Stop;
            box.AddChild(droneLabel);

            droneLabel.Connect("mouse_entered", this, nameof(OnDroneSelectionMouseEnter), new Godot.Collections.Array { drone.name });
            droneLabel.Connect("mouse_exited", this, nameof(DroneInfoBoxClear));

            var buyButton = ButtonNode.New();
            buyButton.Text = "$";
            buyButton.RectSize = new Vector2(64, 64);
            buyButton.RectPosition = new Vector2(-80, -16);
            buyButton.Connect("pressed", this, nameof(OnDroneBuyPressed), new Godot.Collections.Array { drone.name });
            droneLabel.AddChild(buyButton);

            offsetY += 80;
        }
    }

    private void DroneInfoBoxClear() {
        var infoLabel = _dronesPopup.GetNode<Label>("DroneInfoBox/Text");
        infoLabel.Text = "";
    }

    private string GetDroneInfoText(ExplorationDrone drone) {
        var infoLines = new List<string>();
        infoLines.Add(drone.name + " (" + drone.sellingPrice + ")");
        infoLines.Add("");
        infoLines.Add("Max temperature: " + drone.maxTemp);
        infoLines.Add("Exploration rate: " + drone.explorationRate);
        infoLines.Add("Planet types: " + (drone.canExploreGasGiants ? "any" : "rocky"));
        return string.Join("\n", infoLines);
    }

    private void OnDroneBuyPressed(string droneName) {
        var buyingPrice = ExplorationDrone.Find(droneName).sellingPrice;
        if (_gameState.credits < buyingPrice) {
            return;
        }
        if (_gameState.explorationDrones.Count >= RpgGameState.MaxExplorationDrones()) {
            return;
        }

        _gameState.explorationDrones.Add(droneName);
        _gameState.credits -= buyingPrice;

        PlayMoneySound();
        UpdateUI();
    }

    private void OnDroneSelectionMouseEnter(string droneName) {
        var drone = ExplorationDrone.Find(droneName);
        var infoLabel = _dronesPopup.GetNode<Label>("DroneInfoBox/Text");
        infoLabel.Text = GetDroneInfoText(drone);
    }

    private void OnFleetDroneSellPressed(int i) {
        var sellingPrice = ExplorationDrone.Find(_gameState.explorationDrones[i]).sellingPrice / 2;
        _gameState.explorationDrones.RemoveAt(i);
        _gameState.credits += sellingPrice;

        PlayMoneySound();
        UpdateUI();
    }

    private void OnFleetDroneMouseEnter(int i) {
        var drone = ExplorationDrone.Find(_gameState.explorationDrones[i]);
        var infoLabel = _dronesPopup.GetNode<Label>("DroneInfoBox/Text");
        infoLabel.Text = GetDroneInfoText(drone);
    }

    private void SetupUI() {
        var panel = GetNode<Panel>("UnitMenu");

        for (int i = 0; i < 2; i++) {
            var weaponSlot = panel.GetNode<ItemSlotNode>($"Weapon{i}");
            _itemSlotController.AddSlot(weaponSlot);
            weaponSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.weapons[index] = item != null ? ((WeaponDesign)item).name : EmptyWeapon.Design.name;
            });
            weaponSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{weaponSlot});
        }
        {
            var weaponSlot = panel.GetNode<ItemSlotNode>("SpecialWeapon");
            _itemSlotController.AddSlot(weaponSlot);
            weaponSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.specialWeaponName = item != null ? ((WeaponDesign)item).name : EmptyWeapon.Design.name;
            });
            weaponSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{weaponSlot});
        }

        {
            var energySourceSlot = panel.GetNode<ItemSlotNode>("EnergySource");
            _itemSlotController.AddSlot(energySourceSlot);
            energySourceSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.energySourceName = item != null ? ((EnergySource)item).name : "None";
            });
            energySourceSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{energySourceSlot});
        }

        {
            var shieldSlot = panel.GetNode<ItemSlotNode>("Shield");
            _itemSlotController.AddSlot(shieldSlot);
            shieldSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.shieldName = item != null ? ((ShieldDesign)item).name : EmptyShield.Design.name;
            });
            shieldSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{shieldSlot});
        }

        {
            var sentinelSlot = panel.GetNode<ItemSlotNode>("Sentinel");
            _itemSlotController.AddSlot(sentinelSlot);
            sentinelSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.sentinelName = item != null ? ((SentinelDesign)item).name : "Empty";
            });
            sentinelSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{sentinelSlot});
        }

        for (int i = 0; i < 5; i++) {
            var artifactSlot = panel.GetNode<ItemSlotNode>($"Artifact{i}");
            _itemSlotController.AddSlot(artifactSlot);
            artifactSlot.SetAssignItemCallback((int index, IItem item) => {
                _selectedVessel.artifacts[index] = item != null ? ((ArtifactDesign)item).name : EmptyArtifact.Design.name;
            });
            artifactSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{artifactSlot});
        }

        var itemStorageGrid = GetNode<GridContainer>("Storage/ScrollContainer/GridContainer");
        for (int i = 0; i < _gameState.ItemStorageCapacity(); i++) {
            var itemSlot = _itemSlotController.NewSlot(i, ItemKind.Storage);
            _itemSlotController.AddSlot(itemSlot);
            itemStorageGrid.AddChild(itemSlot);
            itemSlot.SetAssignItemCallback((int index, IItem item) => {
                _gameState.PutItemToStorage(item, index);
            });
            itemSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{itemSlot});
            itemSlot.Reset(null, true);

            if (_gameState.storage[i] != null) {
                itemSlot.ApplyItem(_gameState.storage[i].ToItem());
            }
            _storageSlots.Add(itemSlot);
        }

        var itemShopGrid = GetNode<GridContainer>("EquipmentShop/ScrollContainer/GridContainer");
        for (int i = 0; i < 30; i++) {
            var itemSlot = _itemSlotController.NewSlot(i, ItemKind.Shop);
            _itemSlotController.AddSlot(itemSlot);
            itemShopGrid.AddChild(itemSlot);
            itemSlot.Connect("Clicked", this, nameof(OnItemClicked), new Godot.Collections.Array{itemSlot});
            itemSlot.Reset(null, true);
            _shopSlots.Add(itemSlot);
        }

        _buySell.Connect("pressed", this, nameof(OnShopBuySellButton));
    }

    private void OnItemClicked(ItemSlotNode itemSlot) {
        _itemSlotController.OnItemClicked(itemSlot);

        if (_itemSlotController.selected == null) {
            _buySell.Disabled = true;
        } else if (_itemSlotController.selected.GetItemKind() == ItemKind.Shop) {
            _buySell.Disabled = ItemInfo.SellingPrice(_itemSlotController.selected.GetItem()) > _gameState.credits;
            _buySell.Text = "Buy";
        } else {
            _buySell.Text = "Sell";
            _buySell.Disabled = false;
        }

        var infoBox = GetNode<Label>("EquipmentInfo/InfoBox/Body");
        if (_itemSlotController.selected == null) {
            infoBox.Text = "";
        } else {
            infoBox.Text = ItemInfo.RenderHelp(_itemSlotController.selected.GetItem());
        }
    }

    private void SelectMember(int vesselIndex) {
        var panel = GetNode<Panel>("UnitMenu");

        for (int i = 0; i < _humanUnit.fleet.Count; i++) {
            GetNode<Sprite>($"UnitMenu/Unit{i}").Frame = 0;
        }

        var unitPanel = GetNode<Sprite>($"UnitMenu/Unit{vesselIndex}");
        unitPanel.Frame = 1;

        var u = _humanUnit.fleet[vesselIndex].Get();
        _selectedVessel = u;

        panel.GetNode<Sprite>("VesselDesign/Sprite").Texture = u.Design().Texture();

        panel.GetNode<TextureProgress>("HealthBar").Value = QMath.Percantage(u.hp, u.MaxHp());

        {
            var energySourceSlot = panel.GetNode<ItemSlotNode>("EnergySource");
            energySourceSlot.Reset(u, true);
            if (u.energySourceName != "None") {
                energySourceSlot.ApplyItem(u.GetEnergySource());
            }
        }

        {
            var shieldSlot = panel.GetNode<ItemSlotNode>("Shield");
            var canUseShield = u.Design().maxShieldLevel != 0;
            shieldSlot.Reset(u, canUseShield);
            if (canUseShield && u.Shield() != EmptyShield.Design) {
                shieldSlot.ApplyItem(u.Shield());
            }
        }

        {
            var sentinelSlot = panel.GetNode<ItemSlotNode>("Sentinel");
            sentinelSlot.Reset(u, u.Design().sentinelSlot);
            if (u.Design().sentinelSlot && u.sentinelName != "Empty") {
                sentinelSlot.ApplyItem(u.Sentinel());
            }
        }

        {
            var specialWeaponSlot = panel.GetNode<ItemSlotNode>("SpecialWeapon");
            specialWeaponSlot.Reset(u, u.Design().specialSlot);
            if (u.Design().specialSlot && u.SpecialWeapon() != EmptyWeapon.Design) {
                specialWeaponSlot.ApplyItem(u.SpecialWeapon());
            }
        }

        for (int j = 0; j < u.weapons.Count; j++) {
            bool slotAvailable = j < u.Design().weaponSlots;
            var weaponSlot = panel.GetNode<ItemSlotNode>($"Weapon{j}");
            var w = u.weapons[j];
            weaponSlot.Reset(u, slotAvailable);
            if (!slotAvailable) {
                continue;
            }
            if (w != EmptyWeapon.Design.name) {
                weaponSlot.ApplyItem(WeaponDesign.Find(w));
            }
        }

        for (int j = 0; j < u.artifacts.Count; j++) {
            bool slotAvailable = j < u.Design().artifactSlots;
            var artifactSlot = panel.GetNode<ItemSlotNode>($"Artifact{j}");
            var art = u.artifacts[j];
            artifactSlot.Reset(u, slotAvailable);

            if (!slotAvailable) {
                continue;
            }
            if (art == EmptyArtifact.Design.name) {
                continue;
            }

            artifactSlot.ApplyItem(ArtifactDesign.Find(art));
        }
    }

    private void SelectShopCategory(string category) {
        foreach (var slot in _shopSlots) {
            slot.Reset(null, true);
        }

        int i = 0;
        category = category.Replace(" ", ""); // "Special Weapon" -> "SpecialWeapon"
        for (int itemIndex = 0; itemIndex < _shopSelection.Count; itemIndex++) {
            var item = _shopSelection[itemIndex];
            if (item.GetItemKind().ToString() != category) {
                continue;
            }
            var itemNode = _shopSlots[i];
            itemNode.AssignItem(item);
            i++;
        }
    }

    private ItemSlotNode SelectedShopSlot() {
        foreach (var slot in _shopSlots) {
            if (slot.IsEmpty()) {
                break;
            }
            if (slot.IsSelected()) {
                return slot;
            }
        }
        return null;
    }

    private void OnItemBuy() {
        var item = _itemSlotController.selected.GetItem();
        var price = ItemInfo.BuyingPrice(item);
        if (price > _gameState.credits) {
            return;
        }
        _gameState.credits -= price;
        PlayMoneySound();

        var i = _gameState.StorageFreeSlot();
        _gameState.PutItemToStorage(item, i);
        _storageSlots[i].ApplyItem(item);
    
        _buySell.Disabled = ItemInfo.SellingPrice(_itemSlotController.selected.GetItem()) > _gameState.credits;

        UpdateUI();
    }

    private void OnItemSell() {
        var item = _itemSlotController.selected.GetItem();
        var itemName = ItemInfo.Name(item);
        _sellEquipmentPopup.GetNode<Label>("Title").Text = "Sell " + itemName + "?";
        var sellingPrice = ItemInfo.SellingPrice(item) / 2;
        _sellEquipmentPopup.GetNode<Label>("SellingPrice").Text = $"{sellingPrice} RU";
        
        ShowPopup(_sellEquipmentPopup);
    }

    private void OnShopBuySellButton() {
        if (_itemSlotController.selected == null) {
            return;
        }
        if (_buySell.Text == "Sell") {
            OnItemSell();
        } else {
            OnItemBuy();
        }
    }

    private void OnShopCategorySelected(int id) {
        SelectShopCategory(_equipmentCategories[id]);
        UpdateUI();
    }

    private void OnMemberSelected(int i) {
        SelectMember(i);
        UpdateUI();
    }

    private void UpdateUI() {
        var numDrones = _gameState.explorationDrones.Count;
        var maxDrones = RpgGameState.MaxExplorationDrones();
        _dronesPopup.GetNode<Label>("FleetDronesTitle").Text = $"Fleet Drones ({numDrones}/{maxDrones})";
        for (int i = 0; i < 3; i++) {
            var label = _dronesPopup.GetNode<Label>($"FleetDronesBox/Slot{i}");
            if (i < _gameState.explorationDrones.Count) {
                label.Visible = true;
                label.Text = _gameState.explorationDrones[i];
            } else {
                label.Visible = false;
            }
        }

        GetNode<Label>("Status/CreditsValue").Text = _gameState.credits.ToString();
        GetNode<Label>("Status/FuelValue").Text = ((int)_gameState.fuel).ToString() + "/" + RpgGameState.MaxFuel().ToString();
        GetNode<Label>("Status/CargoValue").Text = _humanUnit.CargoSize() + "/" + _humanUnit.CargoCapacity();

        GetNode<TextureProgress>("UnitMenu/HealthBar").Value = QMath.Percantage(_selectedVessel.hp, _selectedVessel.MaxHp());
        GetNode<Label>("UnitMenu/HealthBar/Value").Text = (int)_selectedVessel.hp +"/"+ (int)_selectedVessel.MaxHp();

        GetNode<Button>("UnitMenu/RepairButton").Disabled = _selectedVessel.hp == _selectedVessel.MaxHp();

        GetNode<Button>("Status/RefuelButton").Disabled = _gameState.fuel == RpgGameState.MaxFuel();
    }

    private void PlayMoneySound() {
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
    }

    private void OnSellEquipmentConfirmButton() {
        var item = _itemSlotController.selected.GetItem();
        var sellingPrice = ItemInfo.SellingPrice(item) / 2;
        _gameState.credits += sellingPrice;
        UpdateUI();

        PlayMoneySound();

        _itemSlotController.ClearSelectedSlot();
        GetNode<Label>("EquipmentInfo/InfoBox/Body").Text = "";

        HidePopup(_sellEquipmentPopup);
    }

    private void OnSellEquipmentCancelButton() { HidePopup(_sellEquipmentPopup); }

    private void OnDronesButton() { ShowPopup(_dronesPopup); }
    private void OnDronesDoneButton() { HidePopup(_dronesPopup); }

    private void OnSellDebrisButton() {
        if (_lockControls) {
            return;
        }

        if (_humanUnit.DebrisCount() != 0) {
            PlayMoneySound();
        }
        SellDebris();
        UpdateUI();
    }

    private void SellDebris() {
        var cargo = _humanUnit.cargo;

        _gameState.credits += RpgGameState.enteredBase.DebrisSellPrice().value * _humanUnit.DebrisCount();
        _gameState.researchMaterial.Add(cargo.debris);

        cargo.debris = new DebrisContainer();
    }

    private void OnCargoButton() { ShowPopup(_cargoPopup); }

    private void OnCargoDoneButton() {
        HidePopup(_cargoPopup);
        UpdateUI();
    }

    private void OnRepairCancelButton() {
        HidePopup(_repairPopup);
    }

    private void OnRepairAllButton() {
        if (_lockControls) {
            return;
        }

        var repairedSome = false;

        foreach (var x in _gameState.humanUnit.Get().fleet) {
            var v = x.Get();
            var missingHp = (int)(v.MaxHp() - v.hp);
            if (missingHp == 0) {
                continue;
            }
            var repairPrice = RpgGameState.enteredBase.RepairPrice(v);
            var repairAmount = QMath.ClampMax(missingHp, _gameState.credits / repairPrice);
            if (repairAmount == 0) {
                continue;
            }
            repairedSome = true;
            if (repairAmount == missingHp) {
                v.hp = v.MaxHp();
            } else {
                v.hp += repairAmount;
            }
            _gameState.credits -= repairAmount * repairPrice;
        }

        if (repairedSome) {
            GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/repair.wav")));
            UpdateUI();
        }
    }

    private void OnRepairButton() {
        if (_lockControls) {
            return;
        }

        var repairPrice = RpgGameState.enteredBase.RepairPrice(_selectedVessel);
        var missingHp = (int)(_selectedVessel.MaxHp() - _selectedVessel.hp);
        _repairPopup.GetNode<Label>("RepairFull/Label").Text = (repairPrice * missingHp).ToString() + " cr";
        _repairPopup.GetNode<Button>("RepairMinor").Disabled = _selectedVessel.hp + 25 > _selectedVessel.MaxHp();
        _repairPopup.GetNode<Label>("RepairMinor/Label").Text = (repairPrice * 25).ToString() + " cr";

        ShowPopup(_repairPopup);
    }

    private void OnFullRepairButton() {
        HidePopup(_repairPopup);

        var missingHp = (int)(_selectedVessel.MaxHp() - _selectedVessel.hp);
        var price = RpgGameState.enteredBase.RepairPrice(_selectedVessel) * missingHp;
        if (_gameState.credits < price) {
            return;
        }
        _selectedVessel.hp = _selectedVessel.MaxHp();
        _gameState.credits -= price;
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/interface/repair.wav")));
        UpdateUI();
    }

    private void OnMinorRepairButton() {
        HidePopup(_repairPopup);
        var price = RpgGameState.enteredBase.RepairPrice(_selectedVessel) * 25;
        if (_gameState.credits < price) {
            return;
        }
        _selectedVessel.hp += 25;
        _gameState.credits -= price;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnRefuelButton() {
        if (_lockControls) {
            return;
        }

        var missingFuel = (int)(RpgGameState.MaxFuel() - _gameState.fuel);
        var toBuy = QMath.ClampMax(missingFuel, 50);
        var price = _starBase.FuelPrice() * toBuy;
        if (_gameState.credits < price) {
            return;
        }
        if (toBuy == missingFuel) {
            _gameState.fuel = RpgGameState.MaxFuel();
        } else {
            _gameState.fuel += toBuy;
        }
        _gameState.credits -= price;
        PlayMoneySound();
        UpdateUI();
    }

    private void OnLeavePressed() {
        GetTree().ChangeScene("res://scenes/screens/StarBaseScreen.tscn");
    }
}
