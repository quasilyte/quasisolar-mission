using Godot;
using System.Collections.Generic;

public class ArenaSettings {
    public enum BattleSpeed {
        Slow,
        Normal,
        Fast,
        VeryFast,
    }

    public enum EnvDanger {
        None,
        Star,
        PurpleNebula,
        BlueNebula,
    }

    public static bool isQuickBattle;

    public static BattleSpeed speed;

    public static StarColor starColor;
    public static EnvDanger envDanger;

    public static int defensiveTurretAlliance;
    public static int defensiveTurretShots;
    public static WeaponDesign defensiveTurret;

    // How many asteroids will fly around.
    // 0 is valid option here.
    public static int numAsteroids;

    public static List<Vessel> combatants;
    public static Vessel flagship;

    // Used only for the campaign mode.
    public static Dictionary<Vessel, int> alliances;

    public static void Reset() {
        isQuickBattle = false;

        defensiveTurretAlliance = 0;
        defensiveTurretShots = 0;
        defensiveTurret = null;

        combatants = new List<Vessel>();
        alliances = new Dictionary<Vessel, int>();
        
        speed = BattleSpeed.Normal;
        numAsteroids = 1;

        starColor = StarColor.Yellow;
        envDanger = EnvDanger.None;

        flagship = null;
    }
}
