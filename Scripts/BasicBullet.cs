using Godot;
using System;
using System.Collections;

public partial class BasicBullet : Node2D
{
    [Export]
    private Area2D _collisionArea; 

    [Export]
    private float _speed = 10;

    public override void _PhysicsProcess( double delta )
    {
        var dir = new Vector2( _speed, 0 ).Rotated( Rotation );
        Position += (float)delta * dir;
    }

    public void SetCollisionParams( int layerValue, int maskValue )
    {
        _collisionArea.CollisionLayer = (uint)1 << ( layerValue - 1 );
        _collisionArea.CollisionMask = (uint)1 << ( maskValue - 1 );
    }

    public void OnCollide( Area2D area )
    {
        QueueFree();
    }
}
