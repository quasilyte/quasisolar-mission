using Godot;
using System;
using System.Collections.Generic;

public class QuickBattleMenu : Node2D {
    class WeaponOptionButton {
        public int index; // FIXME: remove if unused?
        public OptionButton node;
    }

    private Label _helpText;

    private int _playerIndex = 0;

    private WeaponOptionButton[] _weaponOptions;
    private OptionButton[] _artifactOptions;
    private OptionButton[] _playerOptions;
    private OptionButton _specialWeaponOption;
    private OptionButton _sentinelOption;
    private OptionButton _shieldOption;
    private OptionButton _hazardOption;

    private OptionButton _vesselOption;
    private OptionButton _energySourceOption;


    private Label _totalCost;

    private List<ArenaSettings.EnvDanger> _hazardSelection = new List<ArenaSettings.EnvDanger>();
    private List<ShieldDesign> _shieldSelection = new List<ShieldDesign>();
    private List<WeaponDesign> _weaponSelection = new List<WeaponDesign>();
    private List<SentinelDesign> _sentinelSelection = new List<SentinelDesign>();
    private List<WeaponDesign> _specialWeaponSelection = new List<WeaponDesign>();
    private List<ArtifactDesign> _artifactSelection = new List<ArtifactDesign>();
    private string[] _gameSpeedSelection;
    private string[] _playerSelection;

    private void UpdateVessel() {
        var vesselPreview = GetNode<Sprite>("SpritePanel/VesselPreview");
        var d = VesselDesign.Find(QuickBattleState.playerSettings[_playerIndex].vesselDesignName);
        vesselPreview.Texture = ItemInfo.Texture(d);

        GetNode<OptionButton>("SpecialWeaponSelect").Disabled = !d.specialSlot;
        GetNode<OptionButton>("SentinelSelect").Disabled = !d.sentinelSlot;
        var weaponsLeft = d.weaponSlots;
        for (int i = 0; i < _weaponOptions.Length; i++) {
            _weaponOptions[i].node.Disabled = weaponsLeft <= 0;
            weaponsLeft--;
        }
        var artifactsLeft = d.artifactSlots;
        for (int i = 0; i < _artifactOptions.Length; i++) {
            _artifactOptions[i].Disabled = artifactsLeft <= 0;
            artifactsLeft--;
        }
    }

    private void UpdatePlayerTab() {
        var settings = QuickBattleState.playerSettings[_playerIndex];

        _vesselOption.Select(Array.IndexOf(VesselDesign.list, VesselDesign.Find(settings.vesselDesignName)));

        _energySourceOption.Select(Array.IndexOf(EnergySource.list, settings.energySource));

        {
            int i = 0;
            foreach (WeaponOptionButton o in _weaponOptions) {
                o.node.Select(idOf(settings.weapons[i]));
                i++;
            }
        
        _sentinelOption.Select(_sentinelSelection.IndexOf(settings.sentinel));}
        _shieldOption.Select(_shieldSelection.IndexOf(settings.shield));
        _specialWeaponOption.Select(_specialWeaponSelection.IndexOf(settings.specialWeapon));
        {
            int i = 0;
            foreach (OptionButton o in _artifactOptions) {
                o.Select(idOf(settings.artifacts[i]));
                i++;
            }
        }

        UpdateVessel();
        UpdateTotalCost();
    }

