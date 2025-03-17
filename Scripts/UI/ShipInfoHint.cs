using Godot;
using System;

public partial class ShipInfoHint : Node {
    [Export]
    private Label _label;
    [Export]
    private Node2D _shipAnchor;

    public void SetHintInfo( int shipCount, PackedScene prefab )
    {
        var shipHint = prefab.Instantiate<EnemyShip>();
        _shipAnchor.Scale = new Vector2( 0.5F, 0.5F );

        var visual = shipHint.VisualNode;
        visual.GetParent().RemoveChild( visual );
        visual.Owner = null;
        _shipAnchor.AddChild( visual );
        shipHint.DisableAllBehavior();
        shipHint.QueueFree();
        _label.Text = shipCount.ToString();

    }
}
