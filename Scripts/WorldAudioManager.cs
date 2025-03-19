using Godot;
using System;

public partial class WorldAudioManager : Node {
    [Export]
    public AudioStreamPlayer BgmPlayer { get; set; }
    [Export]
    private AudioStreamPlayer _buttonClickPlayer { get; set; }
    [Export]
    private AudioStreamPlayer _deathSoundPlayer;

    string _stageName;

    public override void _Ready()
    {
        _stageName = Game.StageName;

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
        var currentBgm = _stageName + "Bgm";
        var interactive = BgmPlayer.GetStreamPlayback() as AudioStreamPlaybackInteractive;
        interactive.SwitchToClipByName( currentBgm );
    }

    public void ButtonClickPlay()
    {
        _buttonClickPlayer.Play();
    }

    public void DeathSoundPlay()
    {
        _deathSoundPlayer.Play();
    }
}