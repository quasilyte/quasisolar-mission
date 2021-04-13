using Godot;
using System;
using System.Collections.Generic;

class RookieBot : AbstractBot {
    private IWeapon _pointDefense = null;
    private int _pointDefenseIndex = 0;

    private bool _preferCloseRange = false;
    private bool _preferLongRange = false;
    private bool _energyUsed = false;

    private bool _hasShield = false;
    private ShieldDesign _shield;

    private float _attackCooldown = 0;
    private float _fleeDelay = 0;

    public RookieBot(VesselNode vessel) : base(vessel) {
        Func<IWeapon, bool> isLongRangeWeapon = (IWeapon w) => {
            return w is TorpedoLauncherWeapon ||
                w is MortarWeapon ||
                w is LancerWeapon ||
                w is DisruptorWeapon ||
                w is PhotonBeamWeapon ||
                w is DiskThrowerWeapon ||
                w is ReaperCannonWeapon;
        };

        for (int i = 0; i < vessel.weapons.Count; i++) {
            var w = vessel.weapons[i];
            if (w is PointDefenseLaserWeapon pointDefense) {
                _pointDefense = pointDefense;
                _pointDefenseIndex = i;
                continue;
            }

            if (w is SpreadGunWeapon) {
                _preferCloseRange = true;
                continue;
            }
            if (w is HellfireWeapon) {
                _preferCloseRange = true;
                continue;
            }

            if (isLongRangeWeapon(w)) {
                _preferLongRange = true;
                continue;
            }
        }
        _shield = vessel.shield.GetDesign();
        _hasShield = _shield != EmptyShield.Design;

        if (!_preferLongRange) {
            _preferLongRange = isLongRangeWeapon(vessel.specialWeapon);
        }

        if (_preferCloseRange && _preferLongRange) {
            _preferCloseRange = false;
        }
    }

    protected override void ActImpl(float delta, BotEvents events) {
        _attackCooldown = QMath.ClampMin(_attackCooldown - delta, 0);
        _fleeDelay = QMath.ClampMin(_fleeDelay - delta, 0);

        _energyUsed = false;

        if (_currentTarget == null) {
            ChooseTarget();
        }

        // If target is destroyed, set it to null.
        if (_currentTarget != null && !IsInstanceValid(_currentTarget.Vessel)) {
            SetTarget(null);
        }

        ActMove();
        ActDefense(events);
        ActFight();
    }

    private void SetTarget(Pilot target) {
        _currentTarget = target;
        if (_currentTarget != null) {
            foreach (var w in _vessel.weapons) {
                if (w is ZapWeapon zapGun) {
                    zapGun.SetTargetLock(_currentTarget.Vessel);
                }
            }
        }
    }

    private void ChooseTarget() {
        var offset = QRandom.IntRange(0, _pilot.Enemies.Count - 1);
        var numTried = 0;
        for (int i = offset; numTried < _pilot.Enemies.Count; i++, numTried++) {
            if (i >= _pilot.Enemies.Count) {
                i = 0;
            }
            var e = _pilot.Enemies[i];
            if (e.Active) {
                SetTarget(e);
                break;
            }
        }
    }

    private Vector2 FleeWaypoint() {
        var fleePos = ((_vessel.Position - TargetPosition()).Normalized() * 256) + _vessel.Position;
        return QMath.RandomizedLocation(fleePos, 32);
    }

    private Vector2 CorrectedPos(Vector2 pos) {
        if (IsOutOfScreen(pos)) {
            return QMath.RandomizedLocation(_screenCenter, 512);
        }
        return pos;
    }

    private Vector2 PickWaypoint(float roll) {
        if (roll < 0.15) {
            return QMath.RandomizedLocation(_vessel.Position, 96);
        }

        var targetDistance = TargetDistance();

        if (_preferLongRange && targetDistance < 192 && roll < 0.9) {
            return FleeWaypoint();
        }

        if (roll < 0.55 || (targetDistance > 300 && roll < 0.65)) {
            var pos = _vessel.Position.MoveToward(TargetPosition(), 192);
            if (_preferLongRange && pos.DistanceTo(TargetPosition()) < 192) {
                return FleeWaypoint();
            }
            return pos;
        }

        if (_preferLongRange) {
            for (int i = 0; i < 3; i++) {
                var pos = QMath.RandomizedLocation(TargetPosition(), 300);
                if (pos.DistanceTo(TargetPosition()) >= 160) {
                    return pos;
                }
            }
        }

        if (_preferCloseRange) {
            return QMath.RandomizedLocation(TargetPosition(), 64);
        }

        if (targetDistance > 500) {
            return QMath.RandomizedLocation(TargetPosition(), 160);
        }

        return QMath.RandomizedLocation(TargetPosition(), 192);
    }

