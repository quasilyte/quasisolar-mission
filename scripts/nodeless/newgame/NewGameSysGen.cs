using Godot;

public class NewGameSysGen {
    public static WorldTemplate.System NewStarSystem(WorldTemplate.Sector sector, Vector2 pos) {
        var world = sector.world;
        var sys = world.config.starSystems.New();
        sys.name = StarSystemNames.UniqStarSystemName(world.starSystenNames);
        sys.color = RandomStarSystemColor();
        sys.pos = pos;

        var worldSys = new WorldTemplate.System{
            sector = sector,
            data = sys,
        };

        PlanetGenerator.GeneratePlanets(worldSys);

        return worldSys;
    }

    private static StarColor RandomStarSystemColor() {
        var colorRoll = QRandom.IntRange(0, 5);
        if (colorRoll == 0) {
            return StarColor.Blue;
        } else if (colorRoll == 1) {
            return StarColor.Green;
        } else if (colorRoll == 2) {
            return StarColor.Yellow;
        } else if (colorRoll == 3) {
            return StarColor.Orange;
        } else if (colorRoll == 4) {
            return StarColor.Red;
        } else {
            return StarColor.White;
        }
    }

    public static Vector2 PickStarSystemPos(WorldTemplate.Sector sector) {
        var attempts = 0;
        var result = Vector2.Zero;
        while (true) {
            attempts++;
            var dist = QRandom.FloatRange(175, 500);
            var toBeConnected = QRandom.Element(sector.systems);
            var candidate = QMath.RandomizedLocation(toBeConnected.data.pos, dist * 2);
            if (!sector.rect.HasPoint(candidate)) {
                continue;
            }
            var retry = false;
            foreach (var sys in sector.systems) {
                if (sys.data.pos.DistanceTo(candidate) < 170) {
                    retry = true;
                    break;
                }
            }
            if (retry) {
                continue;
            }
            result = candidate;
            break;
        }
        if (attempts > 10) {
            GD.Print($"used {attempts} attempts to find a star system spot");
        }
        return result;
    }
}
