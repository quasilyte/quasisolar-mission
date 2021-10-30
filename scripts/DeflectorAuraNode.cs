using Godot;

public class DeflectorAuraNode : Node2D {
    private float _hp;
    private VesselNode _target;

    private Sprite _sprite;

    private static PackedScene _scene = null;
    public static DeflectorAuraNode New(VesselNode target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DeflectorAuraNode.tscn");
        }
        var o = (DeflectorAuraNode)_scene.Instance();
        o._target = target;
        return o;
    }

    public override void _Ready() {
        _hp = DeflectorShield.Design.duration * _target.State.stats.shieldDurationRate;

        _sprite = GetNode<Sprite>("Sprite");
        if (_target.State.vesselSize == VesselDesign.Size.Normal) {
            _sprite.Scale = new Vector2(1.2f, 1.2f);
        } else if (_target.State.vesselSize == VesselDesign.Size.Large) {
            _sprite.Scale = new Vector2(1.5f, 1.5f);
        }
        Position = _target.Position;

        GetNode<Area2D>("Area2D").Connect("area_entered", this, nameof(OnCollision));
    }

    private void OnCollision(Area2D other) {
        var pilot = _target.pilot;

        if (other.GetParent() is Projectile projectile) {
            var firedBy = projectile.FiredBy();
            if (firedBy.alliance == pilot.alliance) {
                return;
            }
            var design = projectile.GetWeaponDesign();
            if (DeflectorShield.CanDeflect(design)) {
                projectile.Deflect(pilot);
            }
        }
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
        _sprite.Rotation += delta * 2;

        Position = _target.Position;
    }
}
