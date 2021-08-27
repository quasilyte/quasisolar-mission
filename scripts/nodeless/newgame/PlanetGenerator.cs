using System.Collections.Generic;

public class PlanetGenerator {
    private static Dictionary<string, int> _planetSprites = new Dictionary<string, int>{
        {"alpine", 6}, // Organic with normal climate
        {"oceanic", 12}, // Organic with normal climate
        {"fungal", 6}, // Organic with bad climate
        {"savannah", 6}, // Organic with bad climate
        {"dry", 7}, // Minerals-only
        {"rock", 5}, // Minerals-only
        {"venusian", 6}, // Minerals-only
        {"gas", 6}, // Power-only
        {"ice", 6}, // Cold planets
        {"volcanic", 6}, // Hot planets
        {"primordial", 6}, // Fallback kind
        {"martian", 8}, // Fallback kind
    };

    public static void GeneratePlanets(WorldTemplate.System sys) {
        var planetSprites = new HashSet<string>();

        var level = sys.sector.level;

        var planetResources = 1; // TODO: this is "PlanetResources" option
        var planetsRollBonus = (float)planetResources * 0.20f;
        var planetsBudget = QRandom.FloatRange(0, 0.6f) + planetsRollBonus;
        if (planetsBudget < 0.1) {
            sys.data.resourcePlanets = new List<ResourcePlanet>{
                NewResourcePlanet(planetsBudget, level, planetSprites),
            };
        } else {
            while (planetsBudget >= 0.1) {
                if (sys.data.resourcePlanets.Count == 2) {
                    sys.data.resourcePlanets.Add(NewResourcePlanet(planetsBudget, level, planetSprites));
                    break;
                }
                var toSpend = QRandom.FloatRange(0.1f, planetsBudget);
                if (toSpend > 0.6) {
                    var change = toSpend - 0.6f;
                    planetsBudget += change;
                    toSpend = 0.6f;
                }
                planetsBudget -= toSpend;
                sys.data.resourcePlanets.Add(NewResourcePlanet(toSpend, level, planetSprites));
            }
        }
    }

    public static string PickPlanetSprite(string kind, HashSet<string> picked) {
        var numChoices = _planetSprites[kind];
        while (true) {
            var i = QRandom.IntRange(1, numChoices);
            var spriteName = $"{kind}{i}";
            if (picked.Contains(spriteName)) {
                continue;
            }
            return spriteName;
        }
    }

    public static ResourcePlanet NewResourcePlanet(float budget, int level, HashSet<string> planetSprites) {
        int minerals = 0;
        int organic = 0;
        int power = 0;

        const float mineralCost = 0.08f;
        const float organicCost = 0.15f;
        const float powerCost = 0.21f;

        while (budget >= mineralCost) {
            var resourceType = QRandom.IntRange(0, 2);
            if (resourceType == 2 && budget >= powerCost) {
                budget -= powerCost;
                power++;
            } else if (resourceType == 1 && budget >= organicCost) {
                budget -= organicCost;
                organic++;
            } else {
                budget -= mineralCost;
                minerals++;
            }
        }

        if (minerals == 0 && organic == 0 && power == 0) {
            minerals = 1;
        }

        var planet = new ResourcePlanet(minerals, organic, power);

        var explorationBonus = QRandom.IntRange(3000, 6000);
        explorationBonus += level * QRandom.IntRange(2500, 3000);

        // 20% - cold
        // 25% - normal
        // 25% - hot
        // 30% - very hot
        var temperatureClassRoll = QRandom.Float();
        if (temperatureClassRoll < 0.2) {
            planet.temperature = QRandom.IntRange(-240, -20);
            explorationBonus = QMath.IntAdjust(explorationBonus, 0.9);
        } else if (temperatureClassRoll < 0.45) {
            planet.temperature = QRandom.IntRange(-70, 100);
        } else if (temperatureClassRoll < 0.7) {
            planet.temperature = QRandom.IntRange(100, 260);
            explorationBonus = QMath.IntAdjust(explorationBonus, 1.1);
        } else {
            planet.temperature = QRandom.IntRange(150, 495);
            explorationBonus = QMath.IntAdjust(explorationBonus, 1.3);
        }

        planet.explorationUnits = QRandom.IntRange(70, 240) + (level * 10);

        if (QRandom.Float() < 0.25) {
            explorationBonus = QMath.IntAdjust(explorationBonus, 1.5);
        }

        if (planet.powerPerDay != 0 && planet.mineralsPerDay == 0 && planet.organicPerDay == 0) {
            planet.textureName = PickPlanetSprite("gas", planetSprites);
            planet.gasGiant = true;
            planet.explorationUnits = QMath.IntAdjust(planet.explorationUnits, 1.25);
        } else if (planet.temperature > 200) {
            planet.textureName = PickPlanetSprite("volcanic", planetSprites);
        } else if (planet.temperature < -70) {
            planet.textureName = PickPlanetSprite("ice", planetSprites);
        } else if (planet.powerPerDay == 0 && planet.mineralsPerDay != 0 && planet.organicPerDay == 0) {
            var roll = QRandom.Float();
            if (roll < 0.33) {
                planet.textureName = PickPlanetSprite("dry", planetSprites);
            } else if (roll < 0.66) {
                planet.textureName = PickPlanetSprite("rock", planetSprites);
            } else {
                planet.textureName = PickPlanetSprite("venusian", planetSprites);
            }
        } else if (planet.organicPerDay != 0) {
            if (planet.temperature < 120) {
                if (QRandom.Bool()) {
                    planet.textureName = PickPlanetSprite("oceanic", planetSprites);
                } else {
                    planet.textureName = PickPlanetSprite("alpine", planetSprites);
                }
            } else {
                if (QRandom.Bool()) {
                    planet.textureName = PickPlanetSprite("savannah", planetSprites);
                } else {
                    planet.textureName = PickPlanetSprite("fungal", planetSprites);
                }
            }
        } else {
            if (QRandom.Bool()) {
                planet.textureName = PickPlanetSprite("martian", planetSprites);
            } else {
                planet.textureName = PickPlanetSprite("primordial", planetSprites);
            }
        }

        if (planet.gasGiant) {
            planet.temperature = QMath.ClampMax(planet.temperature, 205);
        }

        planet.explorationBonus = explorationBonus;

        return planet;
    }
}
