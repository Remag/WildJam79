using Godot;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class PlayerBlob : Node2D {
    [Export]
    private Node2D _weaponAnchor;
    [Export]
    private Node2D _decalAnchor;
    [Export]
    private AnimationPlayer _animations;

    public PackedScene SrcCore;
    public bool IsWeapon { get; set; }

    public Node2D Initialize( PackedScene core )
    {
        Debug.Assert( core != null );
        SrcCore = core;
        Visible = true;
        var newCore = core.Instantiate<Node2D>();
        IsWeapon = newCore is HeroWeaponCore;
        var targetAnchor = IsWeapon ? _weaponAnchor : _decalAnchor;
        targetAnchor.AddChild( newCore );
        _animations.Play( "Appear" );
        return newCore;
    }

    public Node2D SwitchToWeapon( PackedScene weaponCore )
    {
        SrcCore = weaponCore;
        var newCore = weaponCore.Instantiate<Node2D>();
        IsWeapon = newCore is HeroWeaponCore;
        Debug.Assert( IsWeapon );
        _weaponAnchor.AddChild( newCore );
        _weaponAnchor.Scale = Vector2.Zero;
        _animations.Play( "SwitchToWeapon" );
        return newCore;
    }

    public void HideCore()
    {
        _animations.Play( "HideCore" );
    }

    public void Reset()
    {
        Visible = false;
        SrcCore = null;
        foreach (var child in _weaponAnchor.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _decalAnchor.GetChildren())
        {
            child.QueueFree();
        }
    }
}
