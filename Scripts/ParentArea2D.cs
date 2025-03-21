using Godot;

namespace WildJam78.Scripts;

[GlobalClass]
public partial class ParentArea2D : Area2D {
    [Export] public Node RealParent;
    
    [Signal]
    public delegate void AreaCollisionEventHandler(Node parent);

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered( Area2D area2D )
    {
        if( area2D is ParentArea2D parentArea ) {
            EmitSignal( SignalName.AreaCollision, parentArea.RealParent );
        } else {
            EmitSignal( SignalName.AreaCollision, area2D );
        }
    }
}