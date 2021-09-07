using Godot;

public class DiffuserShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Diffuser",
        description = "TODO",
        level = 3,

        activeElectromagneticDamageReceive = 0.1f,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 1,

        duration = 2,
        cooldown = 9,
        energyCost = 20,

        sellingPrice = 10000,
        researchRequired = true,

        visualAuraRotates = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public DiffuserShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/diffuser_shield.png";
        _audioName = "res://audio/heat_shield.wav";
    }
}