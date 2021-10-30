using System.Collections.Generic;
using Godot;

public class Vessel: AbstractPoolValue, IItem {
    public struct Ref {
        public long id;
        public Vessel Get() { return RpgGameState.instance.vessels.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public Faction faction;

    public bool isBot;
    public bool isMercenary = false;

    public int deviceId;
    public bool isGamepad;
    public bool isFlagship = false;

    public string pilotName;

    // There are 3 ranks: 1, 2 and 3.
    public int rank = 2;

    public string designName;
    public string energySourceName;
    public List<string> artifacts = new List<string>();
    public List<string> weapons = new List<string>();
    public List<string> modList = new List<string>();
    public string specialWeaponName = EmptyWeapon.Design.name;
    public string shieldName = EmptyShield.Design.name;
    public string sentinelName = "Empty";

    public Vector2 spawnPos; // TODO: get rid of it?

    public float hp;
    public float energy;
    public int exp;

    public ItemKind GetItemKind() { return ItemKind.Vessel; }

    public void AddEnergy(float amount) {
        energy = QMath.Clamp(energy + amount, 0, MaxBackupEnergy());
    }

    public EnergySource GetEnergySource() { return EnergySource.Find(energySourceName); }

    public WeaponDesign SpecialWeapon() { return WeaponDesign.Find(specialWeaponName); }

    public VesselDesign Design() { return VesselDesign.Find(designName); }

    public ShieldDesign Shield() { return ShieldDesign.Find(shieldName); }

    public SentinelDesign Sentinel() { return SentinelDesign.Find(sentinelName); }

    public float MaxHp() { return (new VesselStats(this)).maxHp; }
    public float MaxBackupEnergy() { return (new VesselStats(this).maxBackupEnergy); }

    public int MaxCargo() { 
        float cargo = (float)Design().cargoSpace;
        var multiplier = 1.0f;
        foreach (var modName in modList) {
            multiplier += VesselMod.modByName[modName].cargoSpace;
        }
        return (int)(cargo * multiplier);
    }

    public VesselStats RecalculateStats() {
        var stats = new VesselStats(this);
        hp = QMath.ClampMax(hp, stats.maxHp);
        energy = QMath.ClampMax(energy, stats.maxBackupEnergy);
        return stats;
    }

    // public ArtifactDesign Artifact(int i) { return ArtifactDesign.Find(artifacts[i]); }

    public int TotalCost() {
        int cost = 0;
        foreach (string w in weapons) {
            cost += WeaponDesign.Find(w).sellingPrice;
        }
        cost += SpecialWeapon().sellingPrice;
        foreach (string art in artifacts) {
            cost += ArtifactDesign.Find(art).sellingPrice;
        }
        cost += GetEnergySource().sellingPrice;
        cost += Design().sellingPrice;
        cost += Shield().sellingPrice;
        cost += Sentinel().sellingPrice;
        return cost;
    }
}
