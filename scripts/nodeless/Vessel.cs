using System.Collections.Generic;
using Godot;

public class Vessel: AbstractItem {
    public Player player;

    public bool isBot;

    public int deviceId;
    public bool isGamepad;

    public string pilotName;

    public string designName;
    public string energySourceName;
    public List<string> artifacts = new List<string>();
    public List<string> weapons = new List<string>();
    public string specialWeaponName = EmptyWeapon.Design.name;
    public string shieldName = EmptyShield.Design.name;

    public Vector2 spawnPos; // TODO: get rid of it?

    public float hp;
    public float energy;

    public override ItemKind Kind() { return ItemKind.Vessel; }

    public void AddEnergy(float amount) {
        energy = QMath.Clamp(energy + amount, 0, GetEnergySource().maxBackupEnergy);
    }

    public EnergySource GetEnergySource() { return EnergySource.Find(energySourceName); }

    public WeaponDesign SpecialWeapon() { return WeaponDesign.Find(specialWeaponName); }

    public VesselDesign Design() { return VesselDesign.Find(designName); }

    public ShieldDesign Shield() { return ShieldDesign.Find(shieldName); }

    // public ArtifactDesign Artifact(int i) { return ArtifactDesign.Find(artifacts[i]); }

    public override string RenderHelp() {
        return "TODO!";
    }

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
        return cost;
    }
}
