using Godot;

public class LatticeShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Lattice",
        description = "TODO",
        level = 2,

        activeElectromagneticDamageReceive = 0.70f,
        activeKineticDamageReceive = 0.80f,
        activeThermalDamageReceive = 0.90f,
        hpBonus = +60,

        duration = 1.20f,
        cooldown = 5,
        energyCost = 13,

        sellingPrice = 4100,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public LatticeShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/lattice_shield.png";
        _audioName = "res://audio/dispersion_shield.wav";
    }
}