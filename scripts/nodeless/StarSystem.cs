using Godot;
using System.Collections.Generic;

public class StarSystem {
    public int id;
    public string name;
    public StarColor color;
    public Vector2 pos;

    public int randomEventCooldown = 0;

    // If intel is null, this system is not visited at all.
    // Non-null intelligence records the latest known information about the system.
    public StarSystemIntel intel = null;

    public List<ResourcePlanet> resourcePlanets = new List<ResourcePlanet>{};

    public StarBase starBase;

    public string artifact;
    public int artifactRecoveryDelay;
}