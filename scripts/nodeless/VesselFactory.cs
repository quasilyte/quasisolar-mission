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
    }

    public static void InitStats(Vessel v) {
        v.hp = v.Design().maxHp;
        v.energy = v.GetEnergySource().maxBackupEnergy;
    }

    public static void Init(Vessel v, VesselDesign design) {
        Init(v, design.affiliation + " " + design.name);
    }

    public static void Init(Vessel v, string kind) {
        if (kind == "Neutral Pirate") {
            InitNeutralPirate(v);
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
        } else if (kind == "Unique Spectre") {
            InitUniqueSpectre(v);
        } else if (kind == "Unique Visitor") {
            InitUniqueVisitor(v);
        } else if (kind == "Scavenger Raider") {
            InitScavengerRaider(v);
        } else if (kind == "Scavenger Marauder") {
            InitScavengerMarauder(v);
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

    private static void InitNeutralPirate(Vessel v) {
        v.designName = "Pirate";
        v.energySourceName = "Power Generator";

        // 60% - assault laser
        // 40% - stinger
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.6) {
            v.weapons.Add(AssaultLaserWeapon.Design.name);
        } else {
            v.weapons.Add(StingerWeapon.Design.name);
        }

        // 70% - spread gun
        // 30% - rocket launcher
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll < 0.7) {
            v.weapons.Add(SpreadGunWeapon.Design.name);
        } else {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        }

        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.25) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else if (shieldRoll < 0.5) {
            v.shieldName = ReflectorShield.Design.name;
        } else if (shieldRoll < 0.75) {
            v.shieldName = LaserPerimeterShield.Design.name;
        }
    }

    private static void InitNeutralNomad(Vessel v) {
        v.designName = "Nomad";
        v.energySourceName = "Vortex Battery";

        // 60% - photon burst
        // 40% - zap
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.6) {
            v.weapons.Add(PhotonBurstCannonWeapon.Design.name);
        } else {
            v.weapons.Add(ZapWeapon.Design.name);
        }

        v.weapons.Add(CutterWeapon.Design.name);

        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.25) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else if (shieldRoll < 0.5) {
            v.shieldName = ReflectorShield.Design.name;
        } else {
            v.shieldName = LatticeShield.Design.name;
        }
    }

    private static void InitNeutralAvenger(Vessel v, bool weak) {
        v.designName = "Avenger";
        v.energySourceName = weak ? "Power Generator" : "Vortex Battery";

        v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);
        if (weak) {
            v.shieldName = HeatScreenShield.Design.name;
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design.name);
            v.shieldName = ReflectorShield.Design.name;
        }
    }

    private static void InitScavengerRaider(Vessel v) {
        v.designName = "Raider";
        v.energySourceName = "Power Generator";

        // 40% - needle gun
        // 30% - zap
        // 20% - point-defense laser
        // 10% - spread gun
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.4) {
            v.weapons.Add(NeedleGunWeapon.Design.name);
        } else if (weaponRoll < 0.7) {
            v.weapons.Add(ZapWeapon.Design.name);
        } else if (weaponRoll < 0.9) {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(SpreadGunWeapon.Design.name);
        }

        // 30% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.3) {
            v.shieldName = IonCurtainShield.Design.name;
        }
    }

    private static void InitScavengerMarauder(Vessel v) {
        v.designName = "Marauder";
        v.energySourceName = "Advanced Power Generator";

        // 30% - rocket launcher
        // 30% - point-defense laser
        // 15% - stinger
        // 15% - pulse laser
        // 10% - assault laser
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.3) {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        } else if (weaponRoll < 0.6) {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        } else if (weaponRoll < 0.75) {
            v.weapons.Add(StingerWeapon.Design.name);
        } else if (weaponRoll < 0.90) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(AssaultLaserWeapon.Design.name);
        }

        v.specialWeaponName = DisruptorWeapon.Design.name;

        // 50% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.5) {
            v.shieldName = IonCurtainShield.Design.name;
        }
    }

    private static void InitEarthlingScout(Vessel v) {
        v.designName = "Scout";
        v.energySourceName = "Power Generator";

        // 60% - spread gun
        // 40% - needle gun
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.6) {
            v.weapons.Add(SpreadGunWeapon.Design.name);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design.name);
        }
    }

    private static void InitEarthlingExplorer(Vessel v) {
        v.designName = "Explorer";
        v.energySourceName = "Power Generator";

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - needle gun
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design.name);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design.name);
        }
    }

    private static void InitEarthlingFreighter(Vessel v) {
        v.designName = "Freighter";
        v.energySourceName = "Power Generator";

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - needle gun
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design.name);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design.name);
        }
    }

    private static void InitEarthlingFighter(Vessel v) {
        v.designName = "Fighter";
        v.energySourceName = "Advanced Power Generator";

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - zap
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design.name);
        } else {
            v.weapons.Add(ZapWeapon.Design.name);
        }
        // 70% - pulse laser
        // 30% - needle gun
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.7) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design.name);
        }

        // 35% - heat shield
        // 25% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.35) {
            v.shieldName = HeatScreenShield.Design.name;
        } else if (shieldRoll < 0.6) {
            v.shieldName = IonCurtainShield.Design.name;
        }
    }

    private static void InitEarthlingInterceptor(Vessel v) {
        v.designName = "Interceptor";
        v.energySourceName = "Vortex Battery";

        float weaponRoll = QRandom.Float();
        // 50% - stinger
        // 30% - pulse laser
        // 20% - ion cannon
        if (weaponRoll < 0.5) {
            v.weapons.Add(StingerWeapon.Design.name);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(IonCannonWeapon.Design.name);
        }
        // 80% - rocket launcher
        // 20% - zap
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.8) {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        } else {
            v.weapons.Add(ZapWeapon.Design.name);
        }

        // 20% - ion shield
        // 30% - heat shield
        // 20% - dispersion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.5) {
            v.shieldName = HeatScreenShield.Design.name;
        } else if (shieldRoll < 0.7) {
            v.shieldName = DispersionFieldShield.Design.name;
        }
    }

    private static void InitEarthlingArk(Vessel v) {
        v.designName = "Ark";
        v.energySourceName = "Power Generator";

        v.specialWeaponName = ReaperCannonWeapon.Design.name;
    }

    private static void InitEarthlingGladiator(Vessel v) {
        v.designName = "Gladiator";
        v.energySourceName = "Vortex Battery";

        float weaponRoll = QRandom.Float();
        // 50% - pulse laser
        // 50% - zap
        if (weaponRoll < 0.5) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(ZapWeapon.Design.name);
        }

        // 60% - reaper cannon
        // 40% - disruptor
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.6) {
            v.weapons.Add(ReaperCannonWeapon.Design.name);
        } else {
            v.weapons.Add(DisruptorWeapon.Design.name);
        }

        // 50% - laser perimeter
        // 30% - dispersion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.5) {
            v.shieldName = LaserPerimeterShield.Design.name;
        } else if (shieldRoll < 0.8) {
            v.shieldName = DispersionFieldShield.Design.name;
        }
    }

    private static void InitKrigiaTalons(Vessel v) {
        v.designName = "Talons";
        v.energySourceName = "Power Generator";

        // 30% - scythe
        // 30% - ion cannon
        // 25% - pulse laser
        // 15% - stinger
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.3) {
            v.weapons.Add(ScytheWeapon.Design.name);
        } else if (weaponRoll < 0.6) {
            v.weapons.Add(IonCannonWeapon.Design.name);
        } else if (weaponRoll < 0.85) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(StingerWeapon.Design.name);
        }
    }

    private static void InitKrigiaClaws(Vessel v) {
        v.designName = "Claws";
        v.energySourceName = "Advanced Power Generator";

        float weaponRoll = QRandom.Float();
        // 50% - pulse laser
        // 30% - ion cannon
        // 20% - point-defense laser
        if (weaponRoll < 0.5) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(IonCannonWeapon.Design.name);
        } else {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        }
        // 50% - rocket launcher
        // 40% - scythe
        // 10% - none
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.5) {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        } else if (weaponRoll2 < 0.9) {
            v.weapons.Add(ScytheWeapon.Design.name);
        } else {
            // No second weapon.   
        }

        if (QRandom.Float() < 0.3) {
            v.shieldName = IonCurtainShield.Design.name;
        }
    }

    private static void InitKrigiaFangs(Vessel v) {
        v.designName = "Fangs";
        v.energySourceName = "Vortex Battery";

        float weaponRoll = QRandom.Float();
        // 60% - pulse laser
        // 20% - assault laser
        // 20% - stinger
        if (weaponRoll < 0.6) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(AssaultLaserWeapon.Design.name);
        } else {
            v.weapons.Add(StingerWeapon.Design.name);
        }
        // 70% - rocket launcher
        // 30% - scythe
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.7) {
            v.weapons.Add(RocketLauncherWeapon.Design.name);
        } else {
            v.weapons.Add(ScytheWeapon.Design.name);
        }

        // 10% - ion shield
        // 20% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.1) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.3) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else if (shieldRoll < 0.6) {
            v.shieldName = ReflectorShield.Design.name;
        }
    }

    private static void InitKrigiaTusks(Vessel v) {
        v.designName = "Tusks";

        var roll = QRandom.Float();
        v.weapons.Add(StingerWeapon.Design.name);
        if (roll < 0.5) {
            v.energySourceName = "Radioisotope Generator";
            float weaponRoll = QRandom.Float();
            if (weaponRoll < 0.5) {
                v.weapons.Add(RocketLauncherWeapon.Design.name);
            } else {
                v.weapons.Add(HurricaneWeapon.Design.name);
            }
        } else {
            v.energySourceName = "Cryogenic Block";
            float weaponRoll = QRandom.Float();
            if (weaponRoll < 0.5) {
                v.weapons.Add(GreatScytheWeapon.Design.name);
            } else {
                v.weapons.Add(AssaultLaserWeapon.Design.name);
            }
        }

        var specialRoll = QRandom.Float();
        if (specialRoll < 0.4) {
            v.specialWeaponName = TorpedoLauncherWeapon.Design.name;
        }

        // 30% - ion shield
        // 20% - dispersion shield
        // 10% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.3) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.5) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else if (shieldRoll < 0.6) {
            v.shieldName = ReflectorShield.Design.name;
        }
    }

    private static void InitKrigiaHorns(Vessel v) {
        v.designName = "Horns";
        v.energySourceName = "Graviton Generator";

        float weaponRoll = QRandom.Float();
        // 50% - pulse laser
        // 50% - ion cannon
        if (weaponRoll < 0.5) {
            v.weapons.Add(PulseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(IonCannonWeapon.Design.name);
        }
        v.weapons.Add(PointDefenseLaserWeapon.Design.name);

        var specialRoll = QRandom.Float();
        if (specialRoll < 0.6) {
            v.specialWeaponName = MortarWeapon.Design.name;
        } else {
            v.specialWeaponName = TorpedoLauncherWeapon.Design.name;
        }

        // 20% - ion shield
        // 50% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.7) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else {
            v.shieldName = ReflectorShield.Design.name;
        }
    }

    private static void InitKrigiaAshes(Vessel v) {
        v.designName = "Ashes";
        v.energySourceName = "Singularial Reactor";

        float weaponRoll = QRandom.Float();

        v.weapons.Add(LancerWeapon.Design.name);
        v.weapons.Add(HurricaneWeapon.Design.name);
        v.specialWeaponName = MortarWeapon.Design.name;

        v.artifacts.Add(DroidArtifact.Design.name);

        // 20% - ion shield
        // 50% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.7) {
            v.shieldName = DispersionFieldShield.Design.name;
        } else {
            v.shieldName = ReflectorShield.Design.name;
        }
    }

    private static void InitWertuProbe(Vessel v) {
        v.designName = "Probe";
        v.energySourceName = "Power Generator";

        // 50% - photon burst cannon
        // 30% - zap
        // 20% - point-laser defense
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.5) {
            v.weapons.Add(PhotonBurstCannonWeapon.Design.name);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(ZapWeapon.Design.name);
        } else {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        }
    }

    private static void InitWertuTransporter(Vessel v) {
        v.designName = "Transporter";
        v.energySourceName = "Power Generator";

        v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);

        // 40% - lattice
        // 40% - laser perimeter
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shieldName = LatticeShield.Design.name;
        } else if (shieldRoll < 0.8) {
            v.shieldName = LaserPerimeterShield.Design.name;
        }
    }

    private static void InitWertuGuardian(Vessel v) {
        v.designName = "Guardian";
        v.energySourceName = "Vortex Battery";

        float weaponRoll = QRandom.Float();
        // 70% - photon burst cannon
        // 30% - twin photon burst cannon
        if (weaponRoll < 0.7) {
            v.weapons.Add(PhotonBurstCannonWeapon.Design.name);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);
        }
        var weaponRoll2 = QRandom.Float();
        // 40% - point-laser defense
        // 30% - cutter
        // 30% - shield breaker
        if (weaponRoll2 < 0.4) {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        } else if (weaponRoll2 < 0.7) {
            v.weapons.Add(CutterWeapon.Design.name);
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design.name);
        }
        v.specialWeaponName = PhotonBeamWeapon.Design.name;

        // 20% - lattice
        // 20% - laser perimeter
        // 20% - heat screen
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shieldName = LatticeShield.Design.name;
        } else if (shieldRoll < 0.4) {
            v.shieldName = LaserPerimeterShield.Design.name;
        } else if (shieldRoll < 0.6) {
            v.shieldName = HeatScreenShield.Design.name;
        }
    }

    private static void InitWertuAngel(Vessel v) {
        v.designName = "Angel";
        v.energySourceName = "Cryogenic Block";

        float weaponRoll = QRandom.Float();
        // 60% - point-defense laser
        // 40% - twin photon burst cannon
        if (weaponRoll < 0.6) {
            v.weapons.Add(PointDefenseLaserWeapon.Design.name);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);
        }

        // 70% - cutter
        // 30% - shield breaker
        var weaponRoll2 = QRandom.Float();
        if (weaponRoll < 0.7) {
            v.weapons.Add(CutterWeapon.Design.name);
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design.name);
        }
        v.specialWeaponName = RestructuringRayWeapon.Design.name;

        // 40% - heat screen
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shieldName = HeatScreenShield.Design.name;
        }
    }

    private static void InitWertuDominator(Vessel v) {
        v.designName = "Dominator";
        v.energySourceName = "Singularial Reactor";

        float weaponRoll = QRandom.Float();
        // 70% - shield breaker
        // 30% - twin photon burst cannon
        if (weaponRoll < 0.7) {
            v.weapons.Add(ShieldBreakerWeapon.Design.name);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design.name);
        }
        v.weapons.Add(PlasmaEmitterWeapon.Design.name);

        // 50% - lattice
        // 50% - laser perimeter
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.5) {
            v.shieldName = LatticeShield.Design.name;
        } else {
            v.shieldName = LaserPerimeterShield.Design.name;
        }
    }

    private static void InitZythHunter(Vessel v) {
        v.designName = "Hunter";
        v.energySourceName = "Advanced Power Generator";

        var weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.5) {
            v.specialWeaponName = HarpoonWeapon.Design.name;
        } else {
            v.specialWeaponName = DisruptorWeapon.Design.name;
        }

        // 40% - hellfire
        // 30% - cutter
        // 20% - assault laser
        // 10% - disk thrower
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.4) {
            v.weapons.Add(HellfireWeapon.Design.name);
        } else if (weaponRoll < 0.7) {
            v.weapons.Add(CutterWeapon.Design.name);
        } else if (weaponRoll < 0.9) {
            v.weapons.Add(AssaultLaserWeapon.Design.name);
        } else {
            v.weapons.Add(DiskThrowerWeapon.Design.name);
        }

        // 40% - ion shield
        // 20% - heat shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shieldName = IonCurtainShield.Design.name;
        } else if (shieldRoll < 0.6) {
            v.shieldName = HeatScreenShield.Design.name;
        }
    }

    private static void InitUniqueSpectre(Vessel v) {
        v.designName = "Spectre";
        v.energySourceName = "Cryogenic Block";

        v.weapons.Add(HurricaneWeapon.Design.name);
        v.weapons.Add(StormbringerWeapon.Design.name);
        v.specialWeaponName = TorpedoLauncherWeapon.Design.name;

        v.shieldName = PhaserShield.Design.name;
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
}