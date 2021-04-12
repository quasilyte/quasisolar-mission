using Godot;
using System.Collections.Generic;

public class ArenaSettings {
    public enum BattleSpeed {
        Slow,
        Normal,
        Fast,
        VeryFast,
    }

    public static bool isQuickBattle;

    public static BattleSpeed speed;

    // How many asteroids will fly around.
    // 0 is valid option here.
    public static int numAsteroids;

    public static List<Vessel> combatants;

    public static void Reset() {
        combatants = new List<Vessel>();
        
        speed = BattleSpeed.Normal;
        numAsteroids = 1;
    }
}
