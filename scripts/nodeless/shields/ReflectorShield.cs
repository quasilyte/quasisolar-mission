using Godot;

public class ReflectorShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Reflector",
        description = "TODO",
        level = 2,

        activeEnergyDamageReceive = 1,
        activeKineticDamageReceive = 0.6f,
        activeThermalDamageReceive = 0.4f,

        duration = 1.6f,
        cooldown = 9,
        energyCost = 20,

        sellingPrice = 3400,
        researchRequired = true,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public ReflectorShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/reflector_shield.png";
        _audioName = "res://audio/reflector_shield.wav";
    }
}