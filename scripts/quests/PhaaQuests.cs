using System.Collections.Generic;

public class PhaaQuests {
    public static List<Quest.Template> list = new List<Quest.Template>{
    };

    public static Quest.Template Find(string name) {
        foreach (var template in list) {
            if (template.name == name) {
                return template;
            }
        }
        throw new System.Exception("can't find a quest template with name " + name);
    }
}
