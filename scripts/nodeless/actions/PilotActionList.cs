using Godot;
using System.Collections.Generic;

public class PilotActionList {
    public List<IPilotAction> list = new List<IPilotAction>();

    public void ChangeWaypoint(Vector2 pos) {
        list.Add(new ChangeWaypointAction{pos = pos});
    }

    public void AddWaypoint(Vector2 pos) {
        list.Add(new AddWaypointAction{pos = pos});
    }

    public void FireAction(int weapon, Vector2 target) {
        list.Add(new FireAction{target = target, weaponIndex = weapon});
    }

    public void SpecialAction(Vector2 target) {
        list.Add(new SpecialAction{target = target});
    }

    public void ShieldAction() {
        list.Add(new ShieldAction{});
    }
}