using Godot;
using System;
using System.Collections.Generic;

// TODO: rename this class.
public static class VesselFactory {
    public static void PadEquipment(Vessel v) {
        v.energySourceName = "None";
        v.weapons = new List<string>{
            EmptyWeapon.Design.name,
            EmptyWeapon.Design.name,
        };
        v.artifacts = new List<string>{
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
        };
        v.sentinelName = "Empty";
    }

    public static void InitStats(Vessel v) {
        var stats = new VesselStats(v);
        v.hp = stats.maxHp;
        v.energy = stats.maxBackupEnergy;
    }

    public static void Init(Vessel v, VesselDesign design) {
        Init(v, design.affiliation + " " + design.name);
    }

    public static Vessel NewVessel(Faction faction, string designName, int rank = 2) {
        var design = VesselDesign.Find(designName);
        var vessel = RpgGameState.instance.NewVessel(faction, design);
        vessel.rank = rank;
        Init(vessel, design);
        return vessel;
    }

    public static void Init(Vessel v, string kind) {
        v.sentinelName = "Empty";

        if (kind == "Neutral X-The-Bit") {
            InitXTheBit(v);
        } else if (kind == "Neutral Ravager") {
            InitNeutralRavager(v);
        } else if (kind == "Neutral Nomad") {
            InitNeutralNomad(v);
        } else if (kind == "Neutral Weak Avenger") {
            InitNeutralAvenger(v, true);
        } else if (kind == "Neutral Avenger") {
            InitNeutralAvenger(v, false);
        } else if (kind == "Krigia Talons") {
            InitKrigiaTalons(v);
        } else if (kind == "Krigia Claws") {
            InitKrigiaClaws(v);
        } else if (kind == "Krigia Fangs") {
            InitKrigiaFangs(v);
        } else if (kind == "Krigia Destroyer") {
            InitKrigiaDestroyer(v);
        } else if (kind == "Krigia Tusks") {
            InitKrigiaTusks(v);
        } else if (kind == "Krigia Horns") {
            InitKrigiaHorns(v);
        } else if (kind == "Krigia Ashes") {
            InitKrigiaAshes(v);
        } else if (kind == "Earthling Scout") {
            InitEarthlingScout(v);
        } else if (kind == "Earthling Explorer") {
            InitEarthlingExplorer(v);
        } else if (kind == "Earthling Freighter") {
            InitEarthlingFreighter(v);
        } else if (kind == "Earthling Fighter") {
            InitEarthlingFighter(v);
        } else if (kind == "Earthling Interceptor") {
            InitEarthlingInterceptor(v);
        } else if (kind == "Earthling Ark") {
            InitEarthlingArk(v);
        } else if (kind == "Earthling Gladiator") {
            InitEarthlingGladiator(v);
        } else if (kind == "Earthling Ragnarok") {
            InitEarthlingRagnarok(v);
        } else if (kind == "Wertu Probe") {
            InitWertuProbe(v);
        } else if (kind == "Wertu Transporter") {
            InitWertuTransporter(v);
        } else if (kind == "Wertu Guardian") {
            InitWertuGuardian(v);
        } else if (kind == "Wertu Angel") {
            InitWertuAngel(v);
        } else if (kind == "Wertu Dominator") {
            InitWertuDominator(v);
        } else if (kind == "Zyth Hunter") {
            InitZythHunter(v);
        } else if (kind == "Zyth Invader") {
            InitZythInvader(v);
        } else if (kind == "Vespion Larva") {
            InitVespionLarva(v);
        } else if (kind == "Vespion Hornet") {
            InitVespionHornet(v);
        } else if (kind == "Vespion Queen") {
            InitVespionQueen(v);
        } else if (kind == "Neutral Spectre") {
            InitUniqueSpectre(v);
        } else if (kind == "Neutral Visitor") {
            InitUniqueVisitor(v);
        } else if (kind == "Draklid Raider") {
            InitDraklidRaider(v);
        } else if (kind == "Draklid Marauder") {
            InitDraklidMarauder(v);
        } else if (kind == "Draklid Plunderer") {
            InitDraklidPlunderer(v);
        } else if (kind == "Phaa Mantis") {
            InitPhaaMantis(v);
        } else if (kind == "PhaaRebel Spacehopper") {
            InitPhaaSpacehopper(v);
        } else if (kind == "Rarilou Leviathan") {
            InitRarilouLeviathan(v);
        } else if (kind == "Lezeona Pin") {
            InitLezeonaPin(v);
        } else if (kind == "Lezeona Core") {
            InitLezeonaCore(v);
        } else {
            throw new Exception($"unexpected player slot kind: {kind}");
        }

        if (v.weapons.Count > 2) {
            throw new Exception($"more than 2 weapons are given to {kind}");
        }

        while (v.weapons.Count < 2) {
            v.weapons.Add(EmptyWeapon.Design.name);
        }
        while (v.artifacts.Count < 5) {
            v.artifacts.Add(EmptyArtifact.Design.name);
        }

        InitStats(v);
    }

