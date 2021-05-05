using System.Collections.Generic;
using Godot;

public class StarBase: AbstractPoolValue {
    public struct Ref {
        public long id;
        public StarBase Get() { return RpgGameState.instance.starBases.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public Faction owner;

    public StarSystem.Ref system;

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
    public Queue<string> productionQueue = new Queue<string>();

    public List<Vessel.Ref> garrison = new List<Vessel.Ref>();

    // For bots: base-controlled space units.
    public HashSet<SpaceUnit.Ref> units = new HashSet<SpaceUnit.Ref>();
    public int botPatrolDelay = 0;
    public int botReinforcementsDelay = 0;
    public int botProductionDelay = 0;

    public const int maxGarrisonSize = 24;
    public const int maxBaseLevel = 5;

    public StarBase() {}

    public Vessel.Ref PopVessel() {
        if (garrison.Count > 0) {
            var vessel = garrison[garrison.Count - 1];
            garrison.RemoveAt(garrison.Count - 1);
            return vessel;
        }
        return new Vessel.Ref{id = 0};
    }

    public List<IItem> ShopSelection() {
        var shopSelection = new List<IItem>();

        var technologiesResearched = RpgGameState.instance.technologiesResearched;

        foreach (WeaponDesign weapon in WeaponDesign.list) {
            if (!Research.IsAvailable(technologiesResearched, weapon.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(weapon)) {
                continue;
            }
            shopSelection.Add(weapon);
        }

        foreach (WeaponDesign weapon in WeaponDesign.specialList) {
            if (!Research.IsAvailable(technologiesResearched, weapon.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(weapon)) {
                continue;
            }
            shopSelection.Add(weapon);
        }

        foreach (ShieldDesign shield in ShieldDesign.list) {
            if (!Research.IsAvailable(technologiesResearched, shield.technologiesNeeded)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(shield)) {
                continue;
            }
            if (shield.researchRequired && !technologiesResearched.Contains(shield.name)) {
                continue;
            }
            shopSelection.Add(shield);
        }

        // Start from 1 to skip the "None" energy source.
        for (int i = 1; i < EnergySource.list.Length; i++) {
            var energySource = EnergySource.list[i];
            if (energySource.researchRequired && !technologiesResearched.Contains(energySource.name)) {
                continue;
            }
            shopSelection.Add(energySource);
        }

        foreach (var art in ArtifactDesign.list) {
            if (!technologiesResearched.Contains(art.name)) {
                continue;
            }
            shopSelection.Add(art);
        }

        return shopSelection;
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