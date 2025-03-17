using System;
using Godot;

namespace WildJam78.Scripts.EnemyMove;

public class EnemyMoveHandler {

    private EnemyMoveConfig _config;
    
    public enum MoveState {
        Turning,
        Accelerating,
        Moving
    }
    private MoveState _currentMoveState = MoveState.Moving;
    private double _moveStateDuration = 0;
    
    private bool _isAccelerating = false;
    private float _targetRotation = 0;
    private float _baseRotation = (float)Math.PI / 2;
    
    private Vector2 _currentAccel = new();
    private Vector2 _currentVelocity = new();

    private Node2D _enemyShip = null;
    
    public EnemyMoveHandler( EnemyMoveConfig config, float targetRotation, Node2D enemyShip )
    {
        _config = config;
        _targetRotation = targetRotation;
        _enemyShip = enemyShip;
    }

    public void OnPhysicsProcess( double delta )
    {
        handleMoveAi( delta );
        turnEnemy( delta );
        calcPosition( delta );
    }
    
    private void handleMoveAi( double delta )
    {
        _moveStateDuration -= delta;
        if( _moveStateDuration <= 0 ) {
            findNewMoveState();
        }
    }

    private void findNewMoveState()
    {
        switch( _currentMoveState ) {
            case MoveState.Turning:
                _currentMoveState = MoveState.Accelerating;
                initAccelState();
                break;
            case MoveState.Accelerating:
                _currentMoveState = MoveState.Moving;
                initMoveState();
                break;
            case MoveState.Moving:
                _currentMoveState = MoveState.Turning;
                initTurnState();
                break;
        }
    }
    
    private void initAccelState()
    {
        _moveStateDuration = _config.aiAccelDuration;
        _isAccelerating = true;
        updateAccel();
    }

    private void initMoveState()
    {
        _moveStateDuration = _config.aiMoveDuration;
        _isAccelerating = false;
        updateAccel();
    }

    private void initTurnState()
    {
        if( Game.Player == null ) {
            return;
        }
        _moveStateDuration = _config.aiTurnDuration;
        var playerPos = Game.Player.GlobalPosition;
        var dirToPlayer = playerPos - _enemyShip.GlobalPosition;
        _targetRotation = dirToPlayer.Angle() + Rng.RandomRange( -_config.aiTurnVarianceRad, _config.aiTurnVarianceRad );
    }
    
    private void turnEnemy( double delta )
    {
        var rotationDir = Math.Sign( _enemyShip.Rotation - _baseRotation - _targetRotation );
        var rotationDelta = delta * _config.rotationSpeedRadSec * rotationDir;
        _enemyShip.Rotation -= (float)rotationDelta;

        updateAccel();
    }

    private void updateAccel()
    {
        _currentAccel = _isAccelerating ? new Vector2( _config.maxAccelPxSec, 0 ).Rotated( _enemyShip.Rotation - _baseRotation ) : Vector2.Zero;
    }

    private void calcPosition( double delta )
    {
        var deltaF = (float)delta;
        var accelValue = _config.maxAccelPxSec * _currentAccel.Normalized();
        _currentVelocity += deltaF * accelValue;
        var velValue = Math.Min( _config.maxVelocityPxSec, _currentVelocity.Length() );
        _currentVelocity = velValue * _currentVelocity.Normalized();
        _enemyShip.Position += deltaF * _currentVelocity;
    }
    
    private void resetMoving()
    {
        _isAccelerating = false;
        _currentAccel = Vector2.Zero;
        _currentMoveState = MoveState.Moving;
        _moveStateDuration = 0.0f;
    }

}