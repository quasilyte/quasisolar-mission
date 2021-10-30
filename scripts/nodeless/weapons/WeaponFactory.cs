using Godot;
using System;

public static class WeaponFactory {
    public static IWeapon New(WeaponDesign w, Pilot p) {
        if (w == EmptyWeapon.Design) {
            return new EmptyWeapon();
        }
        if (w == RestructuringRayWeapon.Design) {
            return new RestructuringRayWeapon(p);
        }
        if (w == SpreadGunWeapon.Design) {
            return new SpreadGunWeapon(p);
        }
        if (w == SpreadLaserWeapon.Design) {
            return new SpreadLaserWeapon(p);
        }
        if (w == NeedleGunWeapon.Design) {
            return new NeedleGunWeapon(p);
        }
        if (w == AssaultLaserWeapon.Design) {
            return new AssaultLaserWeapon(p);
        }
        if (w == ZapWeapon.Design) {
            return new ZapWeapon(p);
        }
        if (w == ReaperCannonWeapon.Design) {
            return new ReaperCannonWeapon(p);
        }
        if (w == ScytheWeapon.Design) {
            return new ScytheWeapon(p);
        }
        if (w == GreatScytheWeapon.Design) {
            return new GreatScytheWeapon(p);
        }
        if (w == PhotonBurstCannonWeapon.Design) {
            return new PhotonBurstCannonWeapon(p);
        }
        if (w == TwinPhotonBurstCannonWeapon.Design) {
            return new TwinPhotonBurstCannonWeapon(p);
        }
        if (w == HeavyPhotonBurstCannonWeapon.Design) {
            return new HeavyPhotonBurstCannonWeapon(p);
        }
        if (w == PhotonBeamWeapon.Design) {
            return new PhotonBeamWeapon(p);
        }
        if (w == PlasmaEmitterWeapon.Design) {
            return new PlasmaEmitterWeapon(p);
        }
        if (w == IonCannonWeapon.Design) {
            return new IonCannonWeapon(p);
        }
        if (w == PulseLaserWeapon.Design) {
            return new PulseLaserWeapon(p);
        }
        if (w == RocketLauncherWeapon.Design) {
            return new RocketLauncherWeapon(p);
        }
        if (w == FlareWeapon.Design) {
            return new FlareWeapon(p);
        }
        if (w == HurricaneWeapon.Design) {
            return new HurricaneWeapon(p);
        }
        if (w == ShieldBreakerWeapon.Design) {
            return new ShieldBreakerWeapon(p);
        }
        if (w == PointDefenseLaserWeapon.Design) {
            return new PointDefenseLaserWeapon(p);
        }
        if (w == StingerWeapon.Design) {
            return new StingerWeapon(p);
        }
        if (w == DiskThrowerWeapon.Design) {
            return new DiskThrowerWeapon(p);
        }
        if (w == HellfireWeapon.Design) {
            return new HellfireWeapon(p);
        }
        if (w == HarpoonWeapon.Design) {
            return new HarpoonWeapon(p);
        }
        if (w == CutterWeapon.Design) {
            return new CutterWeapon(p);
        }
        if (w == CrystalCannonWeapon.Design) {
            return new CrystalCannonWeapon(p);
        }
        if (w == MortarWeapon.Design) {
            return new MortarWeapon(p);
        }
        if (w == MjolnirWeapon.Design) {
            return new MjolnirWeapon(p);
        }
        if (w == StormbringerWeapon.Design) {
            return new StormbringerWeapon(p);
        }
        if (w == LancerWeapon.Design) {
            return new LancerWeapon(p);
        }
        if (w == BubbleGunWeapon.Design) {
            return new BubbleGunWeapon(p);
        }

        if (w == DisintegratorWeapon.Design) {
            return new DisintegratorWeapon(p);
        }
        if (w == WarpDeviceWeapon.Design) {
            return new WarpDeviceWeapon(p);
        }
        if (w == TorpedoLauncherWeapon.Design) {
            return new TorpedoLauncherWeapon(p);
        }
        if (w == DisruptorWeapon.Design) {
            return new DisruptorWeapon(p);
        }
        if (w == ShockwaveCasterWeapon.Design) {
            return new ShockwaveCasterWeapon(p);
        }
        if (w == SwarmSpawnerWeapon.Design) {
            return new SwarmSpawnerWeapon(p);
        }
        if (w == PulseBladeWeapon.Design) {
            return new PulseBladeWeapon(p);
        }
        if (w == HyperCutterWeapon.Design) {
            return new HyperCutterWeapon(p);
        }
        if (w == AfterburnerWeapon.Design) {
            return new AfterburnerWeapon(p);
        }
        if (w == TempestWeapon.Design) {
            return new TempestWeapon(p);
        }

        if (w == null) {
            throw new Exception("null WeaponDesign argument");    
        }

        throw new Exception("invalid WeaponDesign argument");
    }
}
