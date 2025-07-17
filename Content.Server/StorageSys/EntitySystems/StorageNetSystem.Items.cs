using Content.Server.StorageSys.Components;
using Content.Server.StorageSys.Events;
using Content.Server.StorageSys.NodeGroups;
using Content.Shared.Item;
using Content.Shared.Stacks;
using Content.Shared.StorageMarket.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.StorageSys.EntitySystems;

public sealed partial class StorageNetSystem : EntitySystem
{
    private void InitializeItems()
    {
        SubscribeLocalEvent<ItemStorageContainerComponent, StorageNetLoadNodeEvent>(OnItemStorageContainerLoadNode);
        SubscribeLocalEvent<ItemStorageContainerComponent, StorageNetRemoveNodeEvent>(OnItemStorageContainerRemoveNode);
    }

    private void OnItemStorageContainerLoadNode(EntityUid uid, ItemStorageContainerComponent comp, StorageNetLoadNodeEvent args)
    {
        args.Net.ItemContainers.Add(uid);
    }

    private void OnItemStorageContainerRemoveNode(EntityUid uid, ItemStorageContainerComponent comp, StorageNetRemoveNodeEvent args)
    {
        args.Net.ItemContainers.Remove(uid);
    }

    public IEnumerable<(ProtoId<StorageEntryPrototype>, int)> GetEntries(StorageNet net)
    {
        foreach (var containerUid in net.ItemContainers)
            foreach (var storagePair in GetEntries(containerUid))
                yield return storagePair;
    }

    public IEnumerable<(ProtoId<StorageEntryPrototype>, int)> GetEntries(EntityUid containerUid, ItemStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            yield break;

        foreach (var storagePair in container.Storage)
            yield return (storagePair.Key, storagePair.Value);
    }

    public int GetEntryCount(ProtoId<StorageEntryPrototype> entry, StorageNet net)
    {
        var total = 0;

        foreach (var containerUid in net.ItemContainers)
            total += GetEntryCount(entry, containerUid);

        return total;
    }

    public int GetEntryCount(ProtoId<StorageEntryPrototype> entry, EntityUid containerUid, ItemStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;

        return container.Storage.GetValueOrDefault(entry);
    }

    public int GetEntryMaxCount(ProtoId<StorageEntryPrototype> entry, StorageNet net)
    {
        var total = 0;

        foreach (var containerUid in net.ItemContainers)
            total += GetEntryMaxCount(entry, containerUid);

        return total;
    }

    public int GetEntryMaxCount(ProtoId<StorageEntryPrototype> entry, EntityUid containerUid, ItemStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;
        if (!_prototypeManager.TryIndex(entry, out var entryPrototype))
            return 0;

        if (entryPrototype.Prototype != null)
            return GetItemMaxCount(entryPrototype.Prototype.Value, container);
        if (entryPrototype.StackPrototype != null)
            return GetStackMaxCount(entryPrototype.StackPrototype.Value, container);

        return 0;
    }

    private int GetItemMaxCount(EntProtoId protoId, ItemStorageContainerComponent container)
    {
        if (!_prototypeManager.TryIndex(protoId, out var itemPrototype))
            return 0;
        if (!itemPrototype.TryGetComponent<ItemComponent>(out var itemComponent, _componentFactory))
            return 0;
        if (!_prototypeManager.TryIndex(itemComponent.Size, out var sizePrototype))
            return 0;

        return container.Capacity / sizePrototype.Weight;
    }

    private int GetStackMaxCount(ProtoId<StackPrototype> protoId, ItemStorageContainerComponent container)
    {
        if (!_prototypeManager.TryIndex(protoId, out var stackPrototype))
            return 0;
        if (!_prototypeManager.TryIndex(stackPrototype.Spawn, out var itemPrototype))
            return 0;
        if (!itemPrototype.TryGetComponent<ItemComponent>(out var itemComponent, _componentFactory))
            return 0;
        if (!itemPrototype.TryGetComponent<StackComponent>(out var stackComponent, _componentFactory))
            return 0;
        if (!_prototypeManager.TryIndex(itemComponent.Size, out var sizePrototype))
            return 0;

        return container.Capacity * _stackSystem.GetMaxCount(stackComponent) / sizePrototype.Weight;
    }

    public int TryChangeEntryCount(ProtoId<StorageEntryPrototype> entry, int amount, StorageNet net)
    {
        var remainder = amount;

        foreach (var containerUid in net.ItemContainers)
        {
            remainder -= TryChangeEntryCount(entry, remainder, containerUid);

            if (remainder == 0)
                return amount;
        }

        return amount - remainder;
    }

    public int TryChangeEntryCount(ProtoId<StorageEntryPrototype> entry, int amount, EntityUid containerUid, ItemStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;

        var existingAmount = container.Storage.GetValueOrDefault(entry);
        var finalAmount = Math.Clamp(existingAmount + amount, 0, GetEntryMaxCount(entry, containerUid, container));

        if (finalAmount == 0)
            container.Storage.Remove(entry);
        else
            container.Storage[entry] = finalAmount;

        return finalAmount - existingAmount;
    }
}