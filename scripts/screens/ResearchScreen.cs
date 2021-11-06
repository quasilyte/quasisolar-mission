using Godot;
using System.Collections.Generic;
using System;

public class ResearchScreen : Node2D {
    class ResearchNode {
        public Research value;
        public Label label;
        public ButtonNode button;
    }

    private RpgGameState _gameState;

    private List<Research> _researchList;
    private List<ResearchNode> _researchNodes = new List<ResearchNode>();

    public override void _Ready() {
        _gameState = RpgGameState.instance;

        _researchList = new List<Research>(Research.list);

        _researchList.Sort((x, y) => {
            if (x.category == y.category) {
                return x.name.CompareTo(y.name);
            }
            return x.category.CompareTo(y.category);
        });

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

    private void SetupUI() {
        GetNode<TextureButton>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));
        GetNode<Button>("Status/InvestButton").Connect("pressed", this, nameof(OnInvestButton));

        var researchFilters = new List<string>(){
            "Recommended",
            "All",
            "Fundamental",
            "Upgrade",
            "Weapons",
            "Energy Source",
            "Shield",
            "Artifact",
            "Vessel Design",
            "Sentinel",
            "Exploration Drone",
            "Base Module",
        };
        var filterSelect = GetNode<OptionButton>("ProjectList/FilterOptions");
        foreach (var o in researchFilters) {
            filterSelect.AddItem(o);
        }
        filterSelect.Connect("item_selected", this, nameof(OnFilterSelected));
        filterSelect.Select(researchFilters.FindIndex((x) => x == _gameState.selectedResearchCategory));
    }

    private void OnFilterSelected(int index) {
         var filterSelect = GetNode<OptionButton>("ProjectList/FilterOptions");
        _gameState.selectedResearchCategory = filterSelect.GetItemText(index);
        UpdateUI();
    }

    private void OnStartProjectButton(int index) {
        var project = _researchNodes[index];
        _gameState.currentResearch = project.value.name;
        UpdateUI();
    }

    private string ResearchCatogoryString(Research.Category category) {
        if (category == Research.Category.Upgrade) {
            return "upgrade";
        }
        if (category == Research.Category.NewVesselDesign) {
            return "new vessel design";
        }
        if (category == Research.Category.NewSentinel) {
            return "new sentinel";
        }
        if (category == Research.Category.NewWeapon) {
            return "new weapon";
        }
        if (category == Research.Category.NewSpecialWeapon) {
            return "new special weapon";
        }
        if (category == Research.Category.NewEnergySource) {
            return "new energy source";
        }
        if (category == Research.Category.NewArtifact) {
            return "artifact";
        }
        if (category == Research.Category.Fundamental) {
            return "fundamental";
        }
        if (category == Research.Category.NewShield) {
            return "new shield";
        }
        if (category == Research.Category.NewExplorationDrone) {
            return "new drone";
        }
        if (category == Research.Category.NewBaseModule) {
            return "new base module";
        }
        throw new Exception("unexpected research category: " + category.ToString());
    }

    private void OnProjectHover(int index) {
        var project = _researchNodes[index];
        var r = project.value;

        var text = r.name + " [" + ResearchCatogoryString(r.category) + "]\n\n";

        if (r.material != Faction.Neutral) {
            text += "Requires " + r.material + " material.\n";
        }
        text += "Research time: " + r.researchTime + "\n\n";
        
        if (!r.effect.Empty()) {
            text += "Effect: " + r.effect + "\n";
        }
        if (!r.effect2.Empty()) {
            text += "Effect: " + r.effect2 + "\n";
        }

        GetNode<Label>("ProjectInfo/Panel/Label").Text = text;
    }

    private List<Research> RecommendedProjects() {
        var recommendations = new List<List<string>>{
            new List<string>{
                "Flame Eater",
                "Ark",
                "Gauss Turret Capacity",
            },
            new List<string>{
                "Rocket Launcher",
                "Jump Tracer Mk2",
                "Interceptor",
                "Laser Weapons",
                "Seeker",
            },
            new List<string>{
                "Pulse Laser",
                "Draklid Weapons I",
                "Vortex Battery",
                "Level 2 Shields",
                "Aligned Jumping",
                "Fog Shark",
            },
            new List<string>{
                "Decelerator",
                "Disruptor",
                "Point-Defense Laser",
                "Improved Fuel Tanks",
                "Krigia Weapons I",
                "Ifrit",
            },
            new List<string>{
                "Decelerator Guard",
                "Assault Laser",
                "Gladiator",
                "Krigia Weapons II",
            },
            new List<string>{
                "Hurricane",
                "Bomber",
                "High-Capacity Reactors",
                "Krigia Weapons III",
                "Level 3 Shields",
            },
            new List<string>{
                "Graviton Generator",
                "Lancer",
                "Mortar",
                "Phaser",
            },
        };

        var result = new List<Research>();
        foreach (var stage in recommendations) {
            var stageCompleted = true;
            foreach (var projectName in stage) {
                var r = Research.Find(projectName);
                if (!_gameState.technologiesResearched.Contains(projectName)) {
                    if (r.material != Faction.Neutral) {
                        result.Add(r);
                        continue;
                    }
                    stageCompleted = false;
                }
            }
            if (!stageCompleted) {
                foreach (var projectName in stage) {
                    var r = Research.Find(projectName);
                    if (r.material == Faction.Neutral) {
                        result.Add(r);
                    }
                }
                break;
            }
        }

        // Always recommend artifact-related researches.
        foreach (var artifact in _gameState.artifactsRecovered) {
            if (!_gameState.technologiesResearched.Contains(artifact)) {
                result.Add(Research.Find(artifact));
            }
        }

        // Always recommend quest-related researches.
        foreach (var r in _researchList) {
            if (_gameState.technologiesResearched.Contains(r.name)) {
                continue;
            }
            if (_gameState.activeQuests.Find((x) => x.name == r.quest) != null) {
                result.Add(r);
            }
        }

        return result;
    }

    private List<Research> PrepareProjectList() {
        var filterSelect = GetNode<OptionButton>("ProjectList/FilterOptions");
        var selected = filterSelect.GetItemText(filterSelect.Selected);

        Research.Category category = Research.Category.Dummy;
        if (selected == "Fundamental") {
            category = Research.Category.Fundamental;
        }
        if (selected == "Upgrade") {
            category = Research.Category.Upgrade;
        }
        if (selected == "Energy Source") {
            category = Research.Category.NewEnergySource;
        }
        if (selected == "Shield") {
            category = Research.Category.NewShield;
        }
        if (selected == "Artifact") {
            category = Research.Category.NewArtifact;
        }
        if (selected == "Vessel Design") {
            category = Research.Category.NewVesselDesign;
        }
        if (selected == "Sentinel") {
            category = Research.Category.NewSentinel;
        }
        if (selected == "Exploration Drone") {
            category = Research.Category.NewExplorationDrone;
        }
        if (selected == "Base Module") {
            category = Research.Category.NewBaseModule;
        }

        if (selected == "Recommended") {
            return FilterAvailableResearches(RecommendedProjects());
        } else if (selected == "Weapons") {
            var projects = _researchList.FindAll((r) => {
                var c = r.GetFilterCategory();
                return c == Research.Category.NewWeapon || c == Research.Category.NewSpecialWeapon;
            });
            return FilterAvailableResearches(projects);
        } else if (selected == "Fundamental") {
            return FilterAvailableResearches(_researchList.FindAll((r) => r.category == category));
        } else if (category != Research.Category.Dummy) {
            var projects = _researchList.FindAll((r) => r.GetFilterCategory() == category);
            return FilterAvailableResearches(projects);
        } else if (selected == "All") {
            return FilterAvailableResearches(_researchList);
        }

        return new List<Research>();
    }

    private List<Research> FilterAvailableResearches(List<Research> projects) {
        var result = new List<Research>();
        foreach (var r in projects) {
            if (r.researchTime == 0) {
                continue;
            }
            if (r.quest != "") {
                if (_gameState.activeQuests.Find((x) => x.name == r.quest) == null) {
                    continue;
                }
            }
            if (_gameState.technologiesResearched.Contains(r.name)) {
                continue;
            }
            if (!Research.IsAvailable(_gameState.technologiesResearched, r.dependencies)) {
                continue;
            }
            if (r.material != Faction.Neutral) {
                if (_gameState.researchMaterial.Count(r.material) == 0) {
                    continue;
                }
            }
            if (r.category == Research.Category.NewArtifact && !_gameState.artifactsRecovered.Contains(r.name)) {
                continue;
            }
            result.Add(r);
        }
        return result;
    }

    private void UpdateProjectList(List<Research> projects) {
        var projectsPanel = GetNode<VBoxContainer>("ProjectList/ScrollContainer/List");
        foreach (var child in projectsPanel.GetChildren()) {
            ((Node)child).QueueFree();
        }
        _researchNodes.Clear();
        foreach (var r in projects) {
            var project = ListItemNode.New(r.name);

            var label = project.GetNode<Label>("Label");
            label.MouseFilter = Control.MouseFilterEnum.Stop;

            projectsPanel.AddChild(project);

            _researchNodes.Add(new ResearchNode {
                value = r,
                label = label,
                button = project.GetNode<ButtonNode>("Button"),
            });
        }
        for (int i = 0; i < _researchNodes.Count; i++) {
            _researchNodes[i].button.Connect("pressed", this, nameof(OnStartProjectButton), new Godot.Collections.Array { i });
            _researchNodes[i].label.Connect("mouse_entered", this, nameof(OnProjectHover), new Godot.Collections.Array { i });
            _researchNodes[i].button.Connect("mouse_entered", this, nameof(OnProjectHover), new Godot.Collections.Array { i });
        }
    }

    private void UpdateUI() {
        GetNode<Button>("Status/InvestButton").Disabled = _gameState.credits < 1000;

        GetNode<Label>("Material/KrigiaMaterialValue").Text = _gameState.researchMaterial.krigia.ToString();
        GetNode<Label>("Material/WertuMaterialValue").Text = _gameState.researchMaterial.wertu.ToString();
        GetNode<Label>("Material/ZythMaterialValue").Text = _gameState.researchMaterial.zyth.ToString();
        GetNode<Label>("Material/PhaaMaterialValue").Text = _gameState.researchMaterial.phaa.ToString();
        GetNode<Label>("Material/DraklidMaterialValue").Text = _gameState.researchMaterial.draklid.ToString();
        GetNode<Label>("Material/VespionMaterialValue").Text = _gameState.researchMaterial.vespion.ToString();
        GetNode<Label>("Material/RarilouMaterialValue").Text = _gameState.researchMaterial.rarilou.ToString();

        GetNode<Label>("Status/CreditsValue").Text = _gameState.credits.ToString();
        GetNode<Label>("Status/ScienceFuncsValue").Text = _gameState.scienceFunds.ToString();

        UpdateProjectList(PrepareProjectList());

        if (_gameState.currentResearch == "") {
            GetNode<Label>("ResearchProgress/Subject").Text = "<No Research Subject>";
            GetNode<Label>("ResearchProgress/Status").Text = "";
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Visible = false;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Value = 0;
        } else {
            var research = Research.Find(_gameState.currentResearch);
            var statusText = "Research rate: " + (int)(100 * RpgGameState.ResearchRate()) + "%";
            GetNode<Label>("ResearchProgress/Subject").Text = "<" + _gameState.currentResearch + ">";
            GetNode<Label>("ResearchProgress/Status").Text = statusText;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Visible = true;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Value = QMath.Percantage((int)_gameState.researchProgress, research.researchTime);
        }

        for (int i = 0; i < _researchNodes.Count; i++) {
            var project = _researchNodes[i];
            project.button.Disabled = _gameState.currentResearch != "";
            project.label.AddColorOverride("font_color", Color.Color8(255, 255, 255));
            if (project.value.name == _gameState.currentResearch) {
                project.label.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
            }
        }
    }

    private void OnInvestButton() {
        if (_gameState.credits < 1000) {
            return;
        }
        _gameState.credits -= 1000;
        _gameState.scienceFunds += 1000;
        UpdateUI();
    }

    private void OnLeaveButton() {
        RpgGameState.transition = RpgGameState.MapTransition.ExitResearchScreen;
        GetTree().ChangeScene("res://scenes/screens/MapView.tscn");
    }
}
