using System;

public abstract class AbstractMapPopupBuilder {
    private Action _onResolved;

    public void SetOnResolved(Action callback) { _onResolved = callback; }

    protected void Resolve() {
        _onResolved();
    }
}
