using Content.Shared.Stacks;
using Content.Shared.StorageMarket.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.StorageMarket.Data;

[NetSerializable, Serializable]
public readonly struct StorageMarketEntry
{
    [ViewVariables]
    public readonly EntProtoId? Prototype;

    [ViewVariables]
    public readonly ProtoId<StackPrototype>? StackPrototype;

    [ViewVariables]
    public readonly StorageMarketCategories Categories;

    [ViewVariables]
    public readonly StorageMarketDepartments Departments;

    [ViewVariables]
    public readonly int BasePrice;

    [ViewVariables]
    public readonly int Quantity;

    [ViewVariables]
    public readonly bool IsCraftable;

    public StorageMarketEntry(StorageEntryPrototype prototype, int basePrice, int quantity, bool isCraftable)
    {
        Prototype = prototype.Prototype;
        StackPrototype = prototype.StackPrototype;
        Categories = prototype.Categories;
        Departments = prototype.Departments;
        BasePrice = basePrice;
        Quantity = quantity;
        IsCraftable = isCraftable;
    }
}