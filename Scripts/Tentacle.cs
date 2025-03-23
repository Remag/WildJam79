using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Tentacle : Node2D {
    [Export]
    protected float _tentacleExtendSpeed = 100;
    [Export]
    protected float _tentacleShrinkSpeed = 200;
    [Export]
    private double _tentacleAttachDelay = 0;
    [Export]
    private double _lineSavePeriod;

    protected enum TentacleMode {
        Extend,
        Keep,
        Shrink
    }
    protected TentacleMode _currentMode = TentacleMode.Extend;

    [Export]
    protected TentacleLine _tentacleLine;
    [Export]
    private AudioStreamPlayer _catchSoundPlayer;
    public FoodSource AttachedEntity { get; private set; }

    private double _currentKeepTime = 0;
    private double _currentExtendTime = 0;
    private double _currentLineSaveTime;
    
    protected bool _ignoreShrinkSpeedReduction = false;

    public Node2D GetEndAnchor()
    {
        return _tentacleLine.EndAnchor;
    }

    public void AbortExtend()
    {
        if( _currentMode == TentacleMode.Extend ) {
            SetShrink();
        }
    }

    protected void SetShrink()
    {
        Game.Field.WorldAudioManager.TentacleSoundPlay();
        _currentMode = TentacleMode.Shrink;
    }

    public void Attach( FoodSource entity )
    {
        AttachedEntity = entity;
        _currentMode = TentacleMode.Keep;
        _catchSoundPlayer.Play();
    }

    public override void _Ready()
    {
        updateEndRotation( _tentacleLine.Points );
        Game.Field.WorldAudioManager.TentacleSoundPlay();
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
    }

    private void updateExtend( float delta )
    {
        updateLineSave( delta );
    }

    private void updateLineSave( float delta )
    {
        _currentLineSaveTime += delta;
        if( _currentLineSaveTime >= _lineSavePeriod ) {
            _currentLineSaveTime = 0;
            saveLinePositions();
        }
    }

    private void updateKeep( float delta )
    {
        Debug.Assert( AttachedEntity != null );
        if( !IsInstanceValid( AttachedEntity ) ) {
            SetShrink();
            return;
        }

        _tentacleLine.EndAnchor.GlobalPosition = AttachedEntity.GetTentacleAnchor().GlobalPosition;
        updateLineSave( delta );

        _currentKeepTime += delta;
        if( _currentKeepTime >= _tentacleAttachDelay ) {
            SetShrink();
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
        var shrinkSpeedReduction = _ignoreShrinkSpeedReduction ? 1f : AttachedEntity?.ShrinkSpeedReduction ?? 1f;
        var shrinkDistance = _tentacleShrinkSpeed * delta / shrinkSpeedReduction;
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
            AttachedEntity.OnBroughtToPlayer( this );
        }
        Game.Player.DestroyTentacle( this );
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
}
