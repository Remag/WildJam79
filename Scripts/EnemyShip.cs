using Godot;
using System;
using System.ComponentModel;

public partial class EnemyShip : Node2D {
    [Export]
    private int _maxHp = 3;
    private int _currentHp = 3;
    [Export]
    private float _maxVelocityPxSec = 4;
    [Export]
    private float _maxAccelPxSec = 5;
    [Export]
    private float _rotationSpeedRadSec = 0.1F;

    [Export]
    private float _aiTurnDuration = 1;
    [Export]
    private float _aiAccelDuration = 3;
    [Export]
    private float _aiMoveDuration = 5;

    [Export]
    private float _aiTurnVarianceRad = 0.5F;

    [Export]
    private float _aiAttackShootDelay = 0.2F;
    [Export]
    private float _aiAttackShootDuration = 0.6F;
    [Export]
    private float _aiAttackShootPause = 1.2F;

    [Export]
    private PackedScene _bulletPrefab;
    [Export]
    public PackedScene CorePrefab;

    private Vector2 _currentAccel = new();
    private Vector2 _currentVelocity = new();

    public enum MoveState {
        Turning,
        Accelerating,
        Moving
    }
    private MoveState _currentMoveState = MoveState.Moving;
    private double _moveStateDuration = 0;

    public enum AttackState {
        Shooting,
        Pausing
    }
    private AttackState _currentAttackState = AttackState.Pausing;
    private double _attackStateDuration = 0;
    private bool _isShooting = false;
    private double _currentShootDelay = 0;

    private bool _isAccelerating = false;
    private float _targetRotation = 0;
    private float _baseRotation = (float)Math.PI / 2;

    public bool IsDead = false;

    public override void _Ready()
    {
        _targetRotation = Rotation;
        _currentHp = _maxHp;
    }

    public override void _Process( double delta )
    {
        handleMoveAi( delta );
        handleAttackAi( delta );
        turnEnemy( delta );
        calcPosition( delta );
    }

    private void handleAttackAi( double delta )
    {
        _attackStateDuration -= delta;
        if( _attackStateDuration <= 0 ) {
            findNewAttackState();
        }

        shootPlayer( delta );
    }

    private void findNewAttackState()
    {
        switch( _currentAttackState ) {
            case AttackState.Shooting:
                _currentAttackState = AttackState.Pausing;
                initPauseAttackState();
                break;
            case AttackState.Pausing:
                _currentAttackState = AttackState.Shooting;
                initShootAttackState();
                break;
        }
    }

    private void initPauseAttackState()
    {
        _attackStateDuration = _aiAttackShootPause;
        _isShooting = false;
        _currentShootDelay = 0;
    }

    private void initShootAttackState()
    {
        _attackStateDuration = _aiAttackShootDuration;
        _isShooting = true;
        _currentShootDelay = _aiAttackShootDelay;
    }

    private void shootPlayer( double delta )
    {
        if( !_isShooting ) {
            return;
        }
        _currentShootDelay -= delta;
        if( _currentShootDelay < 0 ) {
            spawnBullet();
            _currentShootDelay = _aiAttackShootDelay;
        }

    }

    private void spawnBullet()
    {
        if( Game.Player == null ) {
            return;
        }
        
        var bullet = _bulletPrefab.Instantiate<BasicBullet>();
        var playerPos = Game.Player.Position;
        var bulletDir = playerPos - Position;
        bullet.Rotation = bulletDir.Angle();
        bullet.Position = Position;
        bullet.SetCollisionParams( Game.EnemyBulletLayer, Game.PlayerLayer );
        GetParent().AddChild( bullet );
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
        _moveStateDuration = _aiAccelDuration;
        _isAccelerating = true;
        updateAccel();
    }

    private void initMoveState()
    {
        _moveStateDuration = _aiMoveDuration;
        _isAccelerating = false;
        updateAccel();
    }

    private void initTurnState()
    {
        if( Game.Player == null ) {
            return;
        }
        _moveStateDuration = _aiTurnDuration;
        var playerPos = Game.Player.GlobalPosition;
        var dirToPlayer = playerPos - GlobalPosition;
        _targetRotation = dirToPlayer.Angle() + Rng.RandomRange( -_aiTurnVarianceRad, _aiTurnVarianceRad );
    }

    private void turnEnemy( double delta )
    {
        var rotationDir = Math.Sign( Rotation - _baseRotation - _targetRotation );
        var rotationDelta = delta * _rotationSpeedRadSec * rotationDir;
        Rotation -= (float)rotationDelta;

        updateAccel();
    }

    private void updateAccel()
    {
        _currentAccel = _isAccelerating ? new Vector2( _maxAccelPxSec, 0 ).Rotated( Rotation - _baseRotation ) : Vector2.Zero;
    }

    private void calcPosition( double delta )
    {
        var deltaF = (float)delta;
        var accelValue = _maxAccelPxSec * _currentAccel.Normalized();
        _currentVelocity += deltaF * accelValue;
        var velValue = Math.Min( _maxVelocityPxSec, _currentVelocity.Length() );
        _currentVelocity = velValue * _currentVelocity.Normalized();
        Position += deltaF * _currentVelocity;

        limitPosition();
    }

    private void limitPosition()
    {
        var rect = GetViewport().GetVisibleRect();
        if( GlobalPosition.X < rect.Position.X ) {
            GlobalPosition = new Vector2( rect.Position.X, GlobalPosition.Y );
            _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
            resetMoving();
        }
        if( GlobalPosition.X > rect.End.X ) {
            GlobalPosition = new Vector2( rect.End.X, GlobalPosition.Y );
            _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
            resetMoving();
        }
        if( GlobalPosition.Y < rect.Position.Y ) {
            GlobalPosition = new Vector2( GlobalPosition.X, rect.Position.Y );
            _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
            resetMoving();
        }
        if( GlobalPosition.Y > rect.End.Y ) {

            GlobalPosition = new Vector2( GlobalPosition.X, rect.End.Y );
            _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
            resetMoving();
        }
    }

    private void resetMoving()
    {
        _isAccelerating = false;
        _currentAccel = Vector2.Zero;
        _currentMoveState = MoveState.Moving;
        _moveStateDuration = 0.0f;
    }

    public void OnBulletCollision( Area2D bullet )
    {
        _currentHp--;
        if( _currentHp == 0 ) {
            IsDead = true;
            Game.Map.RemoveExistingShip();
            QueueFree();
        }
        Modulate = Colors.White.Lerp( Colors.Red, (float)_currentHp / _maxHp );
    }

    public void OnTentacleCollision( Area2D tentacle )
    {
        Game.Player?.Assimilate( this );
        IsDead = true;
        Game.Map.RemoveExistingShip();
        QueueFree();
    }
}
