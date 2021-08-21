using Godot;

public class ShieldImpulseNode : Node2D {
    private Texture _texture;

    private ShieldDesign _shield;
    private VesselNode _target;

    private Sprite _sprite;

    private float _scaleMultiplier;
    private Vector2 _maxScale;

    private static PackedScene _scene = null;
    public static ShieldImpulseNode New(VesselNode target, Texture texture, ShieldDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ShieldImpulseNode.tscn");
        }
        var o = (ShieldImpulseNode)_scene.Instance();
        o._texture = texture;
        o._target = target;
        o._shield = design;
        return o;
    }

    public override void _Ready() {
        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Texture = _texture;

        if (_target.State.vesselSize == VesselDesign.Size.Normal) {
            _maxScale = new Vector2(1.2f, 1.2f);
            _scaleMultiplier = 3.6f;
            _sprite.Scale = new Vector2(0.6f, 0.6f);
        } else if (_target.State.vesselSize == VesselDesign.Size.Large) {
            _maxScale = new Vector2(1.5f, 1.5f);
            _scaleMultiplier = 4.5f;
            _sprite.Scale = new Vector2(0.75f, 0.75f);
        } else {
            _maxScale = new Vector2(1, 1);
            _scaleMultiplier = 3.0f;
            _sprite.Scale = new Vector2(0.5f, 0.5f);
        }
        
        Position = _target.Position;
    }

    public override void _Process(float delta) {
        if (_sprite.Scale.x < _maxScale.x) {
            _sprite.Scale = new Vector2(
                _sprite.Scale.x + (delta * _scaleMultiplier),
                _sprite.Scale.y + (delta * _scaleMultiplier)
            );
        } else {
            QueueFree();
            return;
        }

        _sprite.Rotation += delta * 3;

        if (IsInstanceValid(_target)) {
            Position = _target.Position;
        }
    }
}
