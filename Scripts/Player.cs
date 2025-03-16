using Godot;
using System;
using System.Linq.Expressions;

public partial class Player : RigidBody2D {

    [Export]
    private Node2D _mainNode;
    [Export]
    private Node2D _eyeball;
    [Export]
    private float _maxVelocityPxSec = 500;
    [Export]
    private float _maxAccelPxSec = 100000;
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
    private float _currentImpulseCountdown = 0;

    private Tentacle _activeTentacle;

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

    public void OnBulletCollision()
    {
        _currentHp--;
        if( _currentHp == 0 ) {
            QueueFree();
            Game.Map.EndGame();
        }

        _eyeball.Modulate = Colors.Red.Lerp( Colors.White, (float)_currentHp / _maxHp );
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

    public override void _PhysicsProcess( double delta )
    {
        var deltaF = (float)delta;
        var accelVector = new Vector2();
        
        if( Input.IsActionPressed( "CharacterUp" ) ) {
            accelVector += new Vector2( 0, -1 );
        }
        if( Input.IsActionPressed( "CharacterDown" ) ) {
            accelVector += new Vector2( 0, 1 );
        }
        if( Input.IsActionPressed( "CharacterLeft" ) ) {
            accelVector += new Vector2( -1, 0 );
        }
        if( Input.IsActionPressed( "CharacterRight" ) ) {
            accelVector += new Vector2( 1, 0 );
        }
        
        var accelValue = _maxAccelPxSec * accelVector.Normalized();
        ApplyForce( accelValue * deltaF );
        updateShooting( delta );
        // updatePosition( delta );
    }

    public override void _IntegrateForces( PhysicsDirectBodyState2D state )
    {
        base._IntegrateForces( state );
        if( state.LinearVelocity.Length() > _maxVelocityPxSec ) {
            state.LinearVelocity = state.LinearVelocity.Normalized() * _maxVelocityPxSec;
        }
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
        // var deltaF = (float)delta;
        // var accelValue = _maxAccelPxSec * _currentAccel.Normalized();
        // _currentVelocity += deltaF * accelValue;
        // var velValue = Math.Min( _maxVelocityPxSec, _currentVelocity.Length() );
        // _currentVelocity = velValue * _currentVelocity.Normalized();
        // Position += deltaF * _currentVelocity;

        limitPosition();
    }

    private void limitPosition()
    {
        // var rect = GetViewport().GetVisibleRect();
        // if( GlobalPosition.X < rect.Position.X ) {
        //     GlobalPosition = new Vector2( rect.Position.X, GlobalPosition.Y );
        //     _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
        // }
        // if( GlobalPosition.X > rect.End.X ) {
        //     GlobalPosition = new Vector2( rect.End.X, GlobalPosition.Y );
        //     _currentVelocity = new Vector2( -_currentVelocity.X, _currentVelocity.Y );
        // }
        // if( GlobalPosition.Y < rect.Position.Y ) {
        //     GlobalPosition = new Vector2( GlobalPosition.X, rect.Position.Y );
        //     _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
        // }
        // if( GlobalPosition.Y > rect.End.Y ) {
        //
        //     GlobalPosition = new Vector2( GlobalPosition.X, rect.End.Y );
        //     _currentVelocity = new Vector2( _currentVelocity.X, -_currentVelocity.Y );
        // }
    }
}


