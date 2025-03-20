using Godot;
using System;

[GlobalClass]
public partial class FanBulletSpawner : BulletSpawner {
    [Export]
    private PackedScene _bulletPrefab;
    [Export]
    private int _counter = 3;
    [Export]
    private float _maxAngle = 30;
    

    public override void SpawnBullets( Node2D shootAnchor, Vector2 targetCanvasPos, int bulletTargetLayer )
    {
        for(var i = 0; i < _counter; i++ ){
        var bullet = _bulletPrefab.Instantiate<BasicBullet>();
        var bulletDir = targetCanvasPos - Game.Camera.GetCanvasTransform() * shootAnchor.GlobalPosition;
        var ang = _maxAngle*Mathf.Pi/180;
        bullet.Rotation = bulletDir.Angle() - ang + ang / i;
        bullet.GlobalPosition = shootAnchor.GlobalPosition;
        bullet.SetCollisionParams( bulletTargetLayer );
        Game.Field.AddChild( bullet );
        }
    }
}
