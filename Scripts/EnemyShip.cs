using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;
using WildJam78.Scripts;
using WildJam78.Scripts.Audio;
using WildJam78.Scripts.EnemyMove;

public partial class EnemyShip : FoodSource {
	[Export]
	private int _maxHp = 3;
	private int _currentHp = 3;
	[Export]
	public int SizeLevel = 0;
	[Export]
	private EnemyMoveConfigRigid _configRigid = new();

	public double MoveDelay = 0;
	public bool IsShootingEnabled = true;

	[Export]
	private PackedScene _damageEffect;
	[Export]
	private Godot.Collections.Array<PackedScene> _partDecals = new();
	[Export]
	private Node2D _damageEffectAnchor;

	[Export]
	public Node2D VisualNode { get; set; }
	[Export]
	private ShipTrail _trail;

	private EnemyMoveHandlerRigid _moveHandlerRigid = null;

	[Export]
	private OffscreenIndicator _offscreenIndicator = null;
	[Export]
	private Area2D _hitbox = null;
	[Export]
	private Shield _shield = null;

    [Export]
    private RandomPitchSoundPlayer _hullHitSoundPlayer;

    public override void _Ready()
	{
		Debug.Assert( _damageEffect != null, "Damage effect not set in the enemy ship prefab." );
		Debug.Assert( _damageEffectAnchor != null, "Damage effect anchor not set in the enemy ship prefab." );

		AddToGroup( "ClearOnRestart" );
		_moveHandlerRigid = new EnemyMoveHandlerRigid( config: _configRigid, enemyShip: this );

		_currentHp = _maxHp;

		if( _shield != null ) {
			_shield.ShieldDestroy += OnShieldDestroy;
			_hitbox.Monitorable = false;
		}

		if( IsShootingEnabled && _offscreenIndicator != null ) {
			RemoveChild( _offscreenIndicator );
			GetParent().AddChild( _offscreenIndicator );
			var zoom = 1f / Game.Camera.Zoom.X;
			_offscreenIndicator.GlobalScale = new Vector2(zoom, zoom);
		}
	}

	public override void _IntegrateForces( PhysicsDirectBodyState2D state )
	{
		if( MoveDelay <= 0 && !IsDead ) {
			base._IntegrateForces( state );
			_moveHandlerRigid?.OnShipIntegrateForces( state );
		}
		if( IsDead ) {

		}
	}

	public override void _PhysicsProcess( double delta )
	{
		MoveDelay -= delta;
		if( MoveDelay <= 0 && IsShootingEnabled && !IsDead ) {
			handleIndicator();
		}
	}

	private void handleIndicator()
	{
		if( _offscreenIndicator == null ) {
			return;
		}
		var cameraRect = Game.Camera.GetCanvasTransform().AffineInverse() * GetViewportRect();
		var position = GlobalPosition;
		if( !cameraRect.HasPoint( position ) ) {
			_offscreenIndicator.ShowIndicator();
			_offscreenIndicator.Rotation = ( position - cameraRect.GetCenter() ).Angle();
			var newPosition = position.Clamp( cameraRect.Position, cameraRect.End );
			_offscreenIndicator.GlobalPosition = newPosition;
		} else {
			_offscreenIndicator.HideIndicator();
		}
	}

	public void OnScreenWrap( Rect2 screenWrapRect )
	{
		var player = Game.Player;
		if( player == null ) {
			return;
		}

		var screenSize = screenWrapRect.Size;
		var diff = GlobalPosition - player.GlobalPosition;
		if( diff.X > screenSize.X / 2 ) {
			Position -= new Vector2( screenSize.X, 0 );
		}
		if( diff.X < -screenSize.X / 2 ) {
			Position += new Vector2( screenSize.X, 0 );
		}
		if( diff.Y > screenSize.Y / 2 ) {
			Position -= new Vector2( 0, screenSize.Y );
		}
		if( diff.Y < -screenSize.Y / 2 ) {
			Position += new Vector2( 0, screenSize.Y );
		}
	}

