using Godot;
using System;

public class UnitMemberNode : Node2D {
    private string _name;
    private Texture _sprite;
    private double _health;
    private double _energy;

    private static PackedScene _scene = null;
    public static UnitMemberNode New(string name, Texture sprite, double health, double energy) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/UnitMemberNode.tscn");
        }
        var o = (UnitMemberNode)_scene.Instance();
        o._name = name;
        o._sprite = sprite;
        o._health = health;
        o._energy = energy;
        return o;
    }

    public void UpdateStatus(double health, double energy) {
        _health = health;
        _energy = energy;
        GetNode<TextureProgress>("HealthBar").Value = _health;
        GetNode<TextureProgress>("EnergyBar").Value = _energy;
    }

    public override void _Ready() {
        GetNode<Label>("Name").Text = _name;
        GetNode<Sprite>("VesselSprite").Texture = _sprite;
        UpdateStatus(_health, _energy);
    }
}
