using System.Collections.Generic;

public class MapEventRegistry {
    public static AbstractMapEvent[] list;
    public static List<AbstractMapEvent> onSystemEnteredList;
    public static Dictionary<string, AbstractMapEvent> eventByTitle;

    public static void InitLists() {
        list = new AbstractMapEvent[]{
            // Special events.
            new PurpleSystemVisitorMapEvent(),

            // Spectre sequence.
            new SpectreMapEvent(),
            new SpectreAttackMapEvent(),

            // Dark beacon sequence.
            new MysteriousBeaconMapEvent(),
            new BeaconActivityMapEvent(),
            new RiftAmbushMapEvent(),
            new LurkingThreatMapEvent(),
            
            // Low-fuel events.
            new FuelTraderMapEvent(),
            new ScrapsMapEvent(),
            new BlackAsteroidMapEvent(),

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
            new InterceptedSignalMapEvent(),
            new TroubledLinerMapEvent(),
            new AbandonedVesselMapEvent(),
            new DraklidShadowMarketMapEvent(),
            new GuardianCrashSiteMapEvent(),
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
