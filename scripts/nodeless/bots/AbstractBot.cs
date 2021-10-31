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
        _screenCenter = RootSize() / 2;
    }

    protected Vector2 RootSize() {
        return _vessel.GetTree().Root.GetVisibleRect().Size;
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
        var size = RootSize();
        return pos.x > size.x || pos.x < 0 ||
            pos.y > size.y || pos.y < 0;

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
