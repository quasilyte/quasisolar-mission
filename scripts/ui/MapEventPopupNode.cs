using Godot;
using System;
using System.Collections.Generic;

public class MapEventPopupNode : PopupPanel {
    public class Option {
        public string text;
        public Action apply;
        public bool disabled;
    }

    private ActionLabel _hoveredAction = null;

    class ActionLabel {
        public int id;
        public Label node;
        public bool disabled;
        public Action apply;
    }

    private string _title;
    private string _text;
    private List<Option> _options;
    private List<ActionLabel> _actionLabels = new List<ActionLabel>();

    private static PackedScene _scene = null;
    public static MapEventPopupNode New(string title, string text, List<Option> options) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/MapEventPopupNode.tscn");
        }
        var o = (MapEventPopupNode)_scene.Instance();
        o._title = title;
        o._text = text;
        o._options = options;
        return o;
    }

    public override void _Ready() {
        var root = GetNode<VBoxContainer>("Margin/VBoxContainer");

        root.GetNode<Label>("Title").Text = _title;
        root.GetNode<Label>("PanelContainer/Margin/Text").Text = _text;

        var list = root.GetNode<VBoxContainer>("OptionsMargin/VBox");
        for (int id = 0; id < _options.Count; id++) {
            var o = _options[id];

            var label = new Label();
            label.Text = $"{id+1}. {o.text}";
            label.Valign = Label.VAlign.Center;
            label.MouseFilter = Control.MouseFilterEnum.Stop;
            list.AddChild(label);

            var args = new Godot.Collections.Array{id};
            label.Connect("mouse_entered", this, nameof(OnActionMouseEntered), args);
            label.Connect("mouse_exited", this, nameof(OnActionMouseExited), args);

            if (o.disabled) {
                label.AddColorOverride("font_color", Color.Color8(0x5d, 0x60, 0x79));
            }

            _actionLabels.Add(new ActionLabel{
                id = id,
                node = label,
                disabled = o.disabled,
                apply = o.apply,
            });
        }
    }

    private void OnActionMouseEntered(int id) {
        var a = _actionLabels[id];
        _hoveredAction = a;
        if (!a.disabled) {
            _actionLabels[id].node.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
        }
    }

    private void OnActionMouseExited(int id) {
        var a = _actionLabels[id];
        _hoveredAction = null;
        if (!a.disabled) {
            _actionLabels[id].node.AddColorOverride("font_color", Color.Color8(255, 255, 255));
        }
    }

    public override void _Input(InputEvent e) {
        if (Input.IsActionJustPressed("tquestAction1")) {
            HotkeyExecutingAction(0);
        }
        if (Input.IsActionJustPressed("tquestAction2")) {
            HotkeyExecutingAction(1);
        }
        if (Input.IsActionJustPressed("tquestAction3")) {
            HotkeyExecutingAction(2);
        }
        if (Input.IsActionJustPressed("tquestAction4")) {
            HotkeyExecutingAction(3);
        }
        if (Input.IsActionJustPressed("tquestAction5")) {
            HotkeyExecutingAction(4);
        }

        if (e is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed) {
                if (_hoveredAction != null && !_hoveredAction.disabled) {
                    ExecuteAction(_hoveredAction);
                }
            }
        }
    }

    private void HotkeyExecutingAction(int i) {
        if (!Input.IsKeyPressed((int)KeyList.Control)) {
            return;
        }
        if (i >= _actionLabels.Count) {
            return;
        }
        var label = _actionLabels[i];
        if (label.disabled || !label.node.Visible) {
            return;
        }
        ExecuteAction(label);
    }

    private void ExecuteAction(ActionLabel action) {
        action.apply();
        var click = GD.Load<AudioStream>("res://audio/interface/textclick.wav");
        GetParent().AddChild(SoundEffectNode.New(click, -12));
        _hoveredAction = null;

        QueueFree();
    }
}
