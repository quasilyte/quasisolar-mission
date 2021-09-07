using System.Text;
using System.Collections.Generic;
using Godot;

public static class Utils {
    private static Dictionary<string, int> _decimalToRoman = new Dictionary<string, int>{
        {"M", 1000 },
        {"CM", 900},
        {"D", 500},
        {"CD", 400},
        {"C", 100},
        {"XC", 90},
        {"L", 50},
        {"XL", 40},
        {"X", 10},
        {"IX", 9},
        {"V", 5},
        {"IV", 4},
        {"I", 1}
    };

    public static string IntToRoman(int num) {
        var sb = new StringBuilder();
        foreach (var pair in _decimalToRoman) {
            while (num >= pair.Value) {
                sb.Append(pair.Key);
                num -= pair.Value;
            }
        }
        return sb.ToString();
    }

    public static string FormatMultilineText(string text) {
        var paragraphs = new List<string>();
        string p = "";

        var lines = text.Split('\n');
        if (lines.Length == 1) {
            return lines[0];
        }

        foreach (var l in lines) {
            var s = l.Trim();
            if (string.IsNullOrEmpty(s)) {
                if (!string.IsNullOrEmpty(p)) {
                    paragraphs.Add(p);
                }
                p = "";
                continue;
            }
            s = s.Replace("\\n", "\n");
            if (string.IsNullOrEmpty(p)) {
                p = s;
            } else {
                p += " " + s;
            }
        }

        var resultString = string.Join("\n\n", paragraphs);
        return resultString.Replace("\n ", "\n");;
    }

    public static Color FactionColor(Faction faction) {
        if (faction == Faction.Earthling) {
            return Color.Color8(0x82, 0xd5, 0xd5);
        }
        if (faction == Faction.Wertu) {
            return Color.Color8(0x16, 0x4b, 0xb6);
        }
        if (faction == Faction.Krigia) {
            return Color.Color8(0xca, 0x30, 0x4d);
        }
        if (faction == Faction.Zyth) {
            return Color.Color8(0x15, 0xad, 0x27);
        }
        if (faction == Faction.Phaa) {
            return Color.Color8(0x88, 0xc1, 0x19);
        }
        if (faction == Faction.Draklid) {
            return Color.Color8(0x89, 0x1d, 0xcd);
        }
        if (faction == Faction.Vespion) {
            return Color.Color8(0x83, 0x9c, 0xd5);
        }
        if (faction == Faction.Rarilou) {
            return Color.Color8(0xdf, 0xa6, 0x24);
        }
        throw new System.Exception("unexpected faction: " + faction.ToString());
    }
}
