using Godot;
using System;
using System.Collections.Generic;

public static class QRandom {
    private static RandomNumberGenerator rng;

    public static void SetRandomNumberGenerator(RandomNumberGenerator rng) {
        QRandom.rng = rng;
    }

    public static void Sort<T>(List<T> list) {
        for (var i = list.Count-1; i > 0; i--) {
            var randomIndex = QRandom.IntRange(0, i);
            var temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public static T Element<T>(List<T> list) {
        return list[PositiveInt() % list.Count];
    }

    public static bool Bool() {
        return (PositiveInt() & 1) != 0;
    }

    public static int PositiveInt() {
        return Math.Abs((int)rng.Randi());
    }

    public static int IntRange(int from, int to) {
        return rng.RandiRange(from, to);
    }

    public static float Float() {
        return rng.Randf();
    }

    public static float Angle() {
        return rng.RandfRange(0, 2 * Mathf.Pi);
    }

    public static float FloatRange(float from, float to) {
        return rng.RandfRange(from, to);
    }
}
