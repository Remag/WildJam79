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
    
    public void OnAreaCollision( Area2D area2D )
    {
        if( area2D is Shield enemyShield ) {
            
        } else if( area2D.GetParent() is EnemyShip enemyShip ) {
            enemyShip.OnTentacleCollision();
        }
    }
}
