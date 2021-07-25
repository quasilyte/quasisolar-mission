using System.Collections.Generic;

public class MapEventRegistry {
    public static AbstractMapEvent[] list;
    public static List<AbstractMapEvent> onSystemEnteredList;
    public static Dictionary<string, AbstractMapEvent> eventByTitle;

    public static void InitLists() {
        list = new AbstractMapEvent[]{
            // Special events.
            new PurpleSystemVisitorMapEvent(),

            // Text quests.
            new HackingMapEvent(),

            // Spectre sequence.
            new SpectreMapEvent(),
            new SpectreAttackMapEvent(),

            // Normal events.
            new EnigmaMapEvent(),
            new KrigiaDroneMapEvent(),
            new TreasureVaultMapEvent(),
            new DevastatedHomeworldMapEvent(),
            new EarthlingScoutMapEvent(),
            new ClashOfTitansMapEvent(),
            new LoneKrigiaScoutMapEvent(),
            new SpaceNomadsMapEvent(),
            new AsteroidsMapEvent(),
            new RobotsColonyMapEvent(),
            new NaturePresentMapEvent(),
            new InterceptedSignalMapEvent(),
            new TroubledLinerMapEvent(),
            new AbandonedVesselMapEvent(),
        };

        onSystemEnteredList = new List<AbstractMapEvent>();
        eventByTitle = new Dictionary<string, AbstractMapEvent>();
        foreach (var e in list) {
            eventByTitle.Add(e.title, e);
            if (e.triggerKind == AbstractMapEvent.TriggerKind.OnSystemEntered) {
                onSystemEnteredList.Add(e);
            }
        }
    }
}
