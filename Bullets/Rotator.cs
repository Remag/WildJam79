using Godot;
using System;

public partial class Rotator : Node2D
{
    [Export]
    private float _rotationStartMin = -180f;
    [Export]
    private float _rotationStartMax = 180f;
    [Export]
    private float _rotationSpeedDegMin = -180f;
    [Export]
    private float _rotationSpeedDegMax = 180f;

    private float _rotationSpeed = 0f;

    public override void _Ready()
    {
        Rotation = Mathf.DegToRad( Rng.RandomRange( _rotationStartMin, _rotationStartMax ) );
        _rotationSpeed = Mathf.DegToRad( Rng.RandomRange( _rotationSpeedDegMin, _rotationSpeedDegMax ) );
    }

    public override void _PhysicsProcess( double delta )
    {
        Rotation += _rotationSpeed * (float) delta;
    }
}
