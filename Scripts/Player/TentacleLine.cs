using Godot;
using System;

[Tool]
public partial class TentacleLine : Line2D {

    [Export]
    private Node2D _tentacleScaler;
    [Export]
    private Node2D _tentacleAnchor;

    public override void _Process( double delta )
    {
        var anchorRootPosition = _tentacleAnchor.Position * _tentacleScaler.Scale;
        Points = [anchorRootPosition, Points[1]];
    }
}
