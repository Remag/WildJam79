using Godot;
using System;

public partial class PlayerBlob : Node2D
{
    [Export]
    private Node2D _enemyAnchor;

    public PackedScene initCore;

    public Node2D Initialize( PackedScene core )
    {
        initCore = core;
        Visible = true;
        var newCore = core.Instantiate<Node2D>();
        _enemyAnchor.AddChild( newCore );
        return newCore;
    }
}
