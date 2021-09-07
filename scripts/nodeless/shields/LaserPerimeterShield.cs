using Godot;

public class LaserPerimeterShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Laser Perimeter",
        description = "TODO",
        level = 2,

        activeElectromagneticDamageReceive = 0.3f,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 1,

        duration = 1.75f,
        cooldown = 10,
        energyCost = 19,

        sellingPrice = 4000,
        researchRequired = true,

        visualAuraRotates = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public LaserPerimeterShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/acid_shield.png";
        _audioName = "res://audio/heat_shield.wav";
    }
}