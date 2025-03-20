using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Tentacle : Node2D {
    [Export]
    private float _mouseGravity = 5.0f;
    [Export]
    private float _tentacleExtendSpeed = 100;
    [Export]
    private float _tentacleShrinkSpeed = 200;
    [Export]
    public float TentacleMaxDistance = 100;
    [Export]
    public double TentacleMaxTime = 6;
    [Export]
    private double _tentacleAttachDelay = 0;
    [Export]
    private double _lineSavePeriod;

    private enum TentacleMode {
        Extend,
        Keep,
        Shrink
    }
    private TentacleMode _currentMode = TentacleMode.Extend;

    [Export]
    private TentacleLine _tentacleLine;
    [Export]
    private AudioStreamPlayer _tentacleSoundPlayer;
    [Export]
    private AudioStreamPlayer _catchSoundPlayer;
    public FoodSource AttachedEntity { get; private set; }

    private Vector2 _currentVelocity;

    private double _currentKeepTime = 0;
    private double _currentExtendTime = 0;
    private double _currentLineSaveTime;
    private bool _isFirstUpdate = true;

    public void Initialize( Player player )
    {
        var endAnchor = _tentacleLine.EndAnchor;
        endAnchor.GlobalPosition = player.GlobalPosition;
    }

    public Node2D GetEndAnchor()
    {
        return _tentacleLine.EndAnchor;
    }

    public void AbortExtend()
    {
        if( _currentMode == TentacleMode.Extend ) {
            _currentMode = TentacleMode.Shrink;
        }
    }

    public void Attach( FoodSource entity )
    {
        AttachedEntity = entity;
        _currentMode = TentacleMode.Keep;
        _catchSoundPlayer.Play();
    }

    public override void _Ready()
    {
        var canvasMousePos = GetViewport().GetMousePosition();
        var globalMousePos = Game.Camera.GetCanvasTransform().AffineInverse() * canvasMousePos;
        var endAnchor = _tentacleLine.EndAnchor;
        var dirDelta = globalMousePos - endAnchor.GlobalPosition;

        _currentVelocity = new Vector2( _tentacleExtendSpeed, 0 ).Rotated( dirDelta.Angle() );
        updateEndRotation( _tentacleLine.Points );
        _tentacleSoundPlayer.Play();
    }

    public override void _PhysicsProcess( double delta )
    {
        _tentacleLine.StartAnchor.GlobalPosition = Game.Player.GlobalPosition;

        var deltaF = (float)delta;
        switch( _currentMode ) {
            case TentacleMode.Extend:
                updateExtend( deltaF );
                break;
            case TentacleMode.Keep:
                updateKeep( deltaF );
                break;
            case TentacleMode.Shrink:
                updateShrink( deltaF );
                break;
        }
        _isFirstUpdate = false;
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

        updateLineSave( delta );

        _currentExtendTime += delta;
        if( _currentExtendTime >= TentacleMaxTime || ( _tentacleLine.EndAnchor.GlobalPosition - _tentacleLine.StartAnchor.GlobalPosition ).LengthSquared() >= TentacleMaxDistance * TentacleMaxDistance ) {
            _currentMode = TentacleMode.Shrink;
        }
    }

    private void updateLineSave( float delta )
    {
        _currentLineSaveTime += delta;
        if( !_isFirstUpdate && _currentLineSaveTime >= _lineSavePeriod ) {
            _currentLineSaveTime = 0;
            saveLinePositions();
        }
    }

    private void updateKeep( float delta )
    {
        Debug.Assert( AttachedEntity != null );
        if( !IsInstanceValid( AttachedEntity ) ) {
            _currentMode = TentacleMode.Shrink;
            return;
        }

        _tentacleLine.EndAnchor.GlobalPosition = AttachedEntity.GetTentacleAnchor().GlobalPosition;
        updateLineSave( delta );

        _currentKeepTime += delta;
        if( _currentKeepTime >= _tentacleAttachDelay ) {
            _currentMode = TentacleMode.Shrink;
            if( !AttachedEntity.TryTentaclePull( this ) ) {
                var endDir = getTentacleEndDir();
                AttachedEntity.ApplyCentralImpulse( -endDir.Normalized() * 500 );
                AttachedEntity = null;
            }
        }
    }

    private Vector2 getTentacleEndDir()
    {
        var points = _tentacleLine.Points;
        return points[0] - points[1];
    }

    private void updateShrink( float delta )
    {
        var shrinkDistance = _tentacleShrinkSpeed * delta;
        var points = _tentacleLine.Points;
        Debug.Assert( points.Length >= 2 );

        if( AttachedEntity != null && IsInstanceValid( AttachedEntity ) ) {
            AttachedEntity.GlobalPosition = _tentacleLine.EndAnchor.GlobalPosition;
        }

        var currentDistance = shrinkDistance;
        for( var i = 0; i < points.Length - 1; i++ ) {
            var deltaVector = points[i + 1] - points[i];
            var distance = deltaVector.Length();
            if( distance > currentDistance ) {
                points[i] += deltaVector * currentDistance / distance;
                var newPoints = new Vector2[points.Length - i];
                Array.Copy( points, i, newPoints, 0, points.Length - i );
                _tentacleLine.Points = newPoints;
                _tentacleLine.EndAnchor.Position = newPoints[0];
                updateEndRotation( newPoints );
                return;
            } else {
                currentDistance -= distance;
            }
        }

        if( AttachedEntity != null && IsInstanceValid( AttachedEntity ) ) {
            AttachedEntity.OnBroughtToPlayer();
        }
        Game.Player.DestroyTentacle();
    }

    private enum LineDecision {
        Remove,
        Keep,
        Add
    }

    private LineDecision decidePointAction( Vector2 p1, Vector2 p2, Vector2 p3 )
    {
        var p3p2 = ( p3 - p2 ).LengthSquared();
        var p3p1 = ( p3 - p1 ).LengthSquared();

        if( p3p1 < p3p2 ) {
            return LineDecision.Remove;
        }
        if( isAngleTooSteep( p1, p2, p3 ) ) {
            return LineDecision.Remove;
        }
        if( p3p1 > 1.1 * p3p2 ) {
            return LineDecision.Add;
        }
        return LineDecision.Keep;
    }

    private bool isDistanceSmall( Vector2 p1, Vector2 p2 )
    {
        var distanceSq = 200;
        return ( p2 - p1 ).LengthSquared() < distanceSq;
    }

    private bool isAngleTooSteep( Vector2 pos0, Vector2 pos1, Vector2 pos2 )
    {
        var dir1 = pos0 - pos1;
        var dir2 = pos2 - pos1;
        var angle = dir1.AngleTo( dir2 );
        return Math.Abs( angle ) < Mathf.Pi / 2;
    }

    private void saveLinePositions()
    {
        var lineList = _tentacleLine.Points.ToList();
        handleStartPoints( lineList );
        handleEndPoints( lineList );

        var newPoints = lineList.ToArray();
        updateEndRotation( newPoints );
        _tentacleLine.Points = newPoints;
    }

    private void handleStartPoints( List<Vector2> lineList )
    {
        var start1 = _tentacleLine.StartAnchor.Position;

        if( lineList.Count < 3 ) {
            if( !isDistanceSmall( lineList[0], lineList[1] ) ) {
                lineList.Add( start1 );
            }
            return;
        }

        var start2 = lineList[lineList.Count - 2];
        var start3 = lineList[lineList.Count - 3];

        var decision = decidePointAction( start3, start2, start1 );
        switch( decision ) {
            case LineDecision.Keep:
                break;
            case LineDecision.Remove:
                if( lineList.Count > 3 ) {
                    lineList.RemoveAt( lineList.Count - 1 );
                }
                break;
            case LineDecision.Add:
                lineList.Add( start1 );
                break;
        }
    }

    private void handleEndPoints( List<Vector2> lineList )
    {
        var end1 = _tentacleLine.EndAnchor.Position;
        if( lineList.Count < 3 ) {
            if( !isDistanceSmall( lineList[0], lineList[1] ) ) {
                lineList.Insert( 0, end1 );
            }
            return;
        }
        lineList.Insert( 0, end1 );
    }

    private void updateEndRotation( Vector2[] lineList )
    {
        var endAnchor = _tentacleLine.EndAnchor;
        var lineDir = lineList[0] - lineList[1];
        if( lineDir.LengthSquared() > 1 ) {
            endAnchor.Rotation = lineDir.Angle();
        }
    }

    public void OnAreaCollision( Area2D area2D )
    {
        if( area2D is Shield enemyShield ) {
            AbortExtend();
        } else if( area2D.GetParent() is FoodSource foodSource ) {
            if( _currentMode != TentacleMode.Shrink ) {
                Attach( foodSource );
                foodSource.OnTentacleCollision();
            }
        }
    }
}
