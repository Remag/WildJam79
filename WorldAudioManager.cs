using Godot;
using System;

public partial class WorldAudioManager : Node {
    [Export]
    public AudioStreamPlayer BgmPlayer { get; set; }
    string _stageName;

    public override void _Ready()
    {
        _stageName = Game.StageName;
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
}
