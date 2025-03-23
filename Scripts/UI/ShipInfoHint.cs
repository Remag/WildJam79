using Godot;
using System;

public partial class ShipInfoHint : Node {
    [Export]
    private Label _label;
    [Export]
    private Node2D _shipAnchor;
    [Export]
    private Godot.Collections.Array<float> _scaleByShipClass;

    public void SetHintInfo( int shipCount, PackedScene prefab )
    {
        var shipHint = prefab.Instantiate<EnemyShip>();
        var scale = _scaleByShipClass[shipHint.SizeLevel];
        _shipAnchor.Scale = new Vector2( scale, scale );

        var visual = shipHint.VisualNode;
        visual.GetParent().RemoveChild( visual );
        visual.Owner = null;
        _shipAnchor.AddChild( visual );
        shipHint.DisableAllBehavior();
        shipHint.QueueFree();
        _label.Text = shipCount.ToString();
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }
}
