using System.Collections.Generic;

public class RarilouQuests {
    public static List<Quest.Template> list = new List<Quest.Template>{
        new Quest.Template{
            name = "The Krigia Siege",
            cond = () => RpgGameState.instance.completedQuests.Contains("Locating Wertu Bases") && WeakestKrigiaBase() != null,
            constructor = TheKrigiaSiege,
            issueText = (Quest.Data q) => ($@"
                The expedition we arranged to learn more about the
                fate of that unknown artifact concluded that
                Krigia side does have it.

                What we don't know is the role of that artifact in
                the events that are unfolding. This is worrying.

                This time we need a direct access to their databases.
                The star base at [u]{q.Req(Quest.RequirementKind.DestroyBase)}[/u]
                has the weakest forces. We expect you to defeat the garrison
                and infiltrate the base.
            "),
            acceptResponse = (Quest.Data q) => (@"
                We didn't expect you to take such dangerous task,
                but it's our only way to progress in this investigation.

                You'll probably have to destroy all the opposition
                there before you can get your hands on the archives.

                Be sure to prepare a powerful fleet to lead the siege.
            "),
            logEntryText = (Quest.Data q) => ($@"
                In order to find the answers, Rarilou needs to get
                the Krigia databases. I am expected to raid their
                base at {q.Req(Quest.RequirementKind.DestroyBase)}.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I'm here to share the Krigia acrhives.",
            completionResponse = (Quest.CompletionData c) => ($@"
                If what you found is reliable, this looks grim.

                Krigia used the artifact they found to build a new
                vessel prototype. It's more powerful than any other ship
                from their fleet.
                
                The logs say that this ship was sent to the Krigia homeworld
                to win Wertu there. They're planning to bring this vessel back
                here when it's done.

                Captain, we need to be prepared for this.
            "),
        },

        new Quest.Template{
            name = "Locating Wertu Bases",
            cond = () => RpgGameState.instance.completedQuests.Contains("Krigia Origins"),
            constructor = LocatingWertuBases,
            issueText = (Quest.Data q) => ($@"
                Our latest discoveries lead us to a conclusion that
                Krigia found this region because of the Wertu trail.

                One of the questions is: why did Wertu arrived here
                before winning that other war?

                This time, we'll ask you to transfer at least [u]2 Wertu base coordinates[/u].
                Leave the rest to us.
            "),
            acceptResponse = (Quest.Data q) => (@"
                Thank you, captain.

                We're getting closer to the answers.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Rarilou asked to locate 2 Wertu bases for them.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I have the coordinates.",
            completionResponse = (Quest.CompletionData c) => ($@"
                It turned out to be more fascinating than we thought.

                The systems you mentioned are rich with the Legacy era materials.
                Perhaps they were looking for some particular artifact we're not aware of.

                Given that they're still here, I would say that they failed in their search.
                Or maybe they did find it, but someone took it away?
            "),
        },

        new Quest.Template{
            name = "Krigia Origins",
            constructor = KrigiaOrigins,
            issueText = (Quest.Data q) => ($@"
                Krigia forces invaded this region of space in
                a very peculiar moment. It was a short time after the
                Wertu expansion in this area became a major issue.

                We don't believe that it was a coincidence.
                More than that, we have a theory about their origins.
                But to verify it, more information is required.

                This is why we're going to ask you to bring us
                [u]{q.Req(Quest.RequirementKind.GiveKrigiaMaterial)}[/u] units of
                [u]Krigia research material[/u]. If you do that, we'll
                share with our findings with you.
            "),
            acceptResponse = (Quest.Data q) => (@"
                Captain, it may not help you to win the war,
                but it can bring you a little bit more understanding
                about your foe.

                As for us, we want to ensure that we see a whole picture of it.

                We'll await for your return.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Rarilou asked to bring them {q.Req(Quest.RequirementKind.GiveKrigiaMaterial)} units
                of Krigia research material.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I brought the Krigia research material you requested.",
            completionResponse = (Quest.CompletionData c) => ($@"
                Let me run a quick test... This confirms our theory indeed.

                We've already seen this conflict, far away from here.
                It looks like Krigia race has changed a lot since then,
                this is why we needed their samples to be sure
                it's the same race.

                Krigia is not actually invading this region.
                They're escaping to this region.
                We're making an assumption that in their homeworld sector,
                someone else is winning the war.
            "),
        },

        new Quest.Template{
            name = "The Unidentified Attackers",
            constructor = UnidentifiedAttackers,
            issueText = (Quest.Data q) => ($@"
                Several months ago one of our fleets was attacked by
                an unidentified group at [u]{VespionSystemNeighbor()}[/u] system.

                We managed to repell that attack and then scanned the system.
                It had no star bases, so they probably came from another system.

                I would like to ask you to do the investigation for us.
                Find where that group came from.
                We'll try to make a proper contact with them so
                they don't attack us the next time.
            "),
            acceptResponse = (Quest.Data q) => (@"
                Whatever you find there, all we need is the information about
                the origins of this group.

                Everything else is up to you, captain.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Rarilou are interested in finding the unknown alien
                attackers that were seen at {VespionSystemNeighbor()} some
                months ago.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I have the information about the unidentified attackers.",
            completionResponse = (Quest.CompletionData c) => ($@"
                So the group was coming from the [u]{VespionSystem().name}[/u] system and
                the alien race calls itself Vespion?

                That's all we needed to know for now, our reconnaissance unit
                will take it from here.

                You have fulfilled your part of the contract.
            "),
        },

        new Quest.Template{
            name = "Organics for Rarilou",
            constructor = BringOrganics,
            issueText = (Quest.Data q) => ($@"
                We have an urgent need for [u]organic[/u] resources.

                Normally, we synthesize most of the resources we need from scraps
                and debris, but organics are the hardest type of material in
                that regard at our current stage.

                [u]{q.Req(Quest.RequirementKind.GiveOrganic)}[/u] units would be enough for this fleet to
                operate normally for years.

                Can you bring them to us?
            "),
            acceptResponse = (Quest.Data q) => (@"
                Splendid! Come back when you will acquire the requested materials.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Rarilou asked to bring them {q.Req(Quest.RequirementKind.GiveOrganic)} units
                of organic resources.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I brought {c.quest.Req(Quest.RequirementKind.GiveOrganic)} organics as you requested.",
            completionResponse = (Quest.CompletionData c) => ($@"
                Let me see... Yes, that's the exact amount that we need.

                Your help is greatly appreciated.
            "),
        },

        new Quest.Template{
            name = "The Gas Giant Hunt",
            constructor = GasGiantHunt,
            issueText = (Quest.Data q) => ($@"
                As you may already know, this part of the galaxy contains a lot
                of gas giant planets, but most of them are of little interest to us.

                Nevertheless, we know that gas giants mainly consist of the hydrogen
                that can be suitable for power resource usage when combined
                with a nuclear fusion reaction, also known as stellar nucleosynthesis.
                Given the right conditions and technologies,
                such failed stars can be explored and even mined for energy.

                We've been trying to locate such a planet for several months but to no avail.

                Could you lend us a hand and find a [u]gas giant[/u]
                with the abovementioned properties?
            "),
            acceptResponse = (Quest.Data q) => (@"
                That's good news indeed.
                See us again when you'll have the system coordinates.
            "),
            logEntryText = (Quest.Data q) => ($@"
                I need to locate a gas giant planet that is suitable
                for the power resource collection.
            "),
            completionPhrase = (Quest.CompletionData c) => $"{c.values[Quest.RequirementKind.FindGasGiant]} contains a planet you're looking for.",
            completionResponse = (Quest.CompletionData c) => ($@"
                Let me record these coordinates.

                We'll visit the {c.values[Quest.RequirementKind.FindGasGiant]} system as soon as possible.

                Thank you for your assistence.
            "),
        },

        new Quest.Template{
            name = "Energy Conversion Research",
            constructor = EnergyConversionResearch,
            issueText = (Quest.Data q) => ($@"
                For years we've been working on a more efficient ways
                of utilizing resources and converting one kind of energy into another.

                We're close to breakthrough, but it would be useful
                for us to get some fresh views on the problem.

                We can share our current progress with your researchers
                if you'll promise to collaborate on it.
                If this will help us to complete this project,
                the results are going to be shared with Earthlings.
            "),
            acceptResponse = (Quest.Data q) => ($@"
                Good. Return when you'll complete the
                {q.Req(Quest.RequirementKind.CompleteResearch)} research.
            "),
            logEntryText = (Quest.Data q) => ($@"
                Rarilou asked to help them with the energy conversion research.
                I need to share the completed research results with them.
            "),
            completionPhrase = (Quest.CompletionData c) => $"I'm ready to share the research results.",
            completionResponse = (Quest.CompletionData c) => ($@"
                That does validate and completent our findings.

                With these two combined, we can finally apply it to the
                power resource to fuel conversion.

                Normally, we can get around 3 units of fuel per 1 power unit.
                This new method changes that ratio to a more efficient 4-to-1.
            "),
        },
    };

    private static string Currency() { return RpgGameState.alienCurrencyNames[Faction.Rarilou]; }

    public static Quest.Template Find(string name) {
        foreach (var template in list) {
            if (template.name == name) {
                return template;
            }
        }
        throw new System.Exception("can't find a quest template with name " + name);
    }

    private static Quest.Data TheKrigiaSiege(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(150 * roll);
        var rewardRU = QRandom.IntRange(20000, 30000);
        var rewardReputation = 10;
        var targetBase = WeakestKrigiaBase();
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.DestroyBase, targetBase.system.Get().name),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
                new Quest.Reward(Quest.RewardKind.GetRU, rewardRU),
            },
        };
    }

    private static Quest.Data LocatingWertuBases(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(90 * roll);
        var rewardReputation = QRandom.IntRange(4, 6);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.FindTwoWertuSystems),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
            },
        };
    }

    public static Quest.Data KrigiaOrigins(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var materialRequested = (int)(200 * roll);
        var reward = (int)(100 * roll);
        var rewardReputation = QRandom.IntRange(8, 10);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.GiveKrigiaMaterial, materialRequested),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
            },
        };
    }

    public static Quest.Data UnidentifiedAttackers(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(50 * roll);
        var rewardReputation = QRandom.IntRange(5, 8);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.FindGasGiant),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
            },
        };
    }

    public static Quest.Data GasGiantHunt(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(40 * roll);
        var rewardReputation = QRandom.IntRange(4, 9);
        var rewardRU = QRandom.IntRange(5000, 10000);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.FindGasGiant),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
                new Quest.Reward(Quest.RewardKind.GetRU, rewardRU),
            },
        };
    }

    public static Quest.Data BringOrganics(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var organicsRequested = (int)(140 * roll);
        var reward = (int)(20 * roll);
        var rewardReputation = QRandom.IntRange(2, 6);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.GiveOrganic, organicsRequested),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
            },
        };
    }

    private static Quest.Data EnergyConversionResearch(Quest.Template template) {
        var roll = QRandom.FloatRange(0.5f, 1);
        var reward = (int)(45 * roll);
        var rewardReputation = QRandom.IntRange(3, 4);
        return new Quest.Data{
            name = template.name,
            faction = Faction.Rarilou,
            requirements = {
                new Quest.Requirement(Quest.RequirementKind.CompleteResearch, "Rarilou Energy Conversion"),
            },
            rewards = {
                new Quest.Reward(Quest.RewardKind.GetReputation, rewardReputation),
                new Quest.Reward(Quest.RewardKind.GetAlienCurrency, reward),
                new Quest.Reward(Quest.RewardKind.GetTechnology, "Improved Power Conversion"),
            },
        };
    }

    private static StarSystem VespionSystem() {
        return RpgGameState.vespionBase.system.Get();
    }

    private static string VespionSystemNeighbor() {
        StarSystem closest = null;
        StarSystem firstCloseEnough = null;
        var vespionSys = VespionSystem();
        foreach (var sys in RpgGameState.starSystemConnections[vespionSys]) {
            if (closest == null || sys.pos.DistanceTo(vespionSys.pos) < closest.pos.DistanceTo(vespionSys.pos)) {
                closest = sys;
            }
            // Prefer non-earthling systems.
            if (sys.starBase.id != 0 && sys.starBase.Get().owner == Faction.Earthling) {
                continue;
            }
            if (firstCloseEnough == null && sys.pos.DistanceTo(vespionSys.pos) <= 300) {
                firstCloseEnough = sys;
            }
        }
        return firstCloseEnough != null ? firstCloseEnough.name : closest.name;
    }

    private static StarBase WeakestKrigiaBase() {
        StarBase weakest = null;
        foreach (var sys in RpgGameState.starSystemList) {
            if (sys.starBase.id == 0) {
                continue;
            }
            var starBase = sys.starBase.Get();
            if (starBase.owner != Faction.Krigia) {
                continue;
            }
            if (weakest == null || starBase.GarrisonCost() < weakest.GarrisonCost()) {
                weakest = starBase;
            }
        }
        return weakest;
    }
}
