using Godot;
using System;
using System.Collections.Generic;

public class StarSystemNode : Node2D {
    public StarSystem sys;

    private bool focused = false;

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

        if (sys.Visited()) {
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
    }

    public void OnPlayerEnter(Faction faction) {
        if (!sys.Visited()) {
            OnFirstPlayerEnter(faction);
        }
        sys.visitsNum++;
    }

    public List<string> GetKnownInfo() {
        if (!sys.Visited()) {
            return new List<string>{"Unvisited System"};
        }

        var lines = new List<string>();

        var hasBase = sys.starBase.id != 0;
        var starBase = hasBase ? sys.starBase.Get() : null;

        var numPlanets = sys.resourcePlanets.Count;
        int numExplored = 0;
        foreach (var planet in sys.resourcePlanets) {
            if (planet.IsExplored()) {
                numExplored++;
            }
        }

        if (hasBase) {
            var owner = starBase.owner.ToString();
            if (starBase.owner != Faction.Earthling) {
                var status = Utils.DiplomaticStatusString(RpgGameState.instance.diplomaticStatuses[starBase.owner]);
                lines.Add(owner + " System (" + status + ")");
            } else {
                lines.Add(owner + " System");    
            }
        } else {
            lines.Add("Neutral System");
        }

        if (hasBase) {
            var numShips = starBase.garrison.Count;
            var pluralSuffix = numShips == 1 ? "" : "s";
            var level = Utils.IntToRoman(starBase.level);
            lines.Add($"Base {level}: {numShips} ship" + pluralSuffix);
        }

        if (sys.HasArtifact()) {
            lines.Add("Artifact Detected");
        }

        if (numPlanets != 0) {
            if (numExplored == 0) {
                lines.Add($"Planets: {numPlanets}");
            } else {
                lines.Add($"Planets: {numPlanets} ({numExplored} explored)");
            }
        }

        return lines;
    }

    private void OnMouseEnter() {
        MapItemInfoNode.instance.Pin(this, new Vector2(0, -64), GetKnownInfo());
        focused = true;
        Update();
    }

    private void OnMouseExited() {
        MapItemInfoNode.instance.Unpin(this);
        focused = false;
        Update();
    }

    public override void _Draw() {
        if (!focused || !sys.Visited() || sys.starBase.id == 0) {
            return;
        }
        var radius = _starBase.InfluenceRadius();
        if (radius == 0) {
            return;
        }
        var starBase = sys.starBase.Get();
        Color color = Utils.FactionColor(starBase.owner);
        DrawUtils.DrawDashedCircle(this, radius, color);
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
            if (owner == Faction.Lezeona) {
                return MapNodeColor.Gray;
            }
            throw new Exception("unexpected faction: " + owner.ToString());
        };

        _starBase.SetColor(baseColor());
    }

    private void OnFirstPlayerEnter(Faction faction) {
        if (sys.starBase.id != 0) {
            _starBase.Visible = true;
            SetStarBaseColor();
            // if (RpgGameState.instance.FactionsAtWar(sys.starBase.Get().owner, faction)) {
            //     GetNode<SoundQueue>("/root/SoundQueue").AddToQueue(GD.Load<AudioStream>("res://audio/voice/enemy_base_detected.wav"));
            // }
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
