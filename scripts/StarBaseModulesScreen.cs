using Godot;
using System.Collections.Generic;

public class StarBaseModulesScreen : Node2D {
    class ModuleNode {
        public StarBaseModule value;
        public Label label;
        public ButtonNode button;
    }

    private RpgGameState _gameState;
    private StarBase _starBase;

    private List<ModuleNode> _moduleNodes = new List<ModuleNode>();

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _starBase = RpgGameState.enteredBase;
        SetupUI();
        UpdateUI();
    }

    private void SetupUI() {
        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));

        var modulesAvailable = new List<StarBaseModule>();
        foreach (var module in StarBaseModule.list) {
            if (module.researchRequired && !_gameState.technologiesResearched.Contains(module.name)) {
                continue;
            }
            modulesAvailable.Add(module);
        }

        for (int i = 0; i < 4; i++) {
            var moduleNode = GetNode<Sprite>($"Modules/Module{i}");
            moduleNode.GetNode<ButtonNode>("SellButton").Connect("pressed", this, nameof(OnModuleSellButton), new Godot.Collections.Array { i });
        }

        var box = GetNode<Panel>("ModulesAvailable/Box");
        var offsetX = 64;
        var offsetY = 24;
        for (int i = 0; i < modulesAvailable.Count; i++) {
            var module = modulesAvailable[i];
            var moduleLabel = new Label();
            moduleLabel.Text = module.name;
            moduleLabel.RectSize = new Vector2(256, 32);
            moduleLabel.Valign = Label.VAlign.Center;
            moduleLabel.RectPosition = new Vector2(offsetX, offsetY);
            moduleLabel.MouseFilter = Control.MouseFilterEnum.Stop;
            box.AddChild(moduleLabel);

            moduleLabel.Connect("mouse_entered", this, nameof(OnSelectionMouseEnter), new Godot.Collections.Array { module.name });
            moduleLabel.Connect("mouse_exited", this, nameof(InfoBoxClear));

            var startButton = ButtonNode.New();
            startButton.Text = ">";
            startButton.RectSize = new Vector2(32, 32);
            startButton.RectPosition = new Vector2(-48, 0);
            startButton.Connect("pressed", this, nameof(OnStartButton), new Godot.Collections.Array { i });
            moduleLabel.AddChild(startButton);

            offsetY += 48;

            _moduleNodes.Add(new ModuleNode {
                value = module,
                label = moduleLabel,
                button = startButton,
            });
        }

        GetNode<SlotButtonNode>("ProductionMenu/MineralsMode").Connect("pressed", this, nameof(OnModeButton), new Godot.Collections.Array { StarBase.Mode.ProduceMinerals });
        GetNode<SlotButtonNode>("ProductionMenu/PowerMode").Connect("pressed", this, nameof(OnModeButton), new Godot.Collections.Array { StarBase.Mode.ProducePower });
        GetNode<SlotButtonNode>("ProductionMenu/RUMode").Connect("pressed", this, nameof(OnModeButton), new Godot.Collections.Array { StarBase.Mode.ProduceRU });
    }

    private void OnModeButton(StarBase.Mode mode) {
        _starBase.mode = mode;
        UpdateMode();
    }

    private void OnStartButton(int i) {
        var module = _moduleNodes[i];
        if (_gameState.credits < module.value.sellingPrice) {
            return;
        }
        _starBase.constructionTarget = module.value.name;
        _gameState.credits -= module.value.sellingPrice;
        UpdateUI();
    }

    private void OnModuleSellButton(int i) {
        var moduleName = _starBase.modules[i];
        _starBase.modules.RemoveAt(i);
        _gameState.credits += StarBaseModule.Find(moduleName).sellingPrice / 2;
        GetParent().AddChild(SoundEffectNode.New(GD.Load<AudioStream>("res://audio/sell.wav")));
        UpdateUI();
    }

    private void UpdateMode() {
        var mineralsOutline = false;
        var powerOutline = false;
        var ruOutline = false;
        var produtionLabelText = "";
        switch (_starBase.mode) {
        case StarBase.Mode.ProduceMinerals:
            mineralsOutline = true;
            produtionLabelText = "Generating Minerals";
            break;
        case StarBase.Mode.ProducePower:
            powerOutline = true;
            produtionLabelText = "Generating Power";
            break;
        case StarBase.Mode.ProduceRU:
            ruOutline = true;
            produtionLabelText = "Generating RU";
            break;
        }
        GetNode<SlotButtonNode>("ProductionMenu/MineralsMode").SetOutlineVisibility(mineralsOutline);
        GetNode<SlotButtonNode>("ProductionMenu/PowerMode").SetOutlineVisibility(powerOutline);
        GetNode<SlotButtonNode>("ProductionMenu/RUMode").SetOutlineVisibility(ruOutline);
        GetNode<Label>("ProductionMenu/ModeName").Text = produtionLabelText;
    }

    private bool CanBuildModule(StarBaseModule module) {
        if (module.isTurret) {
            var hasTurret = _starBase.modules.Find((x) => StarBaseModule.Find(x).isTurret) != null;
            if (hasTurret) {
                return false;
            }
        }
        if (module.requires != "") {
            if (!_starBase.modules.Contains(module.requires)) {
                return false;
            }
        }
        return _starBase.constructionTarget == "" &&
                _gameState.credits >= module.sellingPrice &&
                _starBase.modules.Count != 4 &&
                !_starBase.modules.Contains(module.name);
    }

    private void UpdateUI() {
        GetNode<Label>("Status/Credits/Value").Text = _gameState.credits.ToString();

        for (int i = 0; i < _moduleNodes.Count; i++) {
            var module = _moduleNodes[i];
            module.button.Disabled = !CanBuildModule(module.value);
            module.label.AddColorOverride("font_color", Color.Color8(255, 255, 255));
            if (module.value.name == _starBase.constructionTarget) {
                module.label.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
            }
        }

        if (_starBase.constructionTarget == "") {
            GetNode<Label>("ConstructionProgress/Name").Text = "";
            GetNode<TextureProgress>("ConstructionProgress/ProgressBar").Visible = false;
            GetNode<TextureProgress>("ConstructionProgress/ProgressBar").Value = 0;
        } else {
            var module = StarBaseModule.Find(_starBase.constructionTarget);
            GetNode<Label>("ConstructionProgress/Name").Text = "<" + _starBase.constructionTarget + ">";
            GetNode<TextureProgress>("ConstructionProgress/ProgressBar").Visible = true;
            GetNode<TextureProgress>("ConstructionProgress/ProgressBar").Value = QMath.Percantage((int)_starBase.constructionProgress, module.buildTime);
        }

        for (int i = 0; i < 4; ++i) {
            var moduleNode = GetNode<Sprite>($"Modules/Module{i}");
            var sellButton = moduleNode.GetNode<ButtonNode>("SellButton");
            if (i >= _starBase.modules.Count) {
                moduleNode.GetNode<Label>("Name").Text = "Empty Module Slot";
                sellButton.Visible = false;
            } else {
                moduleNode.GetNode<Label>("Name").Text = _starBase.modules[i];
                sellButton.Visible = true;
                sellButton.Disabled = null != _starBase.modules.Find(
                    (x) => StarBaseModule.Find(x).requires == _starBase.modules[i]
                );
            }
        }

        UpdateMode();
    }

    private void InfoBoxClear() {
        GetNode<Label>("ModuleInfo/Box/Body").Text = "";
    }

    private void OnSelectionMouseEnter(string moduleName) {
        var module = StarBaseModule.Find(moduleName);
        GetNode<Label>("ModuleInfo/Box/Body").Text = GetModuleInfoText(module);
    }

    private string GetModuleInfoText(StarBaseModule module) {
        var infoLines = new List<string>();
        infoLines.Add(module.name + " (" + module.sellingPrice + ")");
        infoLines.Add("");
        infoLines.Add("Effect: " + module.effect);
        infoLines.Add("Build time: " + module.buildTime);
        if (module.requires != "") {
            infoLines.Add("Requires: " + module.requires);
        }
        return string.Join("\n", infoLines);
    }

    private void OnLeaveButton() {
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
