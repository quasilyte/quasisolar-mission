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

    // How many asteroids will fly around.
    // 0 is valid option here.
    public static int numAsteroids;

    public static List<Vessel> combatants;

    public static void Reset() {
        isQuickBattle = false;

        combatants = new List<Vessel>();
        
        speed = BattleSpeed.Normal;
        numAsteroids = 1;

        starColor = StarColor.Yellow;
        envDanger = EnvDanger.None;
    }
}
