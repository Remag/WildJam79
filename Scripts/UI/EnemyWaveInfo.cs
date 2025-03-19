using Godot;

namespace WildJam78.Scripts.UI;

[GlobalClass]
public partial class EnemyWaveInfo : Resource {
    [Export]
    public Godot.Collections.Array<EnemyInfo> EnemiesInfo;

    [Export] 
    public float SpawnTimePoint;
}