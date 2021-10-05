public class ArenaContext {
    public NodesGroup affectedByEnvHazard = new NodesGroup();
    public NodesGroup asteroids = new NodesGroup();
    public NodesGroup mortarShells = new NodesGroup();
    public NodesGroup rockets = new NodesGroup();

    public void Process() {
        affectedByEnvHazard.Process();
        asteroids.Process();
        mortarShells.Process();
        rockets.Process();
    }
}
