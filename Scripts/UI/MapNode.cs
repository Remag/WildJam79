using Godot;
using System;
using WildJam78.Scripts.UI;

public partial class MapNode : Control {
    [Export]
    private Texture2D _locationBg;
    [Export]
    private EnemyNodeInfo _nodeInfo;
    [Export]
    private Godot.Collections.Array<MapNode> _next;

    public Godot.Collections.Array<MapNode> Next { get { return _next; } }

    [Export]
    private PackedScene _linePrefab;

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
    }

    public void SetStyle( StyleBox newStyle )
    {
        AddThemeStyleboxOverride( "panel", newStyle );
    }

    public void OnNodeEntered()
    {
        var hintEnemies = new Godot.Collections.Array<EnemyInfo>();
        if( _nodeInfo != null ) {
            foreach( var enemyWaveInfo in _nodeInfo.WavesInfo ) {
                foreach( var enemyInfo in enemyWaveInfo.EnemiesInfo ) {
                    hintEnemies.Add( enemyInfo );
                }
            }
        }

        Game.TravelMap.SetEnemyHint( hintEnemies );
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
