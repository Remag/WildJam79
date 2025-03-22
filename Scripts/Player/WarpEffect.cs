using Godot;
using System;
using WildJam78.Scripts.UI;

public partial class WarpEffect : Node2D
{
    [Export]
    private Node2D _anchor;
    [Export]
    private Node2D _circleImage;
    [Export]
    private AnimationPlayer _animations;
    [Export]
    private Godot.Collections.Array<float> _scaleByPlayerSize;

    [Export]
    public float WarpEffectRadius {
        get {
            return _warpEffectRadius;
        }
        set {
            _warpEffectRadius = value;
            Game.Field.SetWarpEffectCircle( _warpEffectRadius );
        }
    }

    [Export]
    private AudioStreamPlayer _warpInSoundPlayer;
    [Export]
    private AudioStreamPlayer _warpOutSoundPlayer;

    private float _warpEffectRadius = 0;

    private Player _player;

    Texture2D _bgTo;
    EnemyNodeInfo _nodeTo;

    public void Initialize( Player player, Texture2D locationBg, EnemyNodeInfo nodeInfo )
    {
        _bgTo = locationBg;
        _nodeTo = nodeInfo;

        var scale = _scaleByPlayerSize[player.CurrentGrowthLevel];
        _circleImage.Scale = new Vector2( scale, scale );
        _anchor.Scale = new Vector2( 1 / scale, 1 / scale );

        _player = player;
        GlobalPosition = player.GlobalPosition;
        Game.Field.RemoveChild( player );
        _anchor.AddChild( player );
        player.Position = Vector2.Zero;

        _animations.Play( "Warp" );
        _warpInSoundPlayer.Play();
    }

    public void OnWarpEnd()
    {
        Game.Field.SwitchLocation( _bgTo, _nodeTo, this );
    }

    public void ReverseAnimation()
    {
        _animations.PlayBackwards( "Unwarp" );
        _warpOutSoundPlayer.Play();
    }

    public void ReturnPlayer()
    {
        _anchor.RemoveChild( _player );
        Game.Field.AddChild( _player );
        _player.GlobalPosition = GlobalPosition;
        Game.Field.InitializeEnemyWave( _nodeTo );
    }

    public void Cleanup()
    {
        QueueFree();
    }
}
