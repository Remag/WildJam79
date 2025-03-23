using Godot;
using System;
using System.Collections.Generic;

public partial class TravelMap : Control {
    [Export]
    private MapNode _startNode;
    [Export]
    private MapNode _finalNode;
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
    [Export]
    private Texture2D _normalStar;
    [Export]
    private Texture2D _activeStar;
    [Export]
    private Texture2D _completedStar;
    [Export]
    private Texture2D _finalStar;

    private MapNode _currentNode;

    private Godot.Collections.Array<MapNode> _availableNodes;

    public override void _EnterTree()
    {
        Game.TravelMap = this;
        SetCurrentNode( _startNode );
        _finalNode.SetActivity( _finalStar, false );
    }

    public void SetCurrentNode( MapNode node )
    {
        if( _availableNodes != null ) {
            foreach( var prevActiveNode in _availableNodes ) {
                prevActiveNode.SetStyle( _normalStyle );
            }
        }

        if( _currentNode != null ) {
            foreach( var prevNext in _currentNode.Next ) {
                prevNext.SetStyle( _normalStyle );
                prevNext.SetActivity( _normalStar, false );
            }
        }

        _currentNode = node;
        _availableNodes = node.Next;
        _currentNode.SetStyle( _completedStyle );
        _currentNode.SetActivity( _completedStar, false );
        foreach( var nextNode in _availableNodes ) {
            nextNode.SetStyle( _activeStyle );
            if( nextNode != _finalNode ) {
                nextNode.SetActivity( _activeStar, true );
            }
        }
        if( Game.Field != null )
            Game.Field.WorldAudioManager.ButtonClickPlay();
    }

    public void SetEnemyHint( Godot.Collections.Dictionary<PackedScene, int> enemiesMap )
    {
        foreach( var enemyKey in enemiesMap ) {
            var hintControl = _hintPrefab.Instantiate<ShipInfoHint>();
            hintControl.SetHintInfo( enemyKey.Value, enemyKey.Key );
            _hintContainer.AddChild( hintControl );
        }
        _hintContainer.Visible = true;
        _hintMarginContainer.Visible = true;
    }

    public void ClearAllHints()
    {
        foreach( var child in _hintContainer.GetChildren() ) {
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
