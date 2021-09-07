using Godot;

public class PhaserShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Phaser",
        description = "TODO",
        level = 3,

        activeElectromagneticDamageReceive = 1,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 1,

        duration = 1.75f,
        cooldown = 8,
        energyCost = 25,

        sellingPrice = 13000,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public PhaserShield(Pilot pilot): base(pilot) {
        _audioName = "res://audio/phaser_shield.wav";
    }

    public override void Activate(VesselState state) {
        base.Activate(state);
        _pilot.Vessel.EnablePhasing(Design.duration);
    }
}