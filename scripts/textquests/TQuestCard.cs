using System.Collections.Generic;

public class TQuestCard {
    public const int MAX_ACTIONS = 5;

    public string text = null;
    public string image = "";
    public List<TQuestAction> actions = new List<TQuestAction>();

    public static TQuestCard exitQuestEnterArena = new TQuestCard{};
    public static TQuestCard exitQuestEnterMap = new TQuestCard{};
}
