using Godot;
using System;
using System.Collections.Generic;

// TODO: rename this class.
public static class VesselFactory {
    public static void InitStats(Vessel v) {
        v.hp = v.design.maxHp;
        v.energy = v.energySource.maxBackupEnergy;
    }

    public static void Init(Vessel v, VesselDesign design) {
        Init(v, design.affiliation + " " + design.name);
    }

    public static void Init(Vessel v, string kind) {
        if (kind == "Neutral Pirate") {
            InitNeutralPirate(v);
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
            v.weapons.Add(EmptyWeapon.Design);
        }
        while (v.artifacts.Count < 5) {
            v.artifacts.Add(EmptyArtifact.Design);
        }

        InitStats(v);
    }

    private static void InitNeutralPirate(Vessel v) {
        v.design = VesselDesign.Find("Neutral", "Pirate");
        v.energySource = EnergySource.Find("Power Generator");

        // 60% - assault laser
        // 40% - stinger
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.6) {
            v.weapons.Add(AssaultLaserWeapon.Design);
        } else {
            v.weapons.Add(StingerWeapon.Design);
        }

        // 70% - spread gun
        // 30% - rocket launcher
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll < 0.7) {
            v.weapons.Add(SpreadGunWeapon.Design);
        } else {
            v.weapons.Add(RocketLauncherWeapon.Design);
        }

        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.25) {
            v.shield = DispersionFieldShield.Design;
        } else if (shieldRoll < 0.5) {
            v.shield = ReflectorShield.Design;
        } else if (shieldRoll < 0.75) {
            v.shield = LaserPerimeterShield.Design;
        }
    }

    private static void InitScavengerRaider(Vessel v) {
        v.design = VesselDesign.Find("Scavenger", "Raider");
        v.energySource = EnergySource.Find("Power Generator");

        // 40% - needle gun
        // 30% - zap
        // 20% - point-defense laser
        // 10% - spread gun
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.4) {
            v.weapons.Add(NeedleGunWeapon.Design);
        } else if (weaponRoll < 0.7) {
            v.weapons.Add(ZapWeapon.Design);
        } else if (weaponRoll < 0.9) {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        } else {
            v.weapons.Add(SpreadGunWeapon.Design);
        }

        // 30% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.3) {
            v.shield = IonCurtainShield.Design;
        }
    }

    private static void InitScavengerMarauder(Vessel v) {
        v.design = VesselDesign.Find("Scavenger", "Marauder");
        v.energySource = EnergySource.Find("Advanced Power Generator");

        // 30% - rocket launcher
        // 30% - point-defense laser
        // 15% - stinger
        // 15% - pulse laser
        // 10% - assault laser
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.3) {
            v.weapons.Add(RocketLauncherWeapon.Design);
        } else if (weaponRoll < 0.6) {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        } else if (weaponRoll < 0.75) {
            v.weapons.Add(StingerWeapon.Design);
        } else if (weaponRoll < 0.90) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else {
            v.weapons.Add(AssaultLaserWeapon.Design);
        }

        v.specialWeapon = DisruptorWeapon.Design;

        // 50% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.5) {
            v.shield = IonCurtainShield.Design;
        }
    }

    private static void InitEarthlingScout(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Scout");
        v.energySource = EnergySource.Find("Power Generator");

        // 60% - spread gun
        // 40% - needle gun
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.6) {
            v.weapons.Add(SpreadGunWeapon.Design);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design);
        }
    }

    private static void InitEarthlingExplorer(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Explorer");
        v.energySource = EnergySource.Find("Power Generator");

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - needle gun
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design);
        }
    }

    private static void InitEarthlingFreighter(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Freighter");
        v.energySource = EnergySource.Find("Power Generator");

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - needle gun
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design);
        }
    }

    private static void InitEarthlingFighter(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Fighter");
        v.energySource = EnergySource.Find("Advanced Power Generator");

        float weaponRoll = QRandom.Float();
        // 50% - ion cannon
        // 50% - zap
        if (weaponRoll < 0.5) {
            v.weapons.Add(IonCannonWeapon.Design);
        } else {
            v.weapons.Add(ZapWeapon.Design);
        }
        // 70% - pulse laser
        // 30% - needle gun
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.7) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else {
            v.weapons.Add(NeedleGunWeapon.Design);
        }

        // 35% - heat shield
        // 25% - ion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.35) {
            v.shield = HeatScreenShield.Design;
        } else if (shieldRoll < 0.6) {
            v.shield = IonCurtainShield.Design;
        }
    }

    private static void InitEarthlingInterceptor(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Interceptor");
        v.energySource = EnergySource.Find("Vortex Battery");

        float weaponRoll = QRandom.Float();
        // 50% - stinger
        // 30% - pulse laser
        // 20% - ion cannon
        if (weaponRoll < 0.5) {
            v.weapons.Add(StingerWeapon.Design);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else {
            v.weapons.Add(IonCannonWeapon.Design);
        }
        // 80% - rocket launcher
        // 20% - zap
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.8) {
            v.weapons.Add(RocketLauncherWeapon.Design);
        } else {
            v.weapons.Add(ZapWeapon.Design);
        }

        // 20% - ion shield
        // 30% - heat shield
        // 20% - dispersion shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.5) {
            v.shield = HeatScreenShield.Design;
        } else if (shieldRoll < 0.7) {
            v.shield = DispersionFieldShield.Design;
        }
    }

    private static void InitEarthlingArk(Vessel v) {
        v.design = VesselDesign.Find("Earthling", "Ark");
        v.energySource = EnergySource.Find("Power Generator");

        v.specialWeapon = ReaperCannonWeapon.Design;
    }

    private static void InitKrigiaTalons(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Talons");
        v.energySource = EnergySource.Find("Power Generator");

        // 30% - scythe
        // 30% - ion cannon
        // 25% - pulse laser
        // 15% - stinger
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.3) {
            v.weapons.Add(ScytheWeapon.Design);
        } else if (weaponRoll < 0.6) {
            v.weapons.Add(IonCannonWeapon.Design);
        } else if (weaponRoll < 0.85) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else {
            v.weapons.Add(StingerWeapon.Design);
        }
    }

    private static void InitKrigiaClaws(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Claws");
        v.energySource = EnergySource.Find("Advanced Power Generator");

        float weaponRoll = QRandom.Float();
        // 50% - pulse laser
        // 30% - ion cannon
        // 20% - point-defense laser
        if (weaponRoll < 0.5) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(IonCannonWeapon.Design);
        } else {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        }
        // 50% - rocket launcher
        // 40% - scythe
        // 10% - none
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.5) {
            v.weapons.Add(RocketLauncherWeapon.Design);
        } else if (weaponRoll2 < 0.9) {
            v.weapons.Add(ScytheWeapon.Design);
        } else {
            // No second weapon.   
        }

        if (QRandom.Float() < 0.3) {
            v.shield = IonCurtainShield.Design;
        }
    }

    private static void InitKrigiaFangs(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Fangs");
        v.energySource = EnergySource.Find("Vortex Battery");

        float weaponRoll = QRandom.Float();
        // 60% - pulse laser
        // 20% - assault laser
        // 20% - stinger
        if (weaponRoll < 0.6) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(AssaultLaserWeapon.Design);
        } else {
            v.weapons.Add(StingerWeapon.Design);
        }
        // 70% - rocket launcher
        // 30% - scythe
        float weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.7) {
            v.weapons.Add(RocketLauncherWeapon.Design);
        } else {
            v.weapons.Add(ScytheWeapon.Design);
        }

        // 10% - ion shield
        // 20% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.1) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.3) {
            v.shield = DispersionFieldShield.Design;
        } else if (shieldRoll < 0.6) {
            v.shield = ReflectorShield.Design;
        }
    }

    private static void InitKrigiaTusks(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Tusks");

        var roll = QRandom.Float();
        v.weapons.Add(StingerWeapon.Design);
        if (roll < 0.5) {
            v.energySource = EnergySource.Find("Radioisotope Generator");
            float weaponRoll = QRandom.Float();
            if (weaponRoll < 0.5) {
                v.weapons.Add(RocketLauncherWeapon.Design);
            } else {
                v.weapons.Add(HurricaneWeapon.Design);
            }
        } else {
            v.energySource = EnergySource.Find("Cryogenic Block");
            float weaponRoll = QRandom.Float();
            if (weaponRoll < 0.5) {
                v.weapons.Add(GreatScytheWeapon.Design);
            } else {
                v.weapons.Add(AssaultLaserWeapon.Design);
            }
        }

        var specialRoll = QRandom.Float();
        if (specialRoll < 0.4) {
            v.specialWeapon = TorpedoLauncherWeapon.Design;
        }

        // 30% - ion shield
        // 20% - dispersion shield
        // 10% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.3) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.5) {
            v.shield = DispersionFieldShield.Design;
        } else if (shieldRoll < 0.6) {
            v.shield = ReflectorShield.Design;
        }
    }

    private static void InitKrigiaHorns(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Horns");
        v.energySource = EnergySource.Find("Graviton Generator");

        float weaponRoll = QRandom.Float();
        // 50% - pulse laser
        // 50% - ion cannon
        if (weaponRoll < 0.5) {
            v.weapons.Add(PulseLaserWeapon.Design);
        } else {
            v.weapons.Add(IonCannonWeapon.Design);
        }
        v.weapons.Add(PointDefenseLaserWeapon.Design);

        var specialRoll = QRandom.Float();
        if (specialRoll < 0.6) {
            v.specialWeapon = MortarWeapon.Design;
        } else {
            v.specialWeapon = TorpedoLauncherWeapon.Design;
        }

        // 20% - ion shield
        // 50% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.7) {
            v.shield = DispersionFieldShield.Design;
        } else {
            v.shield = ReflectorShield.Design;
        }
    }

    private static void InitKrigiaAshes(Vessel v) {
        v.design = VesselDesign.Find("Krigia", "Ashes");
        v.energySource = EnergySource.Find("Singularial Reactor");

        float weaponRoll = QRandom.Float();

        v.weapons.Add(LancerWeapon.Design);
        v.weapons.Add(HurricaneWeapon.Design);
        v.specialWeapon = MortarWeapon.Design;

        v.artifacts.Add(DroidArtifact.Design);

        // 20% - ion shield
        // 50% - dispersion shield
        // 30% - reflector shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.7) {
            v.shield = DispersionFieldShield.Design;
        } else {
            v.shield = ReflectorShield.Design;
        }
    }

    private static void InitWertuProbe(Vessel v) {
        v.design = VesselDesign.Find("Wertu", "Probe");
        v.energySource = EnergySource.Find("Power Generator");

        // 50% - photon burst cannon
        // 30% - zap
        // 20% - point-laser defense
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.5) {
            v.weapons.Add(PhotonBurstCannonWeapon.Design);
        } else if (weaponRoll < 0.8) {
            v.weapons.Add(ZapWeapon.Design);
        } else {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        }
    }

    private static void InitWertuTransporter(Vessel v) {
        v.design = VesselDesign.Find("Wertu", "Transporter");
        v.energySource = EnergySource.Find("Power Generator");

        v.weapons.Add(TwinPhotonBurstCannonWeapon.Design);

        // 40% - lattice
        // 40% - laser perimeter
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shield = LatticeShield.Design;
        } else if (shieldRoll < 0.8) {
            v.shield = LaserPerimeterShield.Design;
        }
    }

    private static void InitWertuGuardian(Vessel v) {
        v.design = VesselDesign.Find("Wertu", "Guardian");
        v.energySource = EnergySource.Find("Vortex Battery");

        float weaponRoll = QRandom.Float();
        // 70% - photon burst cannon
        // 30% - twin photon burst cannon
        if (weaponRoll < 0.7) {
            v.weapons.Add(PhotonBurstCannonWeapon.Design);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design);
        }
        var weaponRoll2 = QRandom.Float();
        // 40% - point-laser defense
        // 30% - cutter
        // 30% - shield breaker
        if (weaponRoll2 < 0.4) {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        } else if (weaponRoll2 < 0.7) {
            v.weapons.Add(CutterWeapon.Design);
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design);
        }
        v.specialWeapon = PhotonBeamWeapon.Design;

        // 20% - lattice
        // 20% - laser perimeter
        // 20% - heat screen
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.2) {
            v.shield = LatticeShield.Design;
        } else if (shieldRoll < 0.4) {
            v.shield = LaserPerimeterShield.Design;
        } else if (shieldRoll < 0.6) {
            v.shield = HeatScreenShield.Design;
        }
    }

    private static void InitWertuAngel(Vessel v) {
        v.design = VesselDesign.Find("Wertu", "Angel");
        v.energySource = EnergySource.Find("Cryogenic Block");

        float weaponRoll = QRandom.Float();
        // 60% - point-defense laser
        // 40% - twin photon burst cannon
        if (weaponRoll < 0.6) {
            v.weapons.Add(PointDefenseLaserWeapon.Design);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design);
        }

        // 70% - cutter
        // 30% - shield breaker
        var weaponRoll2 = QRandom.Float();
        if (weaponRoll < 0.7) {
            v.weapons.Add(CutterWeapon.Design);
        } else {
            v.weapons.Add(ShieldBreakerWeapon.Design);
        }
        v.specialWeapon = RestructuringRayWeapon.Design;

        // 40% - heat screen
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shield = HeatScreenShield.Design;
        }
    }

    private static void InitWertuDominator(Vessel v) {
        v.design = VesselDesign.Find("Wertu", "Dominator");
        v.energySource = EnergySource.Find("Singularial Reactor");

        float weaponRoll = QRandom.Float();
        // 70% - shield breaker
        // 30% - twin photon burst cannon
        if (weaponRoll < 0.7) {
            v.weapons.Add(ShieldBreakerWeapon.Design);
        } else {
            v.weapons.Add(TwinPhotonBurstCannonWeapon.Design);
        }
        v.weapons.Add(PlasmaEmitterWeapon.Design);

        // 50% - lattice
        // 50% - laser perimeter
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.5) {
            v.shield = LatticeShield.Design;
        } else {
            v.shield = LaserPerimeterShield.Design;
        }
    }

    private static void InitZythHunter(Vessel v) {
        v.design = VesselDesign.Find("Zyth", "Hunter");
        v.energySource = EnergySource.Find("Advanced Power Generator");

        var weaponRoll2 = QRandom.Float();
        if (weaponRoll2 < 0.5) {
            v.specialWeapon = HarpoonWeapon.Design;
        } else {
            v.specialWeapon = DisruptorWeapon.Design;
        }

        // 40% - hellfire
        // 30% - cutter
        // 20% - assault laser
        // 10% - disk thrower
        float weaponRoll = QRandom.Float();
        if (weaponRoll < 0.4) {
            v.weapons.Add(HellfireWeapon.Design);
        } else if (weaponRoll < 0.7) {
            v.weapons.Add(CutterWeapon.Design);
        } else if (weaponRoll < 0.9) {
            v.weapons.Add(AssaultLaserWeapon.Design);
        } else {
            v.weapons.Add(DiskThrowerWeapon.Design);
        }

        // 40% - ion shield
        // 20% - heat shield
        var shieldRoll = QRandom.Float();
        if (shieldRoll < 0.4) {
            v.shield = IonCurtainShield.Design;
        } else if (shieldRoll < 0.6) {
            v.shield = HeatScreenShield.Design;
        }
    }

    private static void InitUniqueSpectre(Vessel v) {
        v.design = VesselDesign.Find("Unique", "Spectre");
        v.energySource = EnergySource.Find("Cryogenic Block");

        v.weapons.Add(HurricaneWeapon.Design);
        v.weapons.Add(StormbringerWeapon.Design);
        v.specialWeapon = TorpedoLauncherWeapon.Design;

        v.shield = PhaserShield.Design;
    }
}