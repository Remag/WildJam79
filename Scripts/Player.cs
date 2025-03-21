using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Array = Godot.Collections.Array;

public partial class Player : RigidBody2D {

    [Export]
    private Node2D _mainNode;
    [Export]
    private Node2D _eyeball;
    [Export]
    private AnimationPlayer _animations;
    [Export]
    public Camera2D Camera;
    [Export]
    private float _maxVelocityPxSec = 500;
    [Export]
    private float _maxAccelPxSec = 100000;
    [Export]
    private Godot.Collections.Array<int> _maxHpByLvl;
    [Export]
    private Godot.Collections.Array<PackedScene> _tentaclesByLevel;

    [Export]
    public Godot.Collections.Array<int> GrowthXpByLvl { get; private set; }
    [Export]
    private Godot.Collections.Array<float> _cameraZoomByLvl;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _smallBlobs;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _mediumBlobs;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _largeBlobs;
    private Godot.Collections.Array<PlayerBlob> _prevBlobs;
    private Godot.Collections.Array<PlayerBlob> _currentBlobs;
    [Export]
    private AudioStreamPlayer _shootSoundPlayer;
    [Export]
    private AudioStreamPlayer _chompSoundPlayer;

    public int CurrentGrowthLevel { get; private set; }
    private int _currentGrowthXp = 0;
    private List<HeroWeaponCore> _activeWeapons = new();

    private int _currentBlobIndex = 0;

    private bool _isShooting = false;
    private bool _isPlayerControlled = true;

    private int _currentHp = 5;

    private Tentacle _activeTentacle;

    public override void _Ready()
    {
        Game.Player = this;
        _currentHp = _maxHpByLvl[0];
        _currentBlobs = _smallBlobs;
    }

    public override void _ExitTree()
    {
        if( Game.Player == this ) {
            Game.Player = null;
        }
    }

    public void OnBulletCollision( int damage )
    {
        if( !_isPlayerControlled ) {
            return;
        }

        _currentHp -= damage;
        if( _currentHp <= 0 ) {
            DestroyTentacle();
            QueueFree();
            Game.Field.EndGame();
        }

        _eyeball.Modulate = Colors.Red.Lerp( Colors.White, (float)_currentHp / getMaxHp() );
    }

    private int getMaxHp()
    {
        var level = Math.Clamp( CurrentGrowthLevel, 0, _maxHpByLvl.Count - 1 );
        return _maxHpByLvl[level];
    }

    public void SetControl( bool isSet )
    {
        _isPlayerControlled = isSet;
        if( !_isPlayerControlled ) {
            stopAttacking();
            DestroyTentacle();
        }
    }

    public void Assimilate( FoodSource food )
    {
        if( _currentBlobIndex >= _currentBlobs.Count ) {
            return;
        }
        DestroyTentacle();

        assimilateGeneral( food );
        if( food.IsWeaponSource ) {
            AssimilateWeapon( food );
        }
        _currentHp = Math.Min( _currentHp + food.HealHp, getMaxHp() );
        _chompSoundPlayer.Play();
    }

    public void GainExp( int value )
    {
        _currentGrowthXp += value;
    }

    public void FullHeal()
    {
        _currentHp = getMaxHp();
    }

    public void TryGrow()
    {
        if( CurrentGrowthLevel >= GrowthXpByLvl.Count ) {
            return;
        }
        var targetXp = GrowthXpByLvl[CurrentGrowthLevel];
        if( _currentGrowthXp < targetXp ) {
            return;
        }

        foreach( var oldBlob in _currentBlobs ) {
            oldBlob.HideCore();
        }
        _currentGrowthXp -= targetXp;
        CurrentGrowthLevel++;
        _currentBlobIndex = 0;
        _prevBlobs = _currentBlobs;
        _currentBlobs = getBlobsList( CurrentGrowthLevel );
        _currentHp = getMaxHp();

        var animName = "Growth" + CurrentGrowthLevel.ToString();
        _animations.Play( animName );
    }

    public void TryRegrowWeapons()
    {
        if( _prevBlobs == null ) {
            return;
        }

        var weaponCoreStates = new List<HeroWeaponCore.SavedState>();
        foreach( var heroWeaponCore in _activeWeapons ) {
            weaponCoreStates.Add( heroWeaponCore.SaveState() );
        }
        _activeWeapons.Clear();

        var weaponCoreIndex = 0;
        foreach( var blob in _prevBlobs ) {
            if( !blob.Visible ) {
                break;
            }
            var blobCore = blob.SrcCore;
            var newWeapon = attachBlob( blobCore );
            if( newWeapon is HeroWeaponCore weaponCore ) {
                weaponCore.Initialize( blobCore );
                weaponCore.RestoreState( weaponCoreStates[weaponCoreIndex] );
                _activeWeapons.Add( weaponCore );
                weaponCoreIndex++;
            }
        }

        _prevBlobs = null;
    }

    private Godot.Collections.Array<PlayerBlob> getBlobsList( int level )
    {
        return level == 0 ? _smallBlobs : level == 1 ? _mediumBlobs : _largeBlobs;
    }

    public void ResetExp()
    {
        _currentGrowthXp = 0;
        CurrentGrowthLevel = 0;
        _animations.Play( "RESET" );
        UpdateCameraZoom();
    }

