using Godot;
using System;

public class ShieldSentinelNode : SentinelNode {
    private string _textureName;

    private static PackedScene _scene = null;
    public static new ShieldSentinelNode New(VesselNode vessel, SentinelDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ShieldSentinelNode.tscn");
        }
        var o = (ShieldSentinelNode)_scene.Instance();
        o._vessel = vessel;
        o._design = design;
        o.pilot = vessel.pilot;
        return o;
    }

    protected override void ProcessAttack() {}

    public override void _Ready() {
        base._Ready();
        if (_design.shield == IonCurtainShield.Design) {
            _textureName = "res://images/ion_shield_impulse.png";
        } else if (_design.shield == DeceleratorShield.Design) {
            _textureName = "res://images/decelerator_shield_impulse.png";
            _sprite.Texture = GD.Load<Texture>("res://images/sentinel/type_d_green.png");
        } else {
            throw new Exception("unexpected shield design " + _design.shield.name);
        }
    }

    public float ReduceDamage(float damage, DamageKind kind) {
        if (_attackCooldown != 0 || !CanReduceDamage(kind)) {
            return damage;
        }

        _attackCooldown = _design.attackCooldown;
        var aura = ShieldImpulseNode.New(_vessel, GD.Load<Texture>(_textureName), _design.shield);
        _vessel.GetParent().AddChild(aura);

        return AbstractShield.CalculateReducedDamage(_design.shield, damage, kind);
    }

    // FIXME: copy/pasted from a generic bot.
    // Maybe we can move this logic somewhere.
    private bool CanReduceDamage(DamageKind damageKind) {
        var shield = _design.shield;

        if (shield == PhaserShield.Design) {
            return true;
        }

        if (damageKind == DamageKind.Thermal) {
            return shield.activeThermalDamageReceive != 1;
        }
        if (damageKind == DamageKind.Kinetic) {
            return shield.activeKineticDamageReceive != 1;
        }
        if (damageKind == DamageKind.Electromagnetic) {
            return shield.activeElectromagneticDamageReceive != 1;
        }
        return false;
    }
}
