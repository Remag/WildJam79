using System;
using Godot;

namespace WildJam78.Scripts.EnemyMove;

public class EnemyMoveHandlerRigid {

    [Export] private EnemyMoveConfigRigid _config;
    
    private EnemyShip _ship = null;
    private Vector2 _currentTarget;
    private bool _isTargetReached = false;

    private float _currentTargetChooseTime = 0;
    
    public EnemyMoveHandlerRigid( EnemyMoveConfigRigid config, EnemyShip enemyShip )
    {
        _config = config;
        _ship = enemyShip;
        _currentTarget = ChooseTarget();
    }

    public void OnShipIntegrateForces( PhysicsDirectBodyState2D state )
    {
        var shipGlobalPosition = _ship.GlobalPosition;
        _currentTargetChooseTime += state.Step;
        var isTargetNotReachCondition = !_isTargetReached && _currentTargetChooseTime >= _config.newTargetChooseTime;
        var isTargetReachCondition = _isTargetReached && _currentTargetChooseTime >= _config.targetReachDelay;
        if( isTargetNotReachCondition || isTargetReachCondition ) {
            _currentTargetChooseTime = 0;
            _isTargetReached = false;
            _currentTarget = ChooseTarget();
        }
        if( (_currentTarget - _ship.GlobalPosition).Length() <= _config.targetReachRadius ) {
            _isTargetReached = true;
            _currentTargetChooseTime = 0;
        }
        
        
        state.ApplyCentralForce( Vector2.FromAngle( state.Transform.Rotation ) * _config.acceleration );
        state.LinearVelocity = state.LinearVelocity.LimitLength( _config.maxVelocity );

        if( !_isTargetReached ) {
            var targetAngle = ( _currentTarget - shipGlobalPosition ).Angle();
            var targetAngleDiff = targetAngle - state.Transform.Rotation;
            state.AngularVelocity = Math.Sign( targetAngleDiff ) *
                                    Math.Min( _config.maxAngularVelocity, Math.Abs( targetAngleDiff / state.Step ) );
        }
    }

    private Vector2 ChooseTarget()
    {
        var targetPosition  = Game.Player?.GlobalPosition ?? _ship.GlobalPosition;

        var randomRotation = Rng.RandomRange( -_config.rngAngle, _config.rngAngle );
        var randomRadius = Rng.RandomRange( _config.minRadius, _config.maxRadius );
        return ( _ship.GlobalPosition - targetPosition ).Normalized().Rotated( randomRotation ) * randomRadius + targetPosition;
    }

}