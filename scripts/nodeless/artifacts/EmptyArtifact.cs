using Godot;

public class EmptyArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Empty",
    };

    public void Apply(VesselState state, float delta) {}
    public void Upgrade(VesselState state) {}
}
