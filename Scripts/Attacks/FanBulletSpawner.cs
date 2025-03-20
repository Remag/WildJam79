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
    

    public override void SpawnBullets( Node2D shootAnchor, Vector2 targetCanvasPos, bool isEnemyBullet )
    {
        for( var angleDeg = _fromAngleDeg; angleDeg <= _toAngleDeg; angleDeg += (_toAngleDeg - _fromAngleDeg) / (_counter - 1) ) {
            var bulletDir = targetCanvasPos - Game.Camera.GetCanvasTransform() * shootAnchor.GlobalPosition;
            
            var bullet = _bulletPrefab.Instantiate<BasicBullet>();
            bullet.Rotation = bulletDir.Angle() + Mathf.DegToRad(angleDeg);
            bullet.GlobalPosition = shootAnchor.GlobalPosition;
            bullet.SetIsEnemyBullet( isEnemyBullet );
            Game.Field.AddChild( bullet );
        }
    }
}
