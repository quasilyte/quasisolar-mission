using System.Collections.Generic;
using Godot;

public class StarBase {
    public Player owner;

    public StarSystem system;

    // 0 - not discovered.
    // Non-zero value represents the day at which this base was discovered.
    public int discoveredByKrigia = 0;

    public int hp = 100;

    public int level = 1;

    public float levelProgression = 0;

    public int mineralsStock = 0;
    public int organicStock = 0;
    public int powerStock = 0;

    public int productionProgress = 0;
    public Queue<VesselDesign> productionQueue = new Queue<VesselDesign>();

    public List<Vessel> garrison = new List<Vessel>();

    public List<IItem> shopSelection = new List<IItem> { };

    // For bots: base-controlled space units.
    public HashSet<SpaceUnit> units = new HashSet<SpaceUnit>();
    public int botPatrolDelay = 0;
    public int botReinforcementsDelay = 0;
    public int botProductionDelay = 0;

    public const int maxGarrisonSize = 24;
    public const int maxBaseLevel = 5;

    public StarBase(StarSystem sys, Player player, int level = 1) {
        this.level = level;
        system = sys;
        owner = player;
        mineralsStock = 70 + QRandom.IntRange(0, 30);
        organicStock = 10 + QRandom.IntRange(0, 50);
        powerStock = QRandom.IntRange(0, 50);
    }

    public Vessel PopVessel() {
        if (garrison.Count > 0) {
            var vessel = garrison[garrison.Count - 1];
            garrison.RemoveAt(garrison.Count - 1);
            return vessel;
        }
        return null;
    }

    public void UpdateShopSelection() {
        shopSelection = new List<IItem>();

        foreach (WeaponDesign weapon in WeaponDesign.list) {
            if (!Research.IsAvailable(RpgGameState.technologiesResearched, weapon.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(weapon)) {
                continue;
            }
            shopSelection.Add(weapon);
        }

        foreach (WeaponDesign weapon in WeaponDesign.specialList) {
            if (!Research.IsAvailable(RpgGameState.technologiesResearched, weapon.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(weapon)) {
                continue;
            }
            shopSelection.Add(weapon);
        }

        foreach (ShieldDesign shield in ShieldDesign.list) {
            if (!Research.IsAvailable(RpgGameState.technologiesResearched, shield.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(shield)) {
                continue;
            }
            if (shield.researchRequired && !RpgGameState.technologiesResearched.Contains(shield.name)) {
                continue;
            }
            shopSelection.Add(shield);
        }

        // Start from 1 to skip the "None" energy source.
        for (int i = 1; i < EnergySource.list.Length; i++) {
            var energySource = EnergySource.list[i];
            if (energySource.researchRequired && !RpgGameState.technologiesResearched.Contains(energySource.name)) {
                continue;
            }
            shopSelection.Add(energySource);
        }

        foreach (var art in ArtifactDesign.list) {
            if (!RpgGameState.technologiesResearched.Contains(art.name)) {
                continue;
            }
            shopSelection.Add(art);
        }
    }

    public float LevelUpgradeCost() {
        // 300
        // 600
        // 900
        // 1200
        // 1500
        // = 4500 slowest, 1125 fastest
        return level * 300;
    }
}