using Godot;
using System;

public partial class MainScene : Node {
    [Export]
    private PackedScene _settingsPrefab;
    [Export]
    private Node _hud;

    private Node _settings;

    public override void _EnterTree()
    {
        Game.MainScene = this;
        base._EnterTree();
    }

    public override void _Input( InputEvent inputEvent )
    {
        if( inputEvent.IsActionPressed( "Pause" ) ) {
            if( _settings == null ) {
                ShowSettings();
            } else {
                CloseSettings();
            }
        }
    }

    public void PauseGame( bool isPaused )
    {
        GetTree().Paused = isPaused;
    }

    public void ShowSettings()
    {
        PauseGame( true );
        Game.Field.WorldAudioManager.ButtonClickPlay();
        _settings = _settingsPrefab.Instantiate();
        _hud.AddChild( _settings );
    }

    public void CloseSettings()
    {
        Game.Field.WorldAudioManager.ButtonClickPlay();
        _settings.QueueFree();
        _settings = null;
        PauseGame( false );
    }
}
