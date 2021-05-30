using Godot;
using System.Collections.Generic;

public class BotEvents {
    public List<Area2D> closeRangeCollisions = new List<Area2D>();
    public List<Area2D> midRangeCollisions = new List<Area2D>();
    public bool targetedByZap = false;
}
