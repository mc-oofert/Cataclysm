using Content.Client.StorageMarket.UI;

namespace Content.Client.StorageMarket.BUI;

public sealed class StorageMarketComputerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private StorageMarketMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = new();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _menu?.Dispose();
    }
}