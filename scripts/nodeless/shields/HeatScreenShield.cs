using Godot;

public class HeatScreenShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Heat Screen",
        description = "TODO",
        level = 1,

        activeElectromagneticDamageReceive = 0.8f,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 0.8f,

        duration = 0.8f,
        cooldown = 5,
        energyCost = 8,

        sellingPrice = 1750,

        visualAuraRotates = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public HeatScreenShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/heat_shield.png";
        _audioName = "res://audio/heat_shield.wav";
    }
}