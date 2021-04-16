using Godot;

class GameControls {
    private static bool _initialized = false;

    public static bool preferGamepad = true;

    public static void InitInputMap() {
        if (_initialized) {
            return;
        }
        _initialized = true;

        {
            InputMap.AddAction("escape");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.Escape;
            InputMap.ActionAddEvent("escape", e);
        }

        {
            InputMap.AddAction("openConsole");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.Quoteleft;
            InputMap.ActionAddEvent("openConsole", e);
        }

        {
            InputMap.AddAction("leftMouseButton");

            var e = new InputEventMouseButton();
            e.ButtonIndex = (int)ButtonList.Left;
            InputMap.ActionAddEvent("leftMouseButton", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.XboxA;
            InputMap.ActionAddEvent("leftMouseButton", e2);

            InputMap.AddAction("leftMouseButton/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.XboxA;
            InputMap.ActionAddEvent("leftMouseButton/2", e3);
        }

        {
            InputMap.AddAction("clearWaypoints");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.Space;
            InputMap.ActionAddEvent("clearWaypoints", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.L;
            InputMap.ActionAddEvent("clearWaypoints", e2);

            InputMap.AddAction("clearWaypoints/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.L;
            InputMap.ActionAddEvent("clearWaypoints/2", e3);
        }
    
        {
            InputMap.AddAction("shield");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.R;
            InputMap.ActionAddEvent("shield", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.R;
            InputMap.ActionAddEvent("shield", e2);

            InputMap.AddAction("shield/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.R;
            InputMap.ActionAddEvent("shield/2", e3);
        }

        {
            InputMap.AddAction("weapon0");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.Q;
            InputMap.ActionAddEvent("weapon0", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.XboxX;
            InputMap.ActionAddEvent("weapon0", e2);

            InputMap.AddAction("weapon0/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.XboxX;
            InputMap.ActionAddEvent("weapon0/2", e3);
        }
        {
            InputMap.AddAction("weapon1");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.W;
            InputMap.ActionAddEvent("weapon1", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.XboxY;
            InputMap.ActionAddEvent("weapon1", e2);

            InputMap.AddAction("weapon1/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.XboxY;
            InputMap.ActionAddEvent("weapon1/2", e3);
        }
        {
            InputMap.AddAction("special");

            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.E;
            InputMap.ActionAddEvent("special", e);

            var e2 = new InputEventJoypadButton();
            e2.ButtonIndex = (int)JoystickList.XboxB;
            InputMap.ActionAddEvent("special", e2);

            InputMap.AddAction("special/2");
            var e3 = new InputEventJoypadButton();
            e3.Device = 1;
            e3.ButtonIndex = (int)JoystickList.XboxB;
            InputMap.ActionAddEvent("special/2", e3);
        }

        {
            InputMap.AddAction("cursorUp");
            var e = new InputEventJoypadButton();
            e.ButtonIndex = (int)JoystickList.DpadUp;
            InputMap.ActionAddEvent("cursorUp", e);

            InputMap.AddAction("cursorUp/2");
            var e2 = new InputEventJoypadButton();
            e2.Device = 1;
            e2.ButtonIndex = (int)JoystickList.DpadUp;
            InputMap.ActionAddEvent("cursorUp/2", e2);
        }
        {
            InputMap.AddAction("cursorDown");
            var e = new InputEventJoypadButton();
            e.ButtonIndex = (int)JoystickList.DpadDown;
            InputMap.ActionAddEvent("cursorDown", e);

            InputMap.AddAction("cursorDown/2");
            var e2 = new InputEventJoypadButton();
            e2.Device = 1;
            e2.ButtonIndex = (int)JoystickList.DpadDown;
            InputMap.ActionAddEvent("cursorDown/2", e2);
        }
        {
            InputMap.AddAction("cursorLeft");
            var e = new InputEventJoypadButton();
            e.ButtonIndex = (int)JoystickList.DpadLeft;
            InputMap.ActionAddEvent("cursorLeft", e);

            InputMap.AddAction("cursorLeft/2");
            var e2 = new InputEventJoypadButton();
            e2.Device = 1;
            e2.ButtonIndex = (int)JoystickList.DpadLeft;
            InputMap.ActionAddEvent("cursorLeft/2", e2);
        }
        {
            InputMap.AddAction("cursorRight");
            var e = new InputEventJoypadButton();
            e.ButtonIndex = (int)JoystickList.DpadRight;
            InputMap.ActionAddEvent("cursorRight", e);

            InputMap.AddAction("cursorRight/2");
            var e2 = new InputEventJoypadButton();
            e2.Device = 1;
            e2.ButtonIndex = (int)JoystickList.DpadRight;
            InputMap.ActionAddEvent("cursorRight/2", e2);
        }

        {
            InputMap.AddAction("targetLock");
            var e = new InputEventJoypadButton();
            e.ButtonIndex = (int)JoystickList.L3;
            InputMap.ActionAddEvent("targetLock", e);

            InputMap.AddAction("targetLock/2");
            var e2 = new InputEventJoypadButton();
            e2.Device = 1;
            e2.ButtonIndex = (int)JoystickList.L3;
            InputMap.ActionAddEvent("targetLock/2", e2);
        }

        {
            InputMap.AddAction("attackCursorMotionLeft");
            var e = new InputEventJoypadMotion();
            e.Axis = (int)JoystickList.Axis0; // X axis
            e.AxisValue = -1.0f;
            InputMap.ActionAddEvent("attackCursorMotionLeft", e);
            InputMap.ActionSetDeadzone("attackCursorMotionLeft", 0.1f);

            InputMap.AddAction("attackCursorMotionLeft/2");
            var e2 = new InputEventJoypadMotion();
            e2.Device = 1;
            e2.Axis = (int)JoystickList.Axis0; // X axis
            e2.AxisValue = -1.0f;
            InputMap.ActionAddEvent("attackCursorMotionLeft/2", e2);
            InputMap.ActionSetDeadzone("attackCursorMotionLeft/2", 0.1f);
        }
        {
            InputMap.AddAction("attackCursorMotionRight");
            var e = new InputEventJoypadMotion();
            e.Axis = (int)JoystickList.Axis0; // X axis
            e.AxisValue = 1.0f;
            InputMap.ActionAddEvent("attackCursorMotionRight", e);
            InputMap.ActionSetDeadzone("attackCursorMotionRight", 0.1f);

            InputMap.AddAction("attackCursorMotionRight/2");
            var e2 = new InputEventJoypadMotion();
            e2.Device = 1;
            e2.Axis = (int)JoystickList.Axis0; // X axis
            e2.AxisValue = 1.0f;
            InputMap.ActionAddEvent("attackCursorMotionRight/2", e2);
            InputMap.ActionSetDeadzone("attackCursorMotionRight/2", 0.1f);
        }
        {
            InputMap.AddAction("attackCursorMotionUp");
            var e = new InputEventJoypadMotion();
            e.Axis = (int)JoystickList.Axis1; // Y axis
            e.AxisValue = -1.0f;
            InputMap.ActionAddEvent("attackCursorMotionUp", e);
            InputMap.ActionSetDeadzone("attackCursorMotionUp", 0.1f);

            InputMap.AddAction("attackCursorMotionUp/2");
            var e2 = new InputEventJoypadMotion();
            e2.Device = 1;
            e2.Axis = (int)JoystickList.Axis1; // Y axis
            e2.AxisValue = -1.0f;
            InputMap.ActionAddEvent("attackCursorMotionUp/2", e2);
            InputMap.ActionSetDeadzone("attackCursorMotionUp/2", 0.1f);
        }
        {
            InputMap.AddAction("attackCursorMotionDown");
            var e = new InputEventJoypadMotion();
            e.Axis = (int)JoystickList.Axis1; // Y axis
            e.AxisValue = 1.0f;
            InputMap.ActionAddEvent("attackCursorMotionDown", e);
            InputMap.ActionSetDeadzone("attackCursorMotionDown", 0.1f);

            InputMap.AddAction("attackCursorMotionDown/2");
            var e2 = new InputEventJoypadMotion();
            e2.Device = 1;
            e2.Axis = (int)JoystickList.Axis1; // Y axis
            e2.AxisValue = 1.0f;
            InputMap.ActionAddEvent("attackCursorMotionDown/2", e2);
            InputMap.ActionSetDeadzone("attackCursorMotionDown/2", 0.1f);
        }
        
        // {
        //     InputMap.AddAction("attackCursorDown");
        //     var e = new InputEventJoypadMotion();
        //     e.Axis = 1;
        //     e.AxisValue = 1.0f;
        //     InputMap.ActionAddEvent("attackCursorDown", e);
        // }
        
        {
            InputMap.AddAction("mapMovementToggle");
            var e = new InputEventKey();
            e.Scancode = (uint)KeyList.Space;
            InputMap.ActionAddEvent("mapMovementToggle", e);
        }
    }
}