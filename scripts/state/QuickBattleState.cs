public static class QuickBattleState {
    public class PlayerSlot {
        public string kind;
        public int alliance;   
    }

    public class PlayerSettings {
        public VesselDesign vessel;
        public EnergySource energySource;
        public WeaponDesign[] weapons;
        public WeaponDesign specialWeapon;
        public ArtifactDesign[] artifacts;
        public ShieldDesign shield;

        public PlayerSettings() {
            vessel = VesselDesign.Find("Earthling", "Explorer");
            energySource = EnergySource.Find("Power Generator");
            specialWeapon = EmptyWeapon.Design;
            shield = IonCurtainShield.Design;

            weapons = new WeaponDesign[]{
                IonCannonWeapon.Design,
                EmptyWeapon.Design,
            };

            artifacts = new ArtifactDesign[]{
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
            };
        }
    }

    public static PlayerSettings[] playerSettings;

    public static PlayerSlot[] selectedPlayers;
    public static string gameSpeed;

    public static int numAsteroids;

    public static void Reset() {
        playerSettings = new PlayerSettings[2]{
            new PlayerSettings(),
            new PlayerSettings(),
        };

        numAsteroids = 1;

        selectedPlayers = new PlayerSlot[]{
            new PlayerSlot{kind = "Human 1", alliance = 1},
            new PlayerSlot{kind = "Computer Krigia Claws", alliance = 2},
            new PlayerSlot{kind = "None", alliance = 2},
            new PlayerSlot{kind = "None", alliance = 2},
            new PlayerSlot{kind = "None", alliance = 2},
            new PlayerSlot{kind = "None", alliance = 2},
        };

        gameSpeed = "Normal";
    }
}