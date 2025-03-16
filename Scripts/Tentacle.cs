using Godot;
using System;

public partial class Tentacle : Node2D {
    [Export]
    private AnimationPlayer _animation;

    public void Initialize( Player player )
    {
        _animation.Play( "Extend" );
    }

    public override void _Process( double delta )
    {
        var mousePos = GetViewport().GetMousePosition();
        var dirDelta = mousePos - Game.Player.GlobalPosition;
        Rotation = dirDelta.Angle();
    }

    private void OnCollision( Area2D enemyShip )
    {

    }
}
