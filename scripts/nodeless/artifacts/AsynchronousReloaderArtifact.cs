using Godot;

public class AsynchronousReloaderArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Asynchronous Reloader",
        description = "TODO",
        effect = "-10% special weapon cooldown",
        sellingPrice = 3500,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {}
}
