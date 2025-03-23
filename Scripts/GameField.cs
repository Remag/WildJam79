using Godot;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private PackedScene _playerPrefab;
    [Export]
    private PackedScene _warpEffect;
    [Export]
    private Control _restartControl;
    private int _currentHintIndex = -1;
    [Export]
    private Godot.Collections.Array<Control> _hints;
    [Export]
    private EnemyNodeInfo _testWaveNodeInfo;
    [Export]
    private Control _idleUiControl;
    [Export]
    private Control _warpControl;
    [Export]
    private BackgroundSelection _bgSelection;
    [Export]
    public WorldAudioManager WorldAudioManager { get; set; }
    [Export]
    private ColorRect _warpEffectRect;
    [Export]
    private Sprite2D _glassSprite;
    [Export]
    private PackedScene _glassDebris;
    [Export]
    private Tutorial _tutorial;
    [Export]
    private AnimationPlayer _startAnimation;
    [Export]
    private FinalCutscene _finalCutscene;

    public EnemyNodeInfo CurrentNodeInfo;
    private float _currentLevelTimer = 0;
    private bool _isSpawningEnemies = false;
    private int _nextWaveIndex = 0;
    private bool _isTestWaveActive = false;

    private Player.SavedState _savedPlayerState;

    public override void _Ready()
    {
        _warpEffectRect.Visible = false;
        Game.TestSprite = (Node2D)FindChild( "TestSprite" );
        GD.Randomize();
        Game.Field = this;
        _mapControl.Visible = false;
        randomizeStars();
        randomizeNebula();
    }

    public override void _PhysicsProcess( double delta )
    {
        if( Game.Player != null && _isSpawningEnemies ) {
            _currentLevelTimer += (float)delta;
            if( CurrentNodeInfo != null && _nextWaveIndex < CurrentNodeInfo.WavesInfo.Count ) {
                var waveInfo = CurrentNodeInfo.WavesInfo[_nextWaveIndex];
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

    public void StartGame()
    {
        _startAnimation.Play( "Break" );
    }


    public void SpawnPlayer()
    {
        var player = _playerPrefab.Instantiate<Player>();
        AddChild( player );
        player.Camera = _camera;
        player.GlobalPosition = _glassSprite.GlobalPosition;
        player.Appear();
        _glassSprite.QueueFree();
        var glassDebris = _glassDebris.Instantiate<FoodSource>();
        AddChild( glassDebris );
        glassDebris.GlobalPosition = _glassSprite.GlobalPosition;
        glassDebris.ApplyCentralImpulse( new Vector2( -80, 80 ) );
        player.UpdateCameraZoom();
        player.SetMoveEnabled( false );
        _tutorial.Start();
        WorldAudioManager.BottlePopSoundPlay();
    }

    public bool IsCombat()
    {
        return !_idleUiControl.Visible;
    }

    public void SetWarpEffectCircle( float circleRadius )
    {
        if( circleRadius <= 0 || circleRadius > 1 ) {
            _warpEffectRect.Visible = false;
            return;
        }
        _warpEffectRect.Visible = true;
        var material = (ShaderMaterial)_warpEffectRect.Material;
        material.SetShaderParameter( "outerSize", circleRadius );
        WorldAudioManager.EchoSoundPlay();
    }

    public void SetWarpEffectPower( float power )
    {
        if( power <= 0 || power > 1 ) {
            _warpEffectRect.Visible = false;
            return;
        }
        _warpEffectRect.Visible = true;
        var material = (ShaderMaterial)_warpEffectRect.Material;
        material.SetShaderParameter( "effectPower", power );
        WorldAudioManager.EchoSoundPlay();
    }

    public void OpenMap()
    {
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
        Debug.Assert( nodeInfo != null );
        foreach( var node in GetTree().GetNodesInGroup( "ClearOnLevelClear" ) ) {
            node.QueueFree();
        }
        if( Game.Player != null ) {
            _savedPlayerState = Game.Player.SaveState();
            Game.Player.SetControl( false );
        }

        _isSpawningEnemies = false;
        _idleUiControl.Visible = false;
        var effect = _warpEffect.Instantiate<WarpEffect>();
        AddChild( effect );
        effect.Initialize( Game.Player, locationBg, nodeInfo );
    }

    public void SwitchLocation( Texture2D locationBg, EnemyNodeInfo nodeInfo, WarpEffect effect )
    {
        if( nodeInfo.IsFinalNode ) {
            WorldAudioManager.FinalCutsceneSoundPlay();
            _finalCutscene.Play();
            return;
        }
        CurrentNodeInfo = nodeInfo;
        _isTestWaveActive = false;
        _idleUiControl.Visible = nodeInfo.WavesInfo.Count == 0;
        if( locationBg == null ) {
            randomizeNebula();
        } else {
            _nebulaRect.Texture = locationBg;
        }

        effect.ReverseAnimation();
        Game.StageName = "TestWave";
    }

    public void InitializeEnemyWave( EnemyNodeInfo nodeInfo )
    {
        if( nodeInfo.WavesInfo.Count > 0 ) {
            SpawnWave( nodeInfo.WavesInfo[0] );
            _nextWaveIndex = 1;
            _isSpawningEnemies = true;
        }
    }

    public void SpawnTestWave()
    {
        if( Game.Player != null ) {
            _savedPlayerState = Game.Player.SaveState();
        }
        CurrentNodeInfo = _testWaveNodeInfo;
        _isTestWaveActive = true;

        _idleUiControl.Visible = false;

        InitializeEnemyWave( _testWaveNodeInfo );

        Game.StageName = "TestWave";
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }

    public EnemyShip SpawnEnemy( PackedScene shipPrefab, double moveDelay = 0 )
    {
        if( Game.Player == null ) {
            return null;
        }
        var ship = shipPrefab.Instantiate<EnemyShip>();
        ship.MoveDelay = moveDelay;
        AddChild( ship );
        ship.GlobalPosition = Game.Player.GlobalPosition +
                              Vector2.FromAngle( Rng.RandomRange( 0, 2 * Mathf.Pi ) ) * 700 / Game.Camera.Zoom;
        return ship;
    }

    public void EndTutorial()
    {
        EnableIdleUi();
        _tutorial.QueueFree();
        _tutorial = null;
    }

    public void RemoveExistingShip()
    {
        if( _tutorial != null ) {
            return;
        }

        var existingShips = GetTree().GetNodesInGroup( "Enemy" );
        var aliveCount = getAliveCount( existingShips );
        var hasNewWaves = CurrentNodeInfo != null && _nextWaveIndex < CurrentNodeInfo.WavesInfo.Count;
        if( CurrentNodeInfo != null && aliveCount == CurrentNodeInfo.MinShipsAliveForNextWave ) {
            SpawnNextWave();
        }

        if( aliveCount == 0 && !hasNewWaves ) {
            onLevelClear();
        }
    }

    private int getAliveCount( Godot.Collections.Array<Node> shipNodes )
    {
        var count = 0;
        foreach( EnemyShip ship in shipNodes ) {
            if( !ship.IsDead ) {
                count++;
            }
        }
        return count;
    }

    private void onLevelClear()
    {
        CallDeferred( MethodName.playVictoryAnimation );
        foreach( var node in GetTree().GetNodesInGroup( "ClearOnLevelClear" ) ) {
            node.QueueFree();
        }
    }

    private void playVictoryAnimation()
    {
        Game.Player.EatAllEnemies();
    }
    
    public void OnEndVictoryAnimation()
    {
        EnableIdleUi();
    }

    public void OnPlayerGrowFinish()
    {
        var existingShips = GetTree().GetNodesInGroup( "Enemy" );
        var aliveCount = getAliveCount( existingShips );
        if( aliveCount == 0 ) {
            EnableIdleUi();
        }
    }

    public void EnableIdleUi()
    {
        _idleUiControl.Visible = true;
        _warpControl.Position = new Vector2( 393.0f, -145.0f );
        var tween = _warpControl.CreateTween();
        tween.SetProcessMode( Tween.TweenProcessMode.Physics );
        tween.TweenProperty( _warpControl, "position", new Vector2( 393.0f, 22.0f ), 0.4f ).SetEase( Tween.EaseType.InOut );
    }

    public void EndGame()
    {
        _restartControl.Visible = true;
        var nextHintIndex = _currentHintIndex + 1;
        if( nextHintIndex >= _hints.Count ) {
            nextHintIndex = 0;
        }

        if( _currentHintIndex >= 0 ) {
            _hints[_currentHintIndex].Visible = false;
        }
        _currentHintIndex = nextHintIndex;
        _hints[_currentHintIndex].Visible = true;
    }

    public void RestartGame()
    {
        _nextWaveIndex = 0;
        if( _isTestWaveActive ) {
            CurrentNodeInfo = null;
        }

        Game.Field.WorldAudioManager.ButtonClickPlay();

        _restartControl.Visible = false;
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

        if( !_isTestWaveActive ) {
            SpawnNextWave();
        } else {
            onLevelClear();
        }
    }

    private void SpawnNextWave()
    {
        if( _nextWaveIndex < CurrentNodeInfo.WavesInfo.Count ) {
            var waveInfo = CurrentNodeInfo.WavesInfo[_nextWaveIndex];
            SpawnWave( waveInfo );
            _nextWaveIndex++;
        }
    }

    private void SpawnWave( EnemyWaveInfo waveInfo, double moveDelay = 0 )
    {
        foreach( var enemy in waveInfo.EnemiesInfo ) {
            for( int i = 0; i < enemy.Count; i++ ) {
                CallDeferred( MethodName.SpawnEnemy, enemy.Prefab, moveDelay );
            }
        }

        _currentLevelTimer = waveInfo.SpawnTimePoint;
    }
}
