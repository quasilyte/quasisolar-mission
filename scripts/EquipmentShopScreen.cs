using Godot;
using System;
using System.Collections.Generic;

public class EquipmentShopScreen : Node2D {
    class Merchandise {
        public Sprite sprite;
        public IItem item;
    }

    private bool _lockControls = false;

    private Popup _repairPopup;
    private CargoMenuPopupNode _cargoPopup;
    private DronesShopPopupNode _dronesPopup;
    private Popup _sellEquipmentPopup;

    private RpgGameState _gameState;

    private SpaceUnit _humanUnit;
    private StarBase _starBase;

    private List<IItem> _shopSelection;

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
        "Sentinel",
        "Artifact",
    };

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _humanUnit = RpgGameState.instance.humanUnit.Get();
        _starBase = RpgGameState.enteredBase;

        _shopSelection = RpgGameState.enteredBase.ShopSelection();

        SetupUI();

        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeavePressed));
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
            button.RectSize = new Vector2(64, 64);
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
        var offsetX = 64;
        var offsetY = 24;
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
            buyButton.RectSize = new Vector2(32, 32);
            buyButton.RectPosition = new Vector2(-48, 0);
            buyButton.Connect("pressed", this, nameof(OnDroneBuyPressed), new Godot.Collections.Array { drone.name });
            droneLabel.AddChild(buyButton);

            offsetY += 48;
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

        {
            var sentinelPanel = panel.GetNode<Sprite>("Sentinel");
            var sentinelSlot = ItemSlotNode.New(0, ItemKind.Sentinel);
            sentinelSlot.SetAssignItemCallback((int index, DraggableItemNode itemNode) => {
                _selectedVessel.sentinelName = itemNode != null ? ((SentinelDesign)itemNode.item).name : "Empty";
                return true;
            });
            sentinelSlot.Name = "Slot";
            sentinelPanel.AddChild(sentinelSlot);
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
                    _gameState.PutItemToStorage(itemNode != null ? itemNode.item : null, index);
                    return true;
                });
                itemSlot.Reset(null, true);
                itemSlot.Name = "Slot";
                storagePanel.AddChild(itemSlot);

                if (_gameState.storage[i] != null) {
                    var itemNode = DraggableItemNode.New(itemSlot, _gameState.storage[i].ToItem());
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

        for (int i = 0; i < _humanUnit.fleet.Count; i++) {
            GetNode<Sprite>($"UnitMenu/Unit{i}").Frame = 1;
        }

        var unitPanel = GetNode<Sprite>($"UnitMenu/Unit{vesselIndex}");
        unitPanel.Frame = 2;

        var u = _humanUnit.fleet[vesselIndex].Get();
        _selectedVessel = u;

        panel.GetNode<Sprite>("VesselDesign/Sprite").Texture = u.Design().Texture();

        panel.GetNode<TextureProgress>("HealthBar").Value = QMath.Percantage(u.hp, u.MaxHp());

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
            var sentinelPanel = panel.GetNode<Sprite>("Sentinel");
            var sentinelSlot = sentinelPanel.GetNode<ItemSlotNode>("Slot");
            sentinelPanel.Frame = u.Design().sentinelSlot ? 1 : 0;
            sentinelSlot.Reset(u, u.Design().sentinelSlot);
            if (u.Design().sentinelSlot && u.sentinelName != "Empty") {
                var itemNode = DraggableItemNode.New(sentinelSlot, u.Sentinel());
                sentinelSlot.ApplyItem(null, itemNode);
                GetTree().CurrentScene.AddChild(itemNode);
                itemNode.GlobalPosition = sentinelPanel.GlobalPosition;
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
        for (int itemIndex = 0; itemIndex < _shopSelection.Count; itemIndex++) {
            var item = _shopSelection[itemIndex];
            if (item.GetItemKind().ToString() != category) {
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
        var item = _shopSelection[itemIndex];
        _selectedMerchandise.sprite = _equipmentSlots[merchIndex];
        _selectedMerchandise.item = item;
        _selectedMerchandise.sprite.Frame = 2;

        GetNode<Label>("EquipmentInfo/InfoBoxMerchandise/Body").Text = ItemInfo.RenderHelp(item);

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
                _gameState.PutItemToStorage(item, i);

                var storagePanel = GetNode<Sprite>($"Storage/Item{i}");
                var itemSlot = storagePanel.GetNode<ItemSlotNode>("Slot");
                itemSlot.Reset(null, true);
                var itemNode = DraggableItemNode.New(itemSlot, _gameState.storage[i].ToItem());
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
        // _dronesPopup.GetNode<Label>("ControlLimitValue").Text = _gameState.dronesOwned + "/" + RpgGameState.MaxDrones();

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
        GetNode<Label>("UnitMenu/HealthBar/Value").Text = _selectedVessel.hp +"/"+ _selectedVessel.MaxHp();

        GetNode<Button>("UnitMenu/RepairButton").Disabled = _selectedVessel.hp == _selectedVessel.MaxHp();

        GetNode<Button>("Status/RefuelButton").Disabled = _gameState.fuel == RpgGameState.MaxFuel();

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

        _lockControls = true;
        _dronesPopup.PopupCentered();
    }

    private void OnDronesDoneButton() {
        _dronesPopup.Hide();
        _lockControls = false;
    }

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

        _lockControls = true;
        _repairPopup.PopupCentered();
    }

    private void OnFullRepairButton() {
        _repairPopup.Hide();
        _lockControls = false;
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
        _repairPopup.Hide();
        _lockControls = false;
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
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
