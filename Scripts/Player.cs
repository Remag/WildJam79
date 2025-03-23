using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WildJam78.Scripts;
using Array = Godot.Collections.Array;

public partial class Player : RigidBody2D {

    [Export]
    private Node2D _mainNode;
    [Export]
    private Godot.Collections.Array<Node2D> _eyeballs;
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
    private Godot.Collections.Array<PackedScene> _autoTentaclesBySize;
    [Export]
    private Godot.Collections.Array<int> _tentacleDamageByLevel;

    [Export]
    private ParentArea2D _autoTentacleArea;

    [Export]
    private Godot.Collections.Array<Node2D> _playerHitboxes;
    [Export]
    public Godot.Collections.Array<int> GrowthXpByLvl { get; private set; }
    [Export]
    private Godot.Collections.Array<float> _cameraZoomByLvl;
    [Export]
    private Godot.Collections.Array<float> _heroHitboxScaleByLvl;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _smallBlobs;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _mediumBlobs;
    [Export]
    private Godot.Collections.Array<PlayerBlob> _largeBlobs;
    private Godot.Collections.Array<PlayerBlob> _prevBlobs;
    private Godot.Collections.Array<PlayerBlob> _currentBlobs;
    [Export]
    private AudioStreamPlayer _chompSoundPlayer;

    public int CurrentGrowthLevel { get; private set; }
    private int _currentGrowthXp = 0;
    private List<HeroWeaponCore> _activeWeapons = new();

    private int _currentBlobIndex = 0;

    private bool _isShooting = false;
    private bool _isPlayerControlled = true;
    private bool _canMove = true;
    private bool _isEatingEnemies = false;

    private int _currentHp = 5;
    public bool CanDie = true;

    private TentaclePlayer _activeTentacle;
    private List<TentacleAuto> _autoTentacles = new();

    public override void _EnterTree()
    {
        Game.Player = this;
    }

    public override void _Ready()
    {
        _currentHp = _maxHpByLvl[CurrentGrowthLevel];
        _currentBlobs = getBlobsList( CurrentGrowthLevel );
        _autoTentacleArea.Monitoring = CurrentGrowthLevel >= 2;
        Mass = 10 ^ CurrentGrowthLevel;
    }

    public override void _ExitTree()
    {
        if( Game.Player == this ) {
            Game.Player = null;
        }
    }

    public void SetMoveEnabled( bool isEnabled )
    {
        _canMove = isEnabled;
    }

    public int GetTentacleDamage()
    {
        return _tentacleDamageByLevel[CurrentGrowthLevel];
    }

    public void OnBulletCollision( int damage )
    {
        if( !_isPlayerControlled ) {
            return;
        }

        _currentHp -= damage;
        if( _currentHp <= 0 ) {
            _currentHp = 0;
            if( CanDie ) {
                Die();
            }
        }

        ModulateEyesColor();
    }

    public void Die()
    {
        DestroyAllTentacles();
        _isPlayerControlled = false;
        _animations.Play( "Death" );
        Game.Field.WorldAudioManager.DeathSoundPlay();
    }

