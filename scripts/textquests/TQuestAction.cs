using System;
using System.Collections.Generic;

public class TQuestAction {
    public string text;
    public Func<TQuestCard> next;
    public Action apply = () => {};
    public Func<bool> cond = () => true;
}