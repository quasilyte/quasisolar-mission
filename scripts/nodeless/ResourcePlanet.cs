public class ResourcePlanet {
    public string name;

    public int powerPerDay;
    public int organicPerDay;
    public int mineralsPerDay;

    public int powerCollected = 0;
    public int organicCollected = 0;
    public int mineralsCollected = 0;

    public bool hasMine = false;

    public ResourcePlanet(int minerals, int organic, int power) {
        powerPerDay = power;
        organicPerDay = organic;
        mineralsPerDay = minerals;

        var nameRoll = QRandom.PositiveInt();
        if (power > 0) {
            name = powerWorlds[nameRoll % powerWorlds.Length] + " World";
        } else if (organic > 0) {
            name = organicWorlds[nameRoll % organicWorlds.Length] + " World";
        } else {
            name = mineralWorlds[nameRoll % mineralWorlds.Length] + " World";
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
