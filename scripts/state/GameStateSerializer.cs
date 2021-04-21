using System.IO;
using System.Text;
using System;
using Godot;
using Newtonsoft.Json;

public static class GameStateSerializer {
    public static void Encode(RpgGameState gameState) {
        GD.Print("encoding to " + System.Environment.CurrentDirectory + "/gamedata.json");

        string json = JsonConvert.SerializeObject(gameState, Formatting.Indented);

        using (FileStream fs = new FileStream("gamedata.json", FileMode.Create)) {
            var utf8 = new UTF8Encoding();
            var data = utf8.GetBytes(json);
            fs.Write(data, 0, data.Length);
        }
    }

    public static RpgGameState Decode() {
        GD.Print("decoding from " + System.Environment.CurrentDirectory + "/gamedata.json");

        using (StreamReader r = new StreamReader("gamedata.json")) {
            string json = r.ReadToEnd();
            var gameState = JsonConvert.DeserializeObject<RpgGameState>(json);
            return gameState;
        }
    }
}
