using Content.Server.Power.EntitySystems;
using Robust.Shared.Physics.Events;
using Content.Server.StorageSys.Components;
using Content.Shared.Mining;
using Content.Shared.Damage;
using Robust.Shared.Timing;
using Content.Shared.Mining.Components;

namespace Content.Server.StorageSys.EntitySystems;

public sealed class ShipMiningDrillSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly StorageNetSystem _storageNetSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShipMiningDrillComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ShipMiningDrillComponent, EndCollideEvent>(OnEndCollide);
        SubscribeLocalEvent<ShipMiningDrillComponent, OreVeinCollectedEvent>(OnOreVeinCollected);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ShipMiningDrillComponent>();
        while (query.MoveNext(out var uid, out var drill))
        {
            if (drill.Targets.Count <= 0 || drill.NextDrill > _gameTiming.CurTime)
                continue;
            if (!_powerReceiverSystem.IsPowered(uid))
                continue;
            foreach (EntityUid drilledEntity in drill.Targets)
            {
                TryComp(drilledEntity, out OreVeinComponent? vein);
                if (vein != null)
                    vein.Collector = uid;

                _damageable.TryChangeDamage(drilledEntity, drill.Damage);

                if (vein != null)
                    vein.Collector = null;
            }
            drill.NextDrill = _gameTiming.CurTime + TimeSpan.FromSeconds(drill.DrillCooldown);
        }
    }

    private void OnStartCollide(EntityUid uid, ShipMiningDrillComponent component, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != ShipMiningDrillComponent.DrillFixture)
            return;
        component.Targets.Add(args.OtherEntity);
    }

    private void OnEndCollide(EntityUid uid, ShipMiningDrillComponent component, ref EndCollideEvent args)
    {
        if (args.OurFixtureId != ShipMiningDrillComponent.DrillFixture)
            return;
        component.Targets.Remove(args.OtherEntity);
    }
    private void OnOreVeinCollected(EntityUid uid, ShipMiningDrillComponent component, ref OreVeinCollectedEvent args)
    {
        if (!_storageNetSystem.TryGetStorageNet(uid, out var storageNet))
            return;
        foreach (EntityUid id in args.Loot)
            _storageNetSystem.TryInsertMaterialEntity(id, storageNet);
    }
}
