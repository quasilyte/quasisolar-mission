using System.IO;
using System.Text;
using System;
using Godot;
using Newtonsoft.Json;

public static class GameStateSerializer {
    public static void Encode(RpgGameState gameState) {
        var path = ProjectSettings.GlobalizePath("user://gamedata.json");
        GD.Print("encoding to " + path);

        string json = JsonConvert.SerializeObject(gameState, Formatting.Indented);

        using (FileStream fs = new FileStream(path, FileMode.Create)) {
            var utf8 = new UTF8Encoding();
            var data = utf8.GetBytes(json);
            fs.Write(data, 0, data.Length);
        }
    }

    public static RpgGameState Decode() {
        var path = ProjectSettings.GlobalizePath("user://gamedata.json");
        GD.Print("decoding from " + path);

        using (StreamReader r = new StreamReader(path)) {
            string json = r.ReadToEnd();
            var gameState = JsonConvert.DeserializeObject<RpgGameState>(json);
            return gameState;
        }
    }
}
