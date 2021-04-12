using Godot;
using System;

public class BackgroundMusic : AudioStreamPlayer {
    public static bool _disabled = true;

    private AudioStream _mainMenuMusic = null;
    private AudioStream _outfitMusic = null;
    private AudioStream _shipyardMusic = null;
    private AudioStream _mapMusic = null;
    private AudioStream _battleMusic = null;

    public override void _Ready() {}

    public void PlayMenuMusic() {
        if (_disabled) {
            return;
        }
        if (Playing && Stream == _mainMenuMusic) {
            return;
        }
        Stop();
        if (_mainMenuMusic == null) {
            _mainMenuMusic = GD.Load<AudioStream>("res://audio/music/main_menu.ogg");
        }
        Stream = _mainMenuMusic;
        VolumeDb = -10;
        Play();
    }

    public void PlayOutfitMusic() {
        if (_disabled) {
            return;
        }
        if (Playing && Stream == _outfitMusic) {
            return;
        }
        Stop();
        if (_outfitMusic == null) {
            _outfitMusic = GD.Load<AudioStream>("res://audio/music/outfit.ogg");
        }
        Stream = _outfitMusic;
        VolumeDb = -5;
        Play();
    }

    public void PlayShipyardMusic() {
        if (_disabled) {
            return;
        }
        if (Playing && Stream == _shipyardMusic) {
            return;
        }
        Stop();
        if (_shipyardMusic == null) {
            _shipyardMusic = GD.Load<AudioStream>("res://audio/music/shipyard.ogg");
        }
        Stream = _shipyardMusic;
        VolumeDb = -10;
        Play();
    }

    public void PlayMapMusic() {
        if (_disabled) {
            return;
        }
        if (Playing && Stream == _mapMusic) {
            return;
        }
        Stop();
        if (_mapMusic == null) {
            _mapMusic = GD.Load<AudioStream>("res://audio/music/map.ogg");
        }
        Stream = _mapMusic;
        VolumeDb = -20;
        Play();
    }

    public void PlayBattleMusic() {
        if (_disabled) {
            return;
        }
        if (Playing && Stream == _battleMusic) {
            return;
        }
        Stop();
        if (_battleMusic == null) {
            _battleMusic = GD.Load<AudioStream>("res://audio/music/battle.ogg");
        }
        VolumeDb = 0;
        Stream = _battleMusic;
        Play();
    }
}
