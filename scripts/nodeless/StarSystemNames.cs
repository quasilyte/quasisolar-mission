using System.Collections.Generic;
using Godot;

public static class StarSystemNames {
    public static string UniqStarSystemName(HashSet<string> alreadyUsed) {
        var name = PeekStarSystemName(alreadyUsed);
        alreadyUsed.Add(name);
        return name;
    }

    public static string PeekStarSystemName(HashSet<string> alreadyUsed = null) {
        while (true) {
            var name = QRandom.Element(_names);
            if (alreadyUsed == null || !alreadyUsed.Contains(name)) {
                return name;
            }
        }
    }

    private static List<string> _names = new List<string>{
        "Wolf",

        "Acephalia",
        "Acamar",
        "Achernar",
        "Acrab",
        "Ascella",
        "Alcor",
        "Polaris",
        "Pherkab",
        "Regor",
        "Algol",
        "Altair",
        "Arcturus",
        "Taygeta",
        "Celaeno",
        "Pleione",
        "Ain",
        "Bellatrix",
        "Asterope",
        "Capella",
        "Deneb",
        "Denebola",
        "Fomalhaut",
        "Haedus",
        "Hamal",
        "Heze",
        "Megrez",
        "Mizar",
        "Markab",
        "Meleph",
        "Menkar",
        "Canopus",
        "Adhara",
        "Caph",
        "Avior",
        "Procyon",
        "Regulus",
        "Mira",
        "Gienah",
        "Vega",
        "Vela",
        "Sirius",
        "Pollux",
        "Arrakis",
        "Rastaban",
        "Acrux",
        "Thuban",
        "Rigel",
        "Antares",
        "Loa",
        "Salamander",

        "Ajax",
        "Altair",
        "Alixia",
        "Aurora",
        "Bemus",
        "Centaurus",
        "Castor",
        "Calliope",
        "Deion",
        "Dea",
        "Galen",
        "Geode",
        "Helios",
        "Hydrus",
        "Elke",
        "Gaea",
        "Haron",
        "Hebe",
        "Hestia",
        "Hydra",
        "Orion",
        "Kaia",
        "Kali",
        "Castor",

        "Nereida",
        "Fortuna",
        "Nemesis",
        "Echo",
        "Aldebaran",
    };
}
