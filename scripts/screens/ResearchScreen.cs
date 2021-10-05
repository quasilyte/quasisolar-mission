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

        _researchList.Sort((x, y) => x.name.CompareTo(y.name));

        SetupUI();
        UpdateUI();
    }

    private void SetupUI() {
        GetNode<TextureButton>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));
        GetNode<Button>("Status/InvestButton").Connect("pressed", this, nameof(OnInvestButton));

        var projectsPanel = GetNode<VBoxContainer>("ProjectList/ScrollContainer/List");
        foreach (var r in _researchList) {
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
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }
}
