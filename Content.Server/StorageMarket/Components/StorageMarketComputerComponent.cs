namespace Content.Server.StorageSys.Components;

[RegisterComponent]
public sealed partial class StorageMarketComputerComponent : Component
{
    /// <summary>
    /// The maximum distance from sell pallets and crate machines.
    /// </summary>
    [DataField]
    public int MachineRange = 3;
}