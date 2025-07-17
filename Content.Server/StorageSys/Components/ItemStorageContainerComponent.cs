using Content.Shared.StorageMarket.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.StorageSys.Components;

[RegisterComponent]
public sealed partial class ItemStorageContainerComponent : Component
{
    public Dictionary<ProtoId<StorageEntryPrototype>, int> Storage = [];

    /// <summary>
    /// Per-item maximum storage capacity. A tiny item consumes 1 capacity, small 2, normal 4, so on and so forth.
    /// Stacked items consume one item's worth of space per stack. (e.g. 50 steel sheets consume 4 capacity)
    /// </summary>
    [DataField(required: true)]
    public int Capacity;
}