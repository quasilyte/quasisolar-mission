using Godot;
using System;

public static class ArtifactFactory {
    public static IArtifact New(ArtifactDesign a) {
        if (a == EmptyArtifact.Design) {
            return new EmptyArtifact();
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
        if (a == DivioryThrusterArtifact.Design) {
            return new DivioryThrusterArtifact();
        }
        if (a == ImpulseDevourerArtifact.Design) {
            return new ImpulseDevourerArtifact();
        }
        if (a == ShivaRechargerArtifact.Design) {
            return new ShivaRechargerArtifact();
        }
        if (a == ShieldProlongerArtifact.Design) {
            return new ShieldProlongerArtifact();
        }

        throw new Exception("invalid ArtifactDesign argument");
    }
}
