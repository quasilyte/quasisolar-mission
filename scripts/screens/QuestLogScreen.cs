using Godot;
using System;

public class QuestLogScreen : Node2D {
    public override void _Ready() {
        SetupUI();

        UpdateUI();
    }

    private void SetupUI() {
        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));
    }

    private void UpdateUI() {
        var list = GetNode<VBoxContainer>("ActiveQuests/ScrollContainer/List");
        foreach (var q in RpgGameState.instance.activeQuests) {
            var entry = QuestLogEntryNode.New();
            var isCompleted = Quest.CheckRequirements(RpgGameState.instance, q) != null;
            var status = isCompleted ? "Ready to be completed" : "In progress";
            var description = Utils.FormatMultilineText(RarilouQuests.Find(q.name).logEntryText(q));
            var nameLabel = entry.GetNode<Label>("VBoxContainer/Name");
            nameLabel.Text = "# " + q.name;
            nameLabel.AddColorOverride("font_color", Utils.FactionColor(q.faction));
            entry.GetNode<Label>("VBoxContainer/Status").Text = status;
            entry.GetNode<Label>("VBoxContainer/MarginContainer/Description").Text = description;
            list.AddChild(entry);
        }
    }

    private void OnLeaveButton() {
        RpgGameState.transition = RpgGameState.MapTransition.ExitQuestLogScreen;
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }
}