using Godot;
using System;
using System.ComponentModel;
using WildJam78.Scripts.EnemyMove;

public partial class EnemyShip : Node2D {
	[Export]
	private int _maxHp = 3;
	private int _currentHp = 3;
	[Export] 
	private EnemyMoveConfig _config = new();

	[Export]
	private float _aiAttackShootDelay = 0.2F;
	[Export]
	private float _aiAttackShootDuration = 0.6F;
	[Export]
	private float _aiAttackShootPause = 1.2F;

	private bool _isAiEnabled = true;

	[Export]
	private PackedScene _bulletPrefab;
	public enum AttackType {
		Aim, Burst, CirclePattern, Pattern2
	}
	[Export]
	private AttackType _attackType = AttackType.Aim;
	[Export]
	private float _angle = 0.3F;
	[Export]
	public PackedScene CorePrefab;
	[Export]
	public Node2D VisualNode { get; set; }
	[Export]
	private ShipTrail _trail;

	private EnemyMoveHandler _moveHandler = null; 

	public enum AttackState {
		Shooting,
		Pausing
	}
	private AttackState _currentAttackState = AttackState.Pausing;
	private double _attackStateDuration = 0;
	private bool _isShooting = false;
	private double _currentShootDelay = 0;

	public bool IsDead = false;

	[Export]
	private Node2D _offscreenIndicator = null;
	[Export]
	private Area2D _hitbox = null;
	[Export]
	private Shield _shield = null;

	public override void _Ready()
	{
		_moveHandler = new EnemyMoveHandler(config:_config, targetRotation: Rotation, enemyShip:this );
		
		_currentHp = _maxHp;
		
		if( _shield != null ) {
			_shield.ShieldRegenerate += OnShieldRegenerate;
			_shield.ShieldDestroy += OnShieldDestroy;
			_hitbox.Monitorable = false;
		}

		if( _isAiEnabled && _offscreenIndicator != null) {
			RemoveChild( _offscreenIndicator );
			GetParent().AddChild( _offscreenIndicator );
		}
	}

	public override void _PhysicsProcess( double delta )
	{
		if( _isAiEnabled ) {
			_moveHandler.OnPhysicsProcess( delta );
			handleAttackAi( delta );
			handleIndicator();
		}
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
			spawnBullet(_attackType);
			_currentShootDelay = _aiAttackShootDelay;
		}

	}

	private void spawnBullet( AttackType attackType )
	{
		if( Game.Player == null ) {
			return;
		}
		if( attackType == AttackType.Aim ) {
			var playerPos = Game.Player.Position;
			var bulletDir = playerPos - Position;
			instantiateBullet(bulletDir.Angle());
		}
		if( attackType == AttackType.Burst ) {
			
			var playerPos = Game.Player.Position;
			var bulletDir = playerPos - Position;
			instantiateBullet(bulletDir.Angle() + Rng.RandomRange( -_angle, _angle ));
		}
		if( attackType == AttackType.CirclePattern ) {
			var playerPos = Game.Player.Position;
			var bulletDir = playerPos - Position;
			var Count = 12;
			for( var i = 0f; i < 2*Mathf.Pi; i += 2*Mathf.Pi/Count ) {
				instantiateBullet(bulletDir.Angle() + i);
			}
		}
		if( attackType == AttackType.Pattern2 ) {
			var playerPos = Game.Player.Position;
			var bulletDir = playerPos - Position;
			var Count = 3;
			instantiateBullet(bulletDir.Angle());
			for( var i = 1; i <= Count/2; i += 1 ) {
				instantiateBullet(bulletDir.Angle() + i *_angle);
				instantiateBullet(bulletDir.Angle() - i *_angle);
			}
		}
	}
	private void instantiateBullet( float angle ){
		var bullet = _bulletPrefab.Instantiate<BasicBullet>();
		bullet.Rotation = angle;
		bullet.Position = Position;
		bullet.SetCollisionParams( Game.PlayerLayer );
		GetParent().AddChild( bullet );
	}

	private void handleIndicator()
	{
		if( _offscreenIndicator == null ) {
			return;
		}
		var cameraRect = Game.Camera.GetCanvasTransform().AffineInverse() * GetViewportRect();
		var position = GlobalPosition;
		if( !cameraRect.HasPoint( position ) ) {
			_offscreenIndicator.Show();
			_offscreenIndicator.Rotation = ( position - cameraRect.GetCenter() ).Angle();
			var newPosition = position.Clamp( cameraRect.Position, cameraRect.End );
			_offscreenIndicator.GlobalPosition = newPosition;
		} else {
			_offscreenIndicator.Hide();
		}
	}

	public void OnScreenWrap( Rect2 screenWrapRect )
	{
		var player = Game.Player;
		if( player == null ) {
			return;
		}

		var screenSize = screenWrapRect.Size;
		var diff = GlobalPosition - player.GlobalPosition;
		if( diff.X > screenSize.X / 2 ) {
			Position -= new Vector2( screenSize.X, 0 );
		}
		if( diff.X < -screenSize.X / 2 ) {
			Position += new Vector2( screenSize.X, 0 );
		}
		if( diff.Y > screenSize.Y / 2 ) {
			Position -= new Vector2( 0, screenSize.Y );
		}
		if( diff.Y < -screenSize.Y / 2 ) {
			Position += new Vector2( 0, screenSize.Y );
		}
	}

	public void OnBulletCollision(int damage)
	{
		_currentHp -= damage;
		if( _currentHp <= 0 ) {
			IsDead = true;
			Game.Field.RemoveExistingShip();
			QueueFree();
			_offscreenIndicator.QueueFree();
		}
		VisualNode.Modulate = Colors.Red.Lerp( Colors.White, (float)_currentHp / _maxHp );
	}

	public void OnTentacleCollision()
	{
		Game.Player?.Assimilate( this );
		IsDead = true;
		Game.Field.RemoveExistingShip();
		QueueFree();
		_offscreenIndicator.QueueFree();
	}

	public void DisableAllBehavior()
	{
		_isAiEnabled = false;
	}


	public void SetTrail( bool isSet )
	{
		if( isSet ) {
			_trail.ShowTrail();
		} else {
			_trail.HideTrail();
		}
	}

	public void OnShieldRegenerate()
	{
		_hitbox.SetDeferred( Area2D.PropertyName.Monitorable, false );
	}
	
	public void OnShieldDestroy()
	{
		_hitbox.SetDeferred( Area2D.PropertyName.Monitorable, true );
	}
}
