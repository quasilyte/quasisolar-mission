using Godot;
using System.Collections.Generic;

public class SwarmSpawnerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Swarm Spawner",
        level = 1,
        description = "TODO",
        extraDescription = "Can't launch more than 3 swarms at once",
        targeting = "any direction, auto fighters",
        special = "cripples target rotation on hit",
        sellingPrice = 4500,
        cooldown = 2.5f,
        energyCost = 20,
        range = 3000,
        damage = 2,
        burst = 5,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 160,
        isSpecial = true,
        ignoresAsteroids = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    const int MAX_PROJECTILES = 3;

    private HashSet<SwarmNode> _activeProjectiles = new HashSet<SwarmNode>();

    public SwarmSpawnerWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0 || !state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        _activeProjectiles.RemoveWhere(x => !Godot.Object.IsInstanceValid(x));
        return _activeProjectiles.Count < MAX_PROJECTILES;
    }

    public void Process(VesselState state, float delta) {
        _cooldown = QMath.ClampMin(_cooldown - delta, 0);
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);

        var swarm = SwarmNode.New(_owner);
        swarm.GlobalTransform = _owner.Vessel.GlobalTransform;
        _activeProjectiles.Add(swarm);
        var nearest = QMath.NearestEnemy(cursor, _owner);
        if (nearest != null) {
            swarm.Start(nearest.Vessel);
        } else {
            swarm.Start(null);
        }
        _owner.Vessel.GetParent().AddChild(swarm);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Swarm_Spawner.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
