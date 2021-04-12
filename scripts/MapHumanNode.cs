using Godot;
using System;

public class MapHumanNode : Node2D {
    public SpaceUnitNode node;
    public Player player;

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
        _destDistValueLabel = GetNode<Label>("DestinationDistanceValue");
        node.Connect("PositionChanged", this, nameof(OnPositionChanged));
        node.Connect("DestinationReached", this, nameof(OnDestinationReached));
        if (node.unit.waypoint != Vector2.Zero) {
            SetDestination(node.unit.waypoint);
        }
    }

    public override void _Draw() {
        DrawUtils.DrawCircle(this, RpgGameState.RadarRange(), Color.Color8(200, 200, 200));
        if (node.GetDestination() == Vector2.Zero) {
            return;
        }
        DrawLine(Vector2.Zero, node.GetDestination() - GlobalPosition, Color.Color8(0x4b, 0xc2, 0x75), 1);
    }

    /*
    func draw_circle_arc(center, radius, angle_from, angle_to, color):
    var nb_points = 32
    var points_arc = PackedVector2Array()

    for i in range(nb_points + 1):
        var angle_point = deg2rad(angle_from + i * (angle_to-angle_from) / nb_points - 90)
        points_arc.push_back(center + Vector2(cos(angle_point), sin(angle_point)) * radius)

    for index_point in range(nb_points):
        draw_line(points_arc[index_point], points_arc[index_point + 1], color)

    */

    public void SetDestination(Vector2 dest) {
        node.SetDestination(dest);
        UpdateDestDistValue(dest);
        Update();
        _destDistValueLabel.Visible = true;
    }

    public void UnsetDestination() {
        node.SetDestination(Vector2.Zero);
        _destDistValueLabel.Visible = false;
        Update();
    }

    private void OnDestinationReached() {
        RpgGameState.humanUnit.pos = node.GlobalPosition;
        _destDistValueLabel.Visible = false;
    }

    private void OnPositionChanged(float traveled) {
        GlobalPosition = node.GlobalPosition;
        RpgGameState.fuel -= traveled / 2;
        RpgGameState.humanUnit.pos = node.GlobalPosition;
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
