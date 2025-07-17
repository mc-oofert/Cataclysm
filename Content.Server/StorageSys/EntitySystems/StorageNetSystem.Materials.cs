using Content.Server.StorageSys.Components;
using Content.Server.StorageSys.Events;
using Content.Server.StorageSys.NodeGroups;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server.StorageSys.EntitySystems;

public sealed partial class StorageNetSystem : EntitySystem
{
    private void InitializeMaterials()
    {
        SubscribeLocalEvent<MaterialStorageContainerComponent, StorageNetLoadNodeEvent>(OnMaterialStorageContainerLoadNode);
        SubscribeLocalEvent<MaterialStorageContainerComponent, StorageNetRemoveNodeEvent>(OnMaterialStorageContainerRemoveNode);
    }

    private void OnMaterialStorageContainerLoadNode(EntityUid uid, MaterialStorageContainerComponent comp, StorageNetLoadNodeEvent args)
    {
        args.Net.MaterialContainers.Add(uid);
    }

    private void OnMaterialStorageContainerRemoveNode(EntityUid uid, MaterialStorageContainerComponent comp, StorageNetRemoveNodeEvent args)
    {
        args.Net.MaterialContainers.Remove(uid);
    }

    /// <summary>
    /// Returns the amount of the material in the storage net.
    /// </summary>
    public int GetMaterial(ProtoId<MaterialPrototype> material, StorageNet net)
    {
        var amount = 0;

        foreach (var containerUid in net.MaterialContainers)
            amount += GetMaterial(material, containerUid);

        return amount;
    }

    /// <summary>
    /// Returns the amount of the material in the material storage container.
    /// </summary>
    public int GetMaterial(ProtoId<MaterialPrototype> material, EntityUid containerUid, MaterialStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;

        return container.Storage.GetValueOrDefault(material);
    }

    /// <summary>
    /// Returns the calculated final change in the amount of the material in the storage net, without actually changing it.
    /// </summary>
    public int GetMaxMaterialChange(ProtoId<MaterialPrototype> material, int amount, StorageNet net)
    {
        var remainder = amount;

        foreach (var containerUid in net.MaterialContainers)
        {
            remainder -= GetMaxMaterialChange(material, remainder, containerUid);

            if (remainder == 0)
                return amount;
        }

        return amount - remainder;
    }

    /// <summary>
    /// Returns the calculated final change in the amount of the material in the container, without actually changing it.
    /// </summary>
    public int GetMaxMaterialChange(ProtoId<MaterialPrototype> material, int amount, EntityUid containerUid, MaterialStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;

        var existingAmount = container.Storage.GetValueOrDefault(material);
        var finalAmount = Math.Clamp(existingAmount + amount, 0, container.Capacity);

        return finalAmount - existingAmount;
    }

    /// <summary>
    /// Tries to change the amount of the material in the storage net by the given amount.
    /// </summary>
    /// <returns>The final change in the amount of the material.</returns>
    public int TryChangeMaterial(ProtoId<MaterialPrototype> material, int amount, StorageNet net)
    {
        var remainder = amount;

        foreach (var containerUid in net.MaterialContainers)
        {
            remainder -= TryChangeMaterial(material, remainder, containerUid);

            if (remainder == 0)
                return amount;
        }

        return amount - remainder;
    }

    /// <summary>
    /// Tries to change the amount of the material in the container by the given amount.
    /// </summary>
    /// <returns>The final change in the amount of the material.</returns>
    public int TryChangeMaterial(ProtoId<MaterialPrototype> material, int amount, EntityUid containerUid, MaterialStorageContainerComponent? container = null)
    {
        if (!Resolve(containerUid, ref container))
            return 0;

        var existingAmount = container.Storage.GetValueOrDefault(material);
        var finalAmount = Math.Clamp(existingAmount + amount, 0, container.Capacity);

        if (finalAmount == 0)
            container.Storage.Remove(material);
        else
            container.Storage[material] = finalAmount;

        return finalAmount - existingAmount;
    }

    public bool CanChangeMaterial(ProtoId<MaterialPrototype> material, int amount, StorageNet net)
    {
        return GetMaxMaterialChange(material, amount, net) == amount;
    }

    public bool CanChangeMaterial(ProtoId<MaterialPrototype> material, int amount, EntityUid containerUid, MaterialStorageContainerComponent? container = null)
    {
        return GetMaxMaterialChange(material, amount, containerUid, container) == amount;
    }

    /// <summary>
    /// Tries to insert a material entity (singular or stack) into the storage net.
    /// </summary>
    public void TryInsertMaterialEntity(Entity<PhysicalCompositionComponent?> entity, StorageNet net)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        if (TryComp<StackComponent>(entity, out var stack))
            TryInsertMaterialEntityStack((entity, entity.Comp, stack), net);
        else
            TryInsertMaterialEntitySingle(entity, net);
    }

    /// <summary>
    /// Tries to insert a singular material entity (e.g. a gun) into the storage net.
    /// </summary>
    /// <returns>Whether the entity was successfully inserted.</returns>
    public bool TryInsertMaterialEntitySingle(Entity<PhysicalCompositionComponent?> entity, StorageNet net)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        foreach (var materialPair in entity.Comp.MaterialComposition)
            if (!CanChangeMaterial(materialPair.Key, materialPair.Value, net))
                return false;

        foreach (var materialPair in entity.Comp.MaterialComposition)
            TryChangeMaterial(materialPair.Key, materialPair.Value, net);

        Del(entity);
        return true;
    }

    /// <summary>
    /// Tries to insert a stack of material entities (e.g. a stack of steel sheets) into the storage net.
    /// </summary>
    /// <returns>The number of items in the stack that were successfully inserted.</returns>
    public int TryInsertMaterialEntityStack(Entity<PhysicalCompositionComponent?, StackComponent?> entity, StorageNet net)
    {
        if (!Resolve(entity, ref entity.Comp1, ref entity.Comp2))
            return 0;
        if (entity.Comp2.Count <= 0)
            return 0;

        // The highest number of entities in the stack we can insert.
        var maxInsertions = entity.Comp2.Count;

        foreach (var materialPair in entity.Comp1.MaterialComposition)
        {
            if (materialPair.Value <= 0)
                throw new InvalidOperationException($"Material '{materialPair.Key}' has an invalid unit value of '{materialPair.Value}'");

            var stackAmount = materialPair.Value * maxInsertions;
            var maxChange = GetMaxMaterialChange(materialPair.Key, stackAmount, net);

            maxInsertions = maxChange / materialPair.Value;

            if (maxInsertions <= 0)
                return 0;
        }

        if (!_stackSystem.Use(entity, maxInsertions, entity.Comp2))
            return 0;

        foreach (var materialPair in entity.Comp1.MaterialComposition)
            TryChangeMaterial(materialPair.Key, materialPair.Value * maxInsertions, net);

        return maxInsertions;
    }
}