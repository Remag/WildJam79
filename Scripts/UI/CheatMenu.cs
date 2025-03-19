using Godot;
using System;

public partial class CheatMenu : Node {

    [Export]
    private Godot.Collections.Array<PackedScene> _weaponsToEat;

    public void Grow()
    {
        var level = Game.Player.CurrentGrowthLevel;
        var maxLevel = Game.Player.GrowthXpByLvl.Count - 1;
        Game.Player.GainExp( Game.Player.GrowthXpByLvl[Math.Min( level, maxLevel )] );
        Game.Player.TryGrow();
    }

    public void EatWeapons()
    {
        foreach( var weapon in _weaponsToEat ) {
            var obj = weapon.Instantiate<EnemyShip>();
            Game.Player.AssimilateWeapon( obj );
            obj.QueueFree();
        }
    }
}
