using Godot;

public static class DrawUtils {
    private static Vector2[] CirclePoints(float radius) {
        float angleFrom = 0;
        float angleTo = 360;

        int num_points;
        if (radius <= 100) {
            num_points = (int)(radius / 3);
        } else if (radius <= 200) {
            num_points = (int)(radius / 4);
        } else if (radius <= 350) {
            num_points = (int)(radius / 6);
        } else if (radius <= 550) {
            num_points = (int)(radius / 10);
        } else {
            num_points = (int)(radius / 12);
        }
        var points = new Vector2[num_points + 1];

        for (int i = 0; i < num_points + 1; i++) {
            var angle = QMath.Deg2Rad(angleFrom + i * (angleTo - angleFrom) / num_points - 90);
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        return points;
    }

    public static void DrawCircle(Node2D node, float radius, Color color) {
        var points = CirclePoints(radius);
        var num_points = points.Length - 1;
        for (int i = 0; i < num_points; i++) {
            node.DrawLine(points[i], points[i + 1], color);
        }
    }

    public static void DrawDashedCircle(Node2D node, float radius, Color color) {
        var points = CirclePoints(radius);
        var num_points = points.Length - 1;
        for (int i = 0; i + 1 < num_points; i += 2) {
            node.DrawLine(points[i], points[i + 1], color);
        }
    }
}
