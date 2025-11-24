using CommunityToolkit.WinUI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using WinRT;

namespace MicaForEveryone.App.Controls.Navigation;

public class NavigationViewListSelectionChangedEventArgs
{
    public object? SelectedItem { get; init; }
    public required NavigationViewItem SelectedItemContainer { get; init; }
}

[GeneratedBindableCustomProperty]
internal sealed partial class NavigationViewList : Control
{
    public NavigationViewList()
    {
        this.DefaultStyleKey = typeof(NavigationViewList);
        MenuItems = new();
    }

    [GeneratedDependencyProperty]
    public partial bool CanDragItems { get; set; }

    [GeneratedDependencyProperty]
    public partial bool CanReorderItems { get; set; }

    [GeneratedDependencyProperty]
    public partial List<object>? MenuItems { get; set; }

    [GeneratedDependencyProperty]
    public partial object? ItemsSource { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplate? ItemTemplate { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplateSelector? ItemTemplateSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial Style? ItemContainerStyle { get; set; }

    [GeneratedDependencyProperty]
    public partial StyleSelector? ItemContainerStyleSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial TransitionCollection? ItemContainerTransitions { get; set; }

    private NavigationViewListView? m_controlList;

    private NavigationView? m_nv;

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        m_controlList = (NavigationViewListView)GetTemplateChild("ListControl");
        if (m_controlList is not null)
        {
            m_controlList.ItemsSource = ItemsSource ?? MenuItems;
            m_controlList.ItemClick += ControlList_ItemClick;
        }
    }
    partial void OnItemsSourceChanged(object? newValue)
    {
        m_controlList?.ItemsSource = newValue ?? MenuItems;
    }

    public void AssociateNavigationView(NavigationView navigationView)
    {
        m_nv = navigationView;
    }

    private void ControlList_ItemClick(object sender, ItemClickEventArgs e)
    {
        NavigationViewItem? container = m_controlList!.ContainerFromItem(e.ClickedItem) as NavigationViewItem;
        if (container is null)
        {
            container = m_controlList.GetLastItemItsOwnContainerOverride();
        }
        if (container is null)
            return;

        m_nv?.OnItemInvoke(this, e.ClickedItem, container);

        container.Flyout?.ShowAt(container);

        if (!container.SelectsOnInvoked)
            return;

        /*
        if (oldSelectedItem is not null)
        {
            NavigationViewItem? oldContainer = m_controlList?.ContainerFromItem(oldSelectedItem) as NavigationViewItem;
            if (oldContainer is not null && oldContainer.GetSelectionIndicator() is UIElement selectionIndicator)
            {
                selectionIndicator.Opacity = 0.0f;
            }
        }

        AnimateSelectionChanged(container);
        container.IsSelected = true;

        SelectionChanged?.Invoke(this, new NavigationViewListSelectionChangedEventArgs
        {
            SelectedItem = e.ClickedItem,
            SelectedItemContainer = container
        });
        */

        m_nv?.OnSelectionInvoke(this, e.ClickedItem, container);
    }

    public DependencyObject? ContainerFromItem(object item)
    {
        return m_controlList?.ContainerFromItem(item);
    }

    public object? SelectedItem => m_controlList?.SelectedItem;
}
