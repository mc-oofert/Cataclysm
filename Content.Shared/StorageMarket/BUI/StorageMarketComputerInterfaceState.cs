using Content.Shared.StorageMarket.Data;
using Robust.Shared.Serialization;

namespace Content.Shared.StorageMarket.BUI;

[NetSerializable, Serializable]
public sealed class StorageMarketComputerInterfaceState(List<StorageMarketEntry> entries) : BoundUserInterfaceState
{
    /// <summary>
    /// A snapshot of the market entries available for trade on the connected storage net.
    /// </summary>
    public readonly List<StorageMarketEntry> Entries = entries;
}