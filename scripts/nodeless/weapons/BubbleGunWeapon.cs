using Godot;

public class BubbleGunWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Bubble Gun",
        level = 1,
        description = "TODO",
        targeting = "backward-only, projectiles",
        sellingPrice = 3500,
        cooldown = 0.45f,
        energyCost = 5.0f,
        range = -7,
        damage = 11.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 130.0f,
        botHintSnipe = 0,
        // botHintEffectiveAngle = -0.8f,
        botHintRange = 350,
    };

    public BubbleGunWeapon(Pilot owner) : base(Design, owner) { }

    protected override void CreateProjectile(Vector2 cursor) {
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Bubble_Gun.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);

        var projectile = BubbleNode.New(_owner);
        projectile.Position = _owner.Vessel.Position;
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