    private void InitCommonUI() {
        var playerTab = GetNode<OptionButton>("PlayerTab");
        playerTab.AddItem("Human 1", 0);
        playerTab.AddItem("Human 2", 1);
        playerTab.Connect("item_selected", this, nameof(OnPlayerTabSelected));

        GetNode<Button>("StartBattleButton").Connect("pressed", this, nameof(OnStartBattleButtonPressed));
        GetNode<Button>("BackButton").Connect("pressed", this, nameof(OnBackButtonPressed));

        _vesselOption = GetNode<OptionButton>("VesselSelect");
        for (int i = 0; i < VesselDesign.list.Length; i++) {
            var vessel = VesselDesign.list[i];
            _vesselOption.AddItem(vessel.name + " (" + vessel.affiliation + ")", i);
        }
        _vesselOption.Connect("item_selected", this, nameof(OnVesselSelected));
        _vesselOption.Connect("mouse_entered", this, nameof(OnVesselHover));

        _energySourceOption = GetNode<OptionButton>("EnergySourceSelect");
        for (int i = 0; i < EnergySource.list.Length; i++) {
            _energySourceOption.AddItem(EnergySource.list[i].name, i);
        }
        _energySourceOption.Connect("item_selected", this, nameof(OnEnergySourceSelected));
        _energySourceOption.Connect("mouse_entered", this, nameof(OnEnergySourceHover));

        _playerSelection = new string[]{
            "None",
            "Human 1",
            "Human 1 (Gamepad 1)",
            "Human 2 (Gamepad 2)",
            "Neutral X-The-Bit",
            "Neutral Ravager",
            "Neutral Nomad",
            "Neutral Avenger",
            "Neutral Spectre",
            "Neutral Visitor",
            "Lezeona Pin",
            "Lezeona Core",
            "Zyth Hunter",
            "Zyth Invader",
            "Phaa Mantis",
            "Rarilou Leviathan",
            "Earthling Scout",
            "Earthling Explorer",
            "Earthling Fighter",
            "Earthling Interceptor",
            "Earthling Ark",
            "Earthling Gladiator",
            "Earthling Ragnarok",
            "Vespion Larva",
            "Vespion Hornet",
            "Draklid Raider",
            "Draklid Marauder",
            "Draklid Plunderer",
            "Krigia Talons",
            "Krigia Claws",
            "Krigia Fangs",
            "Krigia Destroyer",
            "Krigia Tusks",
            "Krigia Horns",
            "Krigia Ashes",
            "Wertu Probe",
            "Wertu Transporter",
            "Wertu Guardian",
            "Wertu Angel",
            "Wertu Dominator",
        };

        _specialWeaponOption = GetNode<OptionButton>("SpecialWeaponSelect");
        _sentinelOption = GetNode<OptionButton>("SentinelSelect");

        _playerOptions = new OptionButton[6];
        for (int i = 0; i < _playerOptions.Length; i++) {
            _playerOptions[i] = GetNode<OptionButton>($"PlayerSelect{i + 1}");
        }
        foreach (var o in _playerOptions) {
            for (int i = 0; i < _playerSelection.Length; i++) {
                o.AddItem(_playerSelection[i], i);
            }
        }
        for (int i = 0; i < _playerOptions.Length; i++) {
            var args = new Godot.Collections.Array { i };
            _playerOptions[i].Select(Array.IndexOf(_playerSelection, QuickBattleState.selectedPlayers[i].kind));
            _playerOptions[i].Connect("item_selected", this, nameof(OnPlayerSelected), args);
            var alliance = _playerOptions[i].GetNode<SpinBox>("SpinBox");
            alliance.Value = (float)QuickBattleState.selectedPlayers[i].alliance;
            alliance.Connect("value_changed", this, nameof(OnPlayerAllianceChanged), args);
        }

        _gameSpeedSelection = new string[]{
            "Slow",
            "Normal",
            "Fast",
            "Very Fast",
        };
        var gameSpeed = GetNode<OptionButton>("GameSpeedSelect");
        {
            int id = 0;
            foreach (string speed in _gameSpeedSelection) {
                gameSpeed.AddItem(speed, id);
                id++;
            }
        }
        gameSpeed.Select(Array.IndexOf(_gameSpeedSelection, QuickBattleState.gameSpeed));
        gameSpeed.Connect("item_selected", this, nameof(OnGameSpeedSelected));

        _totalCost = GetNode<Label>("TotalCostValue");

        _weaponOptions = new WeaponOptionButton[]{
            new WeaponOptionButton{index = 0, node = GetNode<OptionButton>("WeaponSelectQ")},
            new WeaponOptionButton{index = 1, node = GetNode<OptionButton>("WeaponSelectW")},
        };

        _artifactOptions = new OptionButton[]{
            GetNode<OptionButton>("ArtifactSelect1"),
            GetNode<OptionButton>("ArtifactSelect2"),
            GetNode<OptionButton>("ArtifactSelect3"),
        };

        _shieldOption = GetNode<OptionButton>("ShieldSelect");

        _artifactSelection.Add(EmptyArtifact.Design);
        foreach (var art in ArtifactDesign.list) {
            _artifactSelection.Add(art);
        }

        _shieldSelection.Add(EmptyShield.Design);
        foreach (var shield in ShieldDesign.list) {
            _shieldSelection.Add(shield);
        }

        _weaponSelection.Add(EmptyWeapon.Design);
        foreach (var w in WeaponDesign.list) {
            _weaponSelection.Add(w);
        }
        _specialWeaponSelection.Add(EmptyWeapon.Design);
        foreach (var w in WeaponDesign.specialList) {
            _specialWeaponSelection.Add(w);
        }

        foreach (var s in SentinelDesign.list) {
            _sentinelSelection.Add(s);
        }

        {
            int id = 0;
            foreach (WeaponDesign w in _weaponSelection) {
                foreach (WeaponOptionButton o in _weaponOptions) {
                    o.node.AddItem(w.name, id);
                }
                id++;
            }
        }
        for (int i = 0; i < _specialWeaponSelection.Count; i++) {
            _specialWeaponOption.AddItem(_specialWeaponSelection[i].name, i);
        }
        for (int i = 0; i < _shieldSelection.Count; i++) {
            _shieldOption.AddItem(_shieldSelection[i].name, i);
        }
        for (int i = 0; i < _sentinelSelection.Count; i++) {
            _sentinelOption.AddItem(_sentinelSelection[i].name, i);
        }
        {
            int id = 0;
            foreach (ArtifactDesign w in _artifactSelection) {
                foreach (OptionButton o in _artifactOptions) {
                    o.AddItem(w.name, id);
                }
                id++;
            }
        }

        GetNode<SpinBox>("AsteroidSelect").Value = QuickBattleState.numAsteroids;

        for (int i = 0; i < _weaponOptions.Length; i++) {
            var args = new Godot.Collections.Array { i };
            _weaponOptions[i].node.Connect("item_selected", this, nameof(OnWeaponSelected), args);
            _weaponOptions[i].node.Connect("mouse_entered", this, nameof(OnWeaponHover), args);
        }
        _specialWeaponOption.Connect("item_selected", this, nameof(OnSpecialWeaponSelected));
        _specialWeaponOption.Connect("mouse_entered", this, nameof(OnSpecialWeaponHover));
        _sentinelOption.Connect("item_selected", this, nameof(OnSentinelSelected));
        _sentinelOption.Connect("mouse_entered", this, nameof(OnSentinelHover));
        _shieldOption.Connect("item_selected", this, nameof(OnShieldSelected));
        _shieldOption.Connect("mouse_entered", this, nameof(OnShieldHover));
        for (int i = 0; i < _weaponOptions.Length; i++) {
            var args = new Godot.Collections.Array { i };
            _artifactOptions[i].Connect("item_selected", this, nameof(OnArtifactSelected), args);
            _artifactOptions[i].Connect("mouse_entered", this, nameof(OnArtifactHover), args);
        }

        _helpText = GetNode<Label>("HelpPanel/HelpText");

        _hazardSelection = new List<ArenaSettings.EnvDanger>{
            ArenaSettings.EnvDanger.None,
            ArenaSettings.EnvDanger.BlueNebula,
            ArenaSettings.EnvDanger.PurpleNebula,
            ArenaSettings.EnvDanger.Star,
        };
        _hazardOption = GetNode<OptionButton>("HazardSelect");
        for (int i = 0; i < _hazardSelection.Count; i++) {
            _hazardOption.AddItem(_hazardSelection[i].ToString(), i);
        }
        _hazardOption.Select(QuickBattleState.envDanger);
    }

