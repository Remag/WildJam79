using Godot;
using System;

public partial class SpaceDebris : FoodSource {

    [Export]
    private float _driftMaxAngularSpeed = 10;
    private float _driftMaxSpeed = 5;

    private float _driftAngularSpeed;
    private Vector2 _driftVelocity;

    public override Node2D GetTentacleAnchor()
    {
        return this;
    }

    public override void OnBroughtToPlayer( Tentacle tentacle )
    {
        Game.Player?.Assimilate( this, tentacle );
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
        _driftAngularSpeed = Rng.RandomRange( -_driftMaxSpeed, _driftMaxSpeed );
    }

    public override void _Process( double delta )
    {
    }

    public override void _IntegrateForces( PhysicsDirectBodyState2D state )
    {
        state.LinearVelocity = _driftVelocity;
        state.AngularVelocity = Mathf.DegToRad( _driftAngularSpeed );
    }

}
