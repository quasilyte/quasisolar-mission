using Godot;
using System;

public class SoundEffectNode : AudioStreamPlayer {
    private AudioStream _stream;
    private float _volumeAdjust;

    private static PackedScene _scene = null;
    public static SoundEffectNode New(AudioStream stream, float volumeAdjust = 0) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SoundEffectNode.tscn");
        }
        var o = (SoundEffectNode)_scene.Instance();
        o._stream = stream;
        o._volumeAdjust = volumeAdjust;
        return o;
    }

    public override void _Ready() {
        Connect("finished", this, nameof(OnFinish));
        Stream = _stream;
        VolumeDb += _volumeAdjust;
        Play();
    }

    private void OnFinish() {
        QueueFree();
    }
}
