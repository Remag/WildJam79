using Godot;
using System;

public partial class ShipTrail : Node
{
    [Export]
    private GpuParticles2D _particles;
    [Export]
    private AnimationTree _animations;
    private AnimationNodeStateMachinePlayback _stateMachine;

    public override void _Ready()
    {
        base._Ready();
	    _stateMachine = _animations.Get( "parameters/playback").As<AnimationNodeStateMachinePlayback>();
    }

    public void ShowTrail()
    {
        _stateMachine.Travel( "Appear" );
        _particles.Emitting = true;
    }

    public void HideTrail()
    {
        _particles.Emitting = false;
        _stateMachine.Travel( "Disappear" );
    }
}
