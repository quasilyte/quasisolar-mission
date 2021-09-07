using Godot;
using System.Collections.Generic;

public class TempestProjectileNode : Node2D, IProjectile {
    private Pilot _firedBy;

    private float _travelled = 0;

    private static PackedScene _scene = null;
    public static TempestProjectileNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ammo/TempestProjectileNode.tscn");
        }
        var o = (TempestProjectileNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public WeaponDesign GetWeaponDesign() { return TempestWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    public void OnImpact() {
        Explode();
    }

    public void Explode() {
        var explosion = TempestExplosionNode.New();
        explosion.GlobalPosition = GlobalPosition;
        GetParent().GetParent().AddChild(explosion);
        QueueFree();
    }

    public override void _Ready() {
        GetNode<Area2D>("Area2D").Connect("area_entered", this, nameof(OnCollision));
    }

    public override void _Process(float delta) {
        if (_travelled < GetWeaponDesign().range) {
            float traveled = GetWeaponDesign().projectileSpeed * delta;
            Position += Transform.x.Normalized() * traveled;
            _travelled += traveled;
        } else {
            Rotation += delta * 10;
        }
    }

    private static HashSet<WeaponDesign> counters = new HashSet<WeaponDesign>{
        CutterWeapon.Design,
        HyperCutterWeapon.Design,
        LancerWeapon.Design,
        StormbringerWeapon.Design,
    };

    private bool CanBlock(WeaponDesign design) {
        return TempestWeapon.canBlock.Contains(design);
    }

    private void OnCollision(Area2D other) {
        if (other.GetParent() is IProjectile projectile) {
            var firedBy = projectile.FiredBy();
            if (firedBy.alliance == _firedBy.alliance) {
                return;
            }
            // TODO: can we live without this check?
            // It's needed since collisions can be processed in weird order.
            // We don't want one projectile to trigger several tempest nodes.
            // Maybe we can have a map in a parent aura and not trigger on
            // a node that is already in a map?
            if (projectile.GetProjectileNode().IsQueuedForDeletion()) {
                return;
            }
            var design = projectile.GetWeaponDesign();
            if (counters.Contains(design)) {
                Explode();
                return;
            }
            if (CanBlock(design)) {
                projectile.OnImpact();
                Explode();
                return;
            }
        }
    }
}
