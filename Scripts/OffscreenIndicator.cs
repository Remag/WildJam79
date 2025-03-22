using Godot;

namespace WildJam78.Scripts;

public partial class OffscreenIndicator : Node2D {
    
    private bool _isShowing = false;
    private float _lerpValue = 0;

    public override void _PhysicsProcess( double delta )
    {
        var deltaF = ( float ) delta;
        if( _isShowing ) {
            var timeScale = 0.5f;
            _lerpValue = Mathf.Min( 1f, _lerpValue + deltaF / timeScale );
        } else {
            var timeScale = 0.1f;
            _lerpValue = Mathf.Max( 0f, _lerpValue - deltaF / timeScale );
        }

        Modulate = Colors.Transparent.Lerp( Colors.White, _lerpValue );
        if( _lerpValue <= 0f ) {
            Hide();
        } else {
            Show();
        }
    }

    public void ShowIndicator()
    {
        _isShowing = true;
    }
    
    public void HideIndicator()
    {
        _isShowing = false;
    }
}