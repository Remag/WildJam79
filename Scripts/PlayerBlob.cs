using Godot;
using System;

public partial class PlayerBlob : Node2D
{
    [Export]
    private Node2D _enemyAnchor;

    public Node2D Initialize( PackedScene core )
    {
        Visible = true;
        var newCore = core.Instantiate<Node2D>();
        _enemyAnchor.AddChild( newCore );
        return newCore;
    }
}
