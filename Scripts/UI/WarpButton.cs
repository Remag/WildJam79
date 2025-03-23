using Godot;
using System;

public partial class WarpButton : Control {
    [Export]
    private Control _scaler;

    private bool _isMouseInside = false;

    private double _animDuration = 0.1;

    public void MouseEnter()
    {
        var tween = _scaler.CreateTween();
        tween.TweenProperty( _scaler, "scale", new Vector2( 1.05f, 1.05f ), _animDuration ).SetEase( Tween.EaseType.InOut );
        _isMouseInside = true;
    }

    public void MouseExit()
    {
        var tween = _scaler.CreateTween();
        tween.TweenProperty( _scaler, "scale", new Vector2( 1.0f, 1.0f ), _animDuration ).SetEase( Tween.EaseType.InOut );
        _isMouseInside = false;
    }

    public void OnPressed()
    {
        _scaler.Scale = new Vector2( 1, 1 );
        // var tween = _scaler.CreateTween();
        // tween.TweenProperty( _scaler, "scale", new Vector2( 1f, 1f ), _animDuration ).SetEase( Tween.EaseType.InOut );
    }

    public void OnReleased()
    {
        var tween = _scaler.CreateTween();
        var targetScale = _isMouseInside ? 1.05f : 1.0f;
        tween.TweenProperty( _scaler, "scale", new Vector2( targetScale, targetScale ), _animDuration ).SetEase( Tween.EaseType.InOut );
    }
}
