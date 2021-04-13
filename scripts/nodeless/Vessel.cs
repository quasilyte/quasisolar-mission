using System.Collections.Generic;
using Godot;

public class Vessel: IItem {
    public Player player;

    public bool isBot;

    public int deviceId;
    public bool isGamepad;

    public string pilotName;

    public VesselDesign design;
    public EnergySource energySource;
    public List<ArtifactDesign> artifacts = new List<ArtifactDesign>();
    public List<WeaponDesign> weapons = new List<WeaponDesign>();
    public WeaponDesign specialWeapon = EmptyWeapon.Design;
    public ShieldDesign shield = EmptyShield.Design;

    public Vector2 spawnPos; // TODO: get rid of it?

    public float hp;
    public float energy;

    public ItemKind Kind() { return ItemKind.Vessel; }

    public void AddEnergy(float amount) {
        energy = QMath.Clamp(energy + amount, 0, energySource.maxBackupEnergy);
    }

    public string RenderHelp() {
        return "TODO!";
    }

    public int TotalCost() {
        int cost = 0;
        foreach (WeaponDesign w in weapons) {
            cost += w.sellingPrice;
        }
        cost += specialWeapon.sellingPrice;
        foreach (ArtifactDesign art in artifacts) {
            cost += art.sellingPrice;
        }
        cost += energySource.sellingPrice;
        cost += design.sellingPrice;
        cost += shield.sellingPrice;
        return cost;
    }
}
