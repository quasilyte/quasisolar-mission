using Godot;

public class ShieldAuraNode : Node2D {
    private Texture _texture;

    private float _hp;
    private ShieldDesign _shield;
    private VesselNode _target;

    private Sprite _sprite;

    private static PackedScene _scene = null;
    public static ShieldAuraNode New(VesselNode target, Texture texture, ShieldDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ShieldAuraNode.tscn");
        }
        var o = (ShieldAuraNode)_scene.Instance();
        o._texture = texture;
        o._target = target;
        o._shield = design;
        return o;
    }

    public override void _Ready() {
        _hp = _shield.duration * _target.State.stats.shieldDurationRate;

        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Texture = _texture;
        if (_target.State.vesselSize == VesselDesign.Size.Normal) {
            _sprite.Scale = new Vector2(1.2f, 1.2f);
        } else if (_target.State.vesselSize == VesselDesign.Size.Large) {
            _sprite.Scale = new Vector2(1.55f, 1.55f);
        }
        Position = _target.Position;
    }

    public override void _Process(float delta) {
        if (!IsInstanceValid(_target)) {
            _hp = 0;
        }
        _hp -= delta;
        if (_hp <= 0) {
            QueueFree();
            return;
        }
        if (_shield.visualAuraRotates) {
            _sprite.Rotation += delta;
        }

        Position = _target.Position;
    }
}
