using Godot;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class PlayerBlob : Node2D {
    [Export]
    private Node2D _enemyAnchor;
    [Export]
    private AnimationPlayer _animations;

    public PackedScene SrcCore;

    public Node2D Initialize( PackedScene core )
    {
        Debug.Assert( core != null );
        SrcCore = core;
        Visible = true;
        var newCore = core.Instantiate<Node2D>();
        _enemyAnchor.AddChild( newCore );
        _animations.Play( "Appear" );
        return newCore;
    }

    public void HideCore()
    {
        _animations.Play( "HideCore" );
    }
}
