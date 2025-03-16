using Godot;
using System;
using System.Runtime.InteropServices;

public partial class Iris : Node2D {
    [Export]
    private float _maxOffsetPx = 10;

    private Vector2 _startPos;

    public override void _Ready()
    {
        _startPos = Position;
    }

    public override void _PhysicsProcess( double delta )
    {
        var deltaPos = GetViewport().GetMousePosition() - Game.Camera.GetCanvasTransform() * GlobalPosition;
        var len = Math.Min( _maxOffsetPx, deltaPos.Length() );
        var deltaVec = deltaPos.Normalized();
        var resultVec = len * deltaVec;
        Position = _startPos + resultVec;
    }
}