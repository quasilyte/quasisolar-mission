using System.Collections.Generic;
using Godot;

public static class PilotNames {
    public static string UniqPhaaName(HashSet<string> alreadyUsed) {
        var name = PickName(phaaNames, phaaNamePrefix, phaaNameSuffix, alreadyUsed);
        alreadyUsed.Add(name);
        return name;
    }

    public static string UniqRarilouName(HashSet<string> alreadyUsed) {
        var name = PickName(rarilouNames, rarilouNamePrefix, rarilouNameSuffix, alreadyUsed);
        alreadyUsed.Add(name);
        return name;
    }

    public static string UniqHumanName(HashSet<string> alreadyUsed) {
        var name = PickName(humanNames, humanNamePrefix, humanNameSuffix, alreadyUsed);
        alreadyUsed.Add(name);
        return name;
    }

    public static string PickName(string[] names, string[] prefixes, string[] suffixes, HashSet<string> alreadyUsed = null) {
        var i = QRandom.PositiveInt() % names.Length;
        for (int attempt = 0; attempt < 5; attempt++) {
            var name = names[i];
            if (alreadyUsed == null || !alreadyUsed.Contains(name)) {
                return name;
            }
            foreach (string prefix in prefixes) {
                if (!alreadyUsed.Contains($"{prefix} {name}")) {
                    return $"{prefix} {name}";
                }
            }
            foreach (string suffix in suffixes) {
                if (!alreadyUsed.Contains($"{name} {suffix}")) {
                    return $"{suffix} {name}";
                }
            }
            i++;
            if (i >= names.Length) {
                i = 0;
            }
        }
        return names[i] + "#" + alreadyUsed.Count;
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

    private static string[] phaaNamePrefix = {
        "Ab",
        "Ub",
        "Bi",
    };

    private static string[] phaaNameSuffix = {};

    private static string[] phaaNames = {
        "Urblob",
        "Mibulb",
        "Kublub",
        "Arkbub",
        "Kiblab",
    };

    private static string[] rarilouNamePrefix = {};

    private static string[] rarilouNameSuffix = {
        "I",
        "II",
        "III",
        "IV",
        "V",
    };

    private static string[] rarilouNames = {
        "Rala",
        "Ralu",
        "Raali",
        "Rakka",
        "Rilu",
        "Rila",
        "Riakka",
        "Rikala",
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
