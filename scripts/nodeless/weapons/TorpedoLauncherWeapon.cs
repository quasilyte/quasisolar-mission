using Godot;

public class TorpedoLauncherWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Torpedo Launcher",
        level = 3,
        description = "TODO",
        extraDescription = "Can't launch more than 1 torpedo at once",
        targeting = "forward-only, homing missles",
        special = "torpedo can survive 1 hit from the point-defense",
        sellingPrice = 10000,
        cooldown = 5.0f,
        range = 2500,
        damage = 45,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 250,
        isSpecial = true,
        ignoresAsteroids = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private TorpedoNode _activeProjectile = null;

    public TorpedoLauncherWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (!Godot.Object.IsInstanceValid(_activeProjectile)) {
            _activeProjectile = null;
        }
        return _activeProjectile == null && _cooldown == 0;
    }

    public void Process(VesselState state, float delta) {
        _cooldown = QMath.ClampMin(_cooldown - delta, 0);
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);

        var torpedo = TorpedoNode.New(_owner);
        torpedo.GlobalTransform = _owner.Vessel.GlobalTransform;
        _activeProjectile = torpedo;
        var nearest = QMath.NearestEnemy(cursor, _owner);
        if (nearest != null) {
            torpedo.Start(nearest.Vessel);
        } else {
            torpedo.Start(null);
        }
        _owner.Vessel.GetParent().AddChild(torpedo);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Torpedo_Launcher.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
