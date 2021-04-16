using Godot;
using System.Collections.Generic;
using System;

public class SkillsScreen : Node2D {
    class SkillNode {
        public Skill value;
        public SkillNode child;

        public Label label;
        public ButtonNode button;
    }

    private List<SkillNode> skillNodes = new List<SkillNode>();

    public override void _Ready() {
        SetupUI();
        UpdateUI();
    }

    private void SetupUI() {
        GetNode<Button>("Status/LeaveButton").Connect("pressed", this, nameof(OnLeaveButton));

        var pass = 1;
        var added = 0;
        var roots = new List<SkillNode>();
        var nodeByName = new Dictionary<string, SkillNode>();

        Func<Skill, SkillNode> addNode = (Skill s) => {
            var n = new SkillNode{value = s};
            nodeByName.Add(s.name, n);
            added++;
            return n;
        };

        while (true) {
            foreach (var s in Skill.list) {
                if (nodeByName.ContainsKey(s.name)) {
                    continue;
                }
                if (s.requires == "" ) {
                    var n = addNode(s);
                    roots.Add(n);
                    continue;
                }
                SkillNode parent;
                if (nodeByName.TryGetValue(s.requires, out parent)) {
                    var n = addNode(s);
                    parent.child = n;
                }
            }
            if (added == 0) {
                break;
            }
            if (pass > 10) {
                throw new Exception("can't build a skill tree");
            }
            added = 0;
            pass++;
        }

        roots.Sort((x, y) => x.value.name.CompareTo(y.value.name));

        var skillTree = GetNode<Panel>("Skills");
        var offsetY = 64;

        Func<float, SkillNode, bool> deploySkill = null;
        deploySkill = (float offsetX, SkillNode s) => {
            var skillLabel = new Label();
            skillLabel.Text = s.value.name;
            skillLabel.RectSize = new Vector2(256, 32);
            skillLabel.Valign = Label.VAlign.Center;
            skillLabel.RectPosition = new Vector2(offsetX, offsetY);
            skillLabel.MouseFilter = Control.MouseFilterEnum.Stop;
            skillTree.AddChild(skillLabel);

            var skillButton = ButtonNode.New();
            skillButton.Text = "+";
            skillButton.RectSize = new Vector2(32, 32);
            skillButton.RectPosition = new Vector2(-48, 0);
            skillLabel.AddChild(skillButton);

            s.label = skillLabel;
            s.button = skillButton;
            skillNodes.Add(s);

            offsetY += 48;

            if (s.child != null) {
                deploySkill(offsetX + 32, s.child);
            }

            return true;
        };

        
        foreach (var s in roots) {
            deploySkill(80, s);
        }

        for (int i = 0; i < skillNodes.Count; i++) {
            skillNodes[i].button.Connect("pressed", this, nameof(OnLearnSkillButton), new Godot.Collections.Array{i});
            skillNodes[i].label.Connect("mouse_entered", this, nameof(OnSkillHover), new Godot.Collections.Array{i});
        }
    }

    private void OnSkillHover(int skillNodeIndex) {
        var s = skillNodes[skillNodeIndex];
        var text = s.value.name;
        if (s.value.IsLearned()) {
            text += " (Learned)\n\n";
        } else {
            text += $"\n\nRequires {s.value.expCost} exp to be learned.\n";
            if (!s.value.IsAvailable()) {
                text += $"Requires {s.value.requires} to be learned first.\n";
            }
            text += "\n";
        }
        text += "Effect: " + s.value.effect + "\n";
        if (!s.value.effect2.Empty()) {
            text += "Effect: " + s.value.effect2 + "\n";
        }
        GetNode<Label>("SkillInfo/InfoBox/Body").Text = text;
    }

    private void OnLearnSkillButton(int skillNodeIndex) {
        var s = skillNodes[skillNodeIndex];
        if (RpgGameState.instance.experience < s.value.expCost) {
            return;
        }
        RpgGameState.instance.experience -= s.value.expCost;
        RpgGameState.instance.skillsLearned.Add(s.value.name);
        UpdateUI();
    }

    private void UpdateUI() {
        GetNode<Label>("Status/ExperienceValue").Text = RpgGameState.instance.experience.ToString();

        foreach (var s in skillNodes) {
            s.label.AddColorOverride("font_color", Color.Color8(255, 255, 255));
            s.button.Disabled = false;

            if (s.value.IsLearned()) {
                s.label.AddColorOverride("font_color", Color.Color8(0x34, 0x8c, 0x2b));
                s.button.Disabled = true;
                continue;
            }
            if (!s.value.IsAvailable()) {
                s.label.AddColorOverride("font_color", Color.Color8(0x5d, 0x60, 0x79));
                s.button.Disabled = true;
                continue;
            }
            if (RpgGameState.instance.experience < s.value.expCost) {
                s.button.Disabled = true;
                continue;
            }
        }
    }

    private void OnLeaveButton() {
        GetTree().ChangeScene("res://scenes/StarBaseScreen.tscn");
    }
}