    public override void _Ready() {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        QRandom.SetRandomNumberGenerator(rng);

        GetNode<BackgroundMusic>("/root/BackgroundMusic").PlayOutfitMusic();

        Engine.TimeScale = 1.0f;

        InitCommonUI();
        UpdatePlayerTab();
    }

    private int idOf(WeaponDesign w) {
        int id = 0;
        foreach (WeaponDesign w2 in _weaponSelection) {
            if (w2 == w) {
                return id;
            }
            id++;
        }
        return -1;
    }

    private int idOf(ArtifactDesign a) {
        int id = 0;
        foreach (ArtifactDesign a2 in _artifactSelection) {
            if (a2 == a) {
                return id;
            }
            id++;
        }
        return -1;
    }

    private QuickBattleState.PlayerSettings PlayerSettings() {
        return QuickBattleState.playerSettings[_playerIndex];
    }

    private void OnPlayerTabSelected(int id) {
        _playerIndex = id;
        UpdatePlayerTab();
    }

    private void OnSpecialWeaponHover() {
        _helpText.Text = _specialWeaponSelection[_specialWeaponOption.Selected].RenderHelp();
    }

    private void OnSentinelHover() {
        _helpText.Text = _sentinelSelection[_sentinelOption.Selected].RenderHelp();
    }

