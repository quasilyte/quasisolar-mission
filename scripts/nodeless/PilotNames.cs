using System.Collections.Generic;
using Godot;

public static class PilotNames {
    public static string UniqHumanName(HashSet<string> alreadyUsed) {
        var name = PeekHumanName(alreadyUsed);
        alreadyUsed.Add(name);
        return name;
    }

    public static string PeekHumanName(HashSet<string> alreadyUsed = null) {
        var i = QRandom.PositiveInt() % humanNames.Length;
        for (int attempt = 0; attempt < 5; attempt++) {
            var name = humanNames[i];
            if (alreadyUsed == null || !alreadyUsed.Contains(name)) {
                return name;
            }
            foreach (string prefix in humanNamePrefix) {
                if (!alreadyUsed.Contains($"{prefix} {name}")) {
                    return $"{prefix} {name}";
                }
            }
            foreach (string suffix in humanNameSuffix) {
                if (!alreadyUsed.Contains($"{name} {suffix}")) {
                    return $"{suffix} {name}";
                }
            }
            i++;
            if (i >= humanNames.Length) {
                i = 0;
            }
        }
        return humanNames[i] + "#" + alreadyUsed.Count;
    }

    private static string[] humanNamePrefix = {
        "Dr",
        "Prof",
        "Lt",
        "Capt",
    };

    private static string[] humanNameSuffix = {
        "Junior",
        "Senior",
    };

    private static string[] humanNames = {
        "Aidan",
        "Bernard",
        "Decker",
        "Edgar",
        "Elmer",
        "Joe",
        "Kirk",
        "Marcel",
        "Michael",
        "Morty",
        "Nicolas",
        "Ren",
        "Rhys",
        "Rick",
        "Rupert",
        "Seth",
        "Ted",
        "Tobias",
        "Trent",
        "Troy",
        "Wu",

        "Alice",
        "Anastasia",
        "Bertha",
        "Evelyn",
        "Kate",
        "Lisa",
        "Lin",
        "Lowri",
        "Maria",
        "Mariam",
        "Mei",
        "Natalie",
        "Natasha",
        "Rebecca",
        "Talana",
        "Yua",
    };
}
