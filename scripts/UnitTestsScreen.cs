using Godot;
using System;
using System.Collections.Generic;

public class UnitTestsScreen : Node2D {
    public override void _Ready() {
        InitState();

        var tests = new List<AbstractTest>{
            new TestResearch(),
        };

        var textLabel = GetNode<RichTextLabel>("Panel/ScrollContainer/MarginContainer/Text");

        Action<string> write = (string msg) => {
            textLabel.BbcodeText += msg + "\n";
        };

        foreach (var test in tests) {
            var className = test.GetType().Name;
            var result = test.Run();
            if (result.err == null) {
                write("[color=#00af00]<OK>[/color] " + className);
            } else {
                write(className + ": " + result.err.message);
                write(result.err.stackTrace);
                write("[color=#af0000]<ERROR>[/color]" + className);
            }
        }
    }

    private void InitState() {
        ShieldDesign.InitLists();
        WeaponDesign.InitLists();
        VesselDesign.InitLists();
        ArtifactDesign.InitLists();
        SentinelDesign.InitLists();
        Research.InitLists();
        MapEventRegistry.InitLists();
        VesselStatus.InitLists();
    }
}
