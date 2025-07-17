using Content.Shared.Damage;

namespace Content.Server.StorageSys.Components;

[RegisterComponent]
public sealed partial class ShipMiningDrillComponent : Component
{
    public const string DrillFixture = "drill";
    [DataField]
    public float DrillCooldown = 0.7f;
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public TimeSpan NextDrill = TimeSpan.Zero;
    public HashSet<EntityUid> Targets = new();
}
