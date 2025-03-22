using System;
using Godot;

namespace WildJam78.Scripts.EnemyTurretAi;

public partial class EnemyTurret : Node2D {
    
    [Export]
    private float _aiAttackShootDelayMin = 0.2F;
    [Export]
    private float _aiAttackShootDuration = 0.6F;
    [Export]
    private float _aiAttackShootPauseMin = 1.2F;
    [Export]
    private float _aiAttackShootPauseMax = 1.2F;
    
    [Export]
    private BulletSpawner _bulletSpawner;

    [Export] 
    private float _angularVelocity = 1f;
    [Export] 
    private float _activationDistance = 200f;
    
    [Export]
    private AudioStreamPlayer2D _shootSoundPlayer2D;
    
    [Export]
    private Node2D _shootTarget;
    [Export]
    private Node2D _turretBase;
    [Export]
    private EnemyShip _enemyShip;
    [Export]
    private Node2D _spawnPoint;
    
    private enum AttackState {
	    Shooting,
	    Pausing
    }
    private AttackState _currentAttackState = AttackState.Pausing;
    private double _attackStateDuration = 0;
    private bool _isShooting = false;
    private double _currentShootDelay = 0;
    
    public override void _PhysicsProcess( double delta )
    {
        if( _enemyShip.IsShootingEnabled ) {

	        if( Game.Player != null ) {
		        var vectorToPlayer = Game.Player.GlobalPosition - GlobalPosition;
		        var vectorToShootTarget = _shootTarget.GlobalPosition - GlobalPosition;
		        var angle = vectorToShootTarget.AngleTo( vectorToPlayer );
		        float rotationAngle;
		        var maxAngle = _angularVelocity * (float)delta;
		        if( Math.Abs( angle ) > maxAngle ) {
			        rotationAngle = Math.Sign( angle ) * maxAngle;
		        } else {
			        rotationAngle = angle;
		        }

		        _shootTarget.GlobalPosition = GlobalPosition + vectorToShootTarget.Rotated( rotationAngle ).Normalized() * vectorToPlayer.Length();
	        }

	        _turretBase.Rotation = ( Position - _shootTarget.Position ).Angle() - float.Pi / 2;

	        HandleAttackAi( delta );
        }
    }
    
    private void HandleAttackAi( double delta )
	{
		_attackStateDuration -= delta;
		if( _attackStateDuration <= 0 ) {
			FindNewAttackState();
		}

		ShootPlayer( delta );
	}

	private void FindNewAttackState()
	{
		switch( _currentAttackState ) {
			case AttackState.Shooting:
				_currentAttackState = AttackState.Pausing;
				InitPauseAttackState();
				break;
			case AttackState.Pausing:
				if( IsTargetInDistance() ) {
					_currentAttackState = AttackState.Shooting;
					InitShootAttackState();
				}
				break;
		}
	}

	private void InitPauseAttackState()
	{
		_attackStateDuration = GetAttackShootPause();
		_isShooting = false;
		_currentShootDelay = 0;
	}

	private void InitShootAttackState()
	{
		_attackStateDuration = _aiAttackShootDuration;
		_isShooting = true;
		_currentShootDelay = 0;
	}

	private void ShootPlayer( double delta )
	{
		if( !_isShooting ) {
			return;
		}
		_currentShootDelay -= delta;
		if( _currentShootDelay < 0 ) {
			_currentShootDelay = _aiAttackShootDelayMin;
			SpawnBulletPattern();
			_shootSoundPlayer2D.Play();

        }

	}

	private void SpawnBulletPattern()
	{
		if( Game.Player == null ) {
			return;
		}
		var targetPos = _shootTarget.GlobalPosition;
		_bulletSpawner.SpawnBullets( this, _spawnPoint ?? this, targetPos, isEnemyBullet:true );
	}
	
	private float GetAttackShootPause()
	{
		return Rng.RandomRange( _aiAttackShootPauseMin, _aiAttackShootPauseMax );
	}

	private bool IsTargetInDistance()
	{
		return ( _shootTarget.GlobalPosition - GlobalPosition ).Length() <= _activationDistance;
	}
}