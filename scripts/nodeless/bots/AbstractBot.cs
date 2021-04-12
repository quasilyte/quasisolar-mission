using Godot;
using System.Collections.Generic;

public abstract class AbstractBot {
    protected abstract void ActImpl(float delta, BotEvents events);

    public List<IPilotAction> Act(float delta, BotEvents events) {
        _actions.list.Clear();
        ActImpl(delta, events);
        return _actions.list;
    }

    public AbstractBot(VesselNode vessel) {
        _vessel = vessel;
        _pilot = vessel.pilot;
    }

    public void Ready() {
        _screenCenter = new Vector2(_vessel.GetTree().Root.Size.x / 2, _vessel.GetTree().Root.Size.y / 2);
    }

    protected PilotActionList _actions = new PilotActionList();

    protected VesselNode _vessel;
    protected Pilot _pilot;
    protected Pilot _currentTarget;

    protected Vector2 _screenCenter;

    protected bool IsInstanceValid(Node x) {
        return Godot.Object.IsInstanceValid(x);
    }

    protected float TargetDistance() {
        return _currentTarget.Vessel.Position.DistanceTo(_vessel.Position);
    }

    protected Vector2 TargetPosition() {
        return _currentTarget.Vessel.Position;
    }

    protected bool IsOutOfScreen(Vector2 pos) {
        return pos.x > _vessel.GetTree().Root.Size.x || pos.x < 0 ||
            pos.y > _vessel.GetTree().Root.Size.y || pos.y < 0;

    }

    protected int numActiveEnemies() {
        var n = 0;
        foreach (var enemy in _pilot.Enemies) {
            if (enemy.Active) {
                n++;
            }
        }
        return n;
    }
}
