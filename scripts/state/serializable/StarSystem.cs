using Godot;
using System.Collections.Generic;

public class StarSystem: AbstractPoolValue {
    public struct Ref {
        public long id;
        public StarSystem Get() { return RpgGameState.instance.starSystems.Get(id); }
    }
    public Ref GetRef() { return new Ref{id = id}; }

    public string name;
    public StarColor color;
    public Vector2 pos;

    public int randomEventCooldown = 0;

    // If intel is null, this system is not visited at all.
    // Non-null intelligence records the latest known information about the system.
    public StarSystemIntel intel = null;

    public int visitsNum = 0;

    public List<ResourcePlanet> resourcePlanets = new List<ResourcePlanet>{};

    public StarBase.Ref starBase;

    public bool HasArtifact() {
        return resourcePlanets.Find((x) => x.artifact != "") != null;
    }
}
