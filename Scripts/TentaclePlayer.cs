using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class TentaclePlayer : Tentacle {
    [Export]
    private float _mouseGravity = 5.0f;
    [Export]
    public float TentacleMaxDistance = 100;
    [Export]
    public double TentacleMaxTime = 6;

    private double _currentExtendTime = 0;
    private Vector2 _currentVelocity;

    public void Initialize( Player player )
    {
        var endAnchor = _tentacleLine.EndAnchor;
        endAnchor.GlobalPosition = player.GlobalPosition;
    }

    public override void _Ready()
    {
        var canvasMousePos = GetViewport().GetMousePosition();
        var globalMousePos = Game.Camera.GetCanvasTransform().AffineInverse() * canvasMousePos;
        var endAnchor = _tentacleLine.EndAnchor;
        var dirDelta = globalMousePos - endAnchor.GlobalPosition;

        _currentVelocity = new Vector2( _tentacleExtendSpeed, 0 ).Rotated( dirDelta.Angle() );

        base._Ready();
    }

    public override void _PhysicsProcess( double delta )
    {
        _tentacleLine.StartAnchor.GlobalPosition = Game.Player.GlobalPosition;
        var deltaF = (float)delta;
        switch( _currentMode ) {
            case TentacleMode.Extend:
                updateExtend( deltaF );
                break;
        }

        base._PhysicsProcess( delta );
    }

    private void updateExtend( float delta )
    {
        var canvasMousePos = GetViewport().GetMousePosition();
        var globalMousePos = Game.Camera.GetCanvasTransform().AffineInverse() * canvasMousePos;
        var endAnchor = _tentacleLine.EndAnchor;
        var dirDelta = globalMousePos - endAnchor.GlobalPosition;
        _currentVelocity += dirDelta * _mouseGravity * delta;
        _currentVelocity = _currentVelocity.Normalized() * _tentacleExtendSpeed;
        endAnchor.GlobalPosition += _currentVelocity * delta;

        _currentExtendTime += delta;
        if( _currentExtendTime >= TentacleMaxTime || ( _tentacleLine.EndAnchor.GlobalPosition - _tentacleLine.StartAnchor.GlobalPosition ).LengthSquared() >= TentacleMaxDistance * TentacleMaxDistance ) {
            _currentMode = TentacleMode.Shrink;
        }
    }

    public void OnAreaCollision( Area2D area2D )
    {
        if( _currentMode == TentacleMode.Shrink ) {
            return;
        }

        if( area2D is Shield enemyShield ) {
            AbortExtend();
        } else if( area2D.GetParent() is FoodSource foodSource ) {
            Attach( foodSource );
            foodSource.OnTentacleCollision();
        } else if( area2D.GetParent() is BasicBullet missile ) {
            missile.HandleDestroy( true );
        }
    }
}
