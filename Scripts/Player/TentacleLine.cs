using Godot;
using System;
using System.Linq;

[Tool]
public partial class TentacleLine : Line2D {

    [Export]
    public Node2D StartAnchor { get; set; }
    [Export]
    public Node2D EndAnchor { get; set; }

    public override void _PhysicsProcess( double delta )
    {
        if( StartAnchor == null || EndAnchor == null ) {
            return;
        }

        var startPosition = EndAnchor.Position;
        var endPosition = StartAnchor.Position;
        var points = Points;
        var newPoints = new Vector2[points.Length];
        newPoints[0] = startPosition;
        newPoints[newPoints.Length - 1] = endPosition;
        if( points.Length > 2 ) {
            Array.Copy( points, 1, newPoints, 1, points.Length - 2 );
        }
        Points = newPoints;
    }
}
