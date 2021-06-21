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
        SetStarBaseColor();
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
            if (sys.starBase.id != 0) {
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

    public void DetachStarBase() {
        _starBase.QueueFree();
        _starBase = null;
        sys.starBase.id = 0;
    }

    public void DestroyStarBase(bool explosionEffect = true) {
        sys.starBase.Get().deleted = true;
        DetachStarBase();

        var explosion = Explosion.New();
        explosion.Position = Position;
        GetParent().AddChild(explosion);

        UpdateInfo();
    }

    public void OnPlayerEnter(Faction faction) {
        sys.visitsNum++;
        if (sys.intel == null) {
            OnFirstPlayerEnter(faction);
        }
        UpdateInfo();
        RenderKnownInfo();
    }

    public void UpdateInfo() {
        var numPlanets = sys.resourcePlanets.Count;
        int numMines = 0;
        foreach (var planet in sys.resourcePlanets) {
            if (planet.hasMine) {
                numMines++;
            }
        }
        sys.intel = new StarSystemIntel{
            hasArtifact = sys.artifact != null,
            hasBase = sys.starBase.id != 0,
            numResourcePlanets = numPlanets,
            numMines = numMines,
        };
        if (sys.starBase.id != 0) {
            var starBase = sys.starBase.Get();
            sys.intel.baseOwner = starBase.owner;
            sys.intel.garrisonSize = starBase.garrison.Count;
            sys.intel.baseLevel = starBase.level;
            sys.intel.baseHp = starBase.hp;
        }
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

        var owner = info.hasBase ? info.baseOwner.ToString() : "Neutral";
        var titleLabel = box.GetNode<Label>("Title");
        titleLabel.Text = owner + " System";

        if (info.hasBase) {
            Label baseLabel;
            if (box.HasNode("Base")) {
                baseLabel = box.GetNode<Label>("Base");
            } else {
                baseLabel = CreateLabel("Base");
                box.AddChild(baseLabel);
            }
            var numShips = info.garrisonSize;
            var pluralSuffix = numShips == 1 ? "" : "s";
            var level = Utils.IntToRoman(info.baseLevel);
            baseLabel.Text = $"Base {level}: {info.baseHp}% HP, {numShips} ship" + pluralSuffix;
        } else {
            if (box.HasNode("Base")) {
                box.GetNode<Label>("Base").QueueFree();
            }
        }

        if (info.hasArtifact) {
            Label artifactLabel;
            if (box.HasNode("Artifact")) {
                artifactLabel = box.GetNode<Label>("Artifact");
            } else {
                artifactLabel = CreateLabel("Artifact");
                box.AddChild(artifactLabel);
            }
            artifactLabel.Text = "Artifact Detected";
        }

        if (info.numResourcePlanets != 0) {
            Label resourcePlanetsLabel;
            if (box.HasNode("ResourcePlanets")) {
                resourcePlanetsLabel = box.GetNode<Label>("ResourcePlanets");
            } else {
                resourcePlanetsLabel = CreateLabel("ResourcePlanets");
                box.AddChild(resourcePlanetsLabel);
            }
            if (info.numMines == 0) {
                resourcePlanetsLabel.Text = $"Resource planets: {info.numResourcePlanets}";
            } else {
                var pluralSuffix = info.numMines == 1 ? "" : "s";
                resourcePlanetsLabel.Text = $"Resource planets: {info.numResourcePlanets} ({info.numMines} mine{pluralSuffix})";
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
            var owner = sys.starBase.Get().owner;
            if (owner == Faction.Earthling) {
                return MapNodeColor.Cyan;
            }
            if (owner == Faction.Draklid) {
                return MapNodeColor.Purple;
            }
            if (owner == Faction.Krigia) {
                return MapNodeColor.Red;
            }
            if (owner == Faction.Wertu) {
                return MapNodeColor.Blue;
            }
            if (owner == Faction.Vespion) {
                return MapNodeColor.LightBlue;
            }
            if (owner == Faction.Zyth) {
                return MapNodeColor.Green;
            }
            if (owner == Faction.Phaa) {
                return MapNodeColor.Lime;
            }
            throw new Exception("unexpected faction: " + owner.ToString());
        };

        _starBase.SetColor(baseColor());
    }

    private void OnFirstPlayerEnter(Faction faction) {
        if (sys.starBase.id != 0) {
            _starBase.Visible = true;
            SetStarBaseColor();
            if (RpgGameState.instance.FactionsAtWar(sys.starBase.Get().owner, faction)) {
                GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/enemy_base_detected.wav"));
            }
        }

        if (sys.artifact != null) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_detected.wav"));
        }
    }

    private void CollectResources() {
        if (RpgGameState.instance.day % 30 == 0) {
            GarrisonCollectResources();
            return;
        }
    }

    private void GarrisonCollectResources() {
        var starBase = sys.starBase.Get();

        if (starBase.garrison.Count == 0) {
            return;
        }

        Vessel v = null;
        foreach (var x in starBase.garrison) {
            if (v == null || v.Design().cargoSpace < x.Get().Design().cargoSpace) {
                v = x.Get();
            }
        }

        TransferResources(v.Design().cargoSpace);
    }

    private void TransferResources(int limit) {
        var starBase = sys.starBase.Get();

        Func<int, int, int> collect = (int price, int amount) => {
            int collectAmount = QMath.ClampMax(amount, limit);
            limit -= collectAmount;
            RpgGameState.instance.credits += price * collectAmount;
            return collectAmount;
        };

        foreach (var p in sys.resourcePlanets) {
            if (!p.hasMine) {
                continue;
            }

            int mineralsCollected = collect(starBase.MineralsSellPrice().value, p.mineralsCollected);
            starBase.mineralsStock += mineralsCollected;
            p.mineralsCollected -= mineralsCollected;

            int organicCollected = collect(starBase.OrganicSellPrice().value, p.organicCollected);
            starBase.organicStock += organicCollected;
            p.organicCollected -= organicCollected;

            int powerCollected = collect(starBase.PowerSellPrice().value, p.powerCollected);
            starBase.powerStock += powerCollected;
            p.powerCollected -= powerCollected;
        }
    }

    public void ProcessDay() {
        if (_starBase != null) {
            _starBase.ProcessDay();
        }

        if (sys.starBase.id == 0) {
            return;
        }

        var starBase = sys.starBase.Get();
        if (starBase.owner == Faction.Earthling) {
            CollectResources();
        }
    }
}
