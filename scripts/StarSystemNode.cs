using Godot;
using System;
using System.Collections.Generic;

public class StarSystemNode : Node2D {
    public StarSystem sys;

    private StarBaseNode _starBase;

    private PanelContainer _infoBox;

    [Signal]
    public delegate void Clicked();

    private static PackedScene _scene = null;
    public static StarSystemNode New(StarSystem system, StarBaseNode starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/StarSystemNode.tscn");
        }
        var o = (StarSystemNode)_scene.Instance();
        o.sys = system;
        o._starBase = starBase;
        return o;
    }

    public void AddStarBase(StarBaseNode starBase) {
        _starBase = starBase;
        _starBase.Visible = true;
        SetStarBaseColor();
        UpdateInfo();
        RenderKnownInfo();
    }

    public override void _Ready() {
        Position = sys.pos;

        _infoBox = GetNode<PanelContainer>("Z/InfoBox");
        _infoBox.Visible = false;

        GetNode<Label>("Label").Text = sys.name;
        GetNode<Sprite>("Sprite").Frame = (int)sys.color;

        GetNode<Area2D>("Area2D").Connect("mouse_entered", this, nameof(OnMouseEnter));
        GetNode<Area2D>("Area2D").Connect("mouse_exited", this, nameof(OnMouseExited));
        GetNode<Area2D>("Area2D").Connect("input_event", this, nameof(OnAreaInput));

        if (sys.intel != null) {
            if (sys.starBase != null) {
                _starBase.Visible = true;
                SetStarBaseColor();
            }
            RenderKnownInfo();
        }
    }

    private void OnAreaInput(Viewport viewport, InputEvent e, int shapeIndex) {
        if (e is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed) {
                EmitSignal(nameof(Clicked));
            }
        }
    }

    public void DestroyStarBase() {
        _starBase.QueueFree();
        _starBase = null;
        sys.starBase = null;

        var explosion = Explosion.New();
        explosion.Position = Position;
        GetParent().AddChild(explosion);

        UpdateInfo();
    }

    public void OnPlayerEnter(Player p) {
        if (sys.intel == null) {
            OnFirstPlayerEnter(p);
        }
        UpdateInfo();
        RenderKnownInfo();
    }

    public void UpdateInfo() {
        sys.intel = new StarSystemIntel{
            starBase = sys.starBase,
            artifact = sys.artifact,
            resourcePlanets = sys.resourcePlanets,
        };
    }

    private static Label CreateLabel(string name) {
        var label = new Label();
        label.Theme = GD.Load<Theme>("res://theme.tres");
        label.Name = name;
        label.Align = Label.AlignEnum.Center;
        return label;
    }

    public void RenderKnownInfo() {
        var box = _infoBox.GetNode<VBoxContainer>("Box");
        var info = sys.intel;

        var owner = info.starBase != null ? info.starBase.owner.PlayerName : "Neutral";
        var titleLabel = box.GetNode<Label>("Title");
        titleLabel.Text = owner + " System";

        if (info.starBase != null) {
            Label baseLabel;
            if (box.HasNode("Base")) {
                baseLabel = box.GetNode<Label>("Base");
            } else {
                baseLabel = CreateLabel("Base");
                box.AddChild(baseLabel);
            }
            var numShips = info.starBase.garrison.Count;
            var pluralSuffix = numShips == 1 ? "" : "s";
            var level = Utils.IntToRoman(info.starBase.level);
            baseLabel.Text = $"Base {level}: {info.starBase.hp}% HP, {numShips} ship" + pluralSuffix;
        } else {
            if (box.HasNode("Base")) {
                box.GetNode<Label>("Base").QueueFree();
            }
        }

        if (info.artifact != null) {
            Label artifactLabel;
            if (box.HasNode("Artifact")) {
                artifactLabel = box.GetNode<Label>("Artifact");
            } else {
                artifactLabel = CreateLabel("Artifact");
                box.AddChild(artifactLabel);
            }
            artifactLabel.Text = "Artifact Detected";
        }

        if (info.resourcePlanets.Count != 0) {
            Label resourcePlanetsLabel;
            if (box.HasNode("ResourcePlanets")) {
                resourcePlanetsLabel = box.GetNode<Label>("ResourcePlanets");
            } else {
                resourcePlanetsLabel = CreateLabel("ResourcePlanets");
                box.AddChild(resourcePlanetsLabel);
            }
            var numPlanets = info.resourcePlanets.Count;
            int numMines = 0;
            foreach (var planet in info.resourcePlanets) {
                if (planet.hasMine) {
                    numMines++;
                }
            }
            if (numMines == 0) {
                resourcePlanetsLabel.Text = $"Resource planets: {numPlanets}";
            } else {
                var pluralSuffix = numMines == 1 ? "" : "s";
                resourcePlanetsLabel.Text = $"Resource planets: {numPlanets} ({numMines} mine{pluralSuffix})";
            }
        }
    }

    private void OnMouseEnter() {
        _infoBox.Visible = true;
    }

    private void OnMouseExited() {
        _infoBox.Visible = false;
    }

    private void SetStarBaseColor() {
        Func<MapNodeColor> baseColor = () => {
            var owner = sys.starBase.owner;
            if (owner == RpgGameState.instance.humanPlayer) {
                return MapNodeColor.Green;
            }
            if (owner == RpgGameState.instance.scavengerPlayer) {
                return MapNodeColor.Purple;
            }
            if (owner == RpgGameState.instance.krigiaPlayer) {
                return MapNodeColor.Red;
            }
            return MapNodeColor.Yellow;
        };

        _starBase.SetColor(baseColor());
    }

    private void OnFirstPlayerEnter(Player p) {
        if (sys.starBase != null) {
            _starBase.Visible = true;
            SetStarBaseColor();
            if (sys.starBase.owner.Alliance != p.Alliance) {
                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/enemy_base_detected.wav"));
            }
        }

        if (sys.artifact != null) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_detected.wav"));
        }
    }

    public void ProcessDay() {
        if (RpgGameState.instance.day % 10 != 0) {
            return;
        }
        if (sys.starBase == null || sys.starBase.owner != RpgGameState.instance.humanPlayer) {
            return;
        }
        foreach (var p in sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }
            RpgGameState.instance.credits += RpgGameState.MineralsSellPrice() * p.mineralsCollected;
            RpgGameState.instance.credits += RpgGameState.OrganicSellPrice() * p.organicCollected;
            RpgGameState.instance.credits += RpgGameState.PowerSellPrice() * p.powerCollected;
            sys.starBase.mineralsStock += p.mineralsCollected;
            sys.starBase.organicStock += p.organicCollected;
            sys.starBase.powerStock += p.powerCollected;
            p.mineralsCollected = 0;
            p.organicCollected = 0;
            p.powerCollected = 0;
        }
    }
}
