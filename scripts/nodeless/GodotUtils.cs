using Godot;

public class GodotUtils {
    private static Vector2 baseScreenSize = new Vector2(1920, 1080);
    public static Vector2 screenResolution;
    public static Vector2 screenSizeRatio;

    public static void Init(Viewport port) {
        screenResolution = port.Size;
        if (screenResolution > baseScreenSize) {
            screenSizeRatio = screenResolution / baseScreenSize;
        } else {
            screenSizeRatio = baseScreenSize / screenResolution;
        }
        GD.Print(screenSizeRatio);
    }

    public static Vector2 CorrectedCursorPos(Viewport port) {
        return port.GetMousePosition() * screenSizeRatio;
    }
}
