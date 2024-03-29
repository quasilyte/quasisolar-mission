using Godot;
using System;

public class DamageScoreNode : Node2D {
    private Color _color;
    private int _damage;
    private bool _damageReduced;
    private bool _critical;

    private static PackedScene _scene = null;
    public static DamageScoreNode New(int damage, Color color, bool damageReduced, bool critical) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DamageScoreNode.tscn");
        }
        var o = (DamageScoreNode)_scene.Instance();
        o._color = color;
        o._damage = damage;
        o._damageReduced = damageReduced;
        o._critical = critical;
        return o;
    }

    public override void _Ready() {
        Modulate = _color;
        var text = _damage.ToString();
        if (_critical) {
            text += "!";
        }
        GetNode<Label>("Value").Text = text;
        if (_damageReduced) {
            GetNode<Sprite>("Shield").Visible = true;
        }
    }

    public override void _Process(float delta) {
        if (Modulate.a <= 0.1) {
            QueueFree();
            return;
        }

        var m = Modulate;
        Modulate = new Color(m.r, m.g, m.b, m.a - 0.02f);

        Position = new Vector2(Position.x, Position.y - 75 * delta);
    }
}
