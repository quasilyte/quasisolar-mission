using Godot;

public class DeceleratorShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Decelerator",
        description = "TODO",
        level = 2,

        activeElectromagneticDamageReceive = 1,
        activeKineticDamageReceive = 0.6f,
        activeThermalDamageReceive = 0.4f,
        hpBonus = +35,

        duration = 1.6f,
        cooldown = 9,
        energyCost = 20,

        sellingPrice = 3400,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public DeceleratorShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/decelerator_shield.png";
        _audioName = "res://audio/decelerator_shield.wav";
    }
}