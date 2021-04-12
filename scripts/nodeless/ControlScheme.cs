using Godot;

public class ControlScheme {
    public class ArenaControls {
        public InputEvent addWaypoint;
        public InputEvent clearWaypoints;
        public InputEvent weapon0;
        public InputEvent weapon1;
        public InputEvent weapon2;
        public InputEvent specialWeapon;
        public InputEvent mainCursorUp;
        public InputEvent mainCursorDown;
        public InputEvent mainCursorLeft;
        public InputEvent mainCursorRight;
        public InputEvent attackCursorLock;
        public InputEvent attackCursorUp;
        public InputEvent attackCursorDown;
        public InputEvent attackCursorLeft;
        public InputEvent attackCursorRight;
    }

    public class MapControls {
        public InputEvent toggleMovement;
    }

    public ArenaControls arenaControls;
    public MapControls mapControls;
}
