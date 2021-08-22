public class TestResearch : AbstractTest {
    protected override void RunTest() {
        TestResearchDependencies();
        TestItemResearches();
    }

    private void TestItemResearches() {
        foreach (var w in WeaponDesign.list) {
            if (w.researchRequired && !Research.researchByName.ContainsKey(w.name)) {
                Error($"Missing a research for {w.name} weapon");
            }
        }
        foreach (var w in WeaponDesign.specialList) {
            if (w.researchRequired && !Research.researchByName.ContainsKey(w.name)) {
                Error($"Missing a research for {w.name} special weapon");
            }
        }
        foreach (var sentinel in SentinelDesign.list) {
            if (sentinel.researchRequired && !Research.researchByName.ContainsKey(sentinel.name)) {
                Error($"Missing a research for {sentinel.name} sentinel");
            }
        }
    }

    private void TestResearchDependencies() {
        foreach (var r in Research.list) {
            foreach (var dep in r.dependencies) {
                if (!Research.researchByName.ContainsKey(dep)) {
                    Error($"{r.name} has undefined dependency {dep}");
                }
            }
        }
    }
}
