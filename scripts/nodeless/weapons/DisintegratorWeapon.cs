using Godot;

public class DisintegratorWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Disintegrator",
        level = 1,
        researchRequired = false,
        description = "TODO",
        targeting = "any direction, chargable",
        sellingPrice = 3000,
        cooldown = 1.25f,
        chargable = true,
        energyCost = 0,
        range = 350.0f,
        damage = 4,
        damageKind = DamageKind.Energy,
        projectileSpeed = 250.0f,
        botHintSnipe = 0.6f,
        isSpecial = true,
    };

    private float _charge = 0;
    private int _powerLevel = 0;
    private EnergyBoltChargerNode _node = null;

    private const float ENERGY_COST_PER_CHARGE = 6;
    private const int MAX_CHARGE_LEVEL = 4;
    private const float SECONDS_PER_CHARGE = 0.75f;

    public DisintegratorWeapon(Pilot owner) : base(Design, owner) { }

    public override void Charge(float delta) {
        if (_cooldown == 0) {
            _charge += delta;
        }
        if (_node == null && _charge != 0) {
            _node = EnergyBoltChargerNode.New();
            _node.Position += _node.Transform.x * 15;
            _owner.Vessel.AddChild(_node);
        }
    }

    public override void Process(VesselState state, float delta) {
        base.Process(state, delta);

        if (_charge > SECONDS_PER_CHARGE && _powerLevel < MAX_CHARGE_LEVEL) {
            if (state.CanConsumeEnergy(ENERGY_COST_PER_CHARGE)) {
                state.ConsumeEnergy(ENERGY_COST_PER_CHARGE);
                _powerLevel++;
                _charge = 0;
                var particles = _node.GetNode<CPUParticles2D>("CPUParticles2D");
                particles.Amount++;
                if (_powerLevel == MAX_CHARGE_LEVEL) {
                    particles.Texture = GD.Load<Texture>("res://images/ammo/Disintegrator_Max.png");
                }
            }
        }
    }

    protected override void CreateProjectile(Vector2 cursor) {
        var projectile = EnergyBoltNode.New(_owner);
        projectile.chargeLevel = _powerLevel;
        projectile.Position = _owner.Vessel.Position + new Vector2(30, 0);
        projectile.Position += _owner.Vessel.Transform.x * 10;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();

        var particles = projectile.GetNode<CPUParticles2D>("CPUParticles2D");
        particles.Amount = 2 + _powerLevel;
        if (_powerLevel == MAX_CHARGE_LEVEL) {
            particles.Texture = GD.Load<Texture>("res://images/ammo/Disintegrator_Max.png");
        }

        _owner.Vessel.GetParent().AddChild(projectile);

        _charge = 0;
        _powerLevel = 0;
        if (_node != null) {
            _node.QueueFree();
            _node = null;
        }
    }
}
