using System.Collections.Generic;
using System;

// DEX:
// - higher chance to escape in encounters
// - higher lockpicking chance
//
// STR:
// - melee damage
// - hp regen
//
// INT:
// - magic damage
// - mp regen

public class PogueLikeTQuest : AbstractTQuest {
    enum RoomKind {
        Empty,

        Skeleton,
        Zombie,
        Beast,

        SmallTreasure,
        BigTreasure,

        GoodEvent,
        BadEvent,

        StairsDown,
    }

    class Room {
        public RoomKind kind = RoomKind.Empty;
        public bool visited = false;
        public int row;
        public int col;
    }

    class MonsterInfo {
        public string name;
        public bool undead;
        public int maxHp;
        public int damage;
        public int expReward;
        public float escapeChance;
    }

    class Monster {
        public MonsterInfo info;
        public int hp;

        public Monster(MonsterInfo info) {
            this.info = info;
            hp = info.maxHp;
        }
    }

    class SpellInfo {
        public string name;
        public bool inCombat;
        public int damage = 0;
        public int manaCost;
        public int minInt;
    }

    private Dictionary<RoomKind, MonsterInfo> _monsters = new Dictionary<RoomKind, MonsterInfo>();

    private static List<SpellInfo> _spells = new List<SpellInfo>{
        new SpellInfo{
            name = "Fireball",
            inCombat = true,
            damage = 20,
            manaCost = 20,
            minInt = 10,
        },

        new SpellInfo{
            name = "Frost Ray",
            inCombat = true,
            damage = 5,
            manaCost = 15,
            minInt = 15,
        },

        new SpellInfo{
            name = "Mana Shield",
            inCombat = true,
            manaCost = 10,
            minInt = 15,
        },

        new SpellInfo{
            name = "Banish Undead",
            inCombat = true,
            damage = 35,
            manaCost = 30,
            minInt = 25,
        },

        new SpellInfo{
            name = "Heal",
            inCombat = false,
            damage = 30,
            manaCost = 35,
            minInt = 20,
        }
    };

    private int _hp = 0;
    private int _maxHp = 0;
    private int _mp = 0;
    private int _maxMp = 0;

    private int _str = 0;
    private int _dex = 0;
    private int _int = 0;

    private int _exp = 0;
    private int _level = 1;

    private string _class = "";

    private string _dungeonApproach = "";

    private Room _room;
    private Monster _enemy;

    private int _posColumn = 0;
    private int _posRow = 0;
    private Room[][] _map;

    public PogueLikeTQuest() {
        DeclareValue("Class", () => _class, false);
        DeclareValue("Level", () => _level, false);
        DeclareValue("Experience", () => _exp, false);

        DeclareValue("HP", () => _hp, false);
        DeclareValue("MP", () => _mp, false);

        DeclareValue("STR", () => _str, false);
        DeclareValue("DEX", () => _dex, false);
        DeclareValue("INT", () => _int, false);        

        _monsters.Add(RoomKind.Skeleton, new MonsterInfo{
            name = "Skeleton",
            undead = true,
            maxHp = 35,
            damage = 5,
            expReward = 20,
            escapeChance = 0.4f,
        });

        _monsters.Add(RoomKind.Zombie, new MonsterInfo{
            name = "Zombie",
            undead = true,
            maxHp = 55,
            damage = 6,
            expReward = 30,
            escapeChance = 0.75f,
        });

        _monsters.Add(RoomKind.Beast, new MonsterInfo{
            name = "Beast",
            undead = false,
            maxHp = 70,
            damage = 4,
            expReward = 40,
            escapeChance = 0.55f,
        });
    }

    public override TQuestCard GetFirstCard() {
        return MainMenu();
    }

    private void ShowStats() {
        SetValueVisibility("STR", true);
        SetValueVisibility("DEX", true);
        SetValueVisibility("INT", true);
        SetValueVisibility("HP", true);
        SetValueVisibility("MP", true);
        SetValueVisibility("Level", true);
        SetValueVisibility("Experience", true);
        SetValueVisibility("Class", true);
    }

