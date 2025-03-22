using Godot;
using System;

public partial class CheatMenu : Node {

    [Export]
    private Godot.Collections.Array<PackedScene> _weaponsToEat;
    [Export]
    private PackedScene _trashPrefab;

    public void ResetGrowth()
    {
        Game.Player.ResetExp();
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }

    public void Grow()
    {
        var level = Game.Player.CurrentGrowthLevel;
        var maxLevel = Game.Player.GrowthXpByLvl.Count - 1;
        Game.Player.GainExp( Game.Player.GrowthXpByLvl[Math.Min( level, maxLevel )] );
        Game.Player.TryGrow(isInstant:true);
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }
    
    public void ResetWeapons()
    {
        Game.Player.ClearWeapons();
    }

    public void EatWeapon1()
    {
        EatWeapons( 0 );
    }
    
    public void EatWeapon2()
    {
        EatWeapons( 1 );
    }
    
    public void EatWeapon3()
    {
        EatWeapons( 2 );
    }

    public void SpawnTrash()
    {
        var playerPos = Game.Player.Position;
        var trash = _trashPrefab.Instantiate<Node2D>();
        var randomDir = new Vector2( 1, 0 ).Rotated( Rng.RandomRange( 0, 2 * Mathf.Pi ) );
        trash.GlobalPosition = playerPos + randomDir * Rng.RandomRange( 100, 500 );
        Game.Field.AddChild( trash );
    }

    public void EatAllTrash()
    {
        foreach( FoodSource node in GetTree().GetNodesInGroup( "EatOnWin" ) ) {
            Game.Player.EatTarget( node );
        }
    }
    
    private void EatWeapons(int index)
    {
        var weapon = _weaponsToEat[index];
        var obj = weapon.Instantiate<EnemyShip>();
        Game.Player.AssimilateWeapon( obj );
        obj.QueueFree();
        Game.Field.WorldAudioManager.ButtonClickPlay();
    }
}
