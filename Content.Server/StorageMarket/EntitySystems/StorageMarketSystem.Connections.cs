using Content.Server.StorageMarket.Components;
using Content.Server.StorageSys.Components;
using Content.Shared._NF.CrateMachine.Components;

namespace Content.Server.StorageMarket.EntitySystems;

public sealed partial class StorageMarketSystem : EntitySystem
{
    public IEnumerable<Entity<StorageMarketSellPalletComponent, TransformComponent>> GetConnectedPallets(EntityUid computerUid, StorageMarketComputerComponent? computer = null, TransformComponent? computerTransform = null)
    {
        if (!Resolve(computerUid, ref computer, ref computerTransform))
            yield break;
        if (!computerTransform.Anchored || computerTransform.GridUid == null)
            yield break;
        if (!_powerReceiverSystem.IsPowered(computerUid))
            yield break;

        var query = EntityQueryEnumerator<StorageMarketSellPalletComponent, TransformComponent>();

        while (query.MoveNext(out var palletUid, out var pallet, out var palletTransform))
        {
            if (!palletTransform.Anchored || computerTransform.GridUid != palletTransform.GridUid)
                continue;
            if (!_transformSystem.InRange((palletUid, palletTransform), (computerUid, computerTransform), computer.MachineRange))
                continue;

            yield return (palletUid, pallet, palletTransform);
        }
    }

    public IEnumerable<EntityUid> GetSellables(EntityUid computerUid)
    {
        foreach (var pallet in GetConnectedPallets(computerUid))
            foreach (var sellableUid in GetSellables(pallet, pallet.Comp1, pallet.Comp2))
                yield return sellableUid;
    }

    public IEnumerable<EntityUid> GetSellables(EntityUid palletUid, StorageMarketSellPalletComponent? pallet = null, TransformComponent? palletTransform = null)
    {
        if (!Resolve(palletUid, ref pallet, ref palletTransform))
            yield break;
        if (!palletTransform.Anchored || palletTransform.GridUid == null)
            yield break;

        foreach (var sellableUid in _entityLookupSystem.GetEntitiesIntersecting(palletUid, LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (!TryComp(sellableUid, out TransformComponent? sellableTransform))
                continue;
            if (sellableTransform.Anchored)
                continue;

            yield return sellableUid;
        }
    }

    public IEnumerable<Entity<CrateMachineComponent, TransformComponent>> GetConnectedCrateMachines(EntityUid computerUid, StorageMarketComputerComponent? computer = null, TransformComponent? computerTransform = null)
    {
        if (!Resolve(computerUid, ref computer, ref computerTransform))
            yield break;
        if (!computerTransform.Anchored || computerTransform.GridUid == null)
            yield break;
        if (!_powerReceiverSystem.IsPowered(computerUid))
            yield break;

        var query = EntityQueryEnumerator<CrateMachineComponent, TransformComponent>();

        while (query.MoveNext(out var crateMachineUid, out var crateMachine, out var crateMachineTransform))
        {
            if (!crateMachineTransform.Anchored || computerTransform.GridUid != crateMachineTransform.GridUid)
                continue;
            if (!_transformSystem.InRange((crateMachineUid, crateMachineTransform), (computerUid, computerTransform), computer.MachineRange))
                continue;

            yield return (crateMachineUid, crateMachine, crateMachineTransform);
        }
    }
}