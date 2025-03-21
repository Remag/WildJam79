using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class TentacleAuto : Tentacle {

    private FoodSource _target;

    [Export]
    private float _startGravity = 0.0f;
    [Export]
    private float _gravityGrowth = 0.5f;
    [Export]
    private float _gravityDelay = 0.1f;
    private Vector2 _currentVelocity;

    public void Initialize( FoodSource target )
    {
        _target = target;

        _tentacleLine.EndAnchor.GlobalPosition = Game.Player.GlobalPosition;
        _currentVelocity = Vector2.FromAngle( Rng.RandomRange( 0, Mathf.Pi * 2 ) );
        _currentVelocity *= _tentacleExtendSpeed;
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess( double delta )
    {
        if( !IsInstanceValid( _target ) ) {
            AbortExtend();
        }

        var endAnchor = _tentacleLine.EndAnchor;
        var deltaF = (float)delta;
        _gravityDelay -= deltaF;
        if( _gravityDelay <= 0 ) {
            var dirDelta = _target.GlobalPosition - endAnchor.GlobalPosition;
            _currentVelocity += dirDelta * _startGravity * deltaF;
            _startGravity += _gravityGrowth;
            _currentVelocity = _currentVelocity.Normalized() * _tentacleExtendSpeed;
        }

        endAnchor.GlobalPosition += _currentVelocity * deltaF;

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
