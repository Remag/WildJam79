using Godot;
using System;

public partial class FinalCutscene : Node
{
    [Export]
    private AnimationPlayer _animations;

    public void Play()
    {
        _animations.Play( "Cutscene" );
    }
}
