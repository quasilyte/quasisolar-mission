using Godot;
using System;
using System.Collections.Generic;

public class VesselNode : Node2D {
    public VesselState State;
    public Pilot pilot;
    public IShield shield;
    public List<IWeapon> weapons = new List<IWeapon>();
    public IWeapon specialWeapon = new EmptyWeapon();
    public List<IArtifact> artifacts = new List<IArtifact>();

    public WaypointLineNode waypointLine;

    private SentinelNode _sentinel = null;

    private Queue<Waypoint> _waypoints = new Queue<Waypoint>();
    private Waypoint _currentWaypoint;

    private bool _hasMagneticNegator = false;
    private bool _hasLaserAbsorber = false;

    private bool _phasing = false;
    private bool _destroyed = false;

    private Texture _texture;

    private ContrailNode _contrail;

    private static AudioStream _destroyedAudioStream;

    private DrawLineNode _dragLine;

    [Signal]
    public delegate void Destroyed();

    [Signal]
    public delegate void TargetedByZap();

    private static PackedScene _scene = null;
    public static VesselNode New(Pilot pilot, VesselState state, Texture texture) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/VesselNode.tscn");
        }
        var o = (VesselNode)_scene.Instance();
        o.pilot = pilot;
        o.State = state;
        o._texture = texture;
        return o;
    }

    public override void _Ready() {
        var area = GetNode<Area2D>("Area2D");
        var mask = area.GetNode<CollisionShape2D>("CollisionShape2D");
        area.Connect("area_entered", this, nameof(OnCollision));
        area.Connect("area_exited", this, nameof(OnAreaExited));

        var sprite = GetNode<Sprite>("Sprite");
        sprite.Texture = _texture;

        _contrail = GetNode<ContrailNode>("Contrail");
        _contrail.Visible = false;
        _contrail.Emitting = false;
        if (State.vesselSize == VesselDesign.Size.Normal) {
            _contrail.Scale = new Vector2(1.6f, 1.6f);
            _contrail.Position -= new Vector2(4, 0);
            mask.Scale = new Vector2(1.1f, 1.1f);
        } else if (State.vesselSize == VesselDesign.Size.Large) {
            _contrail.Scale = new Vector2(1.8f, 1.8f);
            _contrail.Position -= new Vector2(10, 0);
            mask.Scale = new Vector2(1.3f, 1.3f);
        }

        foreach (IArtifact a in artifacts) {
            if (a is MagneticNegatorArtifact) {
                _hasMagneticNegator = true;
            }
            if (a is LaserAbsorberArtifact) {
                _hasLaserAbsorber = true;
            }
        }

        foreach (var w in weapons) {
            w.Ready();
        }
        specialWeapon.Ready();
    }

    private void DragDetach() {
        if (IsInstanceValid(_dragLine)) {
            _dragLine.QueueFree();
        }
        State.dragTime = 0;
        State.draggedBy = null;
    }

    public void SetSentinel(SentinelNode sentinel) {
        _sentinel = sentinel;
    }

    public void EnablePhasing(float duration) {
        _phasing = true;
        State.phasingTime = duration;
        SetAlpha(0.2f);
    }

    private void SetAlpha(float value) {
        var m = Modulate;
        m.a = value;
        Modulate = m;
    }

    public void OnEnvHazardTick() {
        if (State.insideBlueNebula) {
            State.backupEnergy = QMath.ClampMin(State.backupEnergy - 2, 0);
            State.energy = QMath.ClampMin(State.energy - 5, 0);
        }
        if (State.insideStarHazard) {
            var damage = 2 + State.stats.starDamageReceived;
            ApplyDamage(damage, DamageKind.Thermal);
            for (int i = 0; i < 4; i++) {
                var flame = FireEffectNode.New();
                AddChild(flame);
                flame.Rotation = QRandom.FloatRange(-2, 2);
                flame.Position = QMath.RandomizedLocation(Vector2.Zero, 16);
            }
        }
    }

    public override void _Process(float delta) {
        if (!IsInstanceValid(_sentinel)) {
            _sentinel = null;
        }

        foreach (var a in artifacts) {
            a.Apply(State, delta);
        }
        foreach (var w in weapons) {
            w.Process(State, delta);
        }
        specialWeapon.Process(State, delta);
        shield.Process(State, delta);

        if (State.reactorDisabledTime == 0) {
            State.energy += State.stats.energyRegen * delta;
            if (State.energy > State.stats.maxEnergy) {
                State.energy = State.stats.maxEnergy;
            }
        }

        if (State.speedBonus != 0) {
            State.speedBonus = QMath.ClampMin(State.speedBonus - delta*5, 0);
        }

        State.speedPenalty -= delta;
        if (State.speedPenalty < 0) {
            State.speedPenalty = 0;
        }
        State.rotationCrippledTime = QMath.ClampMin(State.rotationCrippledTime - delta, 0);
        State.reactorDisabledTime = QMath.ClampMin(State.reactorDisabledTime - delta, 0);

        State.phasingTime = QMath.ClampMin(State.phasingTime - delta, 0);
        if (_phasing && State.phasingTime == 0) {
            _phasing = false;
            SetAlpha(1);
        }

        if (!IsInstanceValid(State.draggedBy)) {
            DragDetach();
        }
        State.dragTime = QMath.ClampMin(State.dragTime - delta, 0);
        if (State.dragTime == 0 && State.draggedBy != null) {
            DragDetach();
        }

        if (_currentWaypoint == null && _waypoints.Count != 0) {
            _currentWaypoint = _waypoints.Dequeue();
            _currentWaypoint.GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Disabled = false;
        }

        var showContrail = false;
        var moving = false;
        var contrailLength = 0.2f;
        if (_currentWaypoint != null && State.stats.maxSpeed != 0) {
            var engineRotationSpeed = State.insidePurpleNebula ? State.stats.rotationSpeed - 0.5f : State.stats.rotationSpeed;
            var rotationSpeed = State.rotationCrippledTime != 0 ? 0.5f : engineRotationSpeed;
            rotationSpeed = Math.Max(rotationSpeed, 0.5f);
            var dstRotation = _currentWaypoint.Position.AngleToPoint(Position);
            // var rotationDiff = QMath.RotationDiff(dstRotation, Rotation);
            var rotationAmount = rotationSpeed * delta;
            var newRotation = QMath.RotateTo(dstRotation, Rotation, rotationAmount);
            var isRotating = newRotation != dstRotation;
            Rotation = newRotation;
            if (!isRotating) {
                var engineMaxSpeed = State.insidePurpleNebula ? State.stats.maxSpeed / 2 : State.stats.maxSpeed;
                float maxSpeed = engineMaxSpeed - State.speedPenalty + State.speedBonus;
                var acceleration = State.stats.acceleration;
                if (CurrentWaypointDistance() < 150) {
                    var diff = QMath.RotationDiff(dstRotation, State.velocity.Angle());
                    if (Math.Abs(diff) > 0.7) {
                        var speedDecrease = State.stats.maxSpeed * 0.55f;
                        maxSpeed -= speedDecrease;
                        acceleration = (acceleration * 2) + 0.5f;
                        contrailLength = 0.1f;
                    }
                }
                if (maxSpeed < 30) {
                    maxSpeed = 30;
                }
                showContrail = true;
                if (State.velocity.Length() < maxSpeed) {
                    moving = true;
                    State.velocity += Transform.x * acceleration;
                    State.velocity = State.velocity.Clamped(maxSpeed);
                }
            }
        }

        _contrail.Emitting = showContrail;
        _contrail.Visible = showContrail;
        if (showContrail) {
            _contrail.Lifetime = contrailLength;
        }

        if (!moving && State.velocity != Vector2.Zero && State.draggedBy == null) {
            State.velocity -= State.velocity * delta * 0.5f;
            if (Math.Abs(State.velocity.x) < 1.5 && Math.Abs(State.velocity.y) < 1.5) {
                State.velocity = Vector2.Zero;
            }
        }

        Vector2 velocity;
        if (State.draggedBy == null) {
            velocity = State.velocity;
        } else {
            var dragVelocity = (State.draggedBy.Position - Position).Normalized() * 50;
            velocity = State.velocity + dragVelocity;
        }
        Position += velocity * delta;
    }

    public void AddWaypoint(Waypoint wp) {
        _waypoints.Enqueue(wp);
        if (waypointLine != null) {
            waypointLine.Enqueue(wp.GlobalPosition);
        }
    }

    public float CurrentWaypointDistance() {
        if (_currentWaypoint == null) {
            return 0;
        }
        return Position.DistanceTo(_currentWaypoint.Position);
    }

    public bool HasWaypoints() {
        return _currentWaypoint != null || _waypoints.Count != 0;
    }

    public void ClearWaypoints() {
        foreach (Waypoint wp in _waypoints) {
            wp.QueueFree();
        }
        if (_currentWaypoint != null) {
            _currentWaypoint.QueueFree();
            _currentWaypoint = null;
        }
        if (waypointLine != null) {
            waypointLine.Clear();
        }
        _waypoints.Clear();
    }

    public bool CanFire(int weaponIndex, Vector2 cursor) {
        return (weapons.Count - 1 >= weaponIndex) && weapons[weaponIndex].CanFire(State, cursor);
    }

    public void Fire(int weaponIndex, Vector2 cursor) {
        weapons[weaponIndex].Fire(State, cursor);
    }

    private void Destroy() {
        _destroyed = true;
        EmitSignal(nameof(Destroyed));
        if (_destroyedAudioStream == null) {
            _destroyedAudioStream = GD.Load<AudioStream>("res://audio/vessel_destroyed.wav");
        }
        var sfx = SoundEffectNode.New(_destroyedAudioStream, -10);
        GetParent().AddChild(sfx);
        ClearWaypoints();
        // TODO: add OnVesselDestroyed() to a base class?
        if (specialWeapon is PulseBladeWeapon pulseBlade) {
            pulseBlade.OnVesselDestroyed();
        }
        var e = Explosion.New(0.5f);
        e.Position = Position;
        e.Scale = new Vector2(2f, 2f);
        GetParent().AddChild(e);
        QueueFree();
    }

    public void EmitTargetedByZap() {
        EmitSignal(nameof(TargetedByZap));
    }

    public void ApplyDamage(float amount, DamageKind kind, bool critical = false) {
        if (_destroyed) {
            return;
        }

        if (_hasLaserAbsorber && kind == DamageKind.Electromagnetic) {
            if (QRandom.Float() < LaserAbsorberArtifact.chance) {
                var color = Color.Color8(0xff, 0xff, 0xff);
                var score = DamageScoreNode.New(0, color, false, false);
                score.Position = QMath.RandomizedLocation(Position, 8);
                GetParent().AddChild(score);
                return;
            }
        }

        float delta = 0;
        if (kind == DamageKind.Electromagnetic) {
            delta = State.stats.electromagneticDamageReceived;
        } else if (kind == DamageKind.Kinetic) {
            delta = State.stats.kineticDamageReceived;
        } else if (kind == DamageKind.Thermal) {
            delta = State.stats.thermalDamageReceived;
        }
        if (delta != 0) {
            amount = QMath.ClampMin(amount + delta, 1);
        }

        if (amount > 0) {
            var reducedAmount = QMath.ClampMin(shield.ReduceDamage(amount, kind), 1);
            var damageReduced = reducedAmount != amount;

            if (damageReduced && State.hasImpulseDevourer) {
                var damageDelta = amount - reducedAmount;
                State.backupEnergy = QMath.ClampMax(State.backupEnergy + damageDelta, State.stats.maxBackupEnergy);
            }

            if (_sentinel != null && !damageReduced && !shield.IsActive()) {
                if (_sentinel is ShieldSentinelNode shieldSentinel) {
                    reducedAmount = QMath.ClampMin(shieldSentinel.ReduceDamage(amount, kind), 1);
                    damageReduced = reducedAmount != amount;
                }
            }

            Color color;
            var hpPercentage = State.hp / State.stats.maxHp;
            if (hpPercentage >= 0.8) {
                color = Color.Color8(0x47, 0xe5, 0x3f);
            } else if (hpPercentage >= 0.5) {
                color = Color.Color8(0x8f, 0xcf, 0x43);
            } else if (hpPercentage >= 0.2) {
                color = Color.Color8(0xde, 0x7a, 0x31);
            } else {
                color = Color.Color8(0xff, 0x73, 0x47);
            }
            if (damageReduced) {
                amount = reducedAmount;
            }

            if (_sentinel != null && State.hasSentinelLink && amount >= 2) {
                amount /= 2;
                _sentinel.ApplyDamage(amount, kind, critical);
            }

            var score = DamageScoreNode.New((int)amount, color, damageReduced, critical);
            score.Position = QMath.RandomizedLocation(Position, 8);
            GetParent().AddChild(score);
        }

        State.hp -= amount;
        if (State.hp <= 0) {
            State.hp = 0;
            Destroy();
        }
    }

    public void ApplyEnergyDamage(float amount) {
        var energyDamage = _hasMagneticNegator ? amount / 2 : amount;
        State.energy -= energyDamage;
        if (State.energy < 0) {
            State.energy = 0;
        }
    }

    private void HandleCollision(Area2D other) {
        if (other.GetParent() is Asteroid asteroid) {
            asteroid.ApplyDamage(100);
            ApplyDamage(50 + State.stats.asteroidDamageReceived, DamageKind.Kinetic);
            return;
        }

        if (other.GetParent() is PlasmaAura plasmaAura) {
            if (plasmaAura.FiredBy.alliance == pilot.alliance) {
                return;
            }
            ApplyDamage(PlasmaEmitterWeapon.Design.damage, PlasmaEmitterWeapon.Design.damageKind);
            return;
        }

        if (other.GetParent() is IProjectile projectile) {
            var firedBy = projectile.FiredBy();
            if (firedBy.alliance == pilot.alliance) {
                return;
            }
            var design = projectile.GetWeaponDesign();
            projectile.OnImpact();
            var damage = design.damage;
            if (projectile is EnergyBoltNode energyBolt) {
                damage += 4 * energyBolt.chargeLevel;
            }
            if (design == IonCannonWeapon.Design && firedBy.Vessel.State.hasIonCannonSaturator) {
                damage += 4;
            }
            bool critical = false;
            // firedBy.Vessel is null for base defensive turrets.
            if (design.damageKind == DamageKind.Kinetic && firedBy.Vessel != null && firedBy.Vessel.State.hasKineticAccelerator) {
                if (QRandom.Float() < KineticAcceleratorArtifact.chance) {
                    damage *= 2;
                    critical = true;
                }
            }
            ApplyDamage(damage, design.damageKind, critical);
            if (design.energyDamage != 0) {
                ApplyEnergyDamage(design.energyDamage);
            }

            if (design == StingerWeapon.Design) {
                State.speedPenalty += _hasMagneticNegator ? 5 : 10;
            } else if (design == HarpoonWeapon.Design) {
                DragDetach();
                State.rotationCrippledTime += 1;
                State.dragTime = 2;
                State.draggedBy = firedBy.Vessel;
                _dragLine = DrawLineNode.New(State.dragTime, this, State.draggedBy, Color.Color8(0x2c, 0x82, 0x71), 1);
                GetParent().AddChild(_dragLine);
            } else if (design == DisruptorWeapon.Design) {
                State.reactorDisabledTime = 6;
                var explosion = DisruptorExplosionNode.New();
                AddChild(explosion);
                explosion.GlobalPosition = GlobalPosition;
                var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Disruptor_Impact.wav"), -2);
                GetParent().AddChild(sfx);
            } else if (design == ShieldBreakerWeapon.Design) {
                shield.Deactivate();
            } else if (design == ShockwaveCasterWeapon.Design) {
                var p = (Projectile)projectile;
                State.velocity += p.Transform.x * 75;
            } else if (design == SwarmSpawnerWeapon.Design) {
                State.rotationCrippledTime += 1;
            }

            return;
        }
    }

    private void OnCollision(Area2D other) {
        if (_currentWaypoint != null) {
            if (other == _currentWaypoint.Area) {
                _currentWaypoint.QueueFree();
                _currentWaypoint = null;
                if (waypointLine != null) {
                    waypointLine.Dequeue();
                }
            }
        }

        if (!_phasing) {
            HandleCollision(other);
        }

        if (other.GetParent() is PurpleNebulaNode) {
            State.insidePurpleNebula = true;
        } else if (other.GetParent() is BlueNebulaNode) {
            State.insideBlueNebula = true;
        } else if (other.GetParent() is StarHazardNode) {
            State.insideStarHazard = true;
        }
    }

    private void OnAreaExited(Area2D other) {
        if (other.GetParent() is PurpleNebulaNode) {
            State.insidePurpleNebula = false;
        } else if (other.GetParent() is BlueNebulaNode) {
            State.insideBlueNebula = false;
        } else if (other.GetParent() is StarHazardNode) {
            State.insideStarHazard = false;
        }
    }
}
