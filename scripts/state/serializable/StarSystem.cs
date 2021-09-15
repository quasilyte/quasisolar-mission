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

    public int visitsNum = 0;

    public List<ResourcePlanet> resourcePlanets = new List<ResourcePlanet>{};

    public StarBase.Ref starBase;

    public bool HasArtifact() {
        return resourcePlanets.Find((x) => x.artifact != "") != null;
    }

    public bool Visited() { return visitsNum != 0; }
}
