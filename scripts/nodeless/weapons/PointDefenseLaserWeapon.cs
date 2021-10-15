using Godot;

public class PointDefenseLaserWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Point-Defense Laser",
        level = 1,
        description = "A laser system that can be used for both defense and offense",
        extraDescription = "Can hit multiple targets",
        targeting = "automatic, instant hit",
        sellingPrice = 2000,
        cooldown = 1.0f,
        energyCost = 11.0f,
        range = 175.0f,
        damage = 4.0f,
        damageKind = DamageKind.Electromagnetic,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public PointDefenseLaserWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0 || !state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        foreach (var e in _owner.Enemies) {
            if (!e.Active) {
                continue;
            }
            if (e.Vessel.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                continue;
            }
            return true;
        }
        foreach (var n in _owner.context.rockets.GetNodes()) {
            if (n is Rocket rocket) {
                if (rocket.GetWeaponDesign() == ShieldBreakerWeapon.Design) {
                    continue;
                }
                if (rocket.FiredBy().alliance == _owner.alliance) {
                    continue;
                }
                if (rocket.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                    continue;
                }
                return true;
            }
            if (n is TorpedoNode torpedo) {
                if (torpedo.FiredBy().alliance == _owner.alliance) {
                    continue;
                }
                if (torpedo.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                    continue;
                }
                return true;
            }
        }
        return false;
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
    }

    private Beam CreateBeam(bool improved, Vector2 target) {
        var color = improved ? Color.Color8(0x9c, 0x58, 0xf1) : Color.Color8(180, 255, 180);
        var beam = Beam.New(_owner.Vessel.Position, target, color, improved ? 2 : 1);
        beam.damage = Design.damage;
        if (improved) {
            beam.damage += 5;
        }
        beam.damageKind = Design.damageKind;
        return beam;
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += state.hasPointDefenseSaturator ? Design.cooldown * 0.75f : Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);        

        foreach (var e in _owner.Enemies) {
            if (!e.Active) {
                continue;
            }
            if (e.Vessel.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                continue;
            }
            var beam = CreateBeam(state.hasPointDefenseSaturator, QMath.RandomizedLocation(e.Vessel.Position, 8));
            beam.target = e.Vessel;
            _owner.Vessel.GetParent().AddChild(beam);
        }

        foreach (var n in _owner.context.rockets.GetNodes()) {
            if (n is Rocket rocket) {
                if (rocket.GetWeaponDesign() == ShieldBreakerWeapon.Design) {
                    continue;
                }
                if (rocket.FiredBy().alliance == _owner.alliance) {
                    continue;
                }
                if (rocket.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                    continue;
                }
                var beam = CreateBeam(state.hasPointDefenseSaturator, rocket.Position);
                _owner.Vessel.GetParent().AddChild(beam);
                rocket.Explode();
            }
            if (n is TorpedoNode torpedo) {
                if (torpedo.FiredBy().alliance == _owner.alliance) {
                    continue;
                }
                if (torpedo.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                    continue;
                }
                var beam = CreateBeam(state.hasPointDefenseSaturator, torpedo.Position);
                _owner.Vessel.GetParent().AddChild(beam);
                torpedo.ApplyDamage(Design.damage);
            }
        }

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/point_laser.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
