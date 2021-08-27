using Godot;
using System;

public class Projectile : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;
    private Texture _texture;
    private AudioStream _audioStream = null;
    private float _volumeAdjust = 0;
    private bool _spriteRotation = false;

    private static Texture _laserTexture;
    private static Texture _ionTexture;
    private static Texture _stingerTexture;
    private static Texture _scytheTexture;
    private static Texture _photonBurstTexture;
    private static Texture _needleGunTexture;
    private static Texture _shockwaveCasterTexture;
    private static Texture _spreadGunTexture;
    private static Texture _reaperCannonTexture;
    private static Texture _hellfireTexture;
    private static Texture _crystalShardTexture;
    private static Texture _assaultLaserTexture;

    private static AudioStream _laserAudioStream;
    private static AudioStream _ionAudioStream;
    private static AudioStream _stingerAudioStream;
    private static AudioStream _needleGunAudioStream;
    private static AudioStream _needleImpactAudioStream;
    private static AudioStream _shockwaveCasterAudioStream;
    private static AudioStream _reaperCannonAudioStream;

    private Pilot _firedBy;
    private WeaponDesign _weapon;

    public WeaponDesign GetWeaponDesign() { return _weapon; }
    public Pilot FiredBy() { return _firedBy; }

    private static PackedScene _scene = null;
    public static Projectile New(WeaponDesign design, Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Projectile.tscn");
        }
        var o = (Projectile)_scene.Instance();
        o._firedBy = owner;
        o._weapon = design;
        return o;
    }

    public override void _Ready() {
        if (_weapon == IonCannonWeapon.Design) {
            InitIonCannon();
        } else if (_weapon == PulseLaserWeapon.Design) {
            InitLaser();
        } else if (_weapon == StingerWeapon.Design) {
            InitStinger();
        } else if (_weapon == ScytheWeapon.Design || _weapon == GreatScytheWeapon.Design) {
            InitScythe();
        } else if (_weapon == PhotonBurstCannonWeapon.Design || _weapon == TwinPhotonBurstCannonWeapon.Design) {
            InitPhotonBurstCannon();
        } else if (_weapon == NeedleGunWeapon.Design) {
            InitNeedleGun();
        } else if (_weapon == SpreadGunWeapon.Design) {
            InitSpreadGun();
        } else if (_weapon == ReaperCannonWeapon.Design) {
            InitReaperCannon();
        } else if (_weapon == HellfireWeapon.Design) {
            InitHellfire();
        } else if (_weapon == CrystalCannonWeapon.Design) {
            InitCrystalShard();
        } else if (_weapon == AssaultLaserWeapon.Design) {
            InitAssaultLaser();
        } else if (_weapon == ShockwaveCasterWeapon.Design) {
            InitShockwaveCaster();
        }

        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Texture = _texture;

        if (_weapon == HellfireWeapon.Design) {
            _sprite.Scale = new Vector2(0.2f, 0.2f);
        }

        if (_audioStream != null) {
            var sfx = SoundEffectNode.New(_audioStream, _volumeAdjust);
            GetParent().AddChild(sfx);
        }
    }

    private void InitScythe() {
        _hp = ScytheWeapon.Design.range;
        if (_scytheTexture == null) {
            _scytheTexture = GD.Load<Texture>("res://images/ammo/Scythe.png");
        }
        _texture = _scytheTexture;
    }

    private void InitReaperCannon() {
        _hp = ReaperCannonWeapon.Design.range;
        if (_reaperCannonTexture == null) {
            _reaperCannonTexture = GD.Load<Texture>("res://images/ammo/Reaper_Cannon.png");
        }
        if (_reaperCannonAudioStream == null) {
            _reaperCannonAudioStream = GD.Load<AudioStream>("res://audio/weapon/Reaper_Cannon.wav");
        }
        _volumeAdjust = -4;
        _texture = _reaperCannonTexture;
        _audioStream = _reaperCannonAudioStream;
    }

    private void InitSpreadGun() {
        _hp = SpreadGunWeapon.Design.range;
        if (_spreadGunTexture == null) {
            _spreadGunTexture = GD.Load<Texture>("res://images/ammo/Spread_Gun.png");
        }
        _texture = _spreadGunTexture;
    }

    private void InitHellfire() {
        _hp = HellfireWeapon.Design.range;
        if (_hellfireTexture == null) {
            _hellfireTexture = GD.Load<Texture>("res://images/ammo/fireball.png");
        }
        _texture = _hellfireTexture;
        GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Scale = new Vector2(3, 3);
    }

    private void InitCrystalShard() {
        _hp = 125; // FIXME: should not be hardcoded here
        if (_crystalShardTexture == null) {
            _crystalShardTexture = GD.Load<Texture>("res://images/ammo/crystal_shard.png");
        }
        _texture = _crystalShardTexture;
    }

    private void InitAssaultLaser() {
        _hp = AssaultLaserWeapon.Design.range;
        if (_assaultLaserTexture == null) {
            _assaultLaserTexture = GD.Load<Texture>("res://images/ammo/Assault_Laser.png");
        }
        _texture = _assaultLaserTexture;
    }

    private void InitShockwaveCaster() {
        _hp = ShockwaveCasterWeapon.Design.range;
        if (_shockwaveCasterTexture == null) {
            _shockwaveCasterTexture = GD.Load<Texture>("res://images/ammo/Shockwave_Caster.png");
        }
        if (_shockwaveCasterAudioStream == null) {
            _shockwaveCasterAudioStream = GD.Load<AudioStream>("res://audio/weapon/Shockwave_Caster.wav");
        }
        _texture = _shockwaveCasterTexture;
        _audioStream = _shockwaveCasterAudioStream;
    }

    private void InitNeedleGun() {
        _hp = NeedleGunWeapon.Design.range;
        if (_needleGunTexture == null) {
            _needleGunTexture = GD.Load<Texture>("res://images/ammo/Needle_Gun.png");
        }
        if (_needleGunAudioStream == null) {
            _needleGunAudioStream = GD.Load<AudioStream>("res://audio/weapon/Needle_Gun.wav");
        }
        _volumeAdjust = -9;
        _texture = _needleGunTexture;
        _audioStream = _needleGunAudioStream;
    }

    private void InitPhotonBurstCannon() {
        _hp = PhotonBurstCannonWeapon.Design.range;
        if (_photonBurstTexture == null) {
            _photonBurstTexture = GD.Load<Texture>("res://images/ammo/photon_burst.png");
        }
        _texture = _photonBurstTexture;
    }

    private void InitStinger() {
        _hp = StingerWeapon.Design.range;
        if (_stingerTexture == null) {
            _stingerTexture = GD.Load<Texture>("res://images/ammo/stinger.png");
        }
        if (_stingerAudioStream == null) {
            _stingerAudioStream = GD.Load<AudioStream>("res://audio/weapon/Stinger.wav");
        }
        _volumeAdjust = -10;
        _texture = _stingerTexture;
        _audioStream = _stingerAudioStream;
    }

    private void InitIonCannon() {
        _hp = IonCannonWeapon.Design.range;
        if (_ionTexture == null) {
            _ionTexture = GD.Load<Texture>("res://images/ion.png");
        }
        if (_ionAudioStream == null) {
            _ionAudioStream = GD.Load<AudioStream>("res://audio/ion_cannon.wav");
        }
        _volumeAdjust = -5;
        _texture = _ionTexture;
        _audioStream = _ionAudioStream;
    }

    private void InitLaser() {
        _hp = PulseLaserWeapon.Design.range;
        if (_laserTexture == null) {
            _laserTexture = GD.Load<Texture>("res://images/laser.png");
        }
        if (_laserAudioStream == null) {
            _laserAudioStream = GD.Load<AudioStream>("res://audio/pulse_laser.wav");
        }
        _volumeAdjust = -7;
        _texture = _laserTexture;
        _audioStream = _laserAudioStream;
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        if (_spriteRotation) {
            _sprite.Rotation += 10 * delta;
        }

        var speed = _weapon.projectileSpeed;
        if (_weapon == HellfireWeapon.Design) {
            speed += QRandom.FloatRange(-75, 75);
            if (_hp < 100) {
                var m = _sprite.Modulate;
                _sprite.Modulate = new Color(m.r, m.g, m.b, m.a - 0.05f);
            } else if (_sprite.Scale.x < 1.0) {
                _sprite.Scale = new Vector2(_sprite.Scale.x + delta*2, _sprite.Scale.y + delta*2);
            }
            Rotation += QRandom.FloatRange(-0.08f, 0.08f);
        }

        float traveled = speed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
        if (_weapon == NeedleGunWeapon.Design) {
            _hp += 125;
            var explosion = Explosion.New();
            explosion.Modulate = Color.Color8(100, 255, 100);
            explosion.Position = Position;
            explosion.Scale = new Vector2(0.8f, 0.8f);
            GetParent().AddChild(explosion);
            if (_needleImpactAudioStream == null) {
                _needleImpactAudioStream = GD.Load<AudioStream>("res://audio/weapon/Needle_Gun_Impact.wav");
            }
            var sfx = SoundEffectNode.New(_needleImpactAudioStream, -5);
            GetParent().AddChild(sfx);
        } else if (_weapon == ReaperCannonWeapon.Design) {
            QueueFree();
            var explosion = Explosion.New();
            explosion.Position = Position;
            GetParent().AddChild(explosion);
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/rocket_impact.wav"));
            GetParent().AddChild(sfx);
        } else if (_weapon == ShockwaveCasterWeapon.Design) { 
            QueueFree();
            var explosion = ShockwaveCasterExplosion.New();
            explosion.Position = Position;
            GetParent().AddChild(explosion);
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Shockwave_Caster_Impact.wav"), -6);
            GetParent().AddChild(sfx);
        } else {
            QueueFree();
        }
    }
}
