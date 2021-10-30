using System.Collections.Generic;

public class ModTraderEncounterTQuest : AbstractDiplomacyTQuest {
    class Mod {
        public string name;
        public float chance;
        public int minerals = 0;
        public int organic = 0;
        public int power = 0;
    }

    private const float DISCOUNT_PER_ARTIFACT = 0.03f;

    private int _candidate;
    private bool _canAskToSwap = true;
    private float _priceMultiplier = 1;
    private string _wantArtifact = "";
    private List<Mod> _modSelection;

    public ModTraderEncounterTQuest() : base(Faction.Rarilou) {
        _candidate = QRandom.IntRange(0, GetPlayerSpaceUnit().fleet.Count - 1);
        RollMods();
        MaybeWantArtifact();
        _priceMultiplier -= _gameState.modTraderState.artifacts.Count * DISCOUNT_PER_ARTIFACT;

        DeclareValue("Vessel", () => Candidate().pilotName, true);
        DeclareValue("Installed mods", () => Candidate().modList.Count + "/5", true);
        DeclareValue("Prices", () => (int)(_priceMultiplier * 100) + "%", true);
        DeclareValue("Minerals", () => _gameState.humanUnit.Get().cargo.minerals, true);
        DeclareValue("Organic", () => _gameState.humanUnit.Get().cargo.organic, true);
        DeclareValue("Power", () => _gameState.humanUnit.Get().cargo.power, true);
    }

    public override TQuestCard GetFirstCard() {
        return new TQuestCard {
            image = "ModTrader",
            text = GetGreetingsText(),
            actions = GetRootActions(),
        };
    }

    private Vessel Candidate() {
        return GetPlayerSpaceUnit().fleet[_candidate].Get();
    }

