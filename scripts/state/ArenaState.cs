using Godot;
using System.Collections.Generic;

public class ArenaState {
    // Set to the arena star hazard node, if there is one.
    public static StarHazardNode starHazard;

    public static void Reset() {
        starHazard = null;
    }
}
