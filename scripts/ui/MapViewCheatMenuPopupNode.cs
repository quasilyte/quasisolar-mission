using Godot;
using System;

public class MapViewCheatMenuPopupNode : PopupDialog {
    public enum CommandKind {
        StatsChange,
        CurrentSystemChange,
        CallRandomEvent,
        ResearchComplete,
        RevealMap,
    }

    public class Command : Godot.Object {
        public CommandKind kind;
        public object value;
    }

    [Signal]
    public delegate void CommandExecuted(Command command);

    private RpgGameState _gameState;

    private Label _textLog;

    private Command _command;

    public override void _Ready() {
        _gameState = RpgGameState.instance;
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

            case "cheat.map.reveal":
                _command = new Command { kind = CommandKind.RevealMap };
                Log($"Map revealed");
                return;

            case "cheat.exp": {
                var value = ParseInt(arg);
                foreach (var vessel in _gameState.humanUnit.Get().fleet) {
                    vessel.Get().exp = QMath.ClampMin(vessel.Get().exp + value, 0);
                }
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Fleet vessels exp value changed");
                return;
            }

            case "cheat.credits":
                _gameState.credits = QMath.ClampMin(ParseInt(arg) + _gameState.credits, 0);
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Credits value changed");
                return;
            case "cheat.fuel":
                RpgGameState.AddFuel(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Fuel value changed");
                return;
            case "cheat.minerals":
                _gameState.humanUnit.Get().CargoAddMinerals(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Minerals cargo value changed");
                return;
            case "cheat.organic":
                _gameState.humanUnit.Get().CargoAddOrganic(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Organic cargo value changed");
                return;
            case "cheat.power":
                _gameState.humanUnit.Get().CargoAddPower(ParseInt(arg));
                _command = new Command { kind = CommandKind.StatsChange };
                Log($"Power cargo value changed");
                return;

            case "cheat.event":
                var e = FindRandomEvent(arg);
                if (e == null) {
                    throw new Exception($"event {arg} not found");
                }
                _command = new Command { kind = CommandKind.CallRandomEvent, value = e };
                Log($"Triggered {e.title} event");
                return;

            case "cheat.research.complete":
                if (_gameState.currentResearch == "") {
                    throw new Exception("current research is not set");
                }
                _command = new Command { kind = CommandKind.ResearchComplete };
                return;

            case "cheat.base.level": {
                    StarSystem sys;
                    if (!RpgGameState.starSystemByPos.TryGetValue(_gameState.humanUnit.Get().pos, out sys)) {
                        throw new Exception("located outside of a system");
                    }
                    if (sys.starBase.id == 0) {
                        throw new Exception("system has no star bases");
                    }
                    sys.starBase.Get().level = QMath.Clamp(sys.starBase.Get().level + ParseInt(arg), 1, 5);
                    Log("Star base level changed");
                    _command = new Command { kind = CommandKind.CurrentSystemChange };
                    return;
                }

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

    public AbstractMapEvent FindRandomEvent(string name) {
        name = name.ToLower();
        foreach (var e in MapEventRegistry.list) {
            if (e.title.ToLower() == name) {
                return e;
            }
        }
        return null;
    }
}
