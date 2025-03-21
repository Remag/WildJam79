using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class TentacleAuto : Tentacle {

    private FoodSource _target;

    public void Initialize( FoodSource target )
    {
        _target = target;

        var duration = Rng.RandomRange( 0.0, 1.0 );

        _tentacleLine.EndAnchor.GlobalPosition = Game.Player.GlobalPosition;
        var posTween = _tentacleLine.EndAnchor.CreateTween();
        posTween.TweenProperty( _tentacleLine.EndAnchor, "global_position", target.GlobalPosition, duration ).SetEase( Tween.EaseType.In );
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess( double delta )
    {
        base._PhysicsProcess( delta );
    }

    public void OnAreaCollision( Area2D area2D )
    {
        if( _currentMode == TentacleMode.Shrink ) {
            return;
        }

        if( area2D.GetParent() == _target ) {
            Attach( _target );
            _target.OnTentacleCollision();
        }
    }
}
