using System.Collections.Generic;
using Godot;

namespace WildJam78.Scripts.UI;

[GlobalClass]
public partial class EnemyNodeInfo : Resource {
    [Export]
    public Godot.Collections.Array<EnemyWaveInfo> WavesInfo;
    [Export] 
    public int MinShipsAliveForNextWave = 1;
    [Export]
    public int HeroGrowthOnClear = 0;
    [Export]
    public Godot.Collections.Dictionary<PackedScene, int> WeaponLevelsOnClear;
    [Export]
    public bool IsFinalNode = false;
}