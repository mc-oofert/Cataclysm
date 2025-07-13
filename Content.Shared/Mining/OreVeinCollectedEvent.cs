
namespace Content.Shared.Mining;

public sealed class OreVeinCollectedEvent : EntityEventArgs
{
    /// <summary>
    ///     Entity used to attack, for broadcast purposes.
    /// </summary>
    public EntityUid Vein;

    /// <summary>
    ///     Entity that collected this vein
    /// </summary>
    public EntityUid Collector;

    /// <summary>
    ///     The stuff collected from the ore vein
    /// </summary>
    public HashSet<EntityUid> Loot;
    public OreVeinCollectedEvent(EntityUid vein, EntityUid collector, HashSet<EntityUid> loot)
    {
        Vein = vein;
        Collector = collector;
        Loot = loot;
    }
}
