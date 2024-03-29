using Godot;
using System.Collections.Generic;
using System;

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
    public static bool isStarBaseBattle;

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

    public static Action<BattleResult> extraReward;

    public static void Reset() {
        isQuickBattle = false;
        isStarBaseBattle = false;

        extraReward = (BattleResult _) => {};

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
