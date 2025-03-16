using Godot;
using System;

public partial class PlayerBlob : Node2D
{
    [Export]
    private Node2D _enemyAnchor;

    public PlayerBlobCore Initialize( PackedScene core )
    {
        Visible = true;
        var newCore = core.Instantiate<PlayerBlobCore>();
        AddChild( newCore );
        return newCore;
    }
}
