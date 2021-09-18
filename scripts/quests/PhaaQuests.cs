using System.Collections.Generic;

public class PhaaQuests {
    public static List<Quest.Template> list = new List<Quest.Template>{
        new Quest.Template{
            name = "Phaa Rebels",
            constructor = PhaaRebels,
            issueText = (Quest.Data q) => ($@"
                Our culture always faced some amount of opposition among our
                fellow community members.

                Before we parted from our homeworld, it was possible
                to keep the situation under control. It all changed after
                our Wertu war defeat.

                The most radical groups joined together, forming a rebellious movement.
                They captured several space crafts and ventured to unknown places.

                We can't let it slide. This is why we ask you to find and
                eliminate these rebels.
            "),
            acceptResponse = (Quest.Data q) => (@"
                You should focus on neutral systems as they are
                trying to avoid any contact with other factions.

                Report back when you will destroy at least one unit.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Phaa commander asked to eliminate a rebel group
                that hides at some unknown neutral star system.
            "),
            completionPhrase = (Quest.CompletionData c) => $"We dealt with the rebels.",
            completionResponse = (Quest.CompletionData c) => ($@"
                This is not the end of their rebellion, but
                it will help to keep our order intact.

                They may think that they're fighting for a better
                future, but what they actually doing is weakening
                our front even further.

                In any case, it's our internal politics.
                Accept this reward for a job well done.
            "),
        },
    };

    private static Quest.Data PhaaRebels(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(115 * roll);
        var rewardReputation = QRandom.IntRange(6, 9);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Phaa,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.ResolveEvent, "Phaa Rebels"),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
            },
        };
    }

    public static Quest.Template Find(string name) {
        foreach (var template in list) {
            if (template.name == name) {
                return template;
            }
        }
        throw new System.Exception("can't find a quest template with name " + name);
    }
}
