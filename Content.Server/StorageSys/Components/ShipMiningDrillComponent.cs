using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.StorageSys.Components;

[RegisterComponent]
public sealed partial class ShipMiningDrillComponent : Component
{
    public const string DrillFixture = "drill";
    [DataField]
    public float DrillCooldown = 0.7f;
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;
    [DataField(required: true)]
    public SoundSpecifier DrillSound;
    [DataField(required: true)]
    public SoundSpecifier PassiveSound;

    public TimeSpan NextDrill = TimeSpan.Zero;
    public HashSet<EntityUid> Targets = new();
}
