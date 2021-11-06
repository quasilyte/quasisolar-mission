public class ResourcePlanet {
    public string name;

    public string textureName;

    public int powerPerDay;
    public int organicPerDay;
    public int mineralsPerDay;

    public int temperature;
    public int explorationUnits;
    public int explorationBonus;

    public bool gasGiant = false;
    public int explored = 0;
    public string activeDrone = "";

    public string artifact = "";

    public bool IsExplored() {
        return explored >= explorationUnits;
    }

    public ResourcePlanet() {}

    public ResourcePlanet(int minerals, int organic, int power, string planetName = "") {
        powerPerDay = power;
        organicPerDay = organic;
        mineralsPerDay = minerals;

        var nameRoll = QRandom.PositiveInt();
        if (power > 0) {
            name = powerWorlds[nameRoll % powerWorlds.Length] + " World";
        } else if (organic > 0) {
            name = organicWorlds[nameRoll % organicWorlds.Length] + " World";
        } else if (minerals > 0) {
            name = mineralWorlds[nameRoll % mineralWorlds.Length] + " World";
        } else {
            if (planetName == "") {
                throw new System.Exception("empty planet name with empty resources");
            }
            name = planetName;
        }
    }

    private static string[] powerWorlds = new string[]{
        "Emerald",
        "Fluorescent",
        "Halide",
        "Infrared",
        "Magnetic",
        "Noble",
        "Pellucid",
        "Plutonic",
        "Radioactive",
        "Ultraviolet",
    };

    private static string[] organicWorlds = new string[]{
        "Chlorine",
        "Azure",
        "Alkali",
        "Copper",
        "Crimson",
        "Dust",
        "Green",
        "Hydrocarbon",
        "Oolite",
        "Organic",
        "Primordial",
        "Telluric",
        "Ultramarine",
        "Water",
    };

    private static string[] mineralWorlds = new string[]{
        "Acid",
        "Carbide",
        "Chondrite",
        "Cimmerian",
        "Cyanic",
        "Iodine",
        "Lanthanide",
        "Magma",
        "Maroon",
        "Metallic",
        "Opalescent",
        "Purple",
        "Ruby",
        "Sapphire",
        "Selenic",
        "Xenolithic",
        "Yttric",
    };
}
