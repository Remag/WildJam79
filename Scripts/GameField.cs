using Godot;
using System;

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

    private int _existingShips = 0;

    public override void _Ready()
    {
        Game.TestSprite = (Node2D)FindChild( "TestSprite" );
        GD.Randomize();
        Game.Field = this;
        _mapControl.Visible = false;
        randomizeStars();
        randomizeNebula();
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

    public void Travel( Texture2D locationBg, Godot.Collections.Array<EnemyInfo> enemies )
    {
        _idleUiControl.Visible = enemies.Count == 0;
        if( locationBg == null ) {
            randomizeNebula();
        } else {
            _nebulaRect.Texture = locationBg;
        }

        foreach( var enemy in enemies ) {
            for( int i = 0; i < enemy.Count; i++ ) {
                spawnEnemy( enemy.Prefab );
            }
        }
    }

    public void SpawnTestWave()
    {
        _tutorialNode.Visible = false;
        _idleUiControl.Visible = false;
        _existingShips = 0;
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
        _existingShips++;
    }

    public void RemoveExistingShip()
    {
        _existingShips--;
        if( _existingShips == 0 ) {
            onLevelClear();
        }
    }

    private void onLevelClear()
    {
        Game.Player.TryGrow();
        _idleUiControl.Visible = true;
    }

    public void EndGame()
    {
        WorldAudioManager.DeathSoundPlay();
        _restartButton.Visible = true;
    }

    public void RestartGame()
    {
        Game.Field.WorldAudioManager.ButtonClickPlay();
        _camera.Zoom = new Vector2( 1, 1 );
        _restartButton.Visible = false;
        _idleUiControl.Visible = true;
        var player = _playerPrefab.Instantiate<Player>();
        player.Camera = _camera;
        player.Position = new Vector2( 500, 400 );
        AddChild( player );

        foreach( var node in GetTree().GetNodesInGroup( "ClearOnRestart" ) ) {
            node.QueueFree();
        }

    }
}
