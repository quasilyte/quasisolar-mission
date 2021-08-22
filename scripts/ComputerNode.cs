using Godot;
using System;
using System.Collections.Generic;

public class ComputerNode : Node2D {
    public Pilot pilot;

    private AbstractBot _bot;

    private BotEvents _botEvents = new BotEvents();

    [Signal]
    public delegate void Defeated();

    private static PackedScene _scene = null;
    public static ComputerNode New(AbstractBot bot, Pilot player) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ComputerNode.tscn");
        }
        var o = (ComputerNode)_scene.Instance();
        o._bot = bot;
        o.pilot = player;
        return o;
    }

    public override void _Ready() {
        _bot.Ready();
        pilot.Vessel.Connect("Destroyed", this, nameof(OnVesselDestroyed));
        pilot.Vessel.Connect("TargetedByZap", this, nameof(OnTargetedByZap));

        var closeRangeArea = new Area2D();
        {
            var collisionShape = new CollisionShape2D();
            var shape = new CircleShape2D();
            shape.Radius = 40;
            collisionShape.Shape = shape;
            closeRangeArea.AddChild(collisionShape);
        }
        closeRangeArea.Name = "CloseRangeAura";
        pilot.Vessel.AddChild(closeRangeArea);

        var midRangeArea = new Area2D();
        {
            var collisionShape = new CollisionShape2D();
            var shape = new CircleShape2D();
            shape.Radius = 120;
            collisionShape.Shape = shape;
            midRangeArea.AddChild(collisionShape);
        }
        closeRangeArea.Name = "MidRangeAura";
        pilot.Vessel.AddChild(midRangeArea);

        closeRangeArea.Connect("area_entered", this, nameof(OnCloseRangeAreaEntered));
        midRangeArea.Connect("area_entered", this, nameof(OnMidRangeAreaEntered));
    }

    private void OnTargetedByZap() {
        _botEvents.targetedByZap = true;
    }

    private void OnCloseRangeAreaEntered(Area2D other) {
        _botEvents.closeRangeCollisions.Add(other);
    }

    private void OnMidRangeAreaEntered(Area2D other) {
        _botEvents.midRangeCollisions.Add(other);
    }

    private void OnVesselDestroyed() {
        pilot.Active = false;
        QueueFree();
        EmitSignal(nameof(Defeated));
    }

    public override void _Process(float delta) {
        RunActions(delta, _bot.Act(delta, _botEvents));
        
        if (_botEvents.closeRangeCollisions.Count != 0) {
            _botEvents.closeRangeCollisions.Clear();
        }
        if (_botEvents.midRangeCollisions.Count != 0) {
            _botEvents.midRangeCollisions.Clear();
        }
        _botEvents.targetedByZap = false;
    }

    private void RunActions(float delta, List<IPilotAction> actions) {
        foreach (var a in actions) {
            if (!DoAction(delta, a)) {
                var msg = $"{pilot.name}: action failed: {a.DebugString()}";
                GD.Print(msg);
                throw new Exception(msg);
            }
        }
    }

    private bool DoAction(float delta, IPilotAction a) {
        switch (a) {
            case AddWaypointAction addWaypoint:
                return DoAddWaypoint(addWaypoint);
            case FireAction fireAction:
                return DoFireAction(fireAction);
            case ChargeWeaponAction chargeWeaponAction:
                return DoChargeWeaponAction(delta, chargeWeaponAction);
            case SpecialAction specialAction:
                return DoSpecialAction(specialAction);
            case ChangeWaypointAction changeWaypoint:
                return DoChangeWaypointAction(changeWaypoint);
            case ShieldAction shieldAction:
                return DoShieldAction(shieldAction);
            default:
                throw new Exception("unexpected " + a.DebugString() + " action");
        }
    }

    private bool DoChangeWaypointAction(ChangeWaypointAction a) {
        pilot.Vessel.ClearWaypoints();
        var wp = Waypoint.New(false, Color.Color8(255, 100, 100));
        wp.GlobalPosition = new Vector2(a.pos);
        GetParent().AddChild(wp);
        pilot.Vessel.AddWaypoint(wp);
        return true;
    }

    private bool DoAddWaypoint(AddWaypointAction a) {
        var wp = Waypoint.New(false, Color.Color8(255, 100, 100));
        GetParent().AddChild(wp);
        wp.GlobalPosition = new Vector2(a.pos);
        pilot.Vessel.AddWaypoint(wp);
        return true;
    }

    private bool DoChargeWeaponAction(float delta, ChargeWeaponAction a) {
        var vessel = pilot.Vessel;
        vessel.specialWeapon.Charge(delta);
        return true;
    }

    private bool DoFireAction(FireAction a) {
        var vessel = pilot.Vessel;
        var canFire = vessel.CanFire(a.weaponIndex, a.target);
        vessel.Fire(a.weaponIndex, a.target);
        return canFire;
    }

    private bool DoSpecialAction(SpecialAction a) {
        var vessel = pilot.Vessel;
        var canFire = vessel.specialWeapon.CanFire(vessel.State, a.target);
        vessel.specialWeapon.Fire(vessel.State, a.target);
        return canFire;
    }

    private bool DoShieldAction(ShieldAction a) {
        var vessel = pilot.Vessel;
        var canShield = vessel.shield.CanActivate(vessel.State);
        vessel.shield.Activate(vessel.State);
        return canShield;
    }
}
