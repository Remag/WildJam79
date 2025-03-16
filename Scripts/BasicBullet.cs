using Godot;
using System;
using System.Collections;

public partial class BasicBullet : Node2D
{
	[Export]
	private Area2D _collisionArea; 

	[Export]
	private float _speed = 10;
	
	[Export]
	private int _bulletDamage = 1;

	public override void _PhysicsProcess( double delta )
	{
		var dir = new Vector2( _speed, 0 ).Rotated( Rotation );
		Position += (float)delta * dir;
	}

	public void SetCollisionParams( int maskValue )
	{
		_collisionArea.CollisionMask = (uint) 1 << ( maskValue - 1 );
	}

	public void OnBodyCollision( Node2D body )
	{
		GD.Print( "OnBodyCollision " + body.Name );
		if( body is EnemyShip enemyShip ) {
			enemyShip.OnBulletCollision();
		} else if( body is Player player ) {
			player.OnBulletCollision(_bulletDamage);
		}
		QueueFree();
	}
}
