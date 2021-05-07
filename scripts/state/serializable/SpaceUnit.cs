using Godot;
using System.Collections.Generic;

public class SpaceUnit: AbstractPoolValue {
    public struct Ref {
        public long id;
        public SpaceUnit Get() { return RpgGameState.instance.spaceUnits.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public enum Program {
        GenericBehavior,
        KrigiaPatrol,
        KrigiaTaskForce,
        KrigiaReinforcements,
        KrigiaFinalAttack,
    }

    public const int maxFleetSize = 4;

    public Faction owner;
    public Vector2 pos;
    public Vector2 waypoint = Vector2.Zero;
    public List<Vessel.Ref> fleet = new List<Vessel.Ref>();
    public SpaceUnitCargo cargo = new SpaceUnitCargo();

    public int botSystemLeaveDelay = 0;
    public Program botProgram = Program.GenericBehavior;
    public StarBase.Ref botOrigin;

    public int FleetCost() {
        var cost = 0;
        foreach (var v in fleet) {
            cost += v.Get().TotalCost();
        }
        return cost;
    }

    public int CargoCapacity() {
        int max = 0;
        foreach (var v in fleet) {
            max += v.Get().Design().cargoSpace;
        }
        return max;
    }

    public int CargoSize() {
        return cargo.power + cargo.organic + cargo.minerals + DebrisCount();
    }

    public int CargoFree() {
        return CargoCapacity() - CargoSize();
    }

    public int DebrisCount() {
        return cargo.genericDebris + cargo.krigiaDeris + cargo.wertuDebris + cargo.zythDebris;
    }

    public void CargoAddDebris(int amount, Research.Material kind) {
        var toAdd = QMath.ClampMax(amount, CargoFree());
        if (kind == Research.Material.Krigia) {
            cargo.krigiaDeris = QMath.ClampMin(cargo.krigiaDeris + toAdd, 0);
        } else if (kind == Research.Material.Wertu) {
            cargo.wertuDebris = QMath.ClampMin(cargo.wertuDebris + toAdd, 0);
        } else if (kind == Research.Material.Zyth) {
            cargo.zythDebris = QMath.ClampMin(cargo.zythDebris + toAdd, 0);
        } else {
            cargo.genericDebris = QMath.ClampMin(cargo.genericDebris + toAdd, 0);
        }
    }

    public void CargoAddMinerals(int amount) {
        var toAdd = QMath.ClampMax(amount, CargoFree());
        cargo.minerals = QMath.ClampMin(cargo.minerals + toAdd, 0);
    }

    public void CargoAddOrganic(int amount) {
        var toAdd = QMath.ClampMax(amount, CargoFree());
        cargo.organic = QMath.ClampMin(cargo.organic + toAdd, 0);
    }

    public void CargoAddPower(int amount) {
        var toAdd = QMath.ClampMax(amount, CargoFree());
        cargo.power = QMath.ClampMin(cargo.power + toAdd, 0);
    }
}