    private TQuestCard MainMenu() {
        return new TQuestCard {
            image = "Computer",
            text = (@"
                A shiny (but black and white) screen welcomes you, proudly showing the
                title of the game: ""Pogue"".

                It's believed that this game created its own genre at that time called
                Pogue-like. Many titles claimed that they're Pogue-like, meaning ""they're so good"".

                The main menu gives you two options: start a new game or read the
                integrated game manual.
            "),
            actions = {
                new TQuestAction{
                    text = "Start the game.",
                    next = CreateCharacter,
                },
                new TQuestAction{
                    text = "Read the game manual.",
                }
            },
        };
    }

    private TQuestCard CreateCharacter() {
        return new TQuestCard {
            image = "Dark_Room",
            text = (@"
                The game expects you to choose one of the 3 available character classes.

                A [u]warrior[/u] that is strong but really bad at magic.

                A [u]mage[/u] that is physically weak, but has a powerfull magic.

                A [u]rogue[/u] that is a mix of the two.
            "),
            actions = {
                new TQuestAction{
                    text = "Read about the warrior class.",
                    next = WarriorSelection,
                },
                new TQuestAction{
                    text = "Read about the mage class.",
                    next = MageSelection,
                },
                new TQuestAction{
                    text = "Read about the rogue class.",
                    next = RogueSelection,
                },
                new TQuestAction{
                    text = "Back to the main menu.",
                    next = MainMenu,
                }
            },
        };
    }

    private TQuestCard WarriorSelection() {
        return new TQuestCard {
            image = "Warrior_Class",
            text = (@"
                Warriors use physical strength do defeat their foes.

                Strength: 20 (STR)\n
                Dexterity: 15 (DEX)\n
                Intelligence: 5 (INT)\n
                HP: 100\n
                MP: 10
            "),
            actions = {
                new TQuestAction{
                    text = "Pick warrior.",
                    apply = () => {
                        _class = "Warrior";
                        _str = 20;
                        _dex = 15;
                        _int = 5;
                        _maxHp = 100;
                        _maxMp = 10;
                        _hp = _maxHp;
                        _mp = _maxMp;
                        ShowStats();
                    },
                    next = EnterForest,
                },
                new TQuestAction{
                    text = "Back to the class selection.",
                    next = CreateCharacter,
                },
            },
        };
    }

    private TQuestCard RogueSelection() {
        return new TQuestCard {
            image = "Rogue_Class",
            text = (@"
                Rogues try to be adaptive.
                They're jack of all trades, but masters of none.

                Strength: 10 (STR)\n
                Dexterity: 20 (DEX)\n
                Intelligence: 10 (INT)\n
                HP: 75\n
                MP: 50
            "),
            actions = {
                new TQuestAction{
                    text = "Pick rogue.",
                    apply = () => {
                        _class = "Rogue";
                        _str = 10;
                        _dex = 20;
                        _int = 10;
                        _maxHp = 75;
                        _maxMp = 50;
                        _hp = _maxHp;
                        _mp = _maxMp;
                        ShowStats();
                    },
                    next = EnterForest,
                },
                new TQuestAction{
                    text = "Back to the class selection.",
                    next = CreateCharacter,
                },
            },
        };
    }

    private TQuestCard MageSelection() {
        return new TQuestCard {
            image = "Mage_Class",
            text = (@"
                Mages rely on magic to handle any situation.

                Strength: 5 (STR)\n
                Dexterity: 15 (DEX)\n
                Intelligence: 20 (INT)\n
                HP: 40\n
                MP: 100
            "),
            actions = {
                new TQuestAction{
                    text = "Pick mage.",
                    apply = () => {
                        _class = "Mage";
                        _str = 5;
                        _dex = 15;
                        _int = 20;
                        _maxHp = 40;
                        _maxMp = 100;
                        _hp = _maxHp;
                        _mp = _maxMp;
                        ShowStats();
                    },
                    next = EnterForest,
                },
                new TQuestAction{
                    text = "Back to the class selection.",
                    next = CreateCharacter,
                },
            },
        };
    }

    private TQuestCard EnterForest() {
        return new TQuestCard {
            image = "Forest",
            text = (@"
                Your character appears somewhere in a forest.
                You see a road ahead and you feel the urge to follow it.

                What's this place? Why do you want to follow that road?
                These are the questions that have not visited your head,
                so you started the journey.
            "),
            actions = {
                new TQuestAction{
                    text = "Follow the road.",
                    next = EntranceGates,
                },
            },
        };
    }

    private TQuestCard EntranceGates() {
        return new TQuestCard {
            image = "Gates",
            text = (@"
                The samurai has no goal, only path.
                And this path led to a creepy looking dungeon.

                The gates stand before you; they probably hide a lot
                of skeletons in the closet (the literal ones, with
                swords and shields).

                This non-linear game gives you two approaches:
                choose the one that fits your playstyle more.

                As you'll see, this game series adapts to the choices you make.
            "),
            actions = {
                new TQuestAction{
                    text = "Enter the dungeon with courage.",
                    apply = () => EnterDungeon("courage"),
                    next = DungeonEntrance,
                },
                new TQuestAction{
                    text = "Enter the dungion with caution.",
                    apply = () => EnterDungeon("caution"),
                    next = DungeonEntrance,
                }
            },
        };
    }

    private void EnterDungeon(string approach) {
        _dungeonApproach = approach;
        GenerateMap();
    }

    private TQuestCard DungeonEntrance() {
        return new TQuestCard {
            image = "Sanctuary",
            text = ($@"
                As you enter the dungeon with {_dungeonApproach}, you realize that there
                is no coming back. The game doesn't give you that option.

                You came to this place with one goal: to dwell deeper into the dungeon.

                {DrawMap()}
            "),
            actions = GetGenericActions(),
        };
    }

    private TQuestCard TryResting() {
        return new TQuestCard {
            image = "Sewers",
            text = ($@"
                This place is definitely not the most relaxing one,
                but you managed to get some rest.
            "),
            actions = {
                new TQuestAction{
                    text = "Do something else.",
                    next = ChooseGenericAction,
                },
            },
        };
    }

    private bool CanRestHere() {
        return _room.kind != RoomKind.Skeleton &&
            _room.kind != RoomKind.Zombie;
    }

    private List<TQuestAction> GetGenericActions() {
        var list = new List<TQuestAction>();
        list.Add(new TQuestAction{
            text = "Let's move.",
            next = ChooseMapMovement,
        });
        if (CanRestHere()) {
            list.Add(new TQuestAction{
                text = "Try to rest.",
                next = TryResting,
            });
        }
        return list;
    }

    private TQuestCard ChooseMapMovement() {
        var description = QRandom.Element(new List<string>{
            "It's time to choose where we're going.",
            "Do we have some kind of a tactic?",
            "We're lost if you'll ask me. Make your move.",
            "Let's choose a safe path.",
            "Let's choose even safer path.",
            "Are you taking notes?",
            "What is your next action?",
            "Where to?",
            "Pick a way to go.",
            "Your turn.",
        });

        return new TQuestCard {
            image = "Map",
            text = ($@"
                {description}

                {DrawMap()}
            "),
            actions = GetMovementActions(),
        };
    }

    private TQuestCard ChooseGenericAction() {
        return new TQuestCard {
            image = "Sanctuary",
            text = ($@"
                We've made some progress.

                It's time to decide what we're doing next.

                {DrawMap()}
            "),
            actions = GetGenericActions(),
        };
    }

    private TQuestCard HandleMovement() {
        _room = _map[_posRow][_posColumn];
        var firstVisit = !_room.visited;
        _room.visited = true;

        bool isReentrant = _room.kind == RoomKind.StairsDown ||
            _room.kind == RoomKind.Skeleton ||
            _room.kind == RoomKind.Zombie;

        if (!firstVisit && !isReentrant) {
            return ChooseMapMovement();
        }

        if (_room.kind == RoomKind.Empty) {
            return EnterEmptyRoom();
        }
        if (_room.kind == RoomKind.StairsDown) {
            return EnterStairsRoom();
        }

        if (_room.kind == RoomKind.Skeleton) {
            _enemy = new Monster(_monsters[_room.kind]);
            return EncounterSkeleton();
        }

        throw new Exception("unhandled room kind: " + _room.kind);
    }

    private TQuestCard TryEscaping() {
        var escapeRoll = QRandom.Float() + ((float)_dex / 100);
        Godot.GD.Print("roll = " + escapeRoll);
        if (_room.kind == RoomKind.Skeleton) {
            if (QRandom.Float() < 0.4) {
                return EscapingSuccess();
            }
            return EscapingFailure();
        }
        if (_room.kind == RoomKind.Zombie) {
            if (QRandom.Float() < 0.75) {
                return EscapingSuccess();
            }
            return EscapingFailure();
        }

        throw new Exception("unhandled room kind: " + _room.kind);
    }

    private TQuestCard EscapingSuccess() {
        return new TQuestCard{
            image = "Sewers_Mirrored",
            text = ($@"
                You managed to escape the creature.
            "),
            actions = GetGenericActions(),
        };
    }

    private TQuestCard EscapingFailure() {
        return new TQuestCard{
            text = ($@"
                You failed to escape the creature.
            "),
            actions = {
                new TQuestAction{
                    text = "Fight.",
                    next = CombatEnemyTurn,
                },
            },
        };
    }

    private TQuestCard CombatEnemyTurn() {
        if (_room.kind == RoomKind.Skeleton) {
            var description = QRandom.Element(new List<string>{
                "Skeleton swings its sword and deals ...",
            });
            return new TQuestCard{
                text = ($@"
                    {description}
                "),
                actions = {
                    new TQuestAction{
                        text = "Start your turn.",
                        next = CombatPlayerTurn,
                    },
                },
            };
        }
        throw new Exception("unexpected enemy kind: " + _room.kind);
    }

    private TQuestCard CombatPlayerWins() {
        _exp += _enemy.info.expReward;
        return new TQuestCard{
            text = ($@"
                The [u]{_enemy.info.name}[/u] is defeated!

                You earned [u]{_enemy.info.expReward}[/u] experience.
            "),
            actions = {
                new TQuestAction{
                    text = "Continue your adventure.",
                    next = ChooseGenericAction,
                },
            },
        };
    }

    private TQuestCard CombatPlayerAttacks(int damage) {
        return new TQuestCard{
            text = ($@"
                Your attack dealt [u]{damage}[/u] damage.
            "),
            actions = {
                new TQuestAction{
                    text = "End turn.",
                    next = CombatEnemyTurn,
                },
            },
        };
    }

    private TQuestCard CombatPlayerTurn() {
        return new TQuestCard{
            text = ($@"
                It's your turn. What would you do?

                The [u]{_enemy.info.name}[/u] has [u]{_enemy.hp}[/u]/{_enemy.info.maxHp} hit points.
            "),
            actions = {
                new TQuestAction{
                    text = "Attack in melee.",
                    next = () => {
                        var damage = 3 + QRandom.IntRange(0, _str / 2);
                        _enemy.hp = QMath.ClampMin(_enemy.hp - damage, 0);
                        if (_enemy.hp == 0) {
                            _room.kind = RoomKind.Empty;
                            return CombatPlayerWins();
                        }
                        return CombatPlayerAttacks(damage);
                    },
                },
                new TQuestAction{
                    text = "Cast a spell.",
                },
                new TQuestAction{
                    text = "Use an item.",
                },
                new TQuestAction{
                    text = "Inspect your enemy.",
                },
            },
        };
    }

    private TQuestCard EncounterSkeleton() {
        return new TQuestCard{
            image = "Skeleton_Indoors",
            text = ($@"
                You encountered a skeleton!
            "),
            actions = {
                new TQuestAction{
                    text = "Fight.",
                    next = CombatPlayerTurn,
                },
                new TQuestAction{
                    text = "Try to escape.",
                    next = TryEscaping,
                }
            },
        };
    }

    private TQuestCard EnterStairsRoom() {
        return new TQuestCard {
            image = "Dark_Tomb",
            text = ($@"
                You found a dark passage that leads to the next floor of this dungeon.

                Would you like to enter it now?

                {DrawMap()}
            "),
            actions = {
                new TQuestAction{
                    text = "Enter the passage.",
                },
                new TQuestAction{
                    text = "No, let's explore this floor more.",
                    next = ChooseGenericAction,
                }
            },
        };
    }

    private TQuestCard EnterEmptyRoom() {
        var description = QRandom.Element(new List<string>{
            "Looks like this room has nothing of merit.",
            "You entered an empty room.",
        });

        return new TQuestCard {
            image = "Dark_Room",
            text = ($@"
                {description}

                {DrawMap()}
            "),
            actions = GetGenericActions(),
        };
    }

    private List<TQuestAction> GetMovementActions() {
        var list = new List<TQuestAction>();
        if (_posColumn != 0) {
            list.Add(new TQuestAction{
                text = "Go west (left).",
                apply = () => {
                    _posColumn--;
                },
                next = HandleMovement,
            });
        }
        if (_posColumn != 3) {
            list.Add(new TQuestAction{
                text = "Go east (right).",
                apply = () => {
                    _posColumn++;
                },
                next = HandleMovement,
            });
        }
        if (_posRow != 0) {
            list.Add(new TQuestAction{
                text = "Go north (up).",
                apply = () => {
                    _posRow--;
                },
                next = HandleMovement,
            });
        }
        if (_posRow != 3) {
            list.Add(new TQuestAction{
                text = "Go south (down).",
                apply = () => {
                    _posRow++;
                },
                next = HandleMovement,
            });
        }
        list.Add(new TQuestAction{
            text = "Enough movement for now.",
            next = ChooseGenericAction,
        });
        return list;
    }

    private void GenerateMap() {
        _map = new Room[4][];
        for (int i = 0; i < _map.Length; i++) {
            _map[i] = new Room[4];
            for (int j = 0; j < _map[i].Length; j++) {
                _map[i][j] = new Room{row = i, col = j};
            }
        }

        _map[0][0].visited = true;
        _room = _map[0][0];

        // Deploy the stairs.
        if (QRandom.Bool()) {
            var x = QRandom.IntRange(0, 3);
            _map[3][x].kind = RoomKind.StairsDown;
        } else {
            var x = QRandom.IntRange(0, 3);
            _map[x][3].kind = RoomKind.StairsDown;
        }

        _map[0][1].kind = RoomKind.Skeleton;
    }

    private string DrawMap() {
        Func<Room, string> roomToString = (Room room) => {
            if (!room.visited) {
                return "?";
            }
            if (room.col == _posColumn && room.row == _posRow) {
                return "*";
            }
            if (room.kind == RoomKind.Empty) {
                return ".";
            }
            if (room.kind == RoomKind.StairsDown) {
                return "#";
            }
            if (room.kind == RoomKind.Skeleton) {
                return "S";
            }
            if (room.kind == RoomKind.Zombie) {
                return "Z";
            }
            if (room.kind == RoomKind.Beast) {
                return "B";
            }
            throw new Exception("unexpected room kind: " + room.kind);
        };

        var lines = new List<string>();
        for (int row = 0; row < _map.Length; row++) {
            var parts = new List<string>();
            for (int col = 0; col < _map[row].Length; col++) {
                parts.Add(roomToString(_map[row][col]));
            }
            lines.Add(string.Join(" ", parts) + "\\n");
        }
        return string.Join("\n", lines);
    }
}
