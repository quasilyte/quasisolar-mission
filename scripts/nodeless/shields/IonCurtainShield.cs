using Godot;

public class IonCurtainShield : AbstractShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Ion Curtain",
        description = "TODO",
        level = 1,

        activeElectromagneticDamageReceive = 0.60f,
        activeKineticDamageReceive = 1,
        activeThermalDamageReceive = 1,
        hpBonus = +20,

        duration = 1.5f,
        cooldown = 6,
        energyCost = 10,

        sellingPrice = 1700,
    };
    public override ShieldDesign GetDesign() { return Design; }

    public IonCurtainShield(Pilot pilot): base(pilot) {
        _textureName = "res://images/ion_shield.png";
        _audioName = "res://audio/ion_shield.wav";
    }
}