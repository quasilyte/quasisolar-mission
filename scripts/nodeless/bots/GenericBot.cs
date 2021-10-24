using Godot;
using System;
using System.Collections.Generic;

class GenericBot : AbstractBot {
    private IWeapon _antiAsteroid = null;
    private int _antiAsteroidIndex = 0;
    private Asteroid _targetedAsteroid = null;
    private float _asteroidDamageDelivered = 0;

    private float _maxAdvanceDist = 0;

    private IWeapon _pointDefense = null;
    private int _pointDefenseIndex = 0;

    private bool _preferCloseRange = false;
    private bool _preferLongRange = false;
    private bool _energyUsed = false;

    private bool _hasAfterburner = false;
    private float _afterburnedEvasionCooldown = 0;
    private float _afterburnerChargeCooldown = 0;

    private bool _hasTempest = false;
    private float _tempestDuration = 0;

    private bool _hasShield = false;
    private ShieldDesign _shield;

    private float _attackCooldown = 0;
    private float _fleeDelay = 0;

    private bool _followingAlly = false;

    private bool _chargingWeapon = false;
    private float _weaponCharge = 0;
    private float _chargeGoal = 0;

    public GenericBot(VesselNode vessel) : base(vessel) {
        var hasHellfire = false;

        Func<IWeapon, bool> isLongRangeWeapon = (IWeapon w) => {
            return w is TorpedoLauncherWeapon ||
                w is MortarWeapon ||
                w is MjolnirWeapon ||
                w is LancerWeapon ||
                w is DisruptorWeapon ||
                w is PhotonBeamWeapon ||
                w is DiskThrowerWeapon ||
                w is SwarmSpawnerWeapon ||
                w is BubbleGunWeapon ||
                w is ReaperCannonWeapon ||
                w is DisintegratorWeapon;
        };

        _maxAdvanceDist = QRandom.FloatRange(450, 700);

        if (vessel.specialWeapon.GetDesign() == AfterburnerWeapon.Design) {
            _hasAfterburner = true;
        } else if (vessel.specialWeapon.GetDesign() == TempestWeapon.Design) {
            _hasTempest = true;
        }

        for (int i = 0; i < vessel.weapons.Count; i++) {
            var w = vessel.weapons[i];

            // TODO: make it possible to use Shockwave Caster as AA.
            int aaRating = AntiAsteroidRating(w);
            if (aaRating != 0) {
                if (_antiAsteroid == null || aaRating > AntiAsteroidRating(_antiAsteroid)) {
                    _antiAsteroid = w;
                    _antiAsteroidIndex = i;
                }
            }

            if (w is PointDefenseLaserWeapon pointDefense) {
                _pointDefense = pointDefense;
                _pointDefenseIndex = i;
                continue;
            }

            if (w is SpreadGunWeapon) {
                _preferCloseRange = true;
                continue;
            }
            if (w is SpreadLaserWeapon) {
                _preferCloseRange = true;
                continue;
            }
            if (w is HellfireWeapon) {
                _preferCloseRange = true;
                hasHellfire = true;
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

        if (hasHellfire && _preferLongRange) {
            _preferLongRange = false;
        }
        if (_preferCloseRange && _preferLongRange) {
            _preferCloseRange = false;
        }
    }

    protected override void ActImpl(float delta, BotEvents events) {
        _attackCooldown = QMath.ClampMin(_attackCooldown - delta, 0);
        _fleeDelay = QMath.ClampMin(_fleeDelay - delta, 0);
        _afterburnedEvasionCooldown = QMath.ClampMin(_afterburnedEvasionCooldown - delta, 0);
        _afterburnerChargeCooldown = QMath.ClampMin(_afterburnerChargeCooldown - delta, 0);
        _tempestDuration = QMath.ClampMin(_tempestDuration - delta, 0);

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
        ActFight(events, delta);
    }

    protected void SetTarget(Pilot target) {
        _currentTarget = target;
    }

    private Pilot GetRandomEnemy() {
        var offset = QRandom.IntRange(0, _pilot.Enemies.Count - 1);
        var numTried = 0;
        for (int i = offset; numTried < _pilot.Enemies.Count; i++, numTried++) {
            if (i >= _pilot.Enemies.Count) {
                i = 0;
            }
            var e = _pilot.Enemies[i];
            if (e.Active) {
                return e;
            }
        }
        return null;
    }

    private void ChooseTarget() {
        SetTarget(GetRandomEnemy());
    }

    private Vector2 FleeWaypoint(Vector2 danger, float distance = 256) {
        var fleePos = ((_vessel.Position - danger).Normalized() * distance) + _vessel.Position;
        return QMath.RandomizedLocation(fleePos, 32);
    }

    private Vector2 FixOutOfScreenPos(Vector2 pos) {
        if (IsOutOfScreen(pos)) {
            return QMath.RandomizedLocation(_screenCenter, 512);
        }
        return pos;
    }

    private Vector2 CorrectedPos(Vector2 pos) {
        if (ArenaState.starHazard != null) {
            var middlePos = pos.MoveToward(_vessel.Position, pos.DistanceTo(_vessel.Position) / 2);
            var starPos = ArenaState.starHazard.Position;
            if (middlePos.DistanceTo(ArenaState.starHazard.Position) < 200 || pos.DistanceTo(starPos) < 150) {
                var rotation = (middlePos - _vessel.Position).Normalized();
                var pos1 = (rotation.Rotated(0.5f) * 200) + _vessel.Position;
                var pos2 = (rotation.Rotated(-0.5f) * 200) + _vessel.Position;
                if (pos1.DistanceTo(starPos) > pos2.DistanceTo(starPos)) {
                    return FixOutOfScreenPos(pos1);
                }
                return FixOutOfScreenPos(pos2);
            }
        }
        return FixOutOfScreenPos(pos);
    }

    private Vector2 PosNearTarget(float radius) {
        var pos = QMath.RandomizedLocation(TargetPosition(), radius);
        return _vessel.Position.MoveToward(pos, _maxAdvanceDist);
    }

    private Vector2 PickWaypoint() {
        if (QRandom.Float() < 0.15) {
            return QMath.RandomizedLocation(_vessel.Position, 96);
        }

        var targetDistance = TargetDistance();
        var roll = QRandom.Float();

        if (_preferLongRange && targetDistance < 192 && roll < 0.9) {
            return FleeWaypoint(TargetPosition());
        }

        if (roll < 0.55 || (targetDistance > 300 && roll < 0.65)) {
            var pos = _vessel.Position.MoveToward(TargetPosition(), 192);
            if (_preferLongRange && pos.DistanceTo(TargetPosition()) < 192) {
                return FleeWaypoint(TargetPosition());
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
            return PosNearTarget(64);
        }

        if (targetDistance > 500) {
            return PosNearTarget(160);
        }

        return PosNearTarget(192);
    }

    private void ActMove() {
        if (_vessel.HasWaypoints()) {
            if (_followingAlly) {
                return;
            }
            if (_preferCloseRange && _vessel.CurrentWaypointDistance() > 350) {
                if (_currentTarget != null && TargetDistance() < 300) {
                    var pos = QMath.RandomizedLocation(TargetPosition(), 32);
                    ChangeWaypoint(pos);
                }
            }
            if (_preferLongRange && _fleeDelay == 0 && _currentTarget != null && TargetDistance() < 160) {
                var fleeRoll = QRandom.Float();
                if (fleeRoll < 0.05) {
                    _fleeDelay = 3 + fleeRoll;
                    ChangeWaypoint(CorrectedPos(PickWaypoint()));
                }
            }
            return;
        }

        if (IsOutOfScreen(_vessel.Position)) {
            AddWaypoint(QMath.RandomizedLocation(_screenCenter, 512));
            return;
        }

        if (_currentTarget == null) {
            return;
        }

        var roll = QRandom.Float();
        if (roll < 0.5) {
            if (TargetDistance() > 350 && numActiveEnemies() > 1) {
                var closest = QMath.NearestEnemy(_vessel.Position, _pilot);
                if (closest != _currentTarget) {
                    SetTarget(closest);
                }
            }
        } else if (roll < 0.7 && !_preferCloseRange) {
            if (MaybeFollowAlly()) {
                _followingAlly = true;
                return;
            }
        }

        AddWaypoint(CorrectedPos(PickWaypoint()));
    }

    private bool MaybeFollowAlly() {
        Pilot leader = null;
        foreach (var ally in _pilot.Allies) {
            if (!ally.Active) {
                continue;
            }
            if (QRandom.Bool()) {
                leader = ally;
                break;
            }
        }
        if (leader == null) {
            return false;
        }
        AddWaypoint(CorrectedPos(QMath.RandomizedLocation(leader.Vessel.Position, 80)));
        return true;
    }

    private bool CanReduceDamage(DamageKind damageKind) {
        if (_shield == PhaserShield.Design) {
            return true;
        }

        if (damageKind == DamageKind.Electromagnetic) {
            return _shield.activeElectromagneticDamageReceive != 1;
        }
        if (damageKind == DamageKind.Kinetic) {
            return _shield.activeKineticDamageReceive != 1;
        }
        if (damageKind == DamageKind.Thermal) {
            return _shield.activeThermalDamageReceive != 1;
        }
        return false;
    }

    private void MaybeDoAfterburnerEvasion(BotEvents events) {
        if (_afterburnedEvasionCooldown != 0) {
            return;
        }
        if (!CanUseForFree(AfterburnerWeapon.Design.energyCost)) {
            return;
        }
        if (!_vessel.specialWeapon.CanFire(_vessel.State, _vessel.Position)) {
            return;
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
                var weaponDesign = projectile.GetWeaponDesign();
                var canDodge = weaponDesign == IonCannonWeapon.Design ||
                               weaponDesign == NeedleGunWeapon.Design ||
                               weaponDesign == PulseLaserWeapon.Design ||
                               weaponDesign == DiskThrowerWeapon.Design ||
                               weaponDesign == CrystalCannonWeapon.Design ||
                               weaponDesign == DisintegratorWeapon.Design ||
                               weaponDesign == MortarWeapon.Design ||
                               weaponDesign == MjolnirWeapon.Design;
                if (!canDodge) {
                    continue;
                }
                _afterburnedEvasionCooldown = 4;
                FireSpecial(_vessel.Position);
                return;
            }
        }
    }

    private bool MaybeGuardWithTempest(BotEvents events) {
        if (!_hasTempest || _tempestDuration != 0) {
            return false;
        }

        if (!_vessel.specialWeapon.CanFire(_vessel.State, _vessel.Position)) {
            return false;
        }

        foreach (Area2D other in events.midRangeCollisions) {
            if (!IsInstanceValid(other)) {
                continue;
            }

            if (other.GetParent() is IProjectile projectile) {
                var firedBy = projectile.FiredBy();
                if (firedBy.alliance == _pilot.alliance) {
                    continue;
                }
                if (!TempestWeapon.canBlock.Contains(projectile.GetWeaponDesign())) {
                    continue;
                }
                UseTempest();
                return true;
            }
        }

        return false;
    }

    private bool MaybeUseShield(BotEvents events) {
        if (!_hasShield || _energyUsed || !_vessel.shield.CanActivate(_vessel.State)) {
            return false;
        }

        if (events.targetedByZap) {
            if (_shield.activeElectromagneticDamageReceive != 1) {
                Shield();
                return true;
            }
        }

        foreach (var n in _pilot.context.mortarShells.GetNodes()) {
            Node2D shell = null;
            var dangerDistance = 0;
            if (n is MjolnirProjectile mjolnirProjectile) {
                if (mjolnirProjectile.FiredBy.alliance == _pilot.alliance) {
                    continue;
                }
                if (!CanReduceDamage(MjolnirWeapon.Design.damageKind)) {
                    continue;
                }
                dangerDistance = 50;
                shell = mjolnirProjectile;
                
            }
            if (n is MortarProjectile mortarProjectile) {
                if (mortarProjectile.FiredBy.alliance == _pilot.alliance) {
                    continue;
                }
                if (!CanReduceDamage(MjolnirWeapon.Design.damageKind)) {
                    continue;
                }
                dangerDistance = 40;
                shell = mortarProjectile;
            }

            if (shell != null) {
                if (!CanUseForFree(_vessel.shield.GetDesign().energyCost)) {
                    dangerDistance += 10;
                }

                if (_vessel.Position.DistanceTo(shell.Position) > dangerDistance) {
                    continue;
                }
                if (QRandom.Float() < 0.1) {
                    continue;
                }
                Shield();
                return true;
            }
        }

        if (_shield == DeflectorShield.Design) {
            foreach (Area2D other in events.midRangeCollisions) {
                if (!IsInstanceValid(other)) {
                    continue;
                }
                if (other.GetParent() is Projectile projectile) {
                    if (DeflectorShield.CanDeflect(projectile.GetWeaponDesign())) {
                        Shield();
                        return true;
                    }
                }
            }
        } else {
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
                    return true;
                }
            }
        }

        return false;
    }

