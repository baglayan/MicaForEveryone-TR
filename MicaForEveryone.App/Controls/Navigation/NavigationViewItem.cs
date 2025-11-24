using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinRT;

namespace MicaForEveryone.App.Controls.Navigation;

public sealed partial class NavigationViewItem : ListViewItem
{
    public NavigationViewItem()
    {
        this.DefaultStyleKey = typeof(NavigationViewItem);
    }

    [GeneratedDependencyProperty]
    public partial IconElement? Icon { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool SelectsOnInvoked { get; set; }

    [GeneratedDependencyProperty]
    public partial FlyoutBase? Flyout { get; set; }

    private UIElement? m_selectionIndicator;
    public UIElement? GetSelectionIndicator() => m_selectionIndicator;

    [DynamicWindowsRuntimeCast(typeof(UIElement))]
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        m_selectionIndicator = (UIElement)GetTemplateChild("SelectionIndicator");
    }
}