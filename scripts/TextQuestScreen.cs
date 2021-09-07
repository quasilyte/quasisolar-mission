using Godot;
using System.Collections.Generic;

public class TextQuestScreen : Node2D {
    class ActionLabel {
        public int id;
        public Label node;
        public bool disabled;
    }

    private AbstractTQuest _quest;
    private TQuestCard _card;

    private ActionLabel[] _actionLabels;
    private int[] _actionIndexMap;
    private ActionLabel _hoveredAction = null;

    private AudioStream _clickSound;

    public override void _Ready() {
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        QRandom.SetRandomNumberGenerator(rng);

        GameControls.InitInputMap();

        _clickSound = GD.Load<AudioStream>("res://audio/interface/textclick.wav");

        _actionIndexMap = new int[TQuestCard.MAX_ACTIONS];
        _actionLabels = new ActionLabel[TQuestCard.MAX_ACTIONS];
        for (int i = 0; i < TQuestCard.MAX_ACTIONS; i++) {
            _actionLabels[i] = new ActionLabel{
                id = i,
                node = GetNode<Label>($"ActionsPanel/Action{i+1}"),
                disabled = false,
            };
            var args = new Godot.Collections.Array{i};
            _actionLabels[i].node.Connect("mouse_entered", this, nameof(OnActionMouseEntered), args);
            _actionLabels[i].node.Connect("mouse_exited", this, nameof(OnActionMouseExited), args);
        }

        if (RpgGameState.instance == null) {
            var gameState = new RpgGameState();
            gameState.day = 100;
            gameState.missionDeadline = 5000;
            RpgGameState.instance = gameState;

            gameState.starSystems = new ObjectPool<StarSystem>();
            var system = gameState.starSystems.New();
            system.name = "Quasisolar";
            RpgGameState.humanBases = new HashSet<StarBase>{
                new StarBase{
                    discoveredByKrigia = 40,
                    system = system.GetRef(),
                },
            };
        }
        if (RpgGameState.selectedTextQuest != null) {
            _quest = RpgGameState.selectedTextQuest;
        }
        // _quest = new PogueLikeTQuest();
        // _quest = new KrigiaPatrolTQuest();
        UpdateCard(_quest.GetFirstCard());
    }

    private void HotkeyExecutingAction(int i) {
        if (!Input.IsKeyPressed((int)KeyList.Control)) {
            return;
        }
        var label = _actionLabels[i];
        if (label.disabled || !label.node.Visible) {
            return;
        }
        ExecuteAction(_card.actions[_actionIndexMap[i]]);
    }

    public override void _Input(InputEvent e) {
        if (Input.IsActionJustPressed("tquestAction1")) {
            HotkeyExecutingAction(0);
        }
        if (Input.IsActionJustPressed("tquestAction2")) {
            HotkeyExecutingAction(1);
        }
        if (Input.IsActionJustPressed("tquestAction3")) {
            HotkeyExecutingAction(2);
        }
        if (Input.IsActionJustPressed("tquestAction4")) {
            HotkeyExecutingAction(3);
        }
        if (Input.IsActionJustPressed("tquestAction5")) {
            HotkeyExecutingAction(4);
        }

        if (e is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed) {
                if (_hoveredAction != null && !_hoveredAction.disabled) {
                    ExecuteAction(_card.actions[_hoveredAction.id]);
                }
            }
        }
    }

    private void ExecuteAction(TQuestAction action) {
        action.apply();
        UpdateCard(action.next());
        GetParent().AddChild(SoundEffectNode.New(_clickSound, -12));
        _hoveredAction = null;
    }

    private void OnActionMouseEntered(int id) {
        var a = _actionLabels[_actionIndexMap[id]];
        _hoveredAction = a;
        if (!a.disabled) {
            _actionLabels[id].node.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
        }
    }

    private void OnActionMouseExited(int id) {
        var a = _actionLabels[_actionIndexMap[id]];
        _hoveredAction = null;
        if (!a.disabled) {
            _actionLabels[id].node.AddColorOverride("font_color", Color.Color8(255, 255, 255));
        }
    }

    private void UpdateCard(TQuestCard card) {
        if (card == TQuestCard.exitQuestEnterArena) {
            GetTree().ChangeScene("res://scenes/ArenaScreen.tscn");
            return;
        }
        if (card == TQuestCard.exitQuestEnterMap) {
            GetTree().ChangeScene("res://scenes/MapView.tscn");
            return;
        }

        _card = card;

        if (_card.text != null) {
            GetNode<RichTextLabel>("TextPanel/TextBackground/Content").BbcodeText = FormatCardText(_card);
        }

        for (int i = 0; i < TQuestCard.MAX_ACTIONS; i++) {
            _actionLabels[i].node.Visible = false;
        }
        int j = 0;
        for (int i = 0; i < card.actions.Count; i++) {
            var a = card.actions[i];
            _actionIndexMap[j] = i;

            var label = _actionLabels[j];
            label.node.Text = $"{j+1}. " + a.text;
            if (a.cond()) {
                label.disabled = false;
                label.node.AddColorOverride("font_color", Color.Color8(255, 255, 255));
                label.node.Visible = true;
                j++;
            } else {
                label.disabled = true;
                label.node.AddColorOverride("font_color", Color.Color8(0x5d, 0x60, 0x79));
                if (TQuestSettings.showUnavailable) {
                    label.node.Visible = true;
                    j++;
                }
            }
        }

        var statusLines = new List<string>();
        foreach (var v in _quest.values) {
            if (!v.show) {
                continue;
            }
            statusLines.Add(v.name + ": " + v.getter());
        }
        GetNode<Label>("StatusPanel/Box/Content").Text = string.Join("\n", statusLines);

        if (card.image != "") {
            GetNode<Sprite>("PicPanel/Sprite").Texture = GD.Load<Texture>($"res://images/tquest/{card.image}.jpg");
        }
    }

    private string FormatCardText(TQuestCard card) {
        return Utils.FormatMultilineText(card.text);
    }
}
