using Godot;
using System;

public class ArenaViewportContainer : ViewportContainer {
    private Pilot _owner;

    private static PackedScene _scene = null;
    public static ArenaViewportContainer New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ArenaViewportContainer.tscn");
        }
        var o = (ArenaViewportContainer)_scene.Instance();
        o._owner = owner;
        return o;
    }

    public override void _Ready() {
        // var camera = ArenaCameraNode.New(_owner.Vessel);
        // var viewport = GetNode<Viewport>("Viewport");
        // viewport.AddChild(camera);
        // camera.Current = true;
    }
}
