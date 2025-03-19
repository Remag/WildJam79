using Godot;
using System;
using System.Collections.Generic;

public partial class TravelMap : Control {
    [Export]
    private MapNode _startNode;
    [Export]
    private Control _hintMarginContainer;
    [Export]
    private Control _hintContainer;
    [Export]
    private PackedScene _hintPrefab;
    [Export]
    public Control NodeContainer { get; set; }
    [Export]
    private StyleBox _normalStyle;
    [Export]
    private StyleBox _activeStyle;
    [Export]
    private StyleBox _completedStyle;

    private MapNode _currentNode;

    private Godot.Collections.Array<MapNode> _availableNodes;

    public override void _EnterTree()
    {
        Game.TravelMap = this;
        SetCurrentNode( _startNode );
    }

    public void SetCurrentNode( MapNode node )
    {
        if( _availableNodes != null ) {
            foreach( var prevActiveNode in _availableNodes ) {
                prevActiveNode.SetStyle( _normalStyle );
            }
        }

        _currentNode = node;
        _availableNodes = node.Next;
        _currentNode.SetStyle( _completedStyle );
        foreach( var nextNode in _availableNodes ) {
            nextNode.SetStyle( _activeStyle );
        }
    }

    public void SetEnemyHint( Godot.Collections.Array<EnemyInfo> enemies )
    {
        foreach( var enemy in enemies ) {
            var hintControl = _hintPrefab.Instantiate<ShipInfoHint>();
            hintControl.SetHintInfo( enemy.Count, enemy.Prefab );
            _hintContainer.AddChild( hintControl );
        }
        _hintContainer.Visible = true;
        _hintMarginContainer.Visible = true;
    }

    public void ClearAllHints()
    {
        foreach( var child in _hintContainer.GetChildren() ){
            child.QueueFree();
        }
        _hintContainer.Visible = false;
        _hintMarginContainer.Visible = false;
    }

    public bool IsAvailable( MapNode target )
    {
        foreach( var node in _availableNodes ) {
            if( node == target ) {
                return true;
            }
        }

        return false;
    }

    public void CloseMap()
    {
        Game.Field.CloseMap();
    }
}
