using Godot;

namespace WildJam78.Scripts.UI;

[GlobalClass]
public partial class EnemyNodeInfo : Resource {
    [Export]
    public Godot.Collections.Array<EnemyWaveInfo> WavesInfo;

    [Export] 
    public int MinShipsAliveForNextWave = 1;
}