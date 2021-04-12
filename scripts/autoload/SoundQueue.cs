using Godot;
using System.Collections.Generic;

public class SoundQueue : AudioStreamPlayer {
    private Queue<AudioStream> _queue = new Queue<AudioStream>(); 

    public override void _Ready() {
        Connect("finished", this, nameof(OnPlayFinished));
    }

    public void AddToQueue(AudioStream audio) {
        if (_queue.Count == 0 && !Playing) {
            PlayNow(audio);
        } else {
            _queue.Enqueue(audio);        
        }
    }

    private void PlayNow(AudioStream audio) {
        Stream = audio;
        Play();
    }

    private void OnPlayFinished() {
        if (_queue.Count != 0) {
            PlayNow(_queue.Dequeue());
        }
    }
}
