using Godot;
using System;

public class Asteroid : Node2D {
    private float _hp = 10.0f;
    private float _speed = 40.0f;
    private Vector2 _velocity;

    private static PackedScene _scene = null;
    public static Asteroid New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Asteroid.tscn");
        }
        var o = (Asteroid)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        _velocity = Transform.x * (_speed + QRandom.Float() * 30);

        var area = GetNode<Area2D>("Area2D");
        area.Connect("area_entered", this, nameof(OnCollision));
    }

    public void ApplyDamage(float amount) {
        _hp -= amount;
        if (_hp <= 0) {
            var explosion = Explosion.New(1, "AsteroidExplosion");
            explosion.Position = Position;
            explosion.Scale = new Vector2(2, 2);
            GetParent().AddChild(explosion);
            QueueFree();
        }
    }

    private void OnCollision(Area2D other) {
        if (other.GetParent() is PlasmaAura plasmaAura) {
            ApplyDamage(PlasmaEmitterWeapon.Design.damage);
            return;
        }

        if (other.GetParent() is IProjectile projectile) {
            var design = projectile.GetWeaponDesign();
            if (design.ignoresAsteroids) {
                return;
            }
            projectile.OnImpact();
            ApplyDamage(projectile.GetWeaponDesign().damage);
            return;
        }
    }

    public override void _Process(float delta) {
        Rotation -= delta * 3;

        var posX = Position.x;
        var posY = Position.y;
        var spriteWidth = 32f;
        var spriteHeight = 32f;
        var screenWidth = GetTree().Root.Size.x;
        var screenHeight = GetTree().Root.Size.y;
        bool outOfScreen = false;
        if (posX <= 0 - spriteWidth / 2) {
            outOfScreen = true;
        } else if (posX >= screenWidth + spriteWidth / 2) {
            outOfScreen = true;
        }
        if (posY <= 0 - spriteHeight / 2) {
            outOfScreen = true;
        } else if (posY >= screenHeight + spriteHeight / 2) {
            outOfScreen = true;
        }
        if (outOfScreen) {
            QueueFree();
        }
    }

    public override void _PhysicsProcess(float delta) {
        Position += _velocity * delta;
    }
}