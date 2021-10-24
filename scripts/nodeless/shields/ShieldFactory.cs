using Godot;
using System;

public static class ShieldFactory {
    public static IShield New(ShieldDesign shield, Pilot p) {
        if (shield == EmptyShield.Design) {
            return new EmptyShield();
        }

        if (shield == IonCurtainShield.Design) {
            return new IonCurtainShield(p);
        }
        if (shield == HeatScreenShield.Design) {
            return new HeatScreenShield(p);
        }
        if (shield == DispersionFieldShield.Design) {
            return new DispersionFieldShield(p);
        }
        if (shield == DeceleratorShield.Design) {
            return new DeceleratorShield(p);
        }
        if (shield == DeflectorShield.Design) {
            return new DeflectorShield(p);
        }
        if (shield == LaserPerimeterShield.Design) {
            return new LaserPerimeterShield(p);
        }
        if (shield == LatticeShield.Design) {
            return new LatticeShield(p);
        }
        if (shield == PhaserShield.Design) {
            return new PhaserShield(p);
        }
        if (shield == DiffuserShield.Design) {
            return new DiffuserShield(p);
        }
        if (shield == AegisShield.Design) {
            return new AegisShield(p);
        }

        throw new Exception("invalid ShieldDesign argument");
    }
}
