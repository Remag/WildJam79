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
    private int _maxHp = 3;

    [Export]
    private PackedScene _tentaclePrefab;

    [Export]
    private int _requiredDecalXp = 100;
    [Export]
    public Godot.Collections.Array<int> GrowthXpByLvl { get; private set; }
    [Export]
    private Godot.Collections.Array<PlayerBlob> _blobs;
    [Export]
    private AudioStreamPlayer _shootSoundPlayer;
    [Export]
    private AudioStreamPlayer _chompSoundPlayer;

    public int CurrentGrowthLevel { get; private set; }
    private int _currentGrowthXp = 0;
    private List<HeroWeaponCore> _activeWeapons = new();

    private int _decalXpLeft = 0;

    private int _currentBlobIndex = 0;

    private bool _isShooting = false;
    private bool _isPlayerControlled = true;

    private int _currentHp = 5;

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

        _eyeball.Modulate = Colors.Red.Lerp( Colors.White, (float)_currentHp / _maxHp );
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
        if( _currentBlobIndex >= _blobs.Count ) {
            return;
        }
        DestroyTentacle();

        gainGeneralExp( food );
        if( food.IsWeaponSource ) {
            AssimilateWeapon( food );
        }
        _currentHp = Math.Min( _currentHp + food.HealHp, _maxHp );
        _chompSoundPlayer.Play();
    }

    public void GainExp( int value )
    {
        _currentGrowthXp += value;
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

        _currentGrowthXp -= targetXp;
        CurrentGrowthLevel++;
        var animName = "Growth" + CurrentGrowthLevel.ToString();
        _animations.Play( animName );
    }

    public void ResetExp()
    {
        _currentGrowthXp = 0;
        CurrentGrowthLevel = 0;
        _animations.Play( "RESET" );
        UpdateCameraZoom( 1 );
    }

    public void UpdateCameraZoom( float zoomValue )
    {
        var zTween = Camera.CreateTween();
        zTween.TweenProperty( Camera, "zoom", new Vector2( zoomValue, zoomValue ), 0.5 ).SetEase( Tween.EaseType.InOut );
        var sTween = Camera.CreateTween();
        sTween.TweenProperty( Camera, "scale", new Vector2( 1 / zoomValue, 1 / zoomValue ), 0.5 ).SetEase( Tween.EaseType.InOut );
    }

    private void gainGeneralExp( FoodSource food )
    {
        GainExp( food.GeneralExp );
        _decalXpLeft -= food.GeneralExp;

        if( _decalXpLeft <= 0 && !food.IsWeaponSource ) {
            _decalXpLeft = _requiredDecalXp;
            attachBlob( food.CorePrefab );
        }
    }

    private Node2D attachBlob( PackedScene blobPrefab )
    {
        var newBlob = _blobs[_currentBlobIndex];
        var newCore = newBlob.Initialize( blobPrefab );
        _currentBlobIndex++;
        return newCore;
    }

    public void AssimilateWeapon( FoodSource weapon )
    {
        var existingWeapon = _activeWeapons.Find( ( target ) => target.SrcCore == weapon.CorePrefab );
        if( existingWeapon != null ) {
            existingWeapon.GainExp( weapon.WeaponXp );
        } else {
            var newWeapon = (HeroWeaponCore)attachBlob( weapon.CorePrefab );
            newWeapon.Initialize( weapon.CorePrefab );
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
            _activeTentacle = _tentaclePrefab.Instantiate<Tentacle>();
            _activeTentacle.Initialize( this );
            Game.Field.AddChild( _activeTentacle );
        }
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
        state._decalXpLeft = _decalXpLeft;
        state._currentBlobIndex = _currentBlobIndex;
        state._currentHp = _currentHp;
        foreach (var playerBlob in _blobs)
        {
            if( playerBlob.initCore != null ) {
                state.blobCores.Add(playerBlob.initCore);
            }
        }
        foreach (var heroWeaponCore in _activeWeapons)
        {
            state.weaponCoreStates.Add( heroWeaponCore.SaveState() );
        }
        return state;
    }

    public void RestoreState( SavedState state )
    {
        int weaponCoreIndex = 0;
        foreach (var stateBlobCore in state.blobCores) {
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
        _decalXpLeft = state._decalXpLeft;
        _currentBlobIndex = state._currentBlobIndex;
        _currentHp = state._currentHp;
    }
    
    public class SavedState {
        public int CurrentGrowthLevel = 0;
        public int _currentGrowthXp = 0;

        public int _decalXpLeft = 0;
        public int _currentBlobIndex = 0;

        public int _currentHp = 5;

        public Godot.Collections.Array<PackedScene> blobCores = new();
        public List<HeroWeaponCore.SavedState> weaponCoreStates = new();

    }
}
