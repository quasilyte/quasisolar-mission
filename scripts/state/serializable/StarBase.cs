using System.Collections.Generic;
using Godot;

public class StarBase: AbstractPoolValue {
    public struct Ref {
        public long id;
        public StarBase Get() { return RpgGameState.instance.starBases.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public enum Mode {
        ProduceMinerals,
        ProducePower,
        ProduceRU,
    }

    public struct PriceInfo {
        public int value;
        public float multiplier;
    }

    public Faction owner;

    public Mode mode = Mode.ProduceRU;

    public StarSystem.Ref system;

    // 0 - not discovered.
    // Non-zero value represents the day at which this base was discovered.
    public int discoveredByKrigia = 0;

    public int level = 1;

    public float levelProgression = 0;

    public int mineralsStock = 0;
    public int organicStock = 0;
    public int powerStock = 0;

    public int constructionProgress = 0;
    public string constructionTarget = "";

    public double productionProgress = 0;
    public Queue<string> productionQueue = new Queue<string>();

    public List<Vessel.Ref> garrison = new List<Vessel.Ref>();

    public List<string> modules = new List<string>();

    // For bots: base-controlled space units.
    public HashSet<SpaceUnit.Ref> units = new HashSet<SpaceUnit.Ref>();
    public int botPatrolDelay = 0;
    public int botReinforcementsDelay = 0;
    public int botProductionDelay = 0;

    public const int maxGarrisonSize = 24;
    public const int maxBaseLevel = 5;

    public StarBase() {}

    public int GarrisonCost() {
        var cost = 0;
        foreach (var v in garrison) {
            cost += v.Get().TotalCost();
        }
        return cost;
    }

    public int StorageFreeSpace() {
        return StorageCapacity() - mineralsStock - organicStock - powerStock;
    }

    public int StorageCapacity() {
        return 750;
    }

    public void AddMinerals(int amount) {
        mineralsStock += QMath.ClampMax(amount, StorageFreeSpace());
    }

    public void AddOrganic(int amount) {
        organicStock += QMath.ClampMax(amount, StorageFreeSpace());
    }

    public void AddPower(int amount) {
        powerStock += QMath.ClampMax(amount, StorageFreeSpace());
    }

    public Vessel.Ref PopVessel() {
        if (garrison.Count > 0) {
            var vessel = garrison[garrison.Count - 1];
            garrison.RemoveAt(garrison.Count - 1);
            return vessel;
        }
        return new Vessel.Ref{id = 0};
    }

    public int VesselProductionPrice(VesselDesign design) {
        return modules.Contains("Production Facility") ? QMath.IntAdjust(design.sellingPrice, 0.80) : design.sellingPrice;
    }

    public int FuelPrice() {
        return modules.Contains("Refuel Facility") ? 1 : 5;
    }

    public int RepairPrice(Vessel v) {
        int price = 3 + (v.Design().level * 2);
        foreach (var statusName in v.modList) {
            price += VesselMod.modByName[statusName].repairCost;
        }
        if (modules.Contains("Repair Facility")) {
            price /= 2;
        }
        price = QMath.ClampMin(price, 1);
        return price;
    }

    public PriceInfo DebrisSellPrice() {
        if (modules.Contains("Debris Rectifier")) {
            return new PriceInfo{value = 24, multiplier = 1};
        }
        return new PriceInfo{value = 18, multiplier = 1};
    }

    public PriceInfo MineralsSellPrice() {
        return CalculateSellingPrice(mineralsStock, 14);
    }

    public PriceInfo OrganicSellPrice() {
        return CalculateSellingPrice(organicStock, 20);
    }

    public PriceInfo PowerSellPrice() {
        return CalculateSellingPrice(powerStock, 22);
    }

    public int VesselRank(float roll) {
        var rank = 1;
        if (level == 1) {
            rank = GenerateRank(0.9, 0.1);
        } else if (level == 2) {
            rank = GenerateRank(0.7, 0.3);
        } else if (level == 3) {
            rank = GenerateRank(0.5, 0.4);
        } else if (level == 4) {
            rank = GenerateRank(0.3, 0.45);
        } else {
            rank = GenerateRank(0.1, 0.45);
        }
        var maxRank = 3;
        if (roll < 0.3f) {
            maxRank = 1;
        } else if (roll < 0.6f) {
            maxRank = 2;
        }
        return QMath.ClampMax(rank, maxRank);
    }

    private int GenerateRank(double first, double second) {
        var roll = QRandom.Float();
        if (roll < first) {
            return 1;
        }
        if (roll < second) {
            return 2;
        }
        return 3;
    }

    private PriceInfo CalculateSellingPrice(int currentAmount, float basePrice) {
        // 1 = 120%
        // 2 = 110%
        // 3 = 100%
        // 4 = 90%
        // 5 = 80%
        float levelMultiplier = 1.2f - (level * 0.1f);

        // 0   = 140%
        // 50  = 135%
        // 100 = 130%
        // 150 = 125%
        // 200 = 120%
        // 250 = 115%
        // 300 = 110%
        // 350 = 105%
        // 400 = 100%
        // 450 = 95%
        // 500 = 90%
        // 550 = 85%
        float shortageMultiplier = QMath.ClampMin(1.4f - 0.05f * (currentAmount / 50), 0.85f);

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
            if (weapon.researchRequired && !technologiesResearched.Contains(weapon.name)) {
                continue;
            }
            if (level < ItemInfo.MinStarBaseLevel(weapon)) {
                continue;
            }
            shopSelection.Add(weapon);
        }

        foreach (WeaponDesign weapon in WeaponDesign.specialList) {
            if (weapon.researchRequired && !technologiesResearched.Contains(weapon.name)) {
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