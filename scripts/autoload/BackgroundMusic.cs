using Godot;
using System;

public class BackgroundMusic : AudioStreamPlayer {
    public static int volumeSetting = 0;

    private AudioStream _mainMenuMusic = null;
    private AudioStream _outfitMusic = null;
    private AudioStream _shipyardMusic = null;
    private AudioStream _mapMusic = null;
    private AudioStream _mapMusic2 = null;
    private AudioStream _battleMusic = null;
    private AudioStream _battleMusic2 = null;

    public override void _Ready() {}

    public void PlayMenuMusic() {
        if (volumeSetting == 0) {
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
        VolumeDb = -20 + (volumeSetting * 10);
        Play();
    }

    public void PlayOutfitMusic() {
        if (volumeSetting == 0) {
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
        VolumeDb = -15 + (volumeSetting * 10);
        Play();
    }

    public void PlayShipyardMusic() {
        if (volumeSetting == 0) {
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
        VolumeDb = -20 + (volumeSetting * 10);
        Play();
    }

    public bool PlayingMapMusic() {
        return Playing && (Stream == _mapMusic || Stream == _mapMusic2);
    }

    public void PlayMapMusic() {
        if (volumeSetting == 0) {
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
        VolumeDb = -30 + (volumeSetting * 10);
        Play();
    }

    public void PlayMapMusic2() {
        if (volumeSetting == 0) {
            return;
        }
        if (Playing && Stream == _mapMusic2) {
            return;
        }
        Stop();
        if (_mapMusic2 == null) {
            _mapMusic2 = GD.Load<AudioStream>("res://audio/music/map2.ogg");
        }
        Stream = _mapMusic2;
        VolumeDb = -25 + (volumeSetting * 10);
        Play();
    }

    public void PlayBattleMusic() {
        if (volumeSetting == 0) {
            return;
        }
        if (Playing && Stream == _battleMusic) {
            return;
        }
        Stop();
        if (_battleMusic == null) {
            _battleMusic = GD.Load<AudioStream>("res://audio/music/battle.ogg");
        }
        VolumeDb = -10 + (volumeSetting * 10);
        Stream = _battleMusic;
        Play();
    }

    public void PlayBattleMusic2() {
        if (volumeSetting == 0) {
            return;
        }
        if (Playing && Stream == _battleMusic2) {
            return;
        }
        Stop();
        if (_battleMusic2 == null) {
            _battleMusic2 = GD.Load<AudioStream>("res://audio/music/battle2.ogg");
        }
        VolumeDb = -20 + (volumeSetting * 10);
        Stream = _battleMusic2;
        Play();
    }
}
