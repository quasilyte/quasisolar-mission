using Godot;
using System;

public class MapViewCheatMenuPopup : PopupDialog {
    public enum CommandKind {
        StatsChange,
        CurrentSystemChange,
        CallRandomEvent,
        ResearchComplete,
    }

    public class Command : Godot.Object {
        public CommandKind kind;
        public object value;
    }

    [Signal]
    public delegate void CommandExecuted(Command command);

    private Label _textLog;

    private Command _command;

    public override void _Ready() {
        _textLog = GetNode<Label>("ScrollContainer/MarginContainer/Label");

        GetNode<ButtonNode>("Execute").Connect("pressed", this, nameof(OnExecute));
        GetNode<ButtonNode>("ClearLogs").Connect("pressed", this, nameof(OnClearLogs));
    }

    private void OnClearLogs() {
        _textLog.Text = "";
    }

    private void OnExecute() {
        _command = null;
        var text = GetNode<LineEdit>("Input").Text;
        try {
            ExecuteCommand(text);
            if (_command != null) {
                EmitSignal(nameof(CommandExecuted), new object[] { _command });
            }
        } catch (Exception e) {
            Log("Error: " + e.Message);
        }
    }

    private void Log(string s) {
        _textLog.Text += "> " + s + "\n";
    }

    private void ExecuteCommand(string text) {
        string op = "";
        string arg = "";
        var parts = text.Split(new string[] { " " }, 2, StringSplitOptions.None);
        if (parts.Length != 2) {
            op = text;
        } else {
            op = parts[0];
            arg = parts[1];
        }

        switch (op) {
            case "print":
                Log(arg);
                return;

            case "help":
                Log("Press 'Done' to close this window");
                Log("Press 'Clear Logs' to clear this text box");
                Log("Press 'Execute' to run the command from the input");
                return;

            case "cheat.exp":
                RpgGameState.experience = QMath.ClampMin(ParseInt(arg) + RpgGameState.experience, 0);
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Experience value changed");
                return;
            case "cheat.credits":
                RpgGameState.credits = QMath.ClampMin(ParseInt(arg) + RpgGameState.credits, 0);
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Credits value changed");
                return;
            case "cheat.fuel":
                RpgGameState.AddFuel(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Fuel value changed");
                return;
            case "cheat.minerals":
                RpgGameState.humanUnit.CargoAddMinerals(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Minerals cargo value changed");
                return;
            case "cheat.organic":
                RpgGameState.humanUnit.CargoAddOrganic(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Organic cargo value changed");
                return;
            case "cheat.power":
                RpgGameState.humanUnit.CargoAddPower(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Power cargo value changed");
                return;

            case "cheat.event":
                var e = FindRandomEvent(arg);
                if (e == null) {
                    throw new Exception($"event {arg} not found");
                }
                if (!e.condition()) {
                    throw new Exception($"{arg} event condition is not satisfied");
                }
                _command = new Command { kind = CommandKind.CallRandomEvent, value = e };
                Log($"Triggered {e.title} event");
                return;

            case "cheat.research.complete":
                if (RpgGameState.currentResearch == null) {
                    throw new Exception("current research is not set");
                }
                _command = new Command { kind = CommandKind.ResearchComplete };
                return;

            case "cheat.base.level": {
                    StarSystem sys;
                    if (!RpgGameState.starSystemByPos.TryGetValue(RpgGameState.humanUnit.pos, out sys)) {
                        throw new Exception("located outside of a system");
                    }
                    if (sys.starBase == null) {
                        throw new Exception("system has no star bases");
                    }
                    sys.starBase.level = QMath.Clamp(sys.starBase.level + ParseInt(arg), 1, 5);
                    Log("Star base level changed");
                    _command = new Command { kind = CommandKind.CurrentSystemChange };
                    return;
                }

            case "cheat.skill":
                var skill = FindSkill(arg);
                if (skill == null) {
                    throw new Exception($"skill {arg} not found");
                }
                if (!skill.IsAvailable()) {
                    throw new Exception($"{skill.name} skill requirements are not satisfied");
                }
                if (RpgGameState.skillsLearned.Contains(skill.name)) {
                    throw new Exception($"{skill.name} is already learned");
                }
                RpgGameState.skillsLearned.Add(skill.name);
                Log($"Learned {skill.name} skill");
                _command = new Command { kind = CommandKind.StatsChange };
                return;

            default:
                throw new Exception($"unknown command {op}");
        }
    }

    private int ParseInt(string s) { return (int)ParseLong(s); }

    private long ParseLong(string s) {
        try {
            return Int64.Parse(s);
        } catch (Exception) {
            return 0;
        }
    }

    public Skill FindSkill(string name) {
        name = name.ToLower();
        foreach (var s in Skill.list) {
            if (s.name.ToLower() == name) {
                return s;
            }
        }
        return null;
    }

    public RandomEvent FindRandomEvent(string name) {
        name = name.ToLower();
        foreach (var e in RandomEvent.list) {
            if (e.title.ToLower() == name) {
                return e;
            }
        }
        return null;
    }
}
