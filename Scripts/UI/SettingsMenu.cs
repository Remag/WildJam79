using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class SettingsMenu : Control {

    [Export]
    private CheckButton _muteSoundButton;
    [Export]
    private CheckButton _muteMusicButton;
    [Export]
    private Slider _masterSlider;
    [Export]
    private Slider _musicSlider;
    [Export]
    private Slider _soundSlider;

    public override void _Ready()
    {
        _muteSoundButton.SetPressedNoSignal( IsAllSoundMuted() );
        _muteMusicButton.SetPressedNoSignal( IsMusicMuted() );

        _masterSlider.SetValueNoSignal( GetMasterVolume() );
        _musicSlider.SetValueNoSignal( GetMusicVolume() );
        _soundSlider.SetValueNoSignal( GetSfxVolume() );
    }

    public void MuteAllSound( bool isMuted )
    {
        var busIndex = AudioServer.GetBusIndex( "Master" );
        AudioServer.SetBusMute( busIndex, isMuted );
    }

    public bool IsAllSoundMuted()
    {
        var busIndex = AudioServer.GetBusIndex( "Master" );
        return AudioServer.IsBusMute( busIndex );
    }

    public void MuteMusic( bool isMuted )
    {
        var busIndex = AudioServer.GetBusIndex( "music" );
        AudioServer.SetBusMute( busIndex, isMuted );
    }

    public bool IsMusicMuted()
    {
        var busIndex = AudioServer.GetBusIndex( "music" );
        return AudioServer.IsBusMute( busIndex );
    }

    public void SetMasterVolume( float value )
    {
        var busIndex = AudioServer.GetBusIndex( "Master" );
        AudioServer.SetBusVolumeLinear( busIndex, value / 100 );
    }

    public float GetMasterVolume()
    {
        var busIndex = AudioServer.GetBusIndex( "Master" );
        return AudioServer.GetBusVolumeLinear( busIndex ) * 100;
    }

    public void SetMusicVolume( float value )
    {
        var busIndex = AudioServer.GetBusIndex( "music" );
        AudioServer.SetBusVolumeLinear( busIndex, value / 100 );
    }

    public float GetMusicVolume()
    {
        var busIndex = AudioServer.GetBusIndex( "music" );
        return AudioServer.GetBusVolumeLinear( busIndex ) * 100;
    }

    public void SetSfxVolume( float value )
    {
        var busIndex = AudioServer.GetBusIndex( "sfx" );
        AudioServer.SetBusVolumeLinear( busIndex, value / 100 );
    }

    public float GetSfxVolume()
    {
        var busIndex = AudioServer.GetBusIndex( "sfx" );
        return AudioServer.GetBusVolumeLinear( busIndex ) * 100;
    }

    public void CloseSettings()
    {
        Game.MainScene.CloseSettings();
    }
}