    private void OnShieldHover() {
        _helpText.Text = _shieldSelection[_shieldOption.Selected].RenderHelp();
    }

    private void OnWeaponHover(int slotIndex) {
        _helpText.Text = _weaponSelection[_weaponOptions[slotIndex].node.Selected].RenderHelp();
    }

    private void OnEnergySourceHover() {
        _helpText.Text = PlayerSettings().energySource.RenderHelp();
    }

    private void OnVesselHover() {
        _helpText.Text = VesselDesign.Find(PlayerSettings().vesselDesignName).RenderHelp();
    }

    private void OnArtifactHover(int slotIndex) {
        _helpText.Text = _artifactSelection[_artifactOptions[slotIndex].Selected].RenderHelp();
    }

    private void UpdateTotalCost() {
        _totalCost.Text = CalculateTotalCost().ToString();
    }

    private int CalculateTotalCost() {
        // TODO: use Vessel.TotalCost() here.

        int cost = 0;
        foreach (WeaponOptionButton o in _weaponOptions) {
            if (!o.node.Disabled) {
                cost += _weaponSelection[o.node.Selected].sellingPrice;
            }
        }
        cost += PlayerSettings().specialWeapon.sellingPrice;
        foreach (OptionButton o in _artifactOptions) {
            if (!o.Disabled) {
                cost += _artifactSelection[o.Selected].sellingPrice;
            }
        }
        cost += PlayerSettings().energySource.sellingPrice;
        cost += VesselDesign.Find(PlayerSettings().vesselDesignName).sellingPrice;
        cost += PlayerSettings().shield.sellingPrice;
        cost += PlayerSettings().sentinel.sellingPrice;
        return cost;
    }

    private void OnVesselSelected(int id) {
        var d = VesselDesign.list[id];
        PlayerSettings().vesselDesignName = d.name;
        UpdateVessel();
        UpdateTotalCost();
        OnVesselHover();
    }

    private void OnEnergySourceSelected(int id) {
        PlayerSettings().energySource = EnergySource.list[id];
        UpdateTotalCost();
        OnEnergySourceHover();
    }

    private void OnGameSpeedSelected(int id) {
        QuickBattleState.gameSpeed = _gameSpeedSelection[id];
    }

    private void OnArtifactSelected(int id, int slotIndex) {
        PlayerSettings().artifacts[slotIndex] = _artifactSelection[id];
        UpdateTotalCost();
        OnArtifactHover(slotIndex);
    }

    private void OnPlayerSelected(int id, int slotIndex) {
        QuickBattleState.selectedPlayers[slotIndex].kind = _playerSelection[id];
    }

    private void OnPlayerAllianceChanged(float alliance, int slotIndex) {
        QuickBattleState.selectedPlayers[slotIndex].alliance = (int)alliance;
    }

    private void OnShieldSelected(int itemId) {
        PlayerSettings().shield = _shieldSelection[itemId];
        UpdateTotalCost();
        OnShieldHover();
    }

    private void OnSpecialWeaponSelected(int itemId) {
        PlayerSettings().specialWeapon = _specialWeaponSelection[itemId];
        UpdateTotalCost();
        OnSpecialWeaponHover();
    }

    private void OnSentinelSelected(int itemId) {
        PlayerSettings().sentinel = _sentinelSelection[itemId];
        UpdateTotalCost();
        OnSentinelHover();
    }

    private void OnWeaponSelected(int itemId, int slotIndex) {
        PlayerSettings().weapons[slotIndex] = _weaponSelection[itemId];
        UpdateTotalCost();
        OnWeaponHover(slotIndex);
    }

    private void OnStartBattleButtonPressed() {
        QuickBattleState.numAsteroids = (int)GetNode<SpinBox>("AsteroidSelect").Value;

        UpdateArenaSettings();
        GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
    }

    private void OnBackButtonPressed() {
        GetTree().ChangeScene("res://scenes/MainMenu.tscn");
    }