    private static double RankChance(int rank, double first, double second, double third) {
        if (rank == 1) {
            return first;
        }
        if (rank == 2) {
            return second;
        }
        return third;
    }

    private static string Pick(object[] options) {
        var roll = QRandom.Float();
        double current = 0;
        for (int i = 0; i < options.Length; i += 2) {
            double key = (double)options[i];
            string val = (string)options[i+1];
            current += key;
            if (current > 1.0) {
                throw new Exception("total chance space exceeds 1.0");
            }
            if (roll < current) {
                return val;
            }
        }
        return "";
    }

    private static void SetShield(Vessel v, params object[] options) {
        var shield = Pick(options);
        if (shield != "") {
            v.shieldName = shield;
        }
    }

    private static void AddWeapon(Vessel v, params object[] options) {
        var w = Pick(options);
        if (w != "") {
            if (WeaponDesign.Find(w).isSpecial) {
                throw new Exception("added special weapon with AddWeapon");
            }
            v.weapons.Add(w);
        }
    }

    private static void SetSpecialWeapon(Vessel v, params object[] options) {
        var w = Pick(options);
        if (w != "") {
            if (!WeaponDesign.Find(w).isSpecial) {
                throw new Exception("added normal weapon with SetSpecialWeapon");
            }
            v.specialWeaponName = w;
        }
    }

    private static void InitXTheBit(Vessel v) {
        v.designName = "X-The-Bit";
        v.energySourceName = "Cryogenic Block";

        v.specialWeaponName = TempestWeapon.Design.name;
        v.sentinelName = "Stinger Fighter";

        v.shieldName = HeatScreenShield.Design.name;

        v.weapons.Add(FlareWeapon.Design.name);

        v.artifacts.Add(MagneticNegatorArtifact.Design.name);
        v.artifacts.Add(DroidArtifact.Design.name);
        v.artifacts.Add(SentinelControllerArtifact.Design.name);
        v.artifacts.Add(SentinelLinkArtifact.Design.name);

        v.modList.Add("Sentinel Patch: Overclock");
        v.modList.Add("Sentinel Patch: Berserk");
        v.modList.Add("Sentinel Patch: Bastion");
        v.modList.Add("Shield Booster");
    }

    private static void InitNeutralRavager(Vessel v) {
        v.designName = "Ravager";
        v.energySourceName = "Vortex Battery";
        if (v.rank == 1) {
            v.energySourceName = "Advanced Power Generator";
        }

        if (QRandom.Float() < RankChance(v.rank, 0.3, 0.65, 1)) {
            v.specialWeaponName = TempestWeapon.Design.name;
        }

        AddWeapon(v,
            0.6, IonCannonWeapon.Design.name,
            0.4, StingerWeapon.Design.name);
    }

    private static void InitNeutralNomad(Vessel v) {
        v.designName = "Nomad";
        v.energySourceName = "Vortex Battery";

        AddWeapon(v,
            0.6, PhotonBurstCannonWeapon.Design.name,
            0.4, ZapWeapon.Design.name);

        v.weapons.Add(CutterWeapon.Design.name);

        SetShield(v,
            0.25, DispersionFieldShield.Design.name,
            0.25, DeceleratorShield.Design.name,
            0.5, LatticeShield.Design.name);
    }