    private string GetGreetingsText() {
        var proposal = "You are interested in mods, right?";
        if (_wantArtifact != "") {
            proposal = ($@"
                By the way, I noticed that you have an artifact named
                {_wantArtifact}. If you'll send the scans to me,
                I'll install one module of my choice for free. Interested?
            ");
        }
        return ($@"
            Ah, I was looking for you.

            I want to work on your {Candidate().designName} class vessel
            piloted by {Candidate().pilotName}.

            If you're willing to cover up my expenses,
            it could be a delightful experience for everyone.

            {proposal}
        ");
    }

    private List<TQuestAction> GetRootActions() {
        var actions = new List<TQuestAction>();

        actions.Add(new TQuestAction {
            text = "Which mods do you have for sale?",
            next = ShopSelectionCard,
        });

        if (_wantArtifact != "") {
            actions.Add(new TQuestAction{
                text = "I'll send you the artifact scans.",
                cond = () => Candidate().modList.Count < 5,
                next = InstallRandomCard,
            });
        }

        if (_canAskToSwap && GetPlayerSpaceUnit().fleet.Count != 1) {
            actions.Add(new TQuestAction {
                text = "Could you choose a different vessel?",
                next = AskForSwapCard,
            });
        }

        actions.Add(new TQuestAction {
            text = "[End conversation]",
            next = () => TQuestCard.exitQuestEnterMap,
        });

        return actions;
    }

    private int MineralsPrice(Mod mod) { return QMath.IntAdjust(mod.minerals, _priceMultiplier); }
    private int OrganicPrice(Mod mod) { return QMath.IntAdjust(mod.organic, _priceMultiplier); }
    private int PowerPrice(Mod mod) { return QMath.IntAdjust(mod.power, _priceMultiplier); }

    private string FormatModDescription(Mod mod) {
        var priceParts = new List<string>();
        if (mod.minerals != 0) {
            priceParts.Add($"[color=#6688b9]{MineralsPrice(mod)}[/color] minerals");
        }
        if (mod.organic != 0) {
            priceParts.Add($"[color=#61b62b]{OrganicPrice(mod)}[/color] organic");
        }
        if (mod.power != 0) {
            priceParts.Add($"[color=#ffd906]{PowerPrice(mod)}[/color] power");
        }
        var modInfo = VesselMod.modByName[mod.name];
        var text = $"A level {modInfo.level} mod [u]{mod.name}[/u] for " + string.Join(", ", priceParts) + ". ";
        text += "Effects: " + string.Join(", ", modInfo.GetEffects()) + ".";
        return text;
    }

    private TQuestCard ShopSelectionCard() {
        var actions = new List<TQuestAction>();

        var selectionText = "Here is what I can install today:\n\n";
        foreach (var mod in _modSelection) {
            selectionText += "* " + FormatModDescription(mod) + "\n\n";
            actions.Add(new TQuestAction {
                text = $"[Install {mod.name}]",
                cond = () => {
                    return Candidate().modList.Count < 5 &&
                        _gameState.humanUnit.Get().cargo.minerals >= MineralsPrice(mod) &&
                        _gameState.humanUnit.Get().cargo.organic >= OrganicPrice(mod) &&
                        _gameState.humanUnit.Get().cargo.power >= PowerPrice(mod);
                },
                apply = () => {
                    _modSelection.Remove(mod);
                    _gameState.humanUnit.Get().cargo.minerals -= MineralsPrice(mod);
                    _gameState.humanUnit.Get().cargo.organic -= OrganicPrice(mod);
                    _gameState.humanUnit.Get().cargo.power -= PowerPrice(mod);
                    Candidate().modList.Add(mod.name);
                },
                next = ShopSelectionCard,
            });
        }

        actions.Add(new TQuestAction {
            text = "That should be enough for today.",
            next = () => TQuestCard.exitQuestEnterMap,
        });

        return new TQuestCard {
            text = selectionText,
            actions = actions,
        };
    }

    private TQuestCard InstallRandomCard() {
        var mod = QRandom.Element(_modSelection);
        _modSelection.Remove(mod);

        _gameState.modTraderState.artifacts.Add(_wantArtifact);
        _priceMultiplier -= DISCOUNT_PER_ARTIFACT;
        Candidate().modList.Add(mod.name);
        
        var modInfo = VesselMod.modByName[mod.name];
        var description = string.Join(", ", modInfo.GetEffects());
        var art = _wantArtifact;
        _wantArtifact = "";
        return new TQuestCard{
            text = ($@"
                Receiving {art} scans... Done.

                Let me see, what should I pick for your reward?

                Oh, yes! I'll install {mod.name} for you.

                It's a good one, really: {description}.
            "),
            actions = GetRootActions(),
        };
    }

    private TQuestCard AskForSwapCard() {
        _canAskToSwap = false;
        _priceMultiplier += 0.2f;
        var roll = QRandom.Float();
        if (roll < 0.5) {
            return RerolledCandidateCard();
        }
        return FailedRerollCard();
    }

    private TQuestCard RerolledCandidateCard() {
        var newCandidate = QRandom.IntRange(0, GetPlayerSpaceUnit().fleet.Count - 1);
        if (newCandidate == _candidate) {
            return FailedRerollCard();
        }
        _candidate = newCandidate;
        return new TQuestCard {
            text = ($@"
                Ugh, OK.

                Let it be {Candidate().pilotName} then.
            "),
            actions = GetRootActions(),
        };
    }

    private TQuestCard FailedRerollCard() {
        return new TQuestCard {
            text = ($@"
                No!

                I've made my choice.

                You get to choose what mods we're installing,
                but not which vessel I'm going to tinker.
            "),
            actions = GetRootActions(),
        };
    }

    private TQuestCard DialogueRoot() {
        return new TQuestCard {
            actions = GetRootActions(),
        };
    }

    private void MaybeWantArtifact() {
        foreach (var art in _gameState.artifactsRecovered) {
            if (_gameState.modTraderState.artifacts.Contains(art)) {
                continue;
            }
            _wantArtifact = art;
            break;
        }
    }

    private void RollMods() {
        var allOptions = new List<Mod>{
            // Level 1.

            new Mod{
                name = "Reinforced Hull",
                chance = 0.4f,
                minerals = 10,
                organic = 5,
            },
            new Mod{
                name = "Extra Armor",
                chance = 0.3f,
                minerals = 10,
                organic = 20,
            },
            new Mod{
                name = "Shield Booster",
                chance = 0.45f,
                organic = 30,
                power = 5,
            },
            new Mod{
                name = "Customized Power System",
                chance = 0.5f,
                power = 30,
            },
            new Mod{
                name = "Engine Throttler",
                chance = 0.35f,
                minerals = 50,
            },
            new Mod{
                name = "Star Heat Resistor",
                chance = 0.25f,
                minerals = 5,
                power = 5,
            },
            new Mod{
                name = "Extended Storage",
                chance = 0.4f,
                minerals = 45,
                organic = 10,
            },
            new Mod{
                name = "Alternative Cooling System",
                chance = 0.45f,
                organic = 25,
            },
            new Mod{
                name = "Energy Deviator",
                chance = 0.55f,
                power = 55,
            },
            new Mod{
                name = "Heat Deviator",
                chance = 0.55f,
                organic = 50,
                power = 15,
            },
            new Mod{
                name = "Asteroid Blocker",
                chance = 0.5f,
                minerals = 60,
            },

            // Level 2.

            new Mod{
                name = "Unique Alloys",
                chance = 0.2f,
                minerals = 80,
                organic = 40,
            },
            new Mod{
                name = "Anti-Electromagnetic Coating",
                chance = 0.35f,
                minerals = 20,
                organic = 20,
                power = 65,
            },
            new Mod{
                name = "Anti-Kinetic Coating",
                chance = 0.45f,
                minerals = 100,
                organic = 20,
                power = 10,
            },
            new Mod{
                name = "Anti-Thermal Coating",
                chance = 0.4f,
                minerals = 40,
                organic = 30,
                power = 30,
            },
            new Mod{
                name = "Battery Booster",
                chance = 0.65f,
                minerals = 35,
                power = 25,
            },
            new Mod{
                name = "Sentinel Patch: Overclock",
                chance = 0.5f,
                organic = 50,
                power = 10,
            },
            new Mod{
                name = "Sentinel Patch: Berserk",
                chance = 0.3f,
                organic = 75,
                power = 10,
            },
            new Mod{
                name = "Sentinel Patch: Bastion",
                chance = 0.5f,
                organic = 65,
            },
            new Mod{
                name = "Turtle",
                chance = 0.45f,
                minerals = 70,
            },
            new Mod{
                name = "Luck Charm",
                chance = 0.1f,
                organic = 30,
            },
        };

        var num = 3;
        _modSelection = new List<Mod>();
        var selected = new HashSet<string>();

        while (_modSelection.Count < num) {
            var rolled = QRandom.Element(allOptions);
            if (selected.Contains(rolled.name)) {
                continue;
            }
            if (QRandom.Float() >= rolled.chance) {
                continue;
            }
            selected.Add(rolled.name);
            _modSelection.Add(rolled);
        }
    }
}
