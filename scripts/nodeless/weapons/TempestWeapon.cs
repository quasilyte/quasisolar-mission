using Godot;
using System.Collections.Generic;

public class TempestWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Tempest",
        level = 3,
        description = "TODO",
        targeting = "none, circular",
        sellingPrice = 8000,
        cooldown = 7.5f,
        energyCost = 16.0f,
        projectileSpeed = 120,
        range = 70.0f,
        duration = 5,
        damage = 16,
        burst = 15,
        damageScoreMultiplier = 4,
        damageKind = DamageKind.Kinetic,
        botHintRange = 150,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public TempestWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0 && state.CanConsumeEnergy(Design.energyCost);
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);        

        var aura = TempestAuraNode.New(_owner);
        _owner.Vessel.GetParent().AddChild(aura);

        // var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Plasma_Emitter.wav"));
        // _owner.Vessel.GetParent().AddChild(sfx);
    }

    public static HashSet<WeaponDesign> canBlock = new HashSet<WeaponDesign>{
        SpreadGunWeapon.Design,
        SpreadLaserWeapon.Design,
        ScytheWeapon.Design,
        PhotonBurstCannonWeapon.Design,
        IonCannonWeapon.Design,
        HellfireWeapon.Design,
        PulseLaserWeapon.Design,
        AssaultLaserWeapon.Design,
        RocketLauncherWeapon.Design,
        HurricaneWeapon.Design,
        GreatScytheWeapon.Design,
        TwinPhotonBurstCannonWeapon.Design,
        StingerWeapon.Design,
        DiskThrowerWeapon.Design,
        CrystalCannonWeapon.Design,
        BubbleGunWeapon.Design,
        HarpoonWeapon.Design,
        DisintegratorWeapon.Design,
        ReaperCannonWeapon.Design,
        TorpedoLauncherWeapon.Design,
        DisruptorWeapon.Design,
        ShockwaveCasterWeapon.Design,
        SwarmSpawnerWeapon.Design,
    };
}