    private void UpdateArenaSettings() {
        ArenaSettings.Reset();

        ArenaSettings.isQuickBattle = true;

        QuickBattleState.envDanger = _hazardOption.Selected;
        ArenaSettings.envDanger = _hazardSelection[QuickBattleState.envDanger];

        if (ArenaSettings.envDanger == ArenaSettings.EnvDanger.Star) {
            var colors = Enum.GetValues(typeof(StarColor));
            ArenaSettings.starColor = (StarColor)colors.GetValue(QRandom.IntRange(0, colors.Length-1));
        }

        if (QuickBattleState.gameSpeed == "Slow") {
            ArenaSettings.speed = ArenaSettings.BattleSpeed.Slow;
        } else if (QuickBattleState.gameSpeed == "Normal") {
            ArenaSettings.speed = ArenaSettings.BattleSpeed.Normal;
        } else if (QuickBattleState.gameSpeed == "Fast") {
            ArenaSettings.speed = ArenaSettings.BattleSpeed.Fast;
        } else {
            ArenaSettings.speed = ArenaSettings.BattleSpeed.VeryFast;
        }

        ArenaSettings.numAsteroids = QuickBattleState.numAsteroids;

        var playerSpawnSpots = new Vector2[]{
            new Vector2(224, 288 + (192 * 0)),
            new Vector2(1568, 288  + (192 * 0)),
            new Vector2(224, 288 + (192 * 1)),
            new Vector2(1568, 288 + (192 * 1)),
            new Vector2(224, 288 + (192 * 2)),
            new Vector2(1568, 288 + (192 * 2)),
        };

        for (int i = 0; i < QuickBattleState.selectedPlayers.Length; i++) {
            var slot = QuickBattleState.selectedPlayers[i];
            if (slot.kind == "None") {
                continue;
            }

            var v = new Vessel {
                isBot = !slot.kind.StartsWith("Human"),
                spawnPos = playerSpawnSpots[i],
                faction = (Faction)(slot.alliance - 1),
            };
            if (slot.kind == "Human 1 (Gamepad 1)") {
                v.isGamepad = true;
                v.deviceId = 0;
            }
            if (slot.kind == "Human 2 (Gamepad 2)") {
                v.isGamepad = true;
                v.deviceId = 1;
            }

            if (slot.kind.StartsWith("Human")) {
                InitHuman(v);
                VesselFactory.InitStats(v);
            } else {
                VesselFactory.Init(v, slot.kind);
            }

            ArenaSettings.combatants.Add(v);
        }
    }

    public static void InitHuman(Vessel v) {
        var settings = QuickBattleState.playerSettings[v.deviceId];

        v.designName = settings.vesselDesignName;
        v.energySourceName = settings.energySource.name;

        var artifacts = settings.artifacts;
        var weapons = settings.weapons;
        var specialWeapon = settings.specialWeapon;
        var sentinel = settings.sentinel;
        var shield = settings.shield;

        var design = v.Design();

        var artifactSet = new HashSet<ArtifactDesign>();
        foreach (ArtifactDesign a in artifacts) {
            if (v.artifacts.Count >= design.artifactSlots) {
                break;
            }
            if (!artifactSet.Contains(a)) {
                artifactSet.Add(a);
                v.artifacts.Add(a.name);
            } else {
                v.artifacts.Add(EmptyArtifact.Design.name);
            }
        }

        if (design.maxShieldLevel >= shield.level) {
            v.shieldName = shield.name;
        }

        if (design.weaponSlots >= 1) {
            v.weapons.Add(weapons[0].name);
        } else {
            v.weapons.Add(EmptyWeapon.Design.name);
        }
        if (design.weaponSlots >= 2) {
            v.weapons.Add(weapons[1].name);
        } else {
            v.weapons.Add(EmptyWeapon.Design.name);
        }
        if (design.weaponSlots >= 3) {
            v.weapons.Add(weapons[2].name);
        } else {
            v.weapons.Add(EmptyWeapon.Design.name);
        }
        if (design.specialSlot) {
            v.specialWeaponName = specialWeapon.name;
        } else {
            v.specialWeaponName = EmptyWeapon.Design.name;
        }
        if (design.sentinelSlot) {
            v.sentinelName = sentinel.name;
        } else {
            v.sentinelName = "Empty";
        }
    }
}
