using Godot;
using System.Collections.Generic;
using System;

public class BattleResultPopup: AbstractMapPopupBuilder {
    private BattleResult _battleResult;

    public void SetBattleResult(BattleResult result) { _battleResult = result; }

    public MapEventPopupNode Build() {
        var lines = new List<string>();

        var gameState = RpgGameState.instance;
        var humanUnit = gameState.humanUnit.Get();

        var result = RpgGameState.lastBattleResult;

        var description = "The enemy is defeated, your fleet can now collect the spoils of war.";
        if (result.popupText != "") {
            description = result.popupText;
        }

        if (result.exp != 0) {
            gameState.experience += result.exp;
            lines.Add($"+{result.exp} experience");
        }
        if (result.fuel != 0) {
            RpgGameState.AddFuel(result.fuel);
            lines.Add($"+{result.fuel} fuel units");
        }

        if (result.ru != 0) {
            gameState.credits += result.ru;
            lines.Add($"+{result.ru} RU");
        }

        if (result.debris.other != 0) {
            humanUnit.CargoAddDebris(result.debris.other, Faction.Neutral);
            lines.Add($"+{result.debris.other} debris");
        }
        if (result.debris.krigia != 0) {
            humanUnit.CargoAddDebris(result.debris.krigia, Faction.Krigia);
            lines.Add($"+{result.debris.krigia} Krigia debris");
        }
        if (result.debris.wertu != 0) {
            humanUnit.CargoAddDebris(result.debris.wertu, Faction.Wertu);
            lines.Add($"+{result.debris.wertu} Wertu debris");
        }
        if (result.debris.zyth != 0) {
            humanUnit.CargoAddDebris(result.debris.zyth, Faction.Zyth);
            lines.Add($"+{result.debris.zyth} Zyth debris");
        }
        if (result.debris.phaa != 0) {
            humanUnit.CargoAddDebris(result.debris.phaa, Faction.Phaa);
            lines.Add($"+{result.debris.phaa} Phaa debris");
        }
        if (result.debris.draklid != 0) {
            humanUnit.CargoAddDebris(result.debris.draklid, Faction.Draklid);
            lines.Add($"+{result.debris.draklid} Draklid debris");
        }
        if (result.debris.vespion != 0) {
            humanUnit.CargoAddDebris(result.debris.phaa, Faction.Vespion);
            lines.Add($"+{result.debris.vespion} Vespion debris");
        }
        if (result.debris.rarilou != 0) {
            humanUnit.CargoAddDebris(result.debris.rarilou, Faction.Rarilou);
            lines.Add($"+{result.debris.rarilou} Rarilou debris");
        }

        if (result.power != 0) {
            humanUnit.CargoAddPower(result.power);
            lines.Add($"+{result.power} power resource");
        }
        if (result.organic != 0) {
            humanUnit.CargoAddOrganic(result.organic);
            lines.Add($"+{result.organic} organic resource");
        }
        if (result.minerals != 0) {
            humanUnit.CargoAddMinerals(result.minerals);
            lines.Add($"+{result.minerals} mineral resouce");
        }

        if (!string.IsNullOrEmpty(result.technology)) {
            gameState.technologiesResearched.Add(result.technology);
            lines.Add($"{result.technology} unlocked");
        }
        if (!string.IsNullOrEmpty(result.research)) {
            gameState.technologiesResearched.Add(result.research + " Lock");
            lines.Add($"{result.research} research project");
        }

        var title = "Victory!";
        var text = Utils.FormatMultilineText(description + "\n\n" + String.Join("\\n", lines) + "\n");

        var options = new List<MapEventPopupNode.Option>{
            new MapEventPopupNode.Option{
                text = "OK",
                apply = () => Resolve(),
            },
        };

        return MapEventPopupNode.New(title, text, options);
    }
}
