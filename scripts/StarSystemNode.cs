using Godot;
using System;
using System.Collections.Generic;

public class StarSystemNode : Node2D {
    public StarSystem sys;

    private StarBaseNode _starBase;

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

        GetNode<Label>("Label").Text = sys.name;
        GetNode<Sprite>("Sprite").Frame = (int)sys.color;

        GetNode<Area2D>("Area2D").Connect("mouse_entered", this, nameof(OnMouseEnter));
        GetNode<Area2D>("Area2D").Connect("mouse_exited", this, nameof(OnMouseExited));
        GetNode<Area2D>("Area2D").Connect("input_event", this, nameof(OnAreaInput));

        if (sys.intel != null) {
            ShowStarBase();
        }
    }

    public void ShowStarBase() {
        if (sys.starBase.id != 0) {
            SetStarBaseColor();
            _starBase.Visible = true;
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
    }

    public void UpdateInfo() {
        var numPlanets = sys.resourcePlanets.Count;
        int numExplored = 0;
        foreach (var planet in sys.resourcePlanets) {
            if (planet.IsExplored()) {
                numExplored++;
            }
        }
        sys.intel = new StarSystemIntel{
            hasArtifact = sys.HasArtifact(),
            hasBase = sys.starBase.id != 0,
            numResourcePlanets = numPlanets,
            numExplored = numExplored,
        };
        if (sys.starBase.id != 0) {
            var starBase = sys.starBase.Get();
            sys.intel.baseOwner = starBase.owner;
            sys.intel.garrisonSize = starBase.garrison.Count;
            sys.intel.baseLevel = starBase.level;
            sys.intel.baseHp = starBase.hp;
        }
    }

    public List<string> GetKnownInfo() {
        if (sys.intel == null) {
            return new List<string>{"Unvisited System"};
        }

        var info = sys.intel;

        var lines = new List<string>();

        var owner = info.hasBase ? info.baseOwner.ToString() : "Neutral";
        lines.Add(owner + " System");

        if (info.hasBase) {
            var numShips = info.garrisonSize;
            var pluralSuffix = numShips == 1 ? "" : "s";
            var level = Utils.IntToRoman(info.baseLevel);
            lines.Add($"Base {level}: {info.baseHp}% HP, {numShips} ship" + pluralSuffix);
        }

        if (info.hasArtifact) {
            lines.Add("Artifact Detected");
        }

        if (info.numResourcePlanets != 0) {
            if (info.numExplored == 0) {
                lines.Add($"Planets: {info.numResourcePlanets}");
            } else {
                lines.Add($"Planets: {info.numResourcePlanets} ({info.numExplored} explored)");
            }
        }

        return lines;

        // if (info.hasBase) {
        //     Label baseLabel;
        //     if (box.HasNode("Base")) {
        //         baseLabel = box.GetNode<Label>("Base");
        //     } else {
        //         baseLabel = CreateLabel("Base");
        //         box.AddChild(baseLabel);
        //     }
        //     var numShips = info.garrisonSize;
        //     var pluralSuffix = numShips == 1 ? "" : "s";
        //     var level = Utils.IntToRoman(info.baseLevel);
        //     baseLabel.Text = $"Base {level}: {info.baseHp}% HP, {numShips} ship" + pluralSuffix;
        // } else {
        //     if (box.HasNode("Base")) {
        //         box.GetNode<Label>("Base").QueueFree();
        //     }
        // }

        // if (info.hasArtifact) {
        //     Label artifactLabel;
        //     if (box.HasNode("Artifact")) {
        //         artifactLabel = box.GetNode<Label>("Artifact");
        //     } else {
        //         artifactLabel = CreateLabel("Artifact");
        //         box.AddChild(artifactLabel);
        //     }
        //     artifactLabel.Text = "Artifact Detected";
        // }

        // if (info.numResourcePlanets != 0) {
        //     Label resourcePlanetsLabel;
        //     if (box.HasNode("ResourcePlanets")) {
        //         resourcePlanetsLabel = box.GetNode<Label>("ResourcePlanets");
        //     } else {
        //         resourcePlanetsLabel = CreateLabel("ResourcePlanets");
        //         box.AddChild(resourcePlanetsLabel);
        //     }
        //     if (info.numMines == 0) {
        //         resourcePlanetsLabel.Text = $"Resource planets: {info.numResourcePlanets}";
        //     } else {
        //         var pluralSuffix = info.numMines == 1 ? "" : "s";
        //         resourcePlanetsLabel.Text = $"Resource planets: {info.numResourcePlanets} ({info.numMines} mine{pluralSuffix})";
        //     }
        // }
    }

    private void OnMouseEnter() {
        MapItemInfoNode.instance.Pin(this, new Vector2(0, -64), GetKnownInfo());
    }

    private void OnMouseExited() {
        MapItemInfoNode.instance.Unpin(this);
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

        if (sys.HasArtifact()) {
            GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/artifact_detected.wav"));
        }
    }

    public void ProcessDay() {
        if (_starBase != null) {
            _starBase.ProcessDay();
        }

        if (sys.starBase.id == 0) {
            return;
        }
    }
}
