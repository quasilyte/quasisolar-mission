using Godot;
using System;

public class RestructuringGuardNode : SentinelNode {
    // FIXME: too much code is copied from the RestructuringRawWeapon.

    private const int MAX_BURST = 40;
    private int _burst = 0;
    private float _burstCooldown = 0;

    private static PackedScene _scene = null;
    public static new RestructuringGuardNode New(VesselNode vessel, SentinelDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/RestructuringGuardNode.tscn");
        }
        var o = (RestructuringGuardNode)_scene.Instance();
        o._vessel = vessel;
        o._design = design;
        o.pilot = vessel.pilot;
        return o;
    }

    private void FireAt(Vector2 target) {
        var color = Color.Color8(180, 255, 180);
        var beam = Beam.New(_sprite.GlobalPosition, target, color, 1);
        beam.weapon = _design.weapon;
        GetParent().AddChild(beam);
        _attackCooldown = _design.attackCooldown;
        
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/point_laser.wav"));
        GetParent().AddChild(sfx);
    }

    protected override void ProcessFrame(float delta) {
        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            _burstCooldown += 0.05f;

            var color = Color.Color8(0, 255, 100);
            var begin = _sprite.GlobalPosition.MoveToward(_vessel.Position, 2 * (float)(MAX_BURST - _burst));
            var beam = Beam.New(begin, _vessel.Position, color, 2);
            beam.weapon = _design.weapon;
            beam.target = _vessel;
            GetParent().AddChild(beam);
        }
    }

    protected override void ProcessAttack() {
        float missingHp = _vessel.State.initialHp - _vessel.State.hp;
        if (missingHp < 6) {
            return;
        }

        _burst = MAX_BURST;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Restructuring_Ray.wav"));
        GetParent().AddChild(sfx);

        _attackCooldown = _design.attackCooldown;
    }
}
