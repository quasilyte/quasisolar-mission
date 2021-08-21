public class TestResearch : AbstractTest {
    protected override void RunTest() {
        TestResearchDependencies();
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
