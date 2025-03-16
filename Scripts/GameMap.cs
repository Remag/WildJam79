using Godot;
using System;

public partial class GameMap : Node {
    [Export]
    private Control _tutorialNode;
    [Export]
    private PackedScene _playerPrefab;
    [Export]
    private Button _restartButton;
    [Export]
    private Godot.Collections.Array<PackedScene> _wave1;
    [Export]
    private Godot.Collections.Array<PackedScene> _wave2;
    [Export]
    private Godot.Collections.Array<PackedScene> _wave3;
    private int _currentWave = 0;
    [Export]
    private Button _spawnButton;

    private int _existingShips = 0;

    public override void _Ready()
    {
        GD.Randomize();
        Game.Map = this;
    }

    public void SpawnWave()
    {
        _tutorialNode.Visible = false;
        _spawnButton.Visible = false;
        _existingShips = 0;
        var wave = getCurrentWave();
        foreach( var shipPrefab in wave ) {
            var ship = shipPrefab.Instantiate<EnemyShip>();
            AddChild( ship );
            ship.GlobalPosition = new Vector2( Rng.RandomRange( 10, 800 ), Rng.RandomRange( 10, 50 ) );
            _existingShips++;
        }
        _currentWave++;
    }

    private Godot.Collections.Array<PackedScene> getCurrentWave()
    {
        return _currentWave == 0 ? _wave1 : _currentWave == 1 ? _wave2 : _wave3;
    }

    public void RemoveExistingShip()
    {
        _existingShips--;
        if( _existingShips == 0 ) {
            _spawnButton.Visible = true;
        }
    }

    public void EndGame()
    {
        _restartButton.Visible = true;
    }

    public void RestartGame()
    {
        _restartButton.Visible = false;
        _spawnButton.Visible = true;
        var player = _playerPrefab.Instantiate<Player>();
        player.Position = new Vector2( 500, 400 );
        _currentWave = 0;
        AddChild( player );

        foreach( var node in GetTree().GetNodesInGroup( "Enemy" ) ) {
            node.QueueFree();
        }
    }
}
