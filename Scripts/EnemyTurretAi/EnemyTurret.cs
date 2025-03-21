using Godot;

namespace WildJam78.Scripts.EnemyTurretAi;

public partial class EnemyTurret : Node2D {
    
    [Export]
    private float _aiAttackShootDelayMin = 0.2F;
    [Export]
    private float _aiAttackShootDelayMax = 0.2F;
    [Export]
    private float _aiAttackShootDuration = 0.6F;
    [Export]
    private float _aiAttackShootPause = 1.2F;
    
    [Export]
    private BulletSpawner _bulletSpawner;
    
    [Export]
    private AudioStreamPlayer2D _shootSoundPlayer2D;
    
    [Export]
    private Node2D _shootTarget;
    [Export]
    private Node2D _turretBase;
    
    private bool _isAiEnabled = true;
    
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
        if( _isAiEnabled ) {

	        if( Game.Player != null ) {
		        _shootTarget.GlobalPosition = Game.Player.GlobalPosition;
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
				_currentAttackState = AttackState.Shooting;
				InitShootAttackState();
				break;
		}
	}

	private void InitPauseAttackState()
	{
		_attackStateDuration = _aiAttackShootPause;
		_isShooting = false;
		_currentShootDelay = 0;
	}

	private void InitShootAttackState()
	{
		_attackStateDuration = _aiAttackShootDuration;
		_isShooting = true;
		_currentShootDelay = GetAttackShootDelay();
	}

	private void ShootPlayer( double delta )
	{
		if( !_isShooting ) {
			return;
		}
		_currentShootDelay -= delta;
		if( _currentShootDelay < 0 ) {
			_currentShootDelay = GetAttackShootDelay();
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
		_bulletSpawner.SpawnBullets( this, targetPos, isEnemyBullet:true );
	}
	
	private float GetAttackShootDelay()
	{
		return Rng.RandomRange( _aiAttackShootDelayMin, _aiAttackShootDelayMax );
	}
}