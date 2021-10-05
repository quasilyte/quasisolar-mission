using Godot;
using System.Collections.Generic;
using System;

public class RandomMapEventResolvedPopup: AbstractMapPopupBuilder {
    private AbstractMapEvent _mapEvent;
    private AbstractMapEvent.Action _action;
    private AbstractMapEvent.Result _result;

    public void SetMapEvent(AbstractMapEvent e) { _mapEvent = e; }
    public void SetResult(AbstractMapEvent.Result result) { _result = result; }
    public void SetAction(AbstractMapEvent.Action action) { _action = action; }

    public MapEventPopupNode Build() {
        var outcomeText = "<" + _action.name + ">";
        outcomeText += "\n\n" + _result.text;

        var gameState = RpgGameState.instance;

        if (_result.expReward != 0) {
            outcomeText += $"\n\nGained {_result.expReward} experience points.";
            gameState.experience += _result.expReward;
        }

        var title = _mapEvent.title;
        var text = outcomeText;

        var options = new List<MapEventPopupNode.Option>{
            new MapEventPopupNode.Option{
                text = "OK",
                apply = () => Resolve(),
            },
        };

        return MapEventPopupNode.New(title, text, options);
    }
}
