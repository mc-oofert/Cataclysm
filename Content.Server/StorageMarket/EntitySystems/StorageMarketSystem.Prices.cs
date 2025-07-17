using Content.Server.StorageSys.NodeGroups;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Content.Shared.StorageMarket.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.StorageMarket.EntitySystems;

public sealed partial class StorageMarketSystem : EntitySystem
{
    public int GetPrice(int basePrice, int quantity, int stockCount, int idealStockValue)
    {
        if (idealStockValue <= 0)
            throw new ArgumentException("'idealStockValue' must be higher than zero.");
        if (quantity <= 0)
            throw new ArgumentException("'quantity' must be higher than zero.");

        var totalPrice = 0;

        for (var i = 0; i < quantity; i++)
        {
            var adjustedStockCount = Math.Max(0, stockCount - i);
            totalPrice += GetPrice(basePrice, adjustedStockCount, idealStockValue);
        }

        return totalPrice;
    }

    public int GetPrice(int basePrice, int stockCount, int idealStockValue)
    {
        var currentStockValue = stockCount * basePrice;

        // Calculate stock ratio: 1 = perfect stock, less than 1 = undersupply, more than 1 = oversupply
        var stockRatio = (float)currentStockValue / idealStockValue;

        // Inverse logic: 2x oversupply = 0.5x price, 0.5x undersupply = 2x price
        // Clamped between 50% and 200% of base price (maybe make the min and max dynamic at some point?)
        var adjustmentMultiplier = 1f / stockRatio;
        adjustmentMultiplier = Math.Clamp(adjustmentMultiplier, 0.5f, 2f);

        return (int)MathF.Round(basePrice * adjustmentMultiplier);
    }

    public int GetBasePrice(StorageEntryPrototype prototype)
    {
        if (prototype.Prototype != null)
            return GetBasePrice(prototype.Prototype.Value);
        if (prototype.StackPrototype != null)
            return GetBasePrice(prototype.StackPrototype.Value);

        return 0;
    }

    public int GetBasePrice(EntProtoId prototypeId)
    {
        if (!_prototypeManager.TryIndex(prototypeId, out var prototype))
            return 0;
        if (!prototype.TryGetComponent<PhysicalCompositionComponent>(out var physicalComposition, _componentFactory))
            return 0;

        return GetBasePrice(physicalComposition);
    }

    public int GetBasePrice(ProtoId<StackPrototype> prototypeId)
    {
        if (!_prototypeManager.TryIndex(prototypeId, out var stackPrototype))
            return 0;
        if (!_prototypeManager.TryIndex(stackPrototype.Spawn, out var entityPrototype))
            return 0;
        if (!entityPrototype.TryGetComponent<PhysicalCompositionComponent>(out var physicalComposition, _componentFactory))
            return 0;

        return GetBasePrice(physicalComposition);
    }

    public int GetBasePrice(EntityUid uid)
    {
        var total = 0;

        foreach (var entityUid in _sharedContainerUtilitiesSystem.GetContentsAndSelf(uid))
            if (TryComp<PhysicalCompositionComponent>(entityUid, out var physicalComposition))
                total += GetBasePrice(entityUid, physicalComposition);

        return total;
    }

    private int GetBasePrice(EntityUid uid, PhysicalCompositionComponent physicalComposition)
    {
        var price = GetBasePrice(physicalComposition);

        if (TryComp<StackComponent>(uid, out var stack))
            price *= stack.Count;

        return price;
    }

    public int GetBasePrice(PhysicalCompositionComponent physicalComposition)
    {
        var price = 0.0;

        foreach (var materialPair in physicalComposition.MaterialComposition)
        {
            if (!_prototypeManager.TryIndex<MaterialPrototype>(materialPair.Key, out var materialPrototype))
                continue;

            price += materialPrototype.Price * materialPair.Value;
        }

        return (int)Math.Round(price);
    }
}