using Godot;

public class AegisShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Aegis",
        description = "TODO",
        level = 3,

        activeElectromagneticDamageReceive = 0.25f,
        activeKineticDamageReceive = 0.3f,
        activeThermalDamageReceive = 0.5f,

        duration = 1.60f,
        cooldown = 8,
        energyCost = 25,

        sellingPrice = 11000,
        researchRequired = true,

        visualAuraRotates = false,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public AegisShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/aegis_shield.png";
        _audioName = "res://audio/dispersion_shield.wav";
    }
}