using Godot;
using System.Collections.Generic;
using System;
using System.Collections.Generic;

public class MapItemInfoNode : Node2D {
    public static MapItemInfoNode instance = null;

    private PanelContainer _panel;

    private List<Label> _labels = new List<Label>();

    private Node2D _pinnedTo;
    private Vector2 _offset;

    private static PackedScene _scene = null;
    public static MapItemInfoNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MapItemInfoNode.tscn");
        }
        var o = (MapItemInfoNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        _panel = GetNode<PanelContainer>("PanelContainer");
    }

    public void Unpin(Node2D target) {
        if (_pinnedTo != target) {
            return;
        }
        _pinnedTo = null;
        Visible = false;
    }

    public override void _Process(float delta) {
        if (_pinnedTo != null) {
            Position = _pinnedTo.GlobalPosition + _offset;
        }
    }

    public void Pin(Node2D target, Vector2 offset, List<string> lines) {
        if (_pinnedTo != null) {
            return;
        }

        _pinnedTo = target;
        _offset = offset;

        var box = _panel.GetNode<VBoxContainer>("VBoxContainer");

        foreach (var label in _labels) {
            box.RemoveChild(label);
            label.QueueFree();
        }
        _labels.Clear();

        for (int i = 0; i < lines.Count; i++) {
            var label = new Label();
            label.Align = Label.AlignEnum.Center;
            label.Text = lines[i];

            box.AddChild(label);
            _labels.Add(label);
        }

        Visible = true;
    }
}
