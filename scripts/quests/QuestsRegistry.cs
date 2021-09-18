public static class QuestsRegistry {
    public static Quest.Template FindQuest(Quest.Data q) {
        if (q.faction == Faction.Rarilou) {
            return RarilouQuests.Find(q.name);
        }
        if (q.faction == Faction.Phaa) {
            return PhaaQuests.Find(q.name);
        }
        return null;
    }
}
