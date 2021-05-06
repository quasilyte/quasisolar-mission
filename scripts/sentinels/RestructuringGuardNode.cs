using Godot;
using System;

public class RestructuringGuardNode : SentinelNode {
    // FIXME: too much code is copied from the RestructuringRawWeapon.

    private const int MAX_BURST = 10;
    private int _burst = 0;
    private float _burstCooldown = 0;

    private RestructuringLine _line;

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

    protected override void OnDestroy() {
        if (_line != null && IsInstanceValid(_line)) {
            _burst = 0;
            _line.StartFading();
        }
    }

    protected override void ProcessFrame(float delta) {
        _burstCooldown = QMath.ClampMin(_burstCooldown - delta, 0);
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            if (_burst == 0) {
                _line.StartFading();
            }
            _vessel.State.hp += 0.6f;
            _burstCooldown += 0.2f;
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

        _line = RestructuringLine.New(_sprite, _vessel);
        GetParent().AddChild(_line);

        _attackCooldown = _design.attackCooldown;
    }
}
