using Godot;
using System;
using WildJam78.Scripts;
using WildJam78.Scripts.Shield;

public partial class Shield : ParentArea2D {
    [Export]
    private ShieldConfig _config = new();
    [Export]
    private AudioStreamPlayer _shieldDropSoundPlayer;
    [Export]
    private AudioStreamPlayer _shieldReflectSoundPlayer;

    [Signal]
    public delegate void ShieldDestroyEventHandler();

    private int _currentHp;
    private float _currentHpRegenTime;
    public override void _Ready()
    {
        _currentHp = _config.maxHp;
        RealParent = this;
    }

    public void OnBulletCollision( int damage )
    {
        _currentHp -= damage;
        if( _currentHp <= 0 ) {
            _currentHp = 0;
            SetDeferred( Area2D.PropertyName.Monitorable, false );
            Visible = false;
            EmitSignal( SignalName.ShieldDestroy );
            _shieldDropSoundPlayer.Play();
        } else {
            _shieldReflectSoundPlayer.Play();
        }
        Modulate = Colors.Transparent.Lerp( Colors.White, (float)_currentHp / _config.maxHp );
    }
}