    private static void InitNeutralAvenger(Vessel v, bool weak) {
        v.designName = "Avenger";
        v.energySourceName = weak ? "Power Generator" : "Vortex Battery";

        v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);
        if (weak) {
            v.shieldName = HeatScreenShield.Design.name;
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design.name);
            v.shieldName = DeceleratorShield.Design.name;
        }
    }

    private static void InitDraklidRaider(Vessel v) {
        v.designName = "Raider";
        v.energySourceName = "Power Generator";

        AddWeapon(v,
            0.4, NeedleGunWeapon.Design.name,
            0.3, ZapWeapon.Design.name,
            0.2, PointDefenseLaserWeapon.Design.name,
            0.1, SpreadGunWeapon.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0.1, 0.3, 0.8)) {
            v.shieldName = IonCurtainShield.Design.name;
        }
    }

    private static void InitDraklidMarauder(Vessel v) {
        v.designName = "Marauder";
        v.energySourceName = "Advanced Power Generator";

        AddWeapon(v,
            0.3, RocketLauncherWeapon.Design.name,
            0.3, PointDefenseLaserWeapon.Design.name,
            0.15, StingerWeapon.Design.name,
            0.15, PulseLaserWeapon.Design.name,
            0.1, AssaultLaserWeapon.Design.name);

        var specialRoll = QRandom.Float();
        if (specialRoll < RankChance(v.rank, 0.2, 0.3, 0.5)) {
            v.specialWeaponName = DisruptorWeapon.Design.name;
        } else {
            v.specialWeaponName = AfterburnerWeapon.Design.name;
        }

        if (QRandom.Float() < RankChance(v.rank, 0.2, 0.5, 0.9)) {
            v.shieldName = IonCurtainShield.Design.name;
        }

        // 40% - ion fighter
        // 20% - point-defense guard
        if (v.rank >= 2) {
            var sentinelRoll = QRandom.Float();
            if (sentinelRoll < 0.4) {
                v.sentinelName = "Ion Fighter";
            } else if (sentinelRoll < 0.6) {
                v.sentinelName = "Point-Defense Guard";
            }
        }
    }

    private static void InitDraklidPlunderer(Vessel v) {
        v.designName = "Plunderer";
        if (v.rank == 3) {
            v.energySourceName = "Cryogenic Block";
        } else {
            v.energySourceName = "Vortex Battery";
        }

        AddWeapon(v,
            0.4, PulseLaserWeapon.Design.name,
            0.4, AssaultLaserWeapon.Design.name,
            0.2, StingerWeapon.Design.name);

        v.specialWeaponName = DisruptorWeapon.Design.name;

        SetShield(v, 0.5, IonCurtainShield.Design.name);
        if (QRandom.Float() < RankChance(v.rank, 0.6, 0.8, 1)) {
            v.shieldName = LaserPerimeterShield.Design.name;
        } else {
            v.shieldName = IonCurtainShield.Design.name;
        }

        if (QRandom.Float() < RankChance(v.rank, 0.6, 0.7, 1)) {
            v.sentinelName = "Ion Curtain Guard";
        }
    }

    private static void InitEarthlingScout(Vessel v) {
        v.designName = "Scout";
        v.energySourceName = "Power Generator";

        AddWeapon(v,
            0.6, SpreadGunWeapon.Design.name,
            0.4, NeedleGunWeapon.Design.name);
    }

    private static void InitEarthlingExplorer(Vessel v) {
        v.designName = "Explorer";
        v.energySourceName = "Power Generator";

        AddWeapon(v,
            0.5, IonCannonWeapon.Design.name,
            0.5, NeedleGunWeapon.Design.name);
    }

    private static void InitEarthlingFreighter(Vessel v) {
        v.designName = "Freighter";
        v.energySourceName = "Power Generator";

        AddWeapon(v,
            0.5, IonCannonWeapon.Design.name,
            0.5, NeedleGunWeapon.Design.name);
    }

    private static void InitEarthlingFighter(Vessel v) {
        v.designName = "Fighter";
        v.energySourceName = "Advanced Power Generator";

        AddWeapon(v,
            0.5, IonCannonWeapon.Design.name,
            0.5, ZapWeapon.Design.name);

        AddWeapon(v,
            0.7, PulseLaserWeapon.Design.name,
            0.3, NeedleGunWeapon.Design.name);

        SetShield(v,
            0.35, HeatScreenShield.Design.name,
            0.25, IonCurtainShield.Design.name);
    }

    private static void InitEarthlingInterceptor(Vessel v) {
        v.designName = "Interceptor";
        v.energySourceName = "Vortex Battery";

        AddWeapon(v,
            0.5, StingerWeapon.Design.name,
            0.3, PulseLaserWeapon.Design.name,
            0.2, IonCannonWeapon.Design.name);

        AddWeapon(v,
            0.8, RocketLauncherWeapon.Design.name,
            0.2, ZapWeapon.Design.name);

        SetShield(v,
            0.3, HeatScreenShield.Design.name,
            0.2, IonCurtainShield.Design.name,
            0.2, DispersionFieldShield.Design.name);
    }

    private static void InitEarthlingArk(Vessel v) {
        v.designName = "Ark";
        v.energySourceName = "Power Generator";

        v.specialWeaponName = ReaperCannonWeapon.Design.name;
    }

    private static void InitEarthlingGladiator(Vessel v) {
        v.designName = "Gladiator";
        v.energySourceName = "Vortex Battery";

        AddWeapon(v,
            0.5, PulseLaserWeapon.Design.name,
            0.5, ZapWeapon.Design.name);

        SetSpecialWeapon(v,
            0.6, ReaperCannonWeapon.Design.name,
            0.4, DisruptorWeapon.Design.name);

        SetShield(v,
            0.5, LaserPerimeterShield.Design.name,
            0.3, DispersionFieldShield.Design.name);
    }

    private static void InitEarthlingRagnarok(Vessel v) {
        v.designName = "Ragnarok";
        v.energySourceName = "Graviton Generator";

        AddWeapon(v,
            0.7, IonCannonWeapon.Design.name,
            0.3, ZapWeapon.Design.name);

        AddWeapon(v,
            0.7, RocketLauncherWeapon.Design.name,
            0.3, PointDefenseLaserWeapon.Design.name);

        SetSpecialWeapon(v,
            1.0, MjolnirWeapon.Design.name);

        SetShield(v,
            0.6, LaserPerimeterShield.Design.name,
            0.4, DispersionFieldShield.Design.name);
    }

    private static void InitKrigiaTalons(Vessel v) {
        v.designName = "Talons";

        if (v.rank < 3) {
            v.energySourceName = "Power Generator";
        } else {
            v.energySourceName = "Advanced Power Generator";
        }

        if (v.rank == 1) {
            AddWeapon(v, 
                0.5, ScytheWeapon.Design.name,
                0.5, IonCannonWeapon.Design.name);
        } else if (v.rank == 2) {
            AddWeapon(v,
                0.4, ScytheWeapon.Design.name,
                0.4, IonCannonWeapon.Design.name,
                0.2, PulseLaserWeapon.Design.name);
        } else {
            AddWeapon(v, 
                0.4, PulseLaserWeapon.Design.name,
                0.2, ScytheWeapon.Design.name,
                0.2, IonCannonWeapon.Design.name,
                0.2, StingerWeapon.Design.name);
        }

        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaClaws(Vessel v) {
        v.designName = "Claws";

        if (v.rank == 1) {
            v.energySourceName = "Power Generator";    
        } else if (v.rank == 2) {
            v.energySourceName = "Advanced Power Generator";
        } else if (v.rank == 3) {
            if (QRandom.Bool()) {
                v.energySourceName = "Advanced Power Generator";
            } else {
                v.energySourceName = "Advanced Power Generator";
            }
        }

        AddWeapon(v,
            0.5, PulseLaserWeapon.Design.name,
            0.3, IonCannonWeapon.Design.name,
            0.2, PointDefenseLaserWeapon.Design.name);

        AddWeapon(v,
            0.5, RocketLauncherWeapon.Design.name,
            0.4, v.rank < 3 ? ScytheWeapon.Design.name : GreatScytheWeapon.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0.2, 0.3, 0.7)) {
            v.shieldName = IonCurtainShield.Design.name;
        }

        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaFangs(Vessel v) {
        v.designName = "Fangs";
        if (v.rank == 1) {
            v.energySourceName = "Advanced Power Generator";
        } else {
            v.energySourceName = "Vortex Battery";
        }

        AddWeapon(v,
            0.6, PulseLaserWeapon.Design.name,
            0.2, AssaultLaserWeapon.Design.name,
            0.2, StingerWeapon.Design.name);

        AddWeapon(v,
            0.7, RocketLauncherWeapon.Design.name,
            0.3, v.rank < 3 ? ScytheWeapon.Design.name : GreatScytheWeapon.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0, 0.2, 0.5)) {
            v.specialWeaponName = DisintegratorWeapon.Design.name;
        }

        if (v.rank == 3) {
            var sentinelRoll = QRandom.Float();
            if (sentinelRoll < 0.4) {
                v.sentinelName = "Ion Curtain Guard";
            } else if (sentinelRoll < 0.8) {
                v.sentinelName = "Decelerator Guard";
            }
            SetShield(v,
                0.4, DeceleratorShield.Design.name,
                0.3, DispersionFieldShield.Design.name,
                0.2, IonCurtainShield.Design.name);
        } else {
            SetShield(v,
                0.3, DeceleratorShield.Design.name,
                0.2, DispersionFieldShield.Design.name,
                0.1, IonCurtainShield.Design.name);
        }
 
        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaDestroyer(Vessel v) {
        v.designName = "Destroyer";
        v.energySourceName = "Vortex Battery";

        v.weapons.Add(GreatScytheWeapon.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0.7, 0.9, 1)) {
            v.specialWeaponName = TorpedoLauncherWeapon.Design.name;
        } else {
            v.specialWeaponName = DisintegratorWeapon.Design.name;
        }

        SetShield(v,
            0.6, DispersionFieldShield.Design.name,
            0.4, DeceleratorShield.Design.name);

        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaTusks(Vessel v) {
        v.designName = "Tusks";

        var roll = QRandom.Float();
        v.weapons.Add(StingerWeapon.Design.name);
        if (roll < 0.5) {
            v.energySourceName = "Radioisotope Generator";
            AddWeapon(v, 
                0.5, RocketLauncherWeapon.Design.name,
                0.5, HurricaneWeapon.Design.name);
        } else {
            v.energySourceName = "Cryogenic Block";
            AddWeapon(v, 
                0.5, GreatScytheWeapon.Design.name,
                0.5, AssaultLaserWeapon.Design.name);
        }

        var specialRoll = QRandom.Float();
        if (specialRoll < RankChance(v.rank, 0.2, 0.4, 0.8)) {
            v.specialWeaponName = TorpedoLauncherWeapon.Design.name;
        } else {
            v.specialWeaponName = DisintegratorWeapon.Design.name;
        }

        if (QRandom.Float() < RankChance(v.rank, 0, 0.5, 0.9)) {
            v.sentinelName = "Decelerator Guard";
        }

        SetShield(v,
            0.3, IonCurtainShield.Design.name,
            0.2, DispersionFieldShield.Design.name,
            0.1, DeceleratorShield.Design.name);

        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaHorns(Vessel v) {
        v.designName = "Horns";
        if (v.rank == 1) {
            v.energySourceName = "Cryogenic Block";
        } else {
            v.energySourceName = "Graviton Generator";
        }

        AddWeapon(v,
            0.5, PulseLaserWeapon.Design.name,
            0.5, IonCannonWeapon.Design.name);

        v.weapons.Add(PointDefenseLaserWeapon.Design.name);

        SetSpecialWeapon(v,
            0.6, MortarWeapon.Design.name,
            0.4, TorpedoLauncherWeapon.Design.name);

        if (v.rank == 3) {
            var sentinelRoll = QRandom.Float();
            if (sentinelRoll < 0.7) {
                v.sentinelName = "Ion Fighter";
            } else {
                v.sentinelName = "Ion Curtain Guard";
            }
        }

        SetShield(v,
            0.5, DispersionFieldShield.Design.name,
            0.3, DeceleratorShield.Design.name,
            0.2, IonCurtainShield.Design.name);

        v.modList.Add("Asteroid Danger");
    }

    private static void InitKrigiaAshes(Vessel v) {
        v.designName = "Ashes";
        v.energySourceName = "Singularial Reactor";

        float weaponRoll = QRandom.Float();

        v.weapons.Add(LancerWeapon.Design.name);
        v.weapons.Add(HurricaneWeapon.Design.name);
        v.specialWeaponName = MortarWeapon.Design.name;

        v.artifacts.Add(DroidArtifact.Design.name);
        v.artifacts.Add(AsynchronousReloaderArtifact.Design.name);
        v.artifacts.Add(LaserAbsorberArtifact.Design.name);
        v.artifacts.Add(MissileTargeterArtifact.Design.name);
        v.artifacts.Add(MagneticNegatorArtifact.Design.name);

        v.sentinelName = "Point-Defense Guard";

        v.shieldName = DiffuserShield.Design.name;
    }

    private static void InitWertuProbe(Vessel v) {
        v.designName = "Probe";
        v.energySourceName = "Power Generator";

        AddWeapon(v,
            0.5, PhotonBurstCannonWeapon.Design.name,
            0.3, ZapWeapon.Design.name,
            0.2, PointDefenseLaserWeapon.Design.name);
    }

    private static void InitWertuTransporter(Vessel v) {
        v.designName = "Transporter";
        v.energySourceName = "Power Generator";

        v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);

        SetShield(v,
            0.4, LatticeShield.Design.name,
            0.4, LaserPerimeterShield.Design.name);
    }

    private static void InitWertuGuardian(Vessel v) {
        v.designName = "Guardian";
        v.energySourceName = "Vortex Battery";

        if (v.rank > 1) {
            AddWeapon(v,
                0.5, HeavyPhotonBurstCannonWeapon.Design.name,
                0.5, TwinPhotonBurstCannonWeapon.Design.name);
        } else {
            AddWeapon(v,
                0.7, PhotonBurstCannonWeapon.Design.name,
                0.3, TwinPhotonBurstCannonWeapon.Design.name);
        }

        AddWeapon(v,
            0.4, PointDefenseLaserWeapon.Design.name,
            0.3, CutterWeapon.Design.name,
            0.3, ShieldBreakerWeapon.Design.name);

        v.specialWeaponName = PhotonBeamWeapon.Design.name;

        SetShield(v,
            0.2, LatticeShield.Design.name,
            0.2, LaserPerimeterShield.Design.name,
            0.2, HeatScreenShield.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0, 0.2, 0.5)) {
            v.artifacts.Add(PhotoniumArtifact.Design.name);
        }
    }

    private static void InitWertuAngel(Vessel v) {
        v.designName = "Angel";
        v.energySourceName = "Cryogenic Block";

        AddWeapon(v,
            0.5, PointDefenseLaserWeapon.Design.name,
            0.3, TwinPhotonBurstCannonWeapon.Design.name,
            0.2, HeavyPhotonBurstCannonWeapon.Design.name);

        AddWeapon(v,
            0.7, CutterWeapon.Design.name,
            0.3, ShieldBreakerWeapon.Design.name);

        v.specialWeaponName = RestructuringRayWeapon.Design.name;

        if (v.rank == 3) {
            v.sentinelName = "Photon Fighter";
        }

        SetShield(v, 0.4, HeatScreenShield.Design.name);

        if (QRandom.Float() < RankChance(v.rank, 0.2, 0.5, 0.8)) {
            v.artifacts.Add(PhotoniumArtifact.Design.name);
        }
    }

    private static void InitWertuDominator(Vessel v) {
        v.designName = "Dominator";
        v.energySourceName = "Singularial Reactor";

        AddWeapon(v,
            0.7, ShieldBreakerWeapon.Design.name,
            0.3, HeavyPhotonBurstCannonWeapon.Design.name);

        v.weapons.Add(PlasmaEmitterWeapon.Design.name);

        SetShield(v,
            0.5, LatticeShield.Design.name,
            0.5, LaserPerimeterShield.Design.name);

        if (v.rank == 2) {
            v.sentinelName = "Photon Fighter";
        } else if (v.rank == 3) {
            v.sentinelName = "Restructuring Guard";
        }

        if (QRandom.Float() < RankChance(v.rank, 0.7, 0.9, 1)) {
            v.artifacts.Add(PhotoniumArtifact.Design.name);
        }
    }

    private static void InitZythHunter(Vessel v) {
        v.designName = "Hunter";
        
        v.energySourceName = "Advanced Power Generator";
        if (v.rank >= 2) {
            v.energySourceName = "Vortex Battery";
        }

        SetSpecialWeapon(v,
            0.5, HarpoonWeapon.Design.name,
            0.5, DisruptorWeapon.Design.name);

        if (v.specialWeaponName != HarpoonWeapon.Design.name) {
            AddWeapon(v,
                0.4, HellfireWeapon.Design.name,
                0.3, CutterWeapon.Design.name,
                0.3, FlareWeapon.Design.name);
        } else {
            AddWeapon(v,
                0.6, HellfireWeapon.Design.name,
                0.4, FlareWeapon.Design.name);
        }

        if (QRandom.Float() < RankChance(v.rank, 0.5, 0.7, 0.9)) {
            SetShield(v,
                0.6, IonCurtainShield.Design.name,
                0.4, HeatScreenShield.Design.name);
        }
    }

    private static void InitZythInvader(Vessel v) {
        v.designName = "Invader";
        v.energySourceName = "Graviton Generator";

        AddWeapon(v,
            0.6, DiskThrowerWeapon.Design.name,
            0.4, AssaultLaserWeapon.Design.name);

        AddWeapon(v,
            0.6, PulseLaserWeapon.Design.name,
            0.4, FlareWeapon.Design.name);

        SetSpecialWeapon(v, 0.5, DisruptorWeapon.Design.name);

        SetShield(v,
            0.65, DeceleratorShield.Design.name,
            0.35, DispersionFieldShield.Design.name);
    }

    private static void InitPhaaSpacehopper(Vessel v) {
        InitPhaaMantis(v);
        v.designName = "Spacehopper";
    }

    private static void InitPhaaMantis(Vessel v) {
        v.designName = "Mantis";
        v.energySourceName = "Advanced Power Generator";

        float setRoll = QRandom.Float();

        if (setRoll < 0.35) {
            v.weapons.Add(SpreadLaserWeapon.Design.name);
            v.weapons.Add(BubbleGunWeapon.Design.name);
        } else if (setRoll < 0.45) {
            v.energySourceName = "Vortex Battery";
            v.weapons.Add(BubbleGunWeapon.Design.name);
            v.weapons.Add(BubbleGunWeapon.Design.name);
        } else if (setRoll < 0.55) {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        } else {
            AddWeapon(v,
                0.7, BubbleGunWeapon.Design.name,
                0.3, SpreadLaserWeapon.Design.name);
            AddWeapon(v,
                0.7, RocketLauncherWeapon.Design.name,
                0.3, HurricaneWeapon.Design.name);
        }

        if (QRandom.Float() < RankChance(v.rank, 0.2, 0.4, 0.8)) {
            v.shieldName = AegisShield.Design.name;
        } else {
            v.shieldName = DiffuserShield.Design.name;
        }
    }

    private static void InitRarilouLeviathan(Vessel v) {
        v.designName = "Leviathan";
        v.energySourceName = "Vortex Battery";

        AddWeapon(v,
            0.5, TwinPhotonBurstCannonWeapon.Design.name,
            0.5, PhotonBurstCannonWeapon.Design.name);

        if (QRandom.Float() < 0.6) {
            v.specialWeaponName = MjolnirWeapon.Design.name;
        }

        SetShield(v,
            0.2, IonCurtainShield.Design.name,
            0.2, HeatScreenShield.Design.name);
    }

    private static void InitVespionLarva(Vessel v) {
        v.designName = "Larva";
        v.energySourceName = "Power Generator";

        SetSpecialWeapon(v,
            0.4, ShockwaveCasterWeapon.Design.name,
            0.4, SwarmSpawnerWeapon.Design.name,
            0.2, HyperCutterWeapon.Design.name);

        var sentinelRoll = QRandom.Float();
        if (sentinelRoll < 0.4) {
            v.sentinelName = "Point-Defense Guard";
        }

        SetShield(v, 0.6, HeatScreenShield.Design.name);
    }

    private static void InitVespionHornet(Vessel v) {
        v.designName = "Hornet";
        v.energySourceName = "Advanced Power Generator";

        var weaponSetRoll = QRandom.Float();
        if (weaponSetRoll < 0.4) {
            v.weapons.Add(CutterWeapon.Design.name);
            v.specialWeaponName = ShockwaveCasterWeapon.Design.name;
        } else if (weaponSetRoll < 0.7) {
            v.weapons.Add(SpreadLaserWeapon.Design.name);
            v.specialWeaponName = ShockwaveCasterWeapon.Design.name;
        } else {
            if (QRandom.Bool()) {
                v.weapons.Add(SpreadLaserWeapon.Design.name);
            } else {
                v.weapons.Add(IonCannonWeapon.Design.name);
            }
            v.specialWeaponName = HyperCutterWeapon.Design.name;
        }

        SetShield(v,
            0.4, HeatScreenShield.Design.name,
            0.4, LaserPerimeterShield.Design.name);
    }

    private static void InitVespionQueen(Vessel v) {
        v.designName = "Queen";
        if (v.rank == 1) {
            v.energySourceName = "Vortex Battery";
        } else {
            v.energySourceName = "Cryogenic Block";
        }

        var weaponSetRoll = QRandom.Float();
        if (weaponSetRoll < 0.4) {
            v.weapons.Add(CutterWeapon.Design.name);
            v.specialWeaponName = ShockwaveCasterWeapon.Design.name;
        } else if (weaponSetRoll < 0.7) {
            v.weapons.Add(SpreadLaserWeapon.Design.name);
            v.specialWeaponName = ShockwaveCasterWeapon.Design.name;
        } else {
            if (QRandom.Bool()) {
                v.weapons.Add(SpreadLaserWeapon.Design.name);
            } else {
                v.weapons.Add(IonCannonWeapon.Design.name);
            }
            v.specialWeaponName = HyperCutterWeapon.Design.name;
        }

        SetShield(v,
            0.4, HeatScreenShield.Design.name,
            0.4, LaserPerimeterShield.Design.name);
    }

    private static void InitUniqueSpectre(Vessel v) {
        v.designName = "Spectre";
        v.energySourceName = "Cryogenic Block";

        v.weapons.Add(HurricaneWeapon.Design.name);
        v.weapons.Add(StormbringerWeapon.Design.name);
        v.specialWeaponName = TorpedoLauncherWeapon.Design.name;

        v.shieldName = PhaserShield.Design.name;

        v.artifacts.Add(ShieldProlongerArtifact.Design.name);
        v.artifacts.Add(ShivaRechargerArtifact.Design.name);
        v.artifacts.Add(MagneticNegatorArtifact.Design.name);
    }

    private static void InitUniqueVisitor(Vessel v) {
        v.designName = "Visitor";
        v.energySourceName = "Graviton Generator";

        v.weapons.Add(CrystalCannonWeapon.Design.name);
        v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        v.specialWeaponName = WarpDeviceWeapon.Design.name;

        v.shieldName = LaserPerimeterShield.Design.name;

        v.artifacts.Add(ShieldProlongerArtifact.Design.name);
        v.artifacts.Add(ShivaRechargerArtifact.Design.name);
        v.artifacts.Add(MagneticNegatorArtifact.Design.name);
    }

    private static void InitLezeonaPin(Vessel v) {
        v.designName = "Pin";
        v.energySourceName = "Power Generator";

        v.sentinelName = "Ion Fighter";

        if (QRandom.Float() < RankChance(v.rank, 0.4, 0.7, 1)) {
            v.shieldName = "Deflector";
        }

        v.artifacts.Add(IonCannonSaturatorArtifact.Design.name);
    }

    private static void InitLezeonaCore(Vessel v) {
        v.designName = "Core";
        v.energySourceName = "Vortex Battery";

        if (QRandom.Bool()) {
            v.weapons.Add(IonCannonWeapon.Design.name);
            v.artifacts.Add(IonCannonSaturatorArtifact.Design.name);
        } else {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
            v.artifacts.Add(PointDefenseSaturatorArtifact.Design.name);
        }

        if (QRandom.Float() < RankChance(v.rank, 0.3, 0.7, 1)) {
            v.specialWeaponName = PulseBladeWeapon.Design.name;
        }

        if (QRandom.Float() < RankChance(v.rank, 0.5, 0.9, 1)) {
            v.shieldName = "Deflector";
        }
    }
}