    private void ActMove() {
        if (_vessel.HasWaypoints()) {
            if (_preferCloseRange && _vessel.CurrentWaypointDistance() > 350) {
                if (_currentTarget != null && TargetDistance() < 300) {
                    var pos = QMath.RandomizedLocation(TargetPosition(), 32);
                    _actions.ChangeWaypoint(pos);
                }
            }
            if (_preferLongRange && _fleeDelay == 0 && _currentTarget != null && TargetDistance() < 160) {
                var fleeRoll = QRandom.Float();
                if (fleeRoll < 0.05) {
                    _fleeDelay = 3 + fleeRoll;
                    _actions.ChangeWaypoint(CorrectedPos(PickWaypoint(fleeRoll)));
                }
            }
            return;
        }

        if (IsOutOfScreen(_vessel.Position)) {
            _actions.AddWaypoint(QMath.RandomizedLocation(_screenCenter, 512));
            return;
        }

        if (_currentTarget == null) {
            return;
        }

        var roll = QRandom.Float();
        if (TargetDistance() > 300 && roll < 0.1 && numActiveEnemies() > 1) {
            var closest = QMath.NearestEnemy(_vessel.Position, _pilot);
            if (closest != _currentTarget) {
                SetTarget(closest);
            }
        }

        _actions.AddWaypoint(CorrectedPos(PickWaypoint(roll)));
    }

    private bool CanReduceDamage(DamageKind damageKind) {
        if (_shield == PhaserShield.Design) {
            return true;
        }

        if (damageKind == DamageKind.Energy) {
            return _shield.activeEnergyDamageReceive != 1;
        }
        if (damageKind == DamageKind.Kinetic) {
            return _shield.activeKineticDamageReceive != 1;
        }
        if (damageKind == DamageKind.Thermal) {
            return _shield.activeThermalDamageReceive != 1;
        }
        return false;
    }

    private void MaybeUseShield(BotEvents events) {
        if (!_hasShield || _energyUsed || !_vessel.shield.CanActivate(_vessel.State)) {
            return;
        }

        if (events.targetedByZap) {
            if (_shield.activeEnergyDamageReceive != 1) {
                Shield();
                return;
            }
        }

        foreach (Area2D other in events.closeRangeCollisions) {
            if (!IsInstanceValid(other)) {
                continue;
            }

            if (other.GetParent() is IProjectile projectile) {
                var firedBy = projectile.FiredBy();
                if (firedBy.alliance == _pilot.alliance) {
                    continue;
                }
                if (!CanReduceDamage(projectile.GetWeaponDesign().damageKind)) {
                    continue;
                }
                Shield();
                return;
            }
        }
    }

    private void ActDefense(BotEvents events) {
        MaybeUseShield(events);

        if (_pointDefense != null) {
            MaybeBlockRockets();
        }

        if (_vessel.specialWeapon.GetDesign() == RestructuringRayWeapon.Design) {
            MaybeHealAlly();
        }
    }

    private void MaybeHealAlly() {
        if (_energyUsed) {
            return;
        }
        // Do not waste energy on healing if target is nearby.
        if (_currentTarget != null && TargetDistance() < 200) {
            return;
        }
        // Never spend the backup energy on healing.
        if (!CanShootForFree(RestructuringRayWeapon.Design.energyCost)) {
            return;
        }

        var ally = QMath.NearestAlly(_vessel.Position, _pilot);
        if (ally == null) {
            return;
        }
        var hpMissing = ally.Vessel.State.maxHp - ally.Vessel.State.hp;
        if (hpMissing < 40) {
            return;
        }
        if (ally.Vessel.Position.DistanceTo(_vessel.Position) > RestructuringRayWeapon.Design.botHintRange) {
            return;
        }
        if (!_vessel.specialWeapon.CanFire(_vessel.State, ally.Vessel.Position)) {
            return;
        }
        FireSpecial(ally.Vessel.Position);
    }

    private void MaybeBlockRockets() {
        if (_energyUsed) {
            return;
        }
        if (!_pointDefense.CanFire(_vessel.State, _vessel.Position)) {
            return;
        }
        foreach (var n in _vessel.GetTree().GetNodesInGroup("rockets")) {
            if (n is Rocket rocket) {
                if (rocket.FiredBy().alliance == _pilot.alliance) {
                    continue;
                }
                var dist = rocket.Position.DistanceTo(_vessel.Position);
                if (dist < 100) {
                    Fire(_pointDefenseIndex, _vessel.Position);
                    return;
                }
            }
            if (n is TorpedoNode torpedo) {
                if (torpedo.FiredBy().alliance == _pilot.alliance) {
                    continue;
                }
                var dist = torpedo.Position.DistanceTo(_vessel.Position);
                if (dist < 150) {
                    Fire(_pointDefenseIndex, _vessel.Position);
                    return;
                }
            }
        }
    }

    private void ActFight() {
        if (_currentTarget == null) {
            return;
        }

        UseSpecialWeapon();
        UseNormalWeapons();
    }

    private void UseSpecialWeapon() {
        var design = _vessel.specialWeapon.GetDesign();

        if (design == EmptyWeapon.Design) {
            return;
        }

        if (design == TorpedoLauncherWeapon.Design) {
            if (TargetDistance() < 750 && _vessel.specialWeapon.CanFire(_vessel.State, TargetPosition())) {
                FireSpecial(TargetPosition());
            }
            return;
        }

        if (design == MortarWeapon.Design || design == ReaperCannonWeapon.Design || design == HarpoonWeapon.Design || design == DisruptorWeapon.Design) {
            var targetCursor = CalculateFireTarget(_vessel.specialWeapon);
            if (targetCursor != Vector2.Zero) {
                FireSpecial(targetCursor);
            }
            return;
        }

        if (design == PhotonBeamWeapon.Design) {
            if (CalculateFireTarget(_vessel.specialWeapon) != Vector2.Zero) {
                FireSpecial(TargetPosition());
            }
            return;
        }
    }

    private Vector2 CalculateFireTarget(IWeapon w) {
        var design = w.GetDesign();

        if (design.energyCost != 0 && _energyUsed) {
            return Vector2.Zero;
        }

        Vector2 cursor;
        var snipeRoll = design.botHintSnipe == 0 ? 100 : QRandom.Float();
        if (snipeRoll <= design.botHintSnipe) {
            var targetPos = TargetPosition();
            var dist = targetPos.DistanceTo(_vessel.Position);
            var predictedPos = targetPos + _currentTarget.Vessel.State.velocity * (dist / design.projectileSpeed);
            cursor = QMath.RandomizedLocation(predictedPos, 16 * design.botHintScatter);
        } else {
            cursor = QMath.RandomizedLocation(TargetPosition(), 24 * design.botHintScatter);
        }
        var closeRange = TargetDistance() < 150;

        var effectiveRange = design.botHintRange != 0 ? design.botHintRange : design.range;

        if (TargetDistance() <= effectiveRange && w.CanFire(_vessel.State, cursor)) {
            if (!CanShootForFree(design.energyCost) && !closeRange) {
                // Do not waste energy for shooting at the
                // targets that are way too far.
                return Vector2.Zero;
            }
            // Rocket launcher fires only in forward direction.
            // If target is to the sides (or behind), do not use it.
            if (design.botHintEffectiveAngle != 0) {
                var dstRotation = TargetPosition().AngleToPoint(_vessel.Position);
                if (Math.Abs(QMath.RotationDiff(dstRotation, _vessel.Rotation)) > design.botHintEffectiveAngle) {
                    return Vector2.Zero;
                }
            }

            return cursor;
        }

        return Vector2.Zero;
    }

    private void MaybeUseWeapon(int weaponIndex) {
        if (_attackCooldown != 0) {
            return;
        }

        var w = _vessel.weapons[weaponIndex];
        var targetCursor = CalculateFireTarget(w);
        if (targetCursor != Vector2.Zero) {
            Fire(weaponIndex, targetCursor);
        }
    }

    private void UseNormalWeapons() {
        if (_attackCooldown != 0) {
            return;
        }

        if (QRandom.Bool()) {
            MaybeUseWeapon(0);
            MaybeUseWeapon(1);
        } else {
            MaybeUseWeapon(1);
            MaybeUseWeapon(0);
        }
    }

    private void Shield() {
        _actions.ShieldAction();
        _energyUsed = true;
    }

    private void FireSpecial(Vector2 target) {
        _attackCooldown = 0.08f;
        _actions.SpecialAction(target);
        if (_vessel.specialWeapon.GetDesign().energyCost != 0) {
            _energyUsed = true;
        }
    }

    private void Fire(int weaponIndex, Vector2 target) {
        _attackCooldown = 0.08f;
        _actions.FireAction(weaponIndex, target);
        if (_vessel.weapons[weaponIndex].GetDesign().energyCost != 0) {
            _energyUsed = true;
        }
    }

    private bool CanShootForFree(float energy) {
        return _vessel.State.energy >= energy;
    }
}