    public void UpdateCameraZoom()
    {
        var level = Math.Clamp( CurrentGrowthLevel, 0, _cameraZoomByLvl.Count - 1 );
        var zoomValue = _cameraZoomByLvl[level];
        var zTween = Camera.CreateTween();
        zTween.TweenProperty( Camera, "zoom", new Vector2( zoomValue, zoomValue ), 0.75 ).SetEase( Tween.EaseType.InOut );
    }

    private void assimilateGeneral( FoodSource food )
    {
        GainExp( food.GeneralExp );

        var targetXp = GrowthXpByLvl[CurrentGrowthLevel];
        var chunkXp = targetXp * 1.0f / _currentBlobs.Count;
        var blobIndex = (int)Math.Floor( _currentGrowthXp / chunkXp );
        if( blobIndex >= _currentBlobs.Count ) {
            return;
        }
        var targetBlob = _currentBlobs[blobIndex];
        if( !targetBlob.Visible ) {
            attachBlob( food.CorePrefabs[0] );
        }
    }

    private Node2D attachBlob( PackedScene blobPrefab )
    {
        if( _currentBlobs.Count >= _currentBlobIndex ) {
            return null;
        }
        var newBlob = _currentBlobs[_currentBlobIndex];
        var newCore = newBlob.Initialize( blobPrefab );
        _currentBlobIndex++;
        return newCore;
    }

    public void AssimilateWeapon( FoodSource weapon )
    {
        foreach( var prefab in weapon.CorePrefabs ) {
            doAssimilateWeapon( prefab, weapon.WeaponXp );
        }
    }

    private void doAssimilateWeapon( PackedScene core, int weaponXp )
    {
        var existingWeapon = _activeWeapons.Find( ( target ) => target.SrcCore == core );
        if( existingWeapon != null ) {
            existingWeapon.GainExp( weaponXp );
        } else {
            var newWeapon = (HeroWeaponCore)attachBlob( core );
            newWeapon.Initialize( core );
            _activeWeapons.Add( newWeapon );
        }
    }

    public override void _Input( InputEvent e )
    {
        if( !_isPlayerControlled ) {
            return;
        }
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
                    _activeTentacle?.AbortExtend();
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
        if( _activeTentacle == null ) {
            _activeTentacle?.QueueFree();
            var tentaclePrefab = selectTentacle();
            _activeTentacle = tentaclePrefab.Instantiate<Tentacle>();
            _activeTentacle.Initialize( this );
            Game.Field.AddChild( _activeTentacle );
        }
    }

    private PackedScene selectTentacle()
    {
        var level = Math.Clamp( CurrentGrowthLevel, 0, _tentaclesByLevel.Count - 1 );
        return _tentaclesByLevel[level];
    }

    public void DestroyTentacle()
    {
        _activeTentacle?.QueueFree();
        _activeTentacle = null;
    }

    public override void _PhysicsProcess( double delta )
    {
        var deltaF = (float)delta;
        var accelVector = new Vector2();

        if( _isPlayerControlled ) {
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
        }

        var accelValue = _maxAccelPxSec * accelVector.Normalized();
        ApplyForce( accelValue * deltaF );
        updateShooting( delta );
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
        foreach( var weapon in _activeWeapons ) {
            weapon.UpdateShooting( delta );
        }
    }

    internal void ShootSoundPlay()
    {
        _shootSoundPlayer.Play();
    }

    public SavedState SaveState()
    {
        var state = new SavedState();
        state.CurrentGrowthLevel = CurrentGrowthLevel;
        state._currentGrowthXp = _currentGrowthXp;
        state._currentBlobIndex = _currentBlobIndex;
        state._currentHp = _currentHp;
        foreach( var playerBlob in _currentBlobs ) {
            if( playerBlob.SrcCore != null ) {
                state.blobCores.Add( playerBlob.SrcCore );
            }
        }
        foreach( var heroWeaponCore in _activeWeapons ) {
            state.weaponCoreStates.Add( heroWeaponCore.SaveState() );
        }
        return state;
    }

    public void RestoreState( SavedState state )
    {
        int weaponCoreIndex = 0;
        foreach( var stateBlobCore in state.blobCores ) {
            var newWeapon = attachBlob( stateBlobCore );
            if( newWeapon is HeroWeaponCore weaponCore ) {
                weaponCore.Initialize( stateBlobCore );
                weaponCore.RestoreState( state.weaponCoreStates[weaponCoreIndex] );
                _activeWeapons.Add( weaponCore );
                weaponCoreIndex++;
            }
        }

        CurrentGrowthLevel = state.CurrentGrowthLevel;
        _currentGrowthXp = state._currentGrowthXp;
        _currentBlobIndex = state._currentBlobIndex;
        _currentHp = state._currentHp;
    }

    public class SavedState {
        public int CurrentGrowthLevel = 0;
        public int _currentGrowthXp = 0;

        public int _currentBlobIndex = 0;

        public int _currentHp = 5;

        public Godot.Collections.Array<PackedScene> blobCores = new();
        public List<HeroWeaponCore.SavedState> weaponCoreStates = new();

    }
}
