using Godot;
using System;

public class MapHumanNode : Node2D {
    public SpaceUnitNode node;

    private RpgGameState _gameState;
    private SpaceUnit _unit;

    private Vector2 _dest = Vector2.Zero;

    private Label _destDistValueLabel;

    private static PackedScene _scene = null;
    public static MapHumanNode New(SpaceUnitNode unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MapHumanNode.tscn");
        }
        var o = (MapHumanNode)_scene.Instance();
        o.node = unit;
        return o;
    }

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        _unit = _gameState.humanUnit.Get();
        _destDistValueLabel = GetNode<Label>("DestinationDistanceValue");
        node.Connect("PositionChanged", this, nameof(OnPositionChanged));
        node.Connect("DestinationReached", this, nameof(OnDestinationReached));
        if (node.unit.waypoint != Vector2.Zero) {
            SetDestination(node.unit.waypoint);
        }
    }

    public override void _Draw() {
        // FIXME: DrawCircle is slow.
        DrawUtils.DrawCircle(this, RpgGameState.RadarRange(), Color.Color8(200, 200, 200));
        if (node.GetDestination() != Vector2.Zero) {
            DrawLine(Vector2.Zero, node.GetDestination() - GlobalPosition, Color.Color8(0x4b, 0xc2, 0x75), 1);
            return;
        }
        if (_dest != Vector2.Zero) {
            DrawLine(Vector2.Zero, _dest - GlobalPosition, Color.Color8(0xe0 ,0x07, 0x07), 1);
            return;
        }
    }

    public override void _Process(float delta) {
        if (_dest != Vector2.Zero) {
            if (node.GlobalPosition.DistanceTo(_dest) < ((_gameState.fuel * 2) - 1)) {
                node.SetDestination(_dest);
                _dest = Vector2.Zero;
                Update();
            }
        }
    }

    public void SetDestination(Vector2 dest) {
        _dest = dest;
        node.SetDestination(Vector2.Zero);
        UpdateDestDistValue(dest);
        Update();
        _destDistValueLabel.Visible = true;
    }

    public void UnsetDestination() {
        _dest = Vector2.Zero;
        node.SetDestination(Vector2.Zero);
        _destDistValueLabel.Visible = false;
        Update();
    }

    private void OnDestinationReached() {
        _unit.pos = node.GlobalPosition;
        _destDistValueLabel.Visible = false;
    }

    private void OnPositionChanged(float traveled) {
        GlobalPosition = node.GlobalPosition;
        _gameState.fuel -= traveled / 2;
        _unit.pos = node.GlobalPosition;
        var dest = node.GetDestination();
        if (dest == Vector2.Zero) {
            return;
        }
        UpdateDestDistValue(dest);
        Update();
    }

    private void UpdateDestDistValue(Vector2 dest) {
        _destDistValueLabel.RectGlobalPosition = ((dest + node.GlobalPosition) / 2) - new Vector2(8, 32);
        _destDistValueLabel.Text = ((int)(node.GlobalPosition.DistanceTo(dest) / 2)).ToString();
    }
}
