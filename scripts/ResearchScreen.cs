using Godot;
using System.Collections.Generic;

public class ResearchScreen : Node2D {
    class ResearchNode {
        public Research value;
        public Label label;
        public ButtonNode button;
    }

    private List<Research> _researchList;
    private List<ResearchNode> _researchNodes = new List<ResearchNode>();

    public override void _Ready() {
        _researchList = new List<Research>(Research.list);

        foreach (var art in RpgGameState.artifactsRecovered) {
            _researchList.Add(new Research{
                name = art.name,
                category = Research.Category.NewArtifact,
                researchTime = 40,
            });
        }

        _researchList.Sort((x, y) => x.name.CompareTo(y.name));

        SetupUI();
        UpdateUI();
    }

    private void SetupUI() {
        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));
        GetNode<Button>("Status/InvestButton").Connect("pressed", this, nameof(OnInvestButton));

        var projectsPanel = GetNode<VBoxContainer>("ProjectList/ScrollContainer/List");
        foreach (var r in _researchList) {
            if (RpgGameState.technologiesResearched.Contains(r.name)) {
                continue;
            }
            if (!Research.IsAvailable(RpgGameState.technologiesResearched, r.dependencies)) {
                continue;
            }
            if (r.material == Research.Material.Krigia && !RpgGameState.metKrigia) {
                continue;
            }
            if (r.material == Research.Material.Wertu && !RpgGameState.metWertu) {
                continue;
            }
            if (r.material == Research.Material.Zyth && !RpgGameState.metZyth) {
                continue;
            }

            var project = ResearchProjectNode.New();

            var label = project.GetNode<Label>("GridContainer/Name");

            label.Text = r.name;
            // project.RectSize = new Vector2(256, 32);
            // label.RectPosition = new Vector2(offsetX, offsetY);
            projectsPanel.AddChild(project);

            // var button = ButtonNode.New();
            // button.Text = ">";
            // button.RectSize = new Vector2(32, 32);
            // // button.RectPosition = new Vector2(-48, 0);
            // label.AddChild(button);

            _researchNodes.Add(new ResearchNode {
                value = r,
                label = label,
                button = project.GetNode<ButtonNode>("GridContainer/Start"),
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
        RpgGameState.currentResearch = project.value;
        UpdateUI();
    }

    private string ResearchCatogoryString(Research.Category category) {
        if (category == Research.Category.Upgrade) {
            return "upgrade";
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
        return "fundamental";
    }

    private void OnProjectHover(int index) {
        var project = _researchNodes[index];
        var r = project.value;

        var text = r.name + " [" + ResearchCatogoryString(r.category) + "]\n\n";

        if (r.material != Research.Material.None) {
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
        GetNode<Button>("Status/InvestButton").Disabled = RpgGameState.credits < 1000;

        GetNode<Label>("Status/KrigiaMaterialValue").Text = RpgGameState.krigiaMaterial.ToString();
        GetNode<Label>("Status/WertuMaterialValue").Text = RpgGameState.wertuMaterial.ToString();
        GetNode<Label>("Status/ZythMaterialValue").Text = RpgGameState.zythMaterial.ToString();

        GetNode<Label>("Status/CreditsValue").Text = RpgGameState.credits.ToString();
        GetNode<Label>("Status/ScienceFuncsValue").Text = RpgGameState.scienceFunds.ToString();

        if (RpgGameState.currentResearch == null) {
            GetNode<Label>("ResearchProgress/Subject").Text = "<No Research Subject>";
            GetNode<Label>("ResearchProgress/Status").Text = "";
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Visible = false;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Value = 0;
        } else {
            var statusText = "Research rate: " + (int)(100 * RpgGameState.ResearchRate()) + "%";
            GetNode<Label>("ResearchProgress/Subject").Text = "<" + RpgGameState.currentResearch.name + ">";
            GetNode<Label>("ResearchProgress/Status").Text = statusText;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Visible = true;
            GetNode<TextureProgress>("ResearchProgress/ProgressBar").Value = QMath.Percantage((int)RpgGameState.researchProgress, RpgGameState.currentResearch.researchTime);
        }

        for (int i = 0; i < _researchNodes.Count; i++) {
            var project = _researchNodes[i];
            project.button.Disabled = RpgGameState.currentResearch != null;
            project.label.AddColorOverride("font_color", Color.Color8(255, 255, 255));
            if (project.value == RpgGameState.currentResearch) {
                project.label.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
            }
        }
    }

    private void OnInvestButton() {
        if (RpgGameState.credits < 1000) {
            return;
        }
        RpgGameState.credits -= 1000;
        RpgGameState.scienceFunds += 1000;
        UpdateUI();
    }

    private void OnLeaveButton() {
        RpgGameState.transition = RpgGameState.MapTransition.ExitResearchScreen;
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }
}
