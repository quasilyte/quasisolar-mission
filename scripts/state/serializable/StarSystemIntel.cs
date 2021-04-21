using System.Collections.Generic;

// StarSystemIntel is a snapshot of StarSystem information
// that is known to the player.
public class StarSystemIntel {
    public bool hasArtifact;
    public bool hasBase;

    public Faction baseOwner;
    public int garrisonSize;
    public int baseLevel;
    public int baseHp;

    public int numResourcePlanets;
    public int numMines;
}