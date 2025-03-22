using Godot;
using System;
using System.Collections.Generic;

public partial class WorldAudioManager : Node {
    private List<AudioStreamPlayer> _bgmPlayers;
    [Export]
    private AudioStreamPlayer _stageBgmPlayer;
    [Export]
    private AudioStreamPlayer _titleBgmPlayer;
    [Export]
    private AudioStreamPlayer _buttonClickPlayer { get; set; }
    [Export]
    private AudioStreamPlayer _deathSoundPlayer;
    [Export]
    private AudioStreamPlayer _shieldReflectNoDamageSoundPlayer;

    string _stageName;

    public override void _Ready()
    {
        _stageName = Game.StageName;
        _bgmPlayers = new List<AudioStreamPlayer>() {
            _titleBgmPlayer, 
            _stageBgmPlayer
        };
        //setAllMuted( true );
    }

    public override void _Input( InputEvent inputEvent )
    {
        if( inputEvent.IsActionPressed( "Mute" ) ) {
            var busIndex = AudioServer.GetBusIndex( "Master" );
            setAllMuted( !AudioServer.IsBusMute( busIndex ) );
        }
    }

    private void setAllMuted( bool isMuted )
    {
        var busIndex = AudioServer.GetBusIndex( "Master" );
        AudioServer.SetBusMute( busIndex, isMuted );
    }

    public override void _Process( double delta )
    {
        if( _stageName != Game.StageName ) {
            _stageName = Game.StageName;
            UpdateBgmForScene();
        }
    }

    private void UpdateBgmForScene()
    {
        switch(_stageName) {
            case null:
                StopAllBGM(); 
                break;
            case "TestWave":
                StopAllBGM();
                _stageBgmPlayer.Play();
                break;
            case "Title":
                StopAllBGM();
                _titleBgmPlayer.Play();
                break;
        }
        //var interactive = _stageBgmPlayer.GetStreamPlayback() as AudioStreamPlaybackInteractive;
        //interactive.SwitchToClipByName( currentBgm );
    }

    private void StopAllBGM()
    {
        foreach( var bgmPlayer in _bgmPlayers ) {
            bgmPlayer.Stop();
        }
    }

    public void ButtonClickPlay()
    {
        _buttonClickPlayer.Play();
    }

    public void DeathSoundPlay()
    {
        _deathSoundPlayer.Play();
    }

    public void ShieldReflectNoDamageSoundPlay()
    {
        _shieldReflectNoDamageSoundPlayer.Play();
    }
}