using Godot;

namespace WildJam78.Scripts.Shield;

[GlobalClass]
public partial class ShieldConfig : Resource {
    [Export] public int maxHp = 5;
    [Export] public int shieldRegenHp = 5;
    [Export] public float shieldRegenTime = 10;
}
