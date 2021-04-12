public enum UnitMode {
    // - Gain 1 fuel per day
    Idle,

    // - Consumes 3 fuel per day
    // - Can find an artifact, if it's there
    Search,

    // - Consumes 1 fuel per day
    // - 100% chance of a base fleet fight every day
    // - If base has no fleet, deal 4% health damage per ship in the unit group
    Attack,
}