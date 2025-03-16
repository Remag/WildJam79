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
            Position = Game.Player.Position;
        }
    }

    public void OnEnemyLeaveArea( Node2D body )
    {
        if( body is EnemyShip enemyShip ) {
            enemyShip.OnScreenWrap(ScreenWrapArea.Shape.GetRect());
        }
    }

    public void OnBulletLeaveArea( Area2D bullet )
    {
        bullet.GetParent().QueueFree();        
    }
}
