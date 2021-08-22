using System.Collections.Generic;

public class NewGameOptions {
    public class Option {
        public string text;
        public object value = null;
        public int score;
        public bool selected;
    }

    public Dictionary<string, Option[]> byName = new Dictionary<string, Option[]> {
        {
            "StartingCredits",
            new Option[]{
                new Option{text = "12500", value = 12500, score = -5},
                new Option{text = "4000", value = 4000, score = -3},
                new Option{text = "1500", value = 1500, score = 0, selected = true},
                new Option{text = "0", value = 0, score = 2},
            }
        },

        {
            "PlanetResources",
            new Option[]{
                new Option{text = "Rich", value = 2, score = -45},
                new Option{text = "Normal", value = 1, score = 0, selected = true},
                new Option{text = "Poor", value = 0, score = 15},
            }
        },

        {
            "KrigiaPresence",
            new Option[]{
                new Option{text = "Minimal", value = "minimal", score = -20},
                new Option{text = "Normal", value = "normal", score = 0, selected = true},
                new Option{text = "High", value = "high", score = 25},
            }
        },

        {
            "DraklidPresence",
            new Option[]{
                new Option{text = "Minimal", value = "minimal", score = -15},
                new Option{text = "Normal", value = "normal", score = 0, selected = true},
                new Option{text = "High", value = "high", score = 10},
            }
        },

        {
            "MissionDeadline",
            new Option[]{
                new Option{text = "8000 days", value = 8000, score = -30},
                new Option{text = "4000 days", value = 4000, score = 0, selected = true},
                new Option{text = "3000 days", value = 3000, score = 15},
                new Option{text = "2500 days", value = 2500, score = 25},
            }
        },

        {
            "RandomEvents",
            new Option[]{
                new Option{text = "Very Rare", value = 0, score = 0},
                new Option{text = "Occasional", value = 0, score = 0, selected = true},
                new Option{text = "Usual", value = 0, score = 0},
            }
        },

        {
            "Asteroids",
            new Option[]{
                new Option{text = "None", value = 0, score = 0},
                new Option{text = "Few", value = 1, score = 0, selected = true},
                new Option{text = "Average", value = 2, score = 0},
                new Option{text = "Many", value = 3, score = 0},
            }
        },
    };
}
