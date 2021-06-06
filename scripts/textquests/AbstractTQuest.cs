using System;
using System.Collections.Generic;

public abstract class AbstractTQuest {
    public class Value {
        public string name;
        public Func<object> getter;
        public bool show;
    }

    public abstract TQuestCard GetFirstCard();

    public List<Value> values = new List<Value>();

    protected void DeclareValue(string name, Func<object> getter, bool show = false) {
        values.Add(new Value{
            name = name,
            getter = getter,
            show = show,
        });
    }

    protected void SetValueVisibility(string name, bool show) {
        foreach (var v in values) {
            if (v.name == name) {
                v.show = show;
                return;
            }
        }
        throw new Exception("invalid value name: " + name);
    }
}
