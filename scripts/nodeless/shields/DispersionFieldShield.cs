using Godot;

public class DispersionFieldShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Dispersion Field",
        description = "TODO",
        level = 2,

        activeElectromagneticDamageReceive = 0.7f,
        activeKineticDamageReceive = 0.8f,
        activeThermalDamageReceive = 1,

        duration = 1f,
        cooldown = 2.5f,
        energyCost = 14,

        sellingPrice = 2500,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public DispersionFieldShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/dispersion_shield.png";
        _audioName = "res://audio/dispersion_shield.wav";
    }
}