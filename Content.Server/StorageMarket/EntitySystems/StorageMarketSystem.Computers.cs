using Content.Shared.StorageMarket.Prototypes;
using Content.Server.StorageSys.Components;
using Content.Shared.StorageMarket.BUI;
using Content.Shared.StorageMarket.Data;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Stacks;
using Content.Server.StorageSys.NodeGroups;

namespace Content.Server.StorageMarket.EntitySystems;

public sealed partial class StorageMarketSystem : EntitySystem
{
    public void InitializeComputers()
    {
        SubscribeLocalEvent<StorageMarketComputerComponent, BoundUIOpenedEvent>(OnComputerUIOpened);
    }

    private void OnComputerUIOpened(EntityUid uid, StorageMarketComputerComponent comp, BoundUIOpenedEvent args)
    {
        RefreshState(uid, comp);
    }

    public void RefreshState(EntityUid uid, StorageMarketComputerComponent? computer = null)
    {
        if (!Resolve(uid, ref computer))
            return;
        if (!_storageNetSystem.TryGetStorageNet(uid, out var net))
            return;
        if (net.ControllerData == null)
            return;

        var marketData = net.ControllerData.MarketData;

        List<StorageMarketEntry> entries = new();

        foreach (var protoId in marketData.Entries)
            if (TryCreateEntry(protoId, net, out var entry))
                entries.Add(entry.Value);
    }

    public bool TryCreateEntry(ProtoId<StorageEntryPrototype> protoId, StorageNet net, [NotNullWhen(true)] out StorageMarketEntry? entry)
    {
        entry = null;

        if (!_prototypeManager.TryIndex(protoId, out var entryPrototype))
            return false;
        if (entryPrototype.Prototype == null && entryPrototype.StackPrototype == null)
            return false;

        entry = new(entryPrototype, GetBasePrice(entryPrototype), _storageNetSystem.GetEntryCount(protoId, net), false);
        return true;
    }
}