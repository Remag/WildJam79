using Godot;
using System;

public partial class SpaceDebris : FoodSource {

    [Export]
    private float _driftAngularSpeed = 10;
    private float _driftMaxSpeed = 5;

    private Vector2 _driftVelocity;

    public override Node2D GetTentacleAnchor()
    {
        return this;
    }

    public override void OnBroughtToPlayer()
    {
        Game.Player?.Assimilate( this );
        QueueFree();
    }

    public override void OnTentacleCollision()
    {
    }

    public override bool TryTentaclePull( Tentacle tentacle )
    {
        return true;
    }

    public override void _Ready()
    {
        var randomDir = new Vector2( 1, 0 ).Rotated( Rng.RandomRange( 0, 2 * Mathf.Pi ) );
        _driftVelocity = Rng.RandomRange( 0, _driftMaxSpeed ) * randomDir;
    }

    public override void _Process( double delta )
    {
        var deltaF = (float)delta;
        Rotation += Mathf.RadToDeg( _driftAngularSpeed ) * deltaF;
        GlobalPosition += _driftVelocity;
    }

}
