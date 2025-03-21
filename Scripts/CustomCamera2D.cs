using Godot;
using System;

public partial class CustomCamera2D : Camera2D {
    
    [Export] public CollisionShape2D ScreenWrapArea = null;
    
    public override void _Ready()
    {
        Game.Camera = this;
    }

    public override void _PhysicsProcess( double delta )
    {
        base._PhysicsProcess( delta );
        if( Game.Player != null ) {
            Position = Game.Player.GlobalPosition;
        }
    }

    public void OnAreaLeave( Area2D area2D )
    {
        if( area2D.GetParent() is BasicBullet bullet ) {
            bullet.QueueFree();
        } else if( area2D.GetParent() is EnemyShip enemyShip ) {
            var rect = ScreenWrapArea.Shape.GetRect();
            rect.Size *= Scale;
            enemyShip.OnScreenWrap( rect );
        }

    }
}
