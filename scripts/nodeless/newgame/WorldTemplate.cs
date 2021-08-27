using System.Collections.Generic;
using Godot;

public class WorldTemplate {
    public class Sector {
        public WorldTemplate world;
        public int level;
        public List<System> systems = new List<System>();
        public Rect2 rect;
    }

    public class System {
        public Sector sector;
        public StarSystem data;
    }

    public HashSet<string> starSystenNames = new HashSet<string>();
    public List<Sector> sectors;
    public RpgGameState.Config config;
}
