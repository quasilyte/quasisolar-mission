using System.Collections.Generic;
using System;
using Godot;

public static class ArenaManager {
    public static List<Vessel> ConvertVesselList(List<Vessel.Ref> list) {
        var result = new List<Vessel>();
        foreach (var v in list) {
            result.Add(v.Get());
        }
        return result;
    }

    public static SpaceUnit NewSpaceUnit(Faction faction, params Vessel[] fleet) {
        var fleetList = new List<Vessel.Ref>();
        foreach (var v in fleet) {
            fleetList.Add(v.GetRef());
        }

        var spaceUnit = RpgGameState.instance.spaceUnits.New();
        spaceUnit.owner = faction;
        spaceUnit.pos = RpgGameState.instance.humanUnit.Get().pos;
        spaceUnit.fleet = fleetList;
        return spaceUnit;
    }

    public static void SetArenaSettings(StarSystem location, List<Vessel.Ref> enemyFleet, List<Vessel.Ref> alliedFleet) {
        SetArenaSettings(location, ConvertVesselList(enemyFleet), ConvertVesselList(alliedFleet));
    }

    public static void SetArenaSettings(StarSystem location, List<Vessel> enemyFleet, List<Vessel> alliedFleet) {
        var gameState = RpgGameState.instance;

        var spawnMap = DefaultSpawnMap(enemyFleet, alliedFleet);
        var vessels = new List<Vessel>();
        vessels.AddRange(enemyFleet);
        vessels.AddRange(alliedFleet);
        var alliances = new Dictionary<Vessel, int>();
        foreach (var v in vessels) {
            alliances[v] = gameState.FactionsAtWar(Faction.Earthling, v.faction) ? 1 : 0;
        }
        SetArenaSettings(location, vessels, spawnMap, alliances);
    }

    public static void SetStagedArenaSettings(StarSystem location, SpaceUnit bot1, SpaceUnit bot2, SpaceUnit human) {
        Func<Faction, int> factionToAlliance = (Faction f) => {
            if (f == Faction.RandomEventAlly) {
                return 0;
            }
            if (f == Faction.RandomEventHostile) {
                return 1;
            }
            return 2;
        };
        var alliances = new Dictionary<Vessel, int>();
        foreach (var vref in human.fleet) {
            alliances[vref.Get()] = 0;
        }
        foreach (var vref in bot1.fleet) {
            alliances[vref.Get()] = factionToAlliance(bot1.owner);
        }
        foreach (var vref in bot2.fleet) {
            alliances[vref.Get()] = factionToAlliance(bot2.owner);
        }

        var spawnMap = new Dictionary<Vessel, Vector2>();
        var vessels = new List<Vessel>();
        for (int i = 0; i < human.fleet.Count; i++) {
            var v = human.fleet[i].Get();
            spawnMap[v] = QMath.RandomizedLocation(new Vector2(224, 288 + (i * 192)), 40);
            vessels.Add(v);
        }
        foreach (var vref in bot1.fleet) {
            var v = vref.Get();
            spawnMap[v] = v.spawnPos;
            vessels.Add(v);
        }
        foreach (var vref in bot2.fleet) {
            var v = vref.Get();
            spawnMap[v] = v.spawnPos;
            vessels.Add(v);
        }

        SetArenaSettings(location, vessels, spawnMap, alliances);
    }

    private static Dictionary<Vessel, Vector2> DefaultSpawnMap(List<Vessel> enemyFleet, List<Vessel> alliedFleet) {
        var m = new Dictionary<Vessel, Vector2>();

        for (int i = 0; i < enemyFleet.Count; i++) {
            var v = enemyFleet[i];
            m[v] = QMath.RandomizedLocation(new Vector2(1568, 288 + (i * 192)), 40);
        }
        for (int i = 0; i < alliedFleet.Count; i++) {
            var v = alliedFleet[i];
            m[v] = QMath.RandomizedLocation(new Vector2(224, 288 + (i * 192)), 40);
        }

        return m;
    }

    private static void SetArenaSettings(StarSystem location, List<Vessel> vessels, Dictionary<Vessel, Vector2> spawnMap, Dictionary<Vessel, int> alliances) {
        var gameState = RpgGameState.instance;
        var humanUnit = gameState.humanUnit.Get();

        ArenaSettings.Reset();
        ArenaSettings.isQuickBattle = false;
        ArenaSettings.alliances = alliances;

        // TODO: respect the game settings here.
        ArenaSettings.numAsteroids = QRandom.IntRange(0, 3);
        // 30% - none
        // 20% - purple nebula
        // 20% - blue nebula
        // 30% - star
        var envHazardRoll = QRandom.Float();
        if (envHazardRoll < 0.3) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.None;
        } else if (envHazardRoll < 0.5) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.PurpleNebula;
        } else if (envHazardRoll < 0.7) {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.BlueNebula;
        } else {
            ArenaSettings.envDanger = ArenaSettings.EnvDanger.Star;
        }
        ArenaSettings.starColor = location.color;

        foreach (var v in vessels) {
            if (v.id == humanUnit.fleet[0].id && gameState.rpgMode) {
                ArenaSettings.flagship = v;
            }

            v.spawnPos = spawnMap[v];
            ArenaSettings.combatants.Add(v);
            if (!v.isBot) {
                v.isGamepad = GameControls.preferGamepad;
            }
        }

        if (location.starBase.id != 0) {
            // For now, only player-controlled bases can have defensive
            // structures, so we don't really care with setting the
            // proper alliance number here (since it's always 0).
            var starBase = location.starBase.Get();
            if (starBase.modules.Contains("Gauss Turret")) {
                ArenaSettings.defensiveTurretAlliance = 0;
                ArenaSettings.defensiveTurret = NeedleGunWeapon.TurretDesign;
                ArenaSettings.defensiveTurretShots = 3;
                if (gameState.technologiesResearched.Contains("Gauss Turret Capacity")) {
                    ArenaSettings.defensiveTurretShots++;
                }
            } else if (starBase.modules.Contains("Missile Turret")) {
                ArenaSettings.defensiveTurretAlliance = 0;
                ArenaSettings.defensiveTurret = RocketLauncherWeapon.TurretDesign;
                ArenaSettings.defensiveTurretShots = 3;
                if (gameState.technologiesResearched.Contains("Missile Turret Capacity")) {
                    ArenaSettings.defensiveTurretShots++;
                }
            }
        }
    }
}