    private void ActDefense(BotEvents events) {
        if (!MaybeGuardWithTempest(events)) {
            if (!MaybeUseShield(events)) {
                if (_hasAfterburner) {
                    MaybeDoAfterburnerEvasion(events);
                }
            }
        }

        if (_pointDefense != null && _tempestDuration == 0) {
            MaybeBlockRockets();
        }

        if (_vessel.specialWeapon.GetDesign() == RestructuringRayWeapon.Design) {
            MaybeHealAlly();
        }

        if (_antiAsteroid != null) {
            MaybeAttackAsteroid(events);
        }
    }

    private void MaybeAttackAsteroid(BotEvents events) {
        if (!IsInstanceValid(_targetedAsteroid)) {
            _targetedAsteroid = null;
        }
        if (_targetedAsteroid != null) {
            if (_vessel.Position.DistanceTo(_targetedAsteroid.Position) > 150) {
                _targetedAsteroid = null;
            }
            if (_asteroidDamageDelivered >= Asteroid.MAX_HP) {
                _targetedAsteroid = null;
            }
        }

        if (_targetedAsteroid != null && _attackCooldown == 0) {
            var cursor = _targetedAsteroid.Position;
            var attackRoll = QRandom.Float();
            if (attackRoll < 0.2 && _antiAsteroid.CanFire(_vessel.State, cursor)) {
                Fire(_antiAsteroidIndex, cursor);
                _asteroidDamageDelivered += _antiAsteroid.GetDesign().damage;
            }
            return;
        }

        foreach (Area2D other in events.midRangeCollisions) {
            if (!IsInstanceValid(other)) {
                continue;
            }
            if (other.GetParent() is Asteroid asteroid) {
                _targetedAsteroid = asteroid;
                _asteroidDamageDelivered = 0;
                break;
            }
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
        if (!CanUseForFree(RestructuringRayWeapon.Design.energyCost)) {
            return;
        }

        var ally = QMath.NearestAlly(_vessel.Position, _pilot);
        if (ally == null) {
            return;
        }
        var hpMissing = ally.Vessel.State.stats.maxHp - ally.Vessel.State.hp;
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
        foreach (var n in _pilot.context.rockets.GetNodes()) {
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

    private void ActFight(BotEvents events, float delta) {
        if (_currentTarget == null) {
            return;
        }

        UseSpecialWeapon(events);
        UseNormalWeapons();

        MaybeChargeWeapon(delta);
    }

    private void UseTempest() {
        _tempestDuration = TempestWeapon.Design.duration;
        FireSpecial(_vessel.Position);
    }

    private void MaybeChargeWeapon(float delta) {
        if (!_vessel.specialWeapon.GetDesign().chargable) {
            return;
        }
        if (!_chargingWeapon) {
            // TODO: this goal should vary among the weapon.
            _chargeGoal = QRandom.FloatRange(1.2f, 3.2f);
            _chargingWeapon = true;
        }
        _actions.ChargeWeaponAction();
        _weaponCharge += delta;
    }

    protected virtual void UseSpecialWeapon(BotEvents events) {
        var design = _vessel.specialWeapon.GetDesign();

        if (design == EmptyWeapon.Design) {
            return;
        }

        var targetDistance = TargetDistance();

        if (design == TempestWeapon.Design) {
            if (targetDistance > 120 || _tempestDuration != 0) {
                return;
            }
            if (!_vessel.specialWeapon.CanFire(_vessel.State, _vessel.Position)) {
                return;
            }
            UseTempest();
            return;
        }

        if (design == AfterburnerWeapon.Design) {
            if (!_vessel.specialWeapon.CanFire(_vessel.State, _vessel.Position)) {
                return;
            }
            var cursor = TargetPosition();
            // If enemy is in front and we want to close the distance,
            // use afterburner to pursue the target.
            if (_afterburnerChargeCooldown == 0 && targetDistance >= 300 && targetDistance <= 500 && !_preferLongRange && CanUseForFree(AfterburnerWeapon.Design.energyCost)) {
                if (Math.Abs(QMath.RotationDiff(cursor.AngleToPoint(_vessel.Position), _vessel.Rotation)) <= 0.4f) {
                    _afterburnerChargeCooldown = 3 + QRandom.FloatRange(0.5f, 2.5f);
                    FireSpecial(_vessel.Position);
                    return;
                }
            }
            if (targetDistance > 160) {
                return;
            }
            // If enemy is behind - use afterburner to inflict some damage.
            bool shouldFire = false;
            if (targetDistance < 40 && !_preferCloseRange) {
                shouldFire = true;
            } else {
                var dstRotation = cursor.AngleToPoint(_vessel.Position);
                var calculatedAngle = 0.6f + Mathf.Pi/2;
                if (Math.Abs(QMath.RotationDiff(dstRotation, _vessel.Rotation)) >= calculatedAngle) {
                    shouldFire = true;
                }
            }
            if (shouldFire) {
                FireSpecial(_vessel.Position);
                return;
            }
        }

        if (design == DisintegratorWeapon.Design) {
            if (_weaponCharge < _chargeGoal) {
                return;
            }
            var targetCursor = CalculateFireTarget(_vessel.specialWeapon);
            if (targetCursor != Vector2.Zero) {
                FireSpecial(targetCursor);
                _weaponCharge = 0;
                _chargingWeapon = false;
            }
            return;
        }

        if (design == TorpedoLauncherWeapon.Design) {
            if (targetDistance < 750 && _vessel.specialWeapon.CanFire(_vessel.State, TargetPosition())) {
                FireSpecial(TargetPosition());
            }
            return;
        }

        if (design == SwarmSpawnerWeapon.Design) {
            var swarmTarget = GetRandomEnemy();
            if (swarmTarget != null && _vessel.specialWeapon.CanFire(_vessel.State, swarmTarget.Vessel.Position)) {
                FireSpecial(swarmTarget.Vessel.Position);
            }
            return;
        }

        if (design == ShockwaveCasterWeapon.Design) {
            if (targetDistance > 350 && targetDistance < 600 && _vessel.specialWeapon.CanFire(_vessel.State, _vessel.Position)) {
                foreach (var n in _pilot.context.asteroids.GetNodes()) {
                    var a = (Asteroid)n;
                    var dist = _vessel.Position.DistanceTo(a.Position);
                    if (dist > 250) {
                        continue;
                    }
                    var targetDist = TargetPosition().DistanceTo(a.Position);
                    if (targetDist > 200) {
                        continue;
                    }
                    FireSpecial(a.Position);
                    return;
                }
            }
        }

        if (design == HyperCutterWeapon.Design || design == MortarWeapon.Design || design == MjolnirWeapon.Design || design == ReaperCannonWeapon.Design || design == HarpoonWeapon.Design || design == DisruptorWeapon.Design || design == ShockwaveCasterWeapon.Design || design == PulseBladeWeapon.Design) {
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

    private Vector2 CalculateSnipeShot(IWeapon w, Vector2 targetPos, Vector2 targetVelocity) {
        return QMath.CalculateSnipeShot(w.GetDesign(), _vessel.Position, targetPos, _currentTarget.Vessel.State.velocity);
    }

    private Vector2 CalculateFireTarget(IWeapon w) {
        var design = w.GetDesign();

        if (design.energyCost != 0 && _energyUsed) {
            return Vector2.Zero;
        }

        Vector2 cursor;
        var snipeRoll = design.botHintSnipe == 0 ? 100 : QRandom.Float();
        if (snipeRoll <= design.botHintSnipe) {
            var predictedPos = CalculateSnipeShot(w, TargetPosition(), _currentTarget.Vessel.State.velocity);
            cursor = QMath.RandomizedLocation(predictedPos, 16 * design.botHintScatter);
        } else {
            cursor = QMath.RandomizedLocation(TargetPosition(), 24 * design.botHintScatter);
        }
        var closeRange = TargetDistance() < 150;
        var lowOnEnergy = _vessel.State.backupEnergy < (_vessel.State.stats.maxBackupEnergy * 0.33);

        var effectiveRange = design.botHintRange != 0 ? design.botHintRange : design.range;

        if (TargetDistance() <= effectiveRange && w.CanFire(_vessel.State, cursor)) {
            if (lowOnEnergy && !CanUseForFree(design.energyCost) && !closeRange) {
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

    private void MaybeUseBubbleGun(int weaponIndex, IWeapon w) {
        var design = w.GetDesign();
        if (design.energyCost != 0 && _energyUsed) {
            return;
        }

        var cursor = TargetPosition();
        if (!w.CanFire(_vessel.State, cursor)) {
            return;
        }

        var targetDistance = TargetDistance();

        if (targetDistance >= design.botHintRange) {
            return;
        }

        bool shouldFire = false;
        if (targetDistance <= 96) {
            shouldFire = true;
        } else {
            var dstRotation = cursor.AngleToPoint(_vessel.Position);
            var calculatedAngle = 1.0f + Mathf.Pi/2;
            if (Math.Abs(QMath.RotationDiff(dstRotation, _vessel.Rotation)) >= calculatedAngle) {
                shouldFire = true;
            }
        }

        if (shouldFire) {
            Fire(weaponIndex, cursor);
        }
    }

    private void MaybeUseWeapon(int weaponIndex) {
        if (_attackCooldown != 0) {
            return;
        }

        var w = _vessel.weapons[weaponIndex];

        if (w.GetDesign() == BubbleGunWeapon.Design) {
            MaybeUseBubbleGun(weaponIndex, w);
            return;
        }

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

    private void ChangeWaypoint(Vector2 pos) {
        _actions.ChangeWaypoint(pos);
        _followingAlly = false;
    }

    private void AddWaypoint(Vector2 pos) {
        _actions.AddWaypoint(pos);
        _followingAlly = false;
    }

    private void Shield() {
        _actions.ShieldAction();
        _energyUsed = true;
    }

    protected void FireSpecial(Vector2 target) {
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

    private bool CanUseForFree(float energy) {
        return _vessel.State.energy >= energy;
    }

    private int AntiAsteroidRating(IWeapon w) {
        if (w is ScytheWeapon || w is GreatScytheWeapon || w is StingerWeapon) {
            return 1;
        }
        if (w is IonCannonWeapon || w is CutterWeapon) {
            return 2;
        }
        if (w is NeedleGunWeapon || w is PhotonBurstCannonWeapon || w is TwinPhotonBurstCannonWeapon || w is PulseLaserWeapon || w is AssaultLaserWeapon) {
            return 3;
        }
        if (w is ShockwaveCasterWeapon) {
            return 4;
        }
        return 0;
    }
}
