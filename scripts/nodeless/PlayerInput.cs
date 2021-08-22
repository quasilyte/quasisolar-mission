using Godot;

public class PlayerInput {
    private int _deviceId;
    private bool _isGamepad;

    public PlayerInput(bool isGamepad, int deviceId) {
        _isGamepad = isGamepad;
        _deviceId = deviceId;
    }

    public bool IsGamepadControlled() {
        return _isGamepad;
    }

    public int GetDeviceId() {
        return _deviceId;
    }

    public bool IsActionPressed(string action) {
        if (_deviceId == 0) {
            return Input.IsActionPressed(action);
        }
        return Input.IsActionPressed(action + "/2");
    }

    public bool IsActionJustPressed(string action) {
        if (_deviceId == 0) {
            return Input.IsActionJustPressed(action);
        }
        return Input.IsActionJustPressed(action + "/2");
    }

    public bool IsActionJustReleased(string action) {
        if (_deviceId == 0) {
            return Input.IsActionJustReleased(action);
        }
        return Input.IsActionJustReleased(action + "/2");
    }

    public float GetJoyAxis(JoystickList axis) {
        return Input.GetJoyAxis(_deviceId, (int)axis);
    }
}