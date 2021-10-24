using Godot;

public class DeflectorShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Deflector",
        description = "TODO",
        level = 2,

        activeElectromagneticDamageReceive = 1,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 1,

        duration = 2.6f,
        cooldown = 8.5f,
        energyCost = 25,

        sellingPrice = 3200,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public DeflectorShield(Pilot pilot): base(pilot) {
        _audioName = "res://audio/decelerator_shield.wav";
    }

    public override void Activate(VesselState state) {
        base.Activate(state);

        _lastAura = DeflectorAuraNode.New(_pilot.Vessel);
        _pilot.Vessel.GetParent().AddChild(_lastAura);
    }

    public static bool CanDeflect(WeaponDesign design) {
        return design == PulseLaserWeapon.Design ||
            design == AssaultLaserWeapon.Design ||
            design == SpreadLaserWeapon.Design;
    }
}