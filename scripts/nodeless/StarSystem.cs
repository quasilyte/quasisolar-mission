using Godot;
using System.Collections.Generic;

public class StarSystem {
    public enum Color {
        Blue,
        Green,
        Yellow,
        Orange,
        Red,
        White,
    }

    public int id;
    public string name;
    public Color color;
    public Vector2 pos;

    public int randomEventCooldown = 0;

    // If intel is null, this system is not visited at all.
    // Non-null intelligence records the latest known information about the system.
    public StarSystemIntel intel = null;

    public List<ResourcePlanet> resourcePlanets = new List<ResourcePlanet>{};

    public StarBase starBase;

    public ArtifactDesign artifact;
    public int artifactRecoveryDelay;
}