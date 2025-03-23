using Godot;
using System;
using Godot.Collections;
using WildJam78.Scripts.UI;

public partial class MapNode : Control {
    [Export]
    private Texture2D _locationBg;
    [Export]
    private EnemyNodeInfo _nodeInfo;
    [Export]
    private Godot.Collections.Array<MapNode> _next;
    public Godot.Collections.Array<MapNode> Next { get { return _next; } }

    private Godot.Collections.Array<Line2D> _nextLines = new();

    [Export]
    private TextureRect _starRect;
    [Export]
    private PackedScene _linePrefab;

    private bool _isActive = false;

    public override void _Ready()
    {
        foreach( var node in _next ) {
            spawnLine( node );
        }
    }

    private void spawnLine( MapNode next )
    {
        var line = _linePrefab.Instantiate<Line2D>();
        line.Points = [Position + Size / 2, next.Position + next.Size / 2];
        CallDeferred( MethodName.addLine, line );
    }

    private void addLine( Line2D line )
    {
        Game.TravelMap.NodeContainer.AddChild( line );
        _nextLines.Add( line );
    }

    public void HighlightLine( Color color, Node target )
    {
        for( var i = 0; i < _nextLines.Count; i++ ) {
            if( _next[i] == target ) {
                _nextLines[i].Modulate = color;
                return;
            }
        }
    }

    public void SetLineColor( Color color )
    {
        foreach( var line in _nextLines ) {
            line.Modulate = color;
        }
    }

    public void SetStyle( StyleBox newStyle )
    {
        AddThemeStyleboxOverride( "panel", newStyle );
    }

    public void SetActivity( Texture2D starIcon, bool isActive )
    {
        _starRect.Texture = starIcon;
        _isActive = isActive;
    }

    public void OnNodeEntered()
    {
        if( !_isActive ) {
            return;
        }

        var prefabToMap = new Dictionary<PackedScene, int>();
        if( _nodeInfo != null ) {
            foreach( var enemyWaveInfo in _nodeInfo.WavesInfo ) {
                foreach( var enemyInfo in enemyWaveInfo.EnemiesInfo ) {
                    if( prefabToMap.ContainsKey( enemyInfo.Prefab ) ) {
                        prefabToMap[enemyInfo.Prefab] += enemyInfo.Count;
                    } else {
                        prefabToMap[enemyInfo.Prefab] = enemyInfo.Count;
                    }
                }
            }
        }

        Game.TravelMap.SetEnemyHint( prefabToMap );
    }

    public void OnNodeLeft()
    {
        Game.TravelMap.ClearAllHints();
    }

    public void OnNodeClicked()
    {
        if( Game.TravelMap.IsAvailable( this ) ) {
            Game.TravelMap.SetCurrentNode( this );
            Game.Field.CloseMap();
            Game.Field.Travel( _locationBg, _nodeInfo );
            Game.StageName = null;
        }
    }
}
