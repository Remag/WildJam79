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
    [Export]
    private AudioStreamPlayer _tentacleSoundPlayer;
    [Export]
    private AudioStreamPlayer _explosionSoundPlayer;
    [Export]
    private AudioStreamPlayer _bottlePopSoundPlayer;
    [Export]
    private AudioStreamPlayer _echoSoundPlayer;
    [Export]
    private AudioStreamPlayer _finalCutsceneSoundPlayer;
    [Export]
    private AudioStreamPlayer _catchSoundPlayer;

    string _stageName;

    public override void _Ready()
    {
        _stageName = Game.StageName;
        _bgmPlayers = new List<AudioStreamPlayer>() {
            _titleBgmPlayer,
            _stageBgmPlayer
        };
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
        switch( _stageName ) {
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
        if( _buttonClickPlayer.Playing == false || _buttonClickPlayer.GetPlaybackPosition() > 0.2 )
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

    public void TentacleSoundPlay()
    {
        _tentacleSoundPlayer.Play();
    }

    internal void StopTentacleSound()
    {
        _tentacleSoundPlayer.Stop();
    }

    public void ExplosionSoundPlay()
    {
        _explosionSoundPlayer.Play();
    }

    internal void BottlePopSoundPlay()
    {
        _bottlePopSoundPlayer.Play();
    }

    internal void FinalCutsceneSoundPlay()
    {
        _finalCutsceneSoundPlayer.Play();
    }

    internal void EchoSoundPlay()
    {
        if( _echoSoundPlayer.Playing == false )
            _echoSoundPlayer.Play();
    }

    internal void CatchSoundPlay()
    {
        if( _catchSoundPlayer.Playing == false || _catchSoundPlayer.GetPlaybackPosition() > 0.2 )
            _catchSoundPlayer.Play();
    }
}