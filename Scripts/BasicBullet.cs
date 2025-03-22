using System;
using Godot;
using WildJam78.Scripts;
using WildJam78.Scripts.Bullets;

public partial class BasicBullet : Node2D {
	[Export]
	private Area2D _collisionArea;

	[Export]
	private BulletMoveLogic _moveLogic;
	[Export]
	private BulletLifespanLogic _lifespanLogic;
	[Export]
	private BulletDeathLogic _deathLogic;

	[Export]
	private bool _isEnemyBullet = false;
	public bool IsEnemyBullet { get { return _isEnemyBullet; } }

	[Export]
	private int _bulletDamage = 1;

	private float _velocity = 0;
	private float _currentLifetime = 0;

	private bool _isDestroyed = false;

	public override void _Ready()
	{
		_velocity = _moveLogic switch {
			LinearBulletMoveLogic linearBulletMoveLogic => linearBulletMoveLogic.GetStartVelocity(),
			TargetedBulletMoveLogic targetedBulletMoveLogic => targetedBulletMoveLogic.GetStartVelocity(),
			_ => _velocity
		};
	}

	public override void _PhysicsProcess( double delta )
	{
		var deltaF = (float)delta;
		_currentLifetime += deltaF;
		switch( _moveLogic ) {
			case LinearBulletMoveLogic linearBulletMoveLogic: {
					HandleLinearBulletMoveLogic( linearBulletMoveLogic, deltaF );
					break;
				}
			case TargetedBulletMoveLogic targetedBulletMoveLogic: {
					HandleTargetedBulletMoveLogic( targetedBulletMoveLogic, deltaF );
					break;
				}
		}


		if( _lifespanLogic.IsDestroyed( _currentLifetime, deltaF ) ) {
			var tween = GetTree().CreateTween();
			tween.TweenCallback( Callable.From( () => {
				_collisionArea.Monitorable = false;
				_collisionArea.Monitoring = false;
			} ) );
			tween.TweenProperty( this, "modulate", Colors.Transparent, _lifespanLogic.AlphaTweenTime );
			tween.TweenCallback( Callable.From( QueueFree ) );
		}
	}

	public void SetIsEnemyBullet( bool isEnemyBullet )
	{
		_isEnemyBullet = isEnemyBullet;
		SetCollisionParams( isEnemyBullet ? Game.PlayerLayer : Game.EnemyShipLayer );
	}

	private void SetCollisionParams( int maskValue )
	{
		_collisionArea.CollisionMask = (uint)1 << ( maskValue - 1 );
	}

	public void OnAreaCollision( Node node )
	{
		switch( node ) {
			case Shield enemyShield:
				enemyShield.OnBulletCollision( _bulletDamage, this );
				HandleDestroy();
				break;
			case EnemyShip enemyShip:
				if( !enemyShip.IsDead ) {
					enemyShip.OnBulletCollision( _bulletDamage );
					HandleDestroy();
				}
				break;
			case Player player:
				player.OnBulletCollision( _bulletDamage );
				HandleDestroy();
				break;
			case BasicBullet bullet:
				bullet.HandleDestroy( useDeathLogic: true );
				HandleDestroy( useDeathLogic: true );
				break;
		}
	}

	public void HandleDestroy( bool useDeathLogic = false )
	{
		if( _isDestroyed ) return;

		_isDestroyed = true;

		if( useDeathLogic && _deathLogic != null ) {
			var count = _deathLogic.Count;
			for( var i = 0f; i < 2 * Mathf.Pi; i += 2 * Mathf.Pi / count ) {
				CallDeferred( MethodName.InstantiateBullet, _deathLogic.BulletPrefab, i, Position );
			}
			Game.Field.WorldAudioManager.ExplosionSoundPlay();
		}

		QueueFree();
	}

	private void InstantiateBullet( PackedScene bulletPrefab, float angle, Vector2 position )
	{
		var bullet = bulletPrefab.Instantiate<BasicBullet>();
		bullet.Rotation = angle;
		bullet.Position = position;
		bullet.SetCollisionParams( Game.PlayerLayer );
		GetParent().AddChild( bullet );
	}

	private void HandleLinearBulletMoveLogic( LinearBulletMoveLogic linearBulletMoveLogic, float delta )
	{
		_velocity = Math.Min( linearBulletMoveLogic.maxVelocity, _velocity + linearBulletMoveLogic.acceleration * delta );
		var dir = new Vector2( _velocity, 0 ).Rotated( Rotation );
		Position += delta * dir;
	}

	private void HandleTargetedBulletMoveLogic( TargetedBulletMoveLogic targetedBulletMoveLogic, float delta )
	{
		if( _currentLifetime > targetedBulletMoveLogic.activeTime ) {
			_velocity = Math.Max( 0, _velocity - targetedBulletMoveLogic.inactiveFriction * delta );
			var dir = new Vector2( _velocity, 0 ).Rotated( Rotation );
			Position += delta * dir;
		} else {
			_velocity = Math.Min( targetedBulletMoveLogic.maxVelocity, _velocity + targetedBulletMoveLogic.acceleration * delta );

			var target = GetTarget();
			if( target != null ) {
				var targetAngle = ( target.GlobalPosition - GlobalPosition ).Angle();
				var targetAngleDiff = targetAngle - Rotation;
				while( targetAngleDiff > float.Pi ) {
					targetAngleDiff -= 2 * float.Pi;
				}
				while( targetAngleDiff < -float.Pi ) {
					targetAngleDiff += 2 * float.Pi;
				}
				Rotation += Math.Sign( targetAngleDiff ) * Math.Min( targetedBulletMoveLogic.angularVelocity * delta, Math.Abs( targetAngleDiff ) );
			}
			var dir = new Vector2( _velocity, 0 ).Rotated( Rotation );
			Position += delta * dir;
		}
	}

	private Node2D GetTarget()
	{
		if( _isEnemyBullet ) {
			return Game.Player;
		} else {
			var currentPosition = GlobalPosition;
			var currentDistanceSquared = 0f;
			Node2D targetNode = null;

			foreach( var node in GetTree().GetNodesInGroup( "Enemy" ) ) {
				if( node is not Node2D ) continue;

				var node2D = (Node2D)node;
				var distanceSquared = ( node2D.GlobalPosition - currentPosition ).LengthSquared();

				if( targetNode == null || currentDistanceSquared > distanceSquared ) {
					targetNode = node2D;
					currentDistanceSquared = distanceSquared;
				}
			}

			return targetNode;
		}
	}

}
