using Godot;
using System;

public partial class Tentacle : Node2D {
    [Export]
    private AnimationPlayer _animation;

    public void Initialize( Player player )
    {
        _animation.Play( "Extend" );
    }

    public override void _PhysicsProcess( double delta )
    {
        var mousePos = GetViewport().GetMousePosition();
        var dirDelta = mousePos - Game.Camera.GetCanvasTransform() * Game.Player.GlobalPosition;
        Rotation = dirDelta.Angle();
    }

    public void OnBodyCollision( Node2D body )
    {
        if( body is EnemyShip enemyShip ) {
            enemyShip.OnTentacleCollision();
        }
    }
}
