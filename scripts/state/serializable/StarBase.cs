using System.Collections.Generic;
using Godot;

public class StarBase: AbstractPoolValue {
    public struct Ref {
        public long id;
        public StarBase Get() { return RpgGameState.instance.starBases.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public struct PriceInfo {
        public int value;
        public float multiplier;
    }

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

    public PriceInfo DebrisSellPrice() { return new PriceInfo{value = 13, multiplier = 1}; }

    public PriceInfo MineralsSellPrice() {
        return CalculateSellingPrice(mineralsStock, 14);
    }

    public PriceInfo OrganicSellPrice() {
        return CalculateSellingPrice(organicStock, 20);
    }

    public PriceInfo PowerSellPrice() {
        return CalculateSellingPrice(powerStock, 22);
    }

    private PriceInfo CalculateSellingPrice(int currentAmount, float basePrice) {
        // 1 = 120%
        // 2 = 110%
        // 3 = 100%
        // 4 = 90%
        // 5 = 80%
        float levelMultiplier = 1.2f - (level * 0.1f);

        // 0   = 130%
        // 50  = 125%
        // 100 = 120%
        // 150 = 115%
        // 200 = 110%
        // 250 = 105%
        // 300 = 100%
        // 350 = 95%
        // 400 = 90%
        float shortageMultiplier = QMath.ClampMin(1.3f - 0.05f * (currentAmount / 50), 0.9f);

        // Every 300 days the general price goes down.
        // 0-300     = 105%
        // 300-600   = 104%
        // 600-900   = 103%
        // 900-1200  = 102%
        // 1200-1500 = 101%
        // 1500-1800 = 100%
        // 1800-2100 = 99%
        // 2100-2400 = 98%
        // 2400-2700 = 97%
        // 2700-3000 = 96%
        // 3000-3300 = 95%
        // 3300-3600 = 94%
        // 3600-3900 = 93%
        float inflationMultiplier = QMath.ClampMin(1.05f - 0.01f * (RpgGameState.instance.day / 300), 0.5f);

        float multiplier = levelMultiplier * shortageMultiplier * inflationMultiplier;
        int value = (int)(basePrice * multiplier);
        return new PriceInfo{value = value, multiplier = multiplier};
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

        foreach (var sentinel in SentinelDesign.list) {
            if (sentinel.name == "Empty") {
                continue;
            }
            if (sentinel.researchRequired && !technologiesResearched.Contains(sentinel.name)) {
                continue;
            }

            shopSelection.Add(sentinel);
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