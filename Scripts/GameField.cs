using Godot;
using System;
using WildJam78.Scripts.UI;

public partial class GameField : Node {
    [Export]
    private Camera2D _camera;
    [Export]
    private Control _mapControl;
    [Export]
    private TextureRect _starRect;
    [Export]
    private TextureRect _nebulaRect;
    [Export]
    private Control _tutorialNode;
    [Export]
    private PackedScene _playerPrefab;
    [Export]
    private Button _restartButton;
    [Export]
    private Godot.Collections.Array<EnemyInfo> _testWave;
    [Export]
    private Control _idleUiControl;
    [Export]
    private BackgroundSelection _bgSelection;
    [Export]
    public WorldAudioManager WorldAudioManager { get; set; }
    
    private EnemyNodeInfo _currentNodeInfo;
    private float _currentLevelTimer = 0;
    private int _nextWaveIndex = 0;

    private Player.SavedState _savedPlayerState;

    public override void _Ready()
    {
        Game.TestSprite = (Node2D)FindChild( "TestSprite" );
        GD.Randomize();
        Game.Field = this;
        _mapControl.Visible = false;
        randomizeStars();
        randomizeNebula();
    }

    public override void _PhysicsProcess( double delta )
    {
        if( Game.Player != null ) {
            _currentLevelTimer += (float)delta;
            if( _currentNodeInfo != null && _nextWaveIndex < _currentNodeInfo.WavesInfo.Count ) {
                var waveInfo = _currentNodeInfo.WavesInfo[_nextWaveIndex];
                if( _currentLevelTimer >= waveInfo.SpawnTimePoint ) {
                    SpawnWave( waveInfo );
                    _nextWaveIndex++;
                }
            }
        }
    }

    private void randomizeStars()
    {
        _starRect.Texture = Rng.Choose( _bgSelection.Stars );
    }

    private void randomizeNebula()
    {
        _nebulaRect.Texture = Rng.Choose( _bgSelection.Nebulae );
    }

    public void OpenMap()
    {
        _tutorialNode.Visible = false;
        Game.Player.SetControl( false );
        _mapControl.Visible = true;
        _idleUiControl.Visible = false;
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }

    public void CloseMap()
    {
        Game.Player.SetControl( true );
        _mapControl.Visible = false;
        _idleUiControl.Visible = true;
    }

    public void Travel( Texture2D locationBg, EnemyNodeInfo nodeInfo )
    {
        if( Game.Player != null ) {
            _savedPlayerState = Game.Player.SaveState();
        }

        _currentNodeInfo = nodeInfo;
        _idleUiControl.Visible = nodeInfo.WavesInfo.Count == 0;
        if( locationBg == null ) {
            randomizeNebula();
        } else {
            _nebulaRect.Texture = locationBg;
        }

        if( nodeInfo.WavesInfo.Count > 0 ) {
            SpawnWave( nodeInfo.WavesInfo[0] );
            _nextWaveIndex = 1;
        }
    }

    public void SpawnTestWave()
    {
        if( Game.Player != null ) {
            _savedPlayerState = Game.Player.SaveState();
        }
        
        _tutorialNode.Visible = false;
        _idleUiControl.Visible = false;
        
        foreach( var enemy in _testWave ) {
            for( int i = 0; i < enemy.Count; i++ ) {
                spawnEnemy( enemy.Prefab );
            }
        }
        Game.StageName = "TestWave";
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }

    private void spawnEnemy( PackedScene shipPrefab )
    {
        var ship = shipPrefab.Instantiate<EnemyShip>();
        AddChild( ship );
        ship.GlobalPosition = Game.Player.GlobalPosition +
                              Vector2.FromAngle( Rng.RandomRange( 0, 2 * Mathf.Pi ) ) * 700;
    }

    public void RemoveExistingShip()
    {
        var existingShips = GetTree().GetNodesInGroup( "Enemy" ).Count - 1;
        GD.Print( "existingShips " + existingShips );
        if( _currentNodeInfo != null && existingShips == _currentNodeInfo.MinShipsAliveForNextWave ) {
            SpawnNextWave();
        }
        if( existingShips == 0 ) {
            onLevelClear();
        }
    }

    private void onLevelClear()
    {
        Game.Player.TryGrow();
        Game.Player.FullHeal();
        _idleUiControl.Visible = true;
    }

    public void EndGame()
    {
        WorldAudioManager.DeathSoundPlay();
        _restartButton.Visible = true;
    }

    public void RestartGame()
    {
        _nextWaveIndex = 0;
        
        Game.Field.WorldAudioManager.ButtonClickPlay();
        
        _restartButton.Visible = false;
        _idleUiControl.Visible = false;
        var player = _playerPrefab.Instantiate<Player>();
        player.Camera = _camera;
        player.Position = new Vector2( 500, 400 );
        if( _savedPlayerState != null ) {
            player.RestoreState( _savedPlayerState );
        }

        AddChild( player );
        player.UpdateCameraZoom();

        foreach( var node in GetTree().GetNodesInGroup( "ClearOnRestart" ) ) {
            node.QueueFree();
        }

        if( _currentNodeInfo != null ) {
            SpawnNextWave();
        } else {
            onLevelClear();
        }
    }

    private void SpawnNextWave()
    {
        if( _nextWaveIndex < _currentNodeInfo.WavesInfo.Count ) {
            var waveInfo = _currentNodeInfo.WavesInfo[_nextWaveIndex];
            SpawnWave( waveInfo );
            _nextWaveIndex++;
        }
    }

    private void SpawnWave( EnemyWaveInfo waveInfo )
    {
        foreach( var enemy in waveInfo.EnemiesInfo ) {
            for( int i = 0; i < enemy.Count; i++ ) {
                CallDeferred( MethodName.spawnEnemy, enemy.Prefab );
            }
        }

        _currentLevelTimer = waveInfo.SpawnTimePoint;
    }
}
