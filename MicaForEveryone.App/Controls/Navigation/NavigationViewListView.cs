using Microsoft.UI.Xaml.Controls;

namespace MicaForEveryone.App.Controls.Navigation;

public sealed partial class NavigationViewListView : ListView
{
    private NavigationViewItem? _lastItemItsOwnContainerOverride;

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        if (item is NavigationViewItem nvItem)
        {
            _lastItemItsOwnContainerOverride = nvItem;
            return true;
        }
        return false;
    }

    public NavigationViewItem? GetLastItemItsOwnContainerOverride()
    {
        return _lastItemItsOwnContainerOverride;
    }
}