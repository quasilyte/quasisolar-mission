using Godot;
using System;

public static class ArtifactFactory {
    public static IArtifact New(ArtifactDesign a) {
        if (a == EmptyArtifact.Design) {
            return new EmptyArtifact();
        }

        if (a == LaserAbsorberArtifact.Design) {
            return new LaserAbsorberArtifact();
        }
        if (a == AsynchronousReloaderArtifact.Design) {
            return new AsynchronousReloaderArtifact();
        }
        if (a == EngineBoosterArtifact.Design) {
            return new EngineBoosterArtifact();
        }
        if (a == MagneticNegatorArtifact.Design) {
            return new MagneticNegatorArtifact();
        }
        if (a == DroidArtifact.Design) {
            return new DroidArtifact();
        }
        if (a == EnergyConverterArtifact.Design) {
            return new EnergyConverterArtifact();
        }
        if (a == MissileTargeterArtifact.Design) {
            return new MissileTargeterArtifact();
        }
        if (a == MissileCoordinatorArtifact.Design) {
            return new MissileCoordinatorArtifact();
        }
        if (a == DivioryThrusterArtifact.Design) {
            return new DivioryThrusterArtifact();
        }
        if (a == ImpulseDevourerArtifact.Design) {
            return new ImpulseDevourerArtifact();
        }
        if (a == PointDefenseSaturatorArtifact.Design) {
            return new PointDefenseSaturatorArtifact();
        }
        if (a == ShivaRechargerArtifact.Design) {
            return new ShivaRechargerArtifact();
        }
        if (a == ShieldProlongerArtifact.Design) {
            return new ShieldProlongerArtifact();
        }
        if (a == SentinelControllerArtifact.Design) {
            return new SentinelControllerArtifact();
        }
        if (a == SentinelLinkArtifact.Design) {
            return new SentinelLinkArtifact();
        }
        if (a == KineticAcceleratorArtifact.Design) {
            return new KineticAcceleratorArtifact();
        }

        throw new Exception("invalid ArtifactDesign argument");
    }
}
