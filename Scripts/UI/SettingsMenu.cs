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

    }

    public bool IsAllSoundMuted()
    {
        return false;
    }

    public void MuteMusic( bool isMuted )
    {

    }

    public bool IsMusicMuted()
    {
        return false;
    }

    public void SetMasterVolume( float value )
    {

    }

    public float GetMasterVolume()
    {
        return 0.0f;
    }

    public void SetMusicVolume( float value )
    {

    }

    public float GetMusicVolume()
    {
        return 0.0f;
    }

    public void SetSfxVolume( float value )
    {

    }

    public float GetSfxVolume()
    {
        return 0.0f;
    }

    public void CloseSettings()
    {
        Game.MainScene.CloseSettings();
    }
}
