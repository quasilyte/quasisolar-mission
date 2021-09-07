using Godot;

public class QuestLogEntryNode : MarginContainer {
    private static PackedScene _scene = null;
    public static QuestLogEntryNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/QuestLogEntryNode.tscn");
        }
        var o = (QuestLogEntryNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
    }
}
