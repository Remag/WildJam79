using Godot;
using System;
using System.Linq.Expressions;

public partial class Player : Node2D {

    [Export]
    private Node2D _mainNode;
    [Export]
    private Node2D _eyeball;
    [Export]
    private float _maxVelocityPxSec = 10;
    [Export]
    private float _maxAccelPxSec = 10;
    [Export]
    private int _maxHp = 3;

    [Export]
    private PackedScene _tentaclePrefab;

    [Export]
    private Godot.Collections.Array<PlayerBlob> _blobs;
    private int _currentBlobIndex = 0;

    private Godot.Collections.Array<PlayerBlobCore> _cores = new();
    private bool _isShooting = false;

    private int _currentHp = 5;

    private Tentacle _activeTentacle;

    private Vector2 _currentAccel = new();
    private Vector2 _currentVelocity = new();

    public override void _Ready()
    {
        Game.Player = this;
        _currentHp = _maxHp;
    }

    public override void _ExitTree()
    {
        if( Game.Player == this ) {
            Game.Player = null;
        }
    }

    public void OnBulletCollision( Area2D bullet )
    {
        _currentHp--;
        if( _currentHp == 0 ) {
            QueueFree();
            Game.Map.EndGame();
        }

        _eyeball.Modulate = Colors.White.Lerp( Colors.Red, (float)_currentHp / _maxHp );
    }

    public void Assimilate( EnemyShip ship )
    {
        if( _currentBlobIndex >= _blobs.Count ) {
            return;
        }
        destroyTentacle();
        var newBlob = _blobs[_currentBlobIndex];
        var newCore = newBlob.Initialize( ship.CorePrefab );
        _cores.Add( newCore );

        _currentBlobIndex++;
    }

    public override void _Input( InputEvent e )
    {
        if( e is InputEventMouseButton mouseE ) {
            if( mouseE.ButtonIndex == MouseButton.Left ) {
                if( mouseE.Pressed ) {
                    startAttacking();
                } else {
                    stopAttacking();
                }
            }
            if( mouseE.ButtonIndex == MouseButton.Right ) {
                if( mouseE.Pressed ) {
                    spawnTentacle();
                } else {
                    destroyTentacle();
                }
            }
        } else if( e is InputEventKey keyE ) {
            handleMovement( keyE );
        }
    }

    private void startAttacking()
    {
        _isShooting = true;
    }

    private void stopAttacking()
    {
        _isShooting = false;

    }

    private void spawnTentacle()
    {
        _activeTentacle?.QueueFree();
        _activeTentacle = _tentaclePrefab.Instantiate<Tentacle>();
       _mainNode.AddChild( _activeTentacle );
        _activeTentacle.Initialize( this );
    }

    private void destroyTentacle()
    {
        _activeTentacle?.QueueFree();
        _activeTentacle = null;
    }

    private void handleMovement( InputEventKey keyE )
    {
        var accelValue = keyE.Pressed ? _maxAccelPxSec : 0;
        applyAccel( accelValue, keyE.Keycode );
    }

    private void applyAccel( float value, Key key )
    {
        switch( key ) {
            case Key.W:
            case Key.Up:
                _currentAccel = new Vector2( _currentAccel.X, -value );
                break;
            case Key.A:
            case Key.Left:
                _currentAccel = new Vector2( -value, _currentAccel.Y );
                break;
            case Key.S:
            case Key.Down:
                _currentAccel = new Vector2( _currentAccel.X, value );
                break;
            case Key.D:
            case Key.Right:
                _currentAccel = new Vector2( value, _currentAccel.Y );
                break;
        }
    }

    public override void _Process( double delta )
    {
        updateShooting( delta );
        updatePosition( delta );
    }

    private void updateShooting( double delta )
    {
        if( !_isShooting ) {
            return;
        }
        foreach( var core in _cores ) {
            core.UpdateShooting( delta );
        }
    }

    private void updatePosition( double delta )
    {
        var deltaF = (float)delta;
        var accelValue = _maxAccelPxSec * _currentAccel.Normalized();
        _currentVelocity += deltaF * accelValue;
        var velValue = Math.Min( _maxVelocityPxSec, _currentVelocity.Length() );
        _currentVelocity = velValue * _currentVelocity.Normalized();
        Position += deltaF * _currentVelocity;

        limitPosition();
    }

    private void limitPosition()
    {
        var rect = GetViewport().GetVisibleRect();
        if( GlobalPosition.X < rect.Position.X ) {
            GlobalPosition = new Vector2( rect.Position.X, GlobalPosition.Y );
            _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
        }
        if( GlobalPosition.X > rect.End.X ) {
            GlobalPosition = new Vector2( rect.End.X, GlobalPosition.Y );
            _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
        }
        if( GlobalPosition.Y < rect.Position.Y ) {
            GlobalPosition = new Vector2( GlobalPosition.X, rect.Position.Y );
            _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
        }
        if( GlobalPosition.Y > rect.End.Y ) {

            GlobalPosition = new Vector2( GlobalPosition.X, rect.End.Y );
            _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
        }
    }
}


