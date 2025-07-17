using Content.Server._NF.CrateMachine;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Server.StorageSys.EntitySystems;
using Content.Shared.Containers;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.StorageMarket.EntitySystems;

public sealed partial class StorageMarketSystem : EntitySystem
{
    [Dependency] private readonly CrateMachineSystem _crateMachineSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedContainerUtilitySystem _sharedContainerUtilitiesSystem = default!;
    [Dependency] private readonly StorageNetSystem _storageNetSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeComputers();
    }
}