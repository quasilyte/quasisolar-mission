using Godot;
using System.Collections.Generic;

public class Pilot {
    public int alliance;
    public string name;

    public VesselNode Vessel;
    public List<Pilot> Enemies = new List<Pilot>();
    public List<Pilot> Allies = new List<Pilot>();
    public bool Active = true;
}
