using Godot;
using System;
using System.Collections.Generic;

class VisitorBot : GenericBot {
    private float _warpDelay = 0;

    public VisitorBot(VesselNode vessel) : base(vessel) {}

    protected override void ActImpl(float delta, BotEvents events) {
        _warpDelay = QMath.ClampMin(_warpDelay - delta, 0);

        base.ActImpl(delta, events);
    }

    protected override void UseSpecialWeapon() {
        if (TargetDistance() < CrystalCannonWeapon.Design.range) {
            return;
        }
        var desiredPos = QMath.RandomizedLocation(TargetPosition(), 60);
        var warpPos = _vessel.Position.MoveToward(desiredPos, 350);
        if (_warpDelay != 0 || !_vessel.specialWeapon.CanFire(_vessel.State, warpPos)) {
            var closest = QMath.NearestEnemy(_vessel.Position, _pilot);
            if (closest != _currentTarget) {
                SetTarget(closest);
            }
            return;
        }
        FireSpecial(warpPos);
        _warpDelay = QRandom.FloatRange(1, 4);
    }
}