    public void Cleanup()
    {
        Game.Field.EndGame();
        QueueFree();
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
            DestroyAllTentacles();
        }
    }

    public void StartVictoryAnimation()
    {
        _animations.Play( "Victory" );
    }

    public void EatAllEnemies()
    {
        var enemies = GetTree().GetNodesInGroup( "Enemy" );
        if( enemies.Count > 0 ) {
            _isPlayerControlled = false;
            _isShooting = false;
            LinearDamp = 2;
            delayEatAllEnemies();
        } else {
            endVictoryAnimation();
        }
    }

    private async void delayEatAllEnemies()
    {
        await ToSignal( GetTree().CreateTimer( 0.75, processAlways: false, processInPhysics: true ), "timeout" );
        var enemies = GetTree().GetNodesInGroup( "Enemy" );
        foreach( var enemy in enemies ) {
            if( IsInstanceValid( enemy ) ) {
                EatTarget( (EnemyShip)enemy );
            }
        }
        _isEatingEnemies = true;
    }

    public void Assimilate( FoodSource food, Tentacle src )
    {
        GainExp( food.GeneralExp );
        _currentHp = Math.Min( _currentHp + food.HealHp, getMaxHp() );
        _chompSoundPlayer.Play();
        ModulateEyesColor();

        DestroyTentacle( src );

        assimilateGeneral( food );
        if( food.IsWeaponSource ) {
            AssimilateWeapon( food );
        }

        if( !Game.Field.IsCombat() ) {
            TryGrow();
        }
    }

    public void Appear()
    {
        _animations.Play( "Appear" );
    }

    public void GainExp( int value )
    {
        _currentGrowthXp += value;
    }

    public void FullHeal()
    {
        _currentHp = getMaxHp();
        ModulateEyesColor();
    }

    public bool TryGrow( bool isInstant = false )
    {
        var targetXp = getTargetLevelXp();
        if( targetXp < 0 ) {
            return false;
        }
        if( _currentGrowthXp < targetXp ) {
            return false;
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
        ModulateEyesColor();

        var animName = "Growth" + CurrentGrowthLevel.ToString();
        _animations.Play( animName, customSpeed: isInstant ? 100 : 1 );
        _autoTentacleArea.SetDeferred( Area2D.PropertyName.Monitoring, CurrentGrowthLevel >= 2 );
        return true;
    }

    public int getTargetLevelXp()
    {
        if( CurrentGrowthLevel >= GrowthXpByLvl.Count ) {
            return -1;
        }
        return GrowthXpByLvl[CurrentGrowthLevel];
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
            if( !blob.IsWeapon ) {
                continue;
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
        var sTween = Camera.CreateTween();
        zTween.SetProcessMode( Tween.TweenProcessMode.Physics );
        sTween.SetProcessMode( Tween.TweenProcessMode.Physics );
        zTween.TweenProperty( Camera, "zoom", new Vector2( zoomValue, zoomValue ), 0.75 ).SetEase( Tween.EaseType.InOut );
        sTween.TweenProperty( Camera, "scale", new Vector2( 1 / zoomValue, 1 / zoomValue ), 0.75 ).SetEase( Tween.EaseType.InOut );
    }

    private void assimilateGeneral( FoodSource food )
    {
        if( food.IsWeaponSource ) {
            return;
        }
        tryAttachDecalBlob( food.DecalPrefab );
    }

    private void tryAttachDecalBlob( PackedScene decal )
    {
        var targetXp = CurrentGrowthLevel < GrowthXpByLvl.Count ? GrowthXpByLvl[CurrentGrowthLevel] : 1000;
        var chunkXp = targetXp * 1.0f / _currentBlobs.Count;
        var blobIndex = (int)Math.Floor( _currentGrowthXp / chunkXp ) + 1;
        if( blobIndex >= _currentBlobs.Count ) {
            return;
        }
        GD.Print( chunkXp, " ", targetXp, " ", blobIndex, " ", _currentGrowthXp );
        var targetBlob = _currentBlobs[blobIndex];
        if( !targetBlob.Visible ) {
            attachBlob( decal );
        }
    }

    private int getWeaponCount()
    {
        int result = 0;
        foreach( var blob in _currentBlobs ) {
            if( blob.IsWeapon ) {
                result++;
            }
        }
        return result;
    }

    private Node2D attachBlob( PackedScene blobPrefab )
    {
        if( blobPrefab == null || _currentBlobIndex >= _currentBlobs.Count ) {
            return null;
        }
        var newBlob = _currentBlobs[_currentBlobIndex];
        var newCore = newBlob.Initialize( blobPrefab );
        _currentBlobIndex++;
        return newCore;
    }

    public void AssimilateWeapon( FoodSource weapon )
    {
        if( weapon.CorePrefabs == null ) {
            return;
        }
        foreach( var prefab in weapon.CorePrefabs ) {
            doAssimilateWeapon( prefab, weapon.DecalPrefab, weapon.WeaponXp );
        }
    }

    public void ClearWeapons()
    {
        _activeWeapons.Clear();
        _currentBlobIndex = 0;
        foreach( var currentBlob in _currentBlobs ) {
            currentBlob.Reset();
        }
    }

    private void doAssimilateWeapon( PackedScene core, PackedScene backupDecal, int weaponXp )
    {
        var existingWeapon = _activeWeapons.Find( ( target ) => target.SrcCore == core );
        if( existingWeapon != null ) {
            tryAttachDecalBlob( backupDecal );
            existingWeapon.GainExp( weaponXp );

        } else {
            var newWeapon = (HeroWeaponCore)attachBlob( core );
            if( newWeapon == null ) {
                newWeapon = (HeroWeaponCore)switchToWeaponBlob( core );
            }
            if( newWeapon != null ) {
                newWeapon.Initialize( core );
                _activeWeapons.Add( newWeapon );
            }
        }
    }

    private Node2D switchToWeaponBlob( PackedScene core )
    {
        foreach( var blob in _currentBlobs ) {
            if( !blob.IsWeapon ) {
                return blob.SwitchToWeapon( core );
            }
        }
        return null;
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
            _activeTentacle = tentaclePrefab.Instantiate<TentaclePlayer>();
            _activeTentacle.Initialize( this );
            Game.Field.AddChild( _activeTentacle );
        }
    }

    private PackedScene selectTentacle()
    {
        var level = Math.Clamp( CurrentGrowthLevel, 0, _tentaclesByLevel.Count - 1 );
        return _tentaclesByLevel[level];
    }

    public void EatTarget( FoodSource target )
    {
        var tentacle = _autoTentaclesBySize[0].Instantiate<TentacleAuto>();
        tentacle.Initialize( target );
        Game.Field.AddChild( tentacle );
        _autoTentacles.Add( tentacle );
    }

    public void DestroyPlayerTentacle()
    {
        _activeTentacle?.QueueFree();
        _activeTentacle = null;
    }

    public void DestroyTentacle( Tentacle target )
    {
        if( target == _activeTentacle ) {
            DestroyPlayerTentacle();
        }

        for( var i = 0; i < _autoTentacles.Count; i++ ) {
            if( _autoTentacles[i] == target ) {
                target.QueueFree();
                _autoTentacles.RemoveAt( i );
                return;
            }
        }
    }

    public void DestroyAllTentacles()
    {
        DestroyPlayerTentacle();

        foreach( var tentacle in _autoTentacles ) {
            tentacle.QueueFree();
        }
        _autoTentacles.Clear();
    }

    public override void _PhysicsProcess( double delta )
    {
        if( _isEatingEnemies ) {
            updateVictoryEating();
        }

        var deltaF = (float)delta;
        var accelVector = new Vector2();

        if( _isPlayerControlled && _canMove ) {
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
        ApplyForce( accelValue * deltaF * Mass );
        updateShooting( delta );
    }

    private void updateVictoryEating()
    {
        var enemies = GetTree().GetNodesInGroup( "Enemy" );
        if( enemies.Count == 0 ) {
            gainSafetyLevels();
            endVictoryAnimation();
        }
    }

    private void gainSafetyLevels()
    {
        var target = Game.Field.CurrentNodeInfo;
        if( target == null ) {
            return;
        }
        if( CurrentGrowthLevel < target.HeroGrowthOnClear ) {
            var targetXp = getTargetLevelXp();
            if( targetXp > 0 ) {
                GD.Print( "Safety level" );
                _currentGrowthXp = targetXp;
            }
        }

        foreach( var data in target.WeaponLevelsOnClear ) {
            var existingWeapon = _activeWeapons.Find( ( target ) => target.SrcCore == data.Key );
            if( existingWeapon != null ) {
                if( existingWeapon.CurrentLevel < data.Value ) {
                    GD.Print( "Safety weapon" );
                    existingWeapon.GainExp( existingWeapon.GetRequiredExp() );
                }
            }
        }

    }

    private void endVictoryAnimation()
    {
        _isPlayerControlled = true;
        Game.Player.FullHeal();
        if( !Game.Player.TryGrow() ) {
            LinearDamp = 1;
            Game.Field.OnEndVictoryAnimation();
        }
        _isEatingEnemies = false;
    }

    public void OnGrowFinish()
    {
        var level = Math.Clamp( CurrentGrowthLevel, 0, _heroHitboxScaleByLvl.Count - 1 );
        var hitboxScale = _heroHitboxScaleByLvl[level];
        foreach( var hitbox in _playerHitboxes ) {
            hitbox.Scale = new Vector2( hitboxScale, hitboxScale );
        }
        LinearDamp = 1;
        Game.Field.OnPlayerGrowFinish();
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

    private void ModulateEyesColor()
    {
        var modulateColor = Colors.Red.Lerp( Colors.White, (float)_currentHp / getMaxHp() );
        foreach( var eyeball in _eyeballs ) {
            eyeball.Modulate = modulateColor;
        }
    }

    public void OnAutoTentacleAreaEntered( Node node )
    {
        switch( node ) {
            case EnemyShip enemyShip: {
                    if( enemyShip.SizeLevel == 0 || (enemyShip.SizeLevel == 1 && enemyShip.IsDead) ) {
                        CallDeferred( MethodName.EatTarget, enemyShip );
                    }

                    break;
                }
            case FoodSource foodSource:
                CallDeferred( MethodName.EatTarget, foodSource );
                break;
        }
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
        CurrentGrowthLevel = state.CurrentGrowthLevel;
        _currentBlobs = getBlobsList( CurrentGrowthLevel );

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


        _currentGrowthXp = state._currentGrowthXp;
        _currentBlobIndex = state._currentBlobIndex;
        _currentHp = state._currentHp;
        Mass = 10 ^ CurrentGrowthLevel;
        if( CurrentGrowthLevel > 0 ) {
            var animName = "Growth" + CurrentGrowthLevel;

            _animations.Play( animName, customSpeed: 100f );
        }
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
