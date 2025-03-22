using Godot;
using System;

[GlobalClass]
public abstract partial class BulletSpawner : Resource
{
    public abstract void SpawnBullets( Node2D shootAnchor, Node2D spawnPoint, Vector2 targetGlobalPos, bool isEnemyBullet );
}
