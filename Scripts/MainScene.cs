using Godot;
using System;

public partial class MainScene : Node
{
    public override void _Input( InputEvent inputEvent )
    {
        if( inputEvent.IsActionPressed( "Pause" ) ) {
            GetTree().Paused = !GetTree().Paused;
        }
    }
}