	public void OnBulletCollision( int damage )
	{
		doDamage( damage, null );
        _hullHitSoundPlayer.Play();
    }

    public override Node2D GetTentacleAnchor()
	{
		return _damageEffectAnchor;
	}

	public override void OnTentacleCollision()
	{
	}

	public override bool TryTentaclePull( Tentacle tentacle )
	{
		if( IsPulledByTentacle ) {
			return false;
		}
		var playerSize = Game.Player.CurrentGrowthLevel;
		var dmg = Game.Player.GetTentacleDamage();
		if( playerSize > SizeLevel || dmg >= _currentHp || IsDead ) {
			prepareTentacleAttach();
			return true;
		} else {
			doDamage( dmg, tentacle.GetEndAnchor() );
			return false;
		}
	}

	private void doDamage( int dmg, Node2D dmgPositionSrc )
	{
		var prevWeak = isWeak();
		_currentHp -= dmg;

		if( _currentHp <= 0 ) {
			IsDead = true;
			IsShootingEnabled = false;
			Game.Field.RemoveExistingShip();
			resetColor();
			onMajorDamage( dmgPositionSrc );
			_trail.HideTrail();
			if( IsInstanceValid( _offscreenIndicator ) ) {
				_offscreenIndicator.QueueFree();
			}

		} else if( !prevWeak && isWeak() ) {
			onMajorDamage( dmgPositionSrc );
		}
		VisualNode.Modulate = Colors.Red.Lerp( Colors.White, (float)_currentHp / _maxHp );
	}

	private void resetColor()
	{
		var tween = CreateTween();
		tween.TweenProperty( VisualNode, "modulate", new Color( 0.25f, 0.25f, 0.25f ), 0.7 );
	}

	private void onMajorDamage( Node2D dmgPositionSource )
	{
		spawnParts( dmgPositionSource );
		if( _damageEffect != null ) {
			var dmgEffect = _damageEffect.Instantiate<Node2D>();
			_damageEffectAnchor.AddChild( dmgEffect );
			if( dmgPositionSource != null ) {
				dmgEffect.GlobalPosition = dmgPositionSource.GlobalPosition;
			}
		}
	}

	private void spawnParts( Node2D dmgPositionSource )
	{
		if( _partDecals.Count == 0 ) {
			return;
		}
		var partPrefab = Rng.Choose( _partDecals );
		var part = partPrefab.Instantiate<Node2D>();
		var posSource = dmgPositionSource ?? this;
		part.GlobalPosition = posSource.GlobalPosition;
		Game.Field.AddChild( part );
	}

	private bool isWeak()
	{
		return _currentHp < _maxHp / 2.0;
	}

	private void prepareTentacleAttach()
	{
		Freeze = true;
		IsShootingEnabled = false;
		IsPulledByTentacle = true;
		CollisionLayer = 0;
	}

	public override void OnBroughtToPlayer( Tentacle tentacle )
	{
		Game.Player?.Assimilate( this, tentacle );
		if( !IsDead ) {
			IsDead = true;
			Game.Field.RemoveExistingShip();
		}

		var removeAction = () => { QueueFree(); };
		var tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty( VisualNode, "scale", new Vector2( 0, 0 ), 0.2 );
		tween.TweenCallback( Callable.From( removeAction ) ).SetDelay( 0.2 );
		if( IsInstanceValid( _offscreenIndicator ) ) {
			_offscreenIndicator.QueueFree();
		}
	}

	public void DisableAllBehavior()
	{
		IsShootingEnabled = false;
	}

	public void SetTrail( bool isSet )
	{
		if( isSet ) {
			_trail.ShowTrail();
		} else {
			_trail.HideTrail();
		}
	}

	public void OnShieldDestroy()
	{
		_hitbox.SetDeferred( Area2D.PropertyName.Monitorable, true );
	}
}
