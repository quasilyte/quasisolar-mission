using Godot;
using System;
using System.Collections.Generic;

static class QMath {
    public static int Clamp(int v, int min, int max) { return (v < min) ? min : (v > max ? max : v); }
    public static int ClampMax(int v, int max) { return v > max ? max : v; }
    public static int ClampMin(int v, int min) { return v < min ? min : v; }

    public static float Clamp(float v, float min, float max) { return (v < min) ? min : (v > max ? max : v); }
    public static float ClampMax(float v, float max) { return v > max ? max : v; }
    public static float ClampMin(float v, float min) { return v < min ? min : v; }

    public static double Clamp(double v, double min, double max) { return (v < min) ? min : (v > max ? max : v); }
    public static double ClampMax(double v, double max) { return v > max ? max : v; }
    public static double ClampMin(double v, double min) { return v < min ? min : v; }

    public static int Percantage(int value, int max) {
        if (max == 0 && value == 0) {
            return 0;
        }
        return (int)(100 * ((float)value / (float)max));
    }
    public static float Percantage(float value, float max) {
        if (max == 0 && value == 0) {
            return 0;
        }
        return 100 * (value / max);
    }

    public static float Rad2Deg(float rad) { return (float)((double)rad * (180 / Math.PI)); }
    public static float Deg2Rad(float deg) { return (float)((double)deg * (Math.PI / 180)); }

    public static int IntAdjust(int x, double v) { return (int)((double)x * v); }

    public static float RotationDiff(float dstRotation, float rotation) {
        float rotationDiff = rotation - dstRotation;
        if (rotationDiff > Math.PI) {
            rotationDiff -= 2 * (float)Math.PI;
        }
        if (rotationDiff < -Math.PI) {
            rotationDiff += 2 * (float)Math.PI;
        }
        return rotationDiff;
    }

    public static float RotateTo(float dstRotation, float rotation, float amount) {
        var rotationDiff = QMath.RotationDiff(dstRotation, rotation);
        if (Math.Abs(rotationDiff) >= amount) {
            if (rotationDiff < 0) {
                return rotation + amount;
            } else {
                return rotation - amount;
            }
        }
        return dstRotation;
    }

    public static Vector2 CalculateSnipeShot(WeaponDesign w, Vector2 fireFrom, Vector2 targetPos, Vector2 targetVelocity) {
        var dist = targetPos.DistanceTo(fireFrom);
        var predictedPos = targetPos + targetVelocity * (dist / w.projectileSpeed);
        return predictedPos;
    }

    public static Vector2 RandomizedLocation(Vector2 loc, float radius) {
        float x = loc.x + QRandom.FloatRange(-radius, radius);
        float y = loc.y + QRandom.FloatRange(-radius, radius);
        return new Vector2(x, y);
    }

    public static Vector2 TranslateViewportPos(ArenaCameraNode camera, Vector2 globalPos) {
        var offset = camera.GetCameraScreenCenter() - camera.GetViewportRect().Size / 2;
        return globalPos - offset;
    }

    public static Pilot NearestEnemy(Vector2 pos, Pilot p) {
        return Nearest(pos, p, p.Enemies);
    }

    public static Pilot NearestAlly(Vector2 pos, Pilot p) {
        return Nearest(pos, p, p.Allies);
    }

    private static Pilot Nearest(Vector2 pos, Pilot p, List<Pilot> list) {
        if (list.Count == 0) {
            return null;
        }
        Pilot closest = null;
        float minDistance = float.MaxValue;
        foreach (var x in list) {
            if (!x.Active) {
                continue;
            }
            float distance = x.Vessel.GlobalPosition.DistanceTo(pos);
            if (distance < minDistance) {
                minDistance = distance;
                closest = x;
            }
        }
        return closest;
    }
}