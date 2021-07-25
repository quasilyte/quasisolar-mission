using System.Text;
using System.Collections.Generic;

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

        foreach (var l in text.Split('\n')) {
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
}
