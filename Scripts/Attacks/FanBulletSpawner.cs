using Godot;
using System;

[GlobalClass]
public partial class FanBulletSpawner : BulletSpawner {
    [Export]
    private PackedScene _bulletPrefab;
    [Export]
    private int _counter = 3;
    [Export]
    private float _fromAngleDeg = -30;
    [Export]
    private float _toAngleDeg = 30;
    [Export]
    private bool _isFullCircle = false;
    

    public override void SpawnBullets( Node2D shootAnchor, Vector2 targetGlobalPos, bool isEnemyBullet )
    {
        var angleCounter = _isFullCircle ? _counter : ( _counter - 1 );
        for( var i = 0; i < _counter; i++ ) {
            var angleDeg = _fromAngleDeg + ( _toAngleDeg - _fromAngleDeg ) * i / angleCounter;
            
            var bulletDir = targetGlobalPos - shootAnchor.GlobalPosition;
            var bullet = _bulletPrefab.Instantiate<BasicBullet>();
            bullet.Rotation = bulletDir.Angle() + Mathf.DegToRad(angleDeg);
            bullet.GlobalPosition = shootAnchor.GlobalPosition;
            bullet.SetIsEnemyBullet( isEnemyBullet );
            Game.Field.AddChild( bullet );
        }
    }
}
