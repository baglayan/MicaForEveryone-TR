using CommunityToolkit.WinUI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Numerics;
using TerraFX.Interop.Windows;
using Windows.Foundation;
using WinRT;

namespace MicaForEveryone.App.Controls.Navigation;

public class NavigationViewSelectionChangedEventArgs
{
    public object? SelectedItem { get; init; }
    public required NavigationViewItem SelectedItemContainer { get; init; }
}

public class NavigationViewItemInvokedEventArgs
{
    public object? InvokedItem { get; init; }
    public required NavigationViewItem InvokedItemContainer { get; init; }
}

[ContentProperty(Name = nameof(Content))]
[GeneratedBindableCustomProperty]
public sealed partial class NavigationView : Control
{
    public NavigationView()
    {
        this.DefaultStyleKey = typeof(NavigationView);
        MenuItems = new();
        FooterMenuItems = new();
    }

    [GeneratedDependencyProperty]
    public partial List<object>? MenuItems { get; set; }

    [GeneratedDependencyProperty]
    public partial object? MenuItemsSource { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplate? MenuItemTemplate { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplateSelector? MenuItemTemplateSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial Style? MenuItemContainerStyle { get; set; }

    [GeneratedDependencyProperty]
    public partial StyleSelector? MenuItemContainerStyleSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial TransitionCollection? MenuItemContainerTransitions { get; set; }

    [GeneratedDependencyProperty]
    public partial List<object>? FooterMenuItems { get; set; }

    [GeneratedDependencyProperty]
    public partial object? FooterMenuItemsSource { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplate? FooterMenuItemTemplate { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplateSelector? FooterMenuItemTemplateSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial Style? FooterMenuItemContainerStyle { get; set; }

    [GeneratedDependencyProperty]
    public partial StyleSelector? FooterMenuItemContainerStyleSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial TransitionCollection? FooterMenuItemContainerTransitions { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsPaneOpen { get; set; }

    [GeneratedDependencyProperty(DefaultValue = 280.0)]
    public partial double OpenPaneLength { get; set; }

    [GeneratedDependencyProperty(DefaultValue = SplitViewDisplayMode.Overlay)]
    public partial SplitViewDisplayMode DisplayMode { get; set; }

    [GeneratedDependencyProperty]
    public partial object? Content { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplate? ContentTemplate { get; set; }

    [GeneratedDependencyProperty]
    public partial DataTemplateSelector? ContentTemplateSelector { get; set; }

    [GeneratedDependencyProperty]
    public partial object? SelectedItem { get; set; }

    [GeneratedDependencyProperty]
    public partial Brush? PaneBackground { get; set; }

    public event TypedEventHandler<NavigationView, NavigationViewSelectionChangedEventArgs>? SelectionChanged;
    public event TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs>? ItemInvoked;

    private SplitView? m_splitView;
    private NavigationViewList? m_menuItemsList;
    private NavigationViewList? m_footerMenuItemsList;

    private UIElement? m_prevIndicator;
    private UIElement? m_nextIndicator;
    private UIElement? m_activeIndicator;

    [DynamicWindowsRuntimeCast(typeof(SplitView))]
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        m_splitView = (SplitView)GetTemplateChild("NavigationSplitView");
        m_menuItemsList = (NavigationViewList?)GetTemplateChild("MenuItemsList");
        m_footerMenuItemsList = (NavigationViewList?)GetTemplateChild("FooterMenuItemsList");

        if (m_menuItemsList is not null)
        {
            m_menuItemsList.AssociateNavigationView(this);
        }

        if (m_footerMenuItemsList is not null)
        {
            m_footerMenuItemsList.AssociateNavigationView(this);
        }
    }

    private void MenuItemsList_SelectionChanged(NavigationViewList sender, NavigationViewListSelectionChangedEventArgs args)
    {
        // Clear footer selection when main menu item is selected
        if (m_footerMenuItemsList is not null)
        {
            if (m_footerMenuItemsList.SelectedItem is object item)
            {
                NavigationViewItem? itemContainer = m_footerMenuItemsList.ContainerFromItem(item) as NavigationViewItem;
                itemContainer?.IsSelected = false;
            }
        }
        
        SelectedItem = args.SelectedItem;
        SelectionChanged?.Invoke(this, new NavigationViewSelectionChangedEventArgs
        {
            SelectedItem = args.SelectedItem,
            SelectedItemContainer = args.SelectedItemContainer
        });
    }

    private void FooterMenuItemsList_SelectionChanged(NavigationViewList sender, NavigationViewListSelectionChangedEventArgs args)
    {
        // Clear main menu selection when footer item is selected
        if (m_menuItemsList is not null)
        {
            if (m_menuItemsList.SelectedItem is object item)
            {
                NavigationViewItem? itemContainer = m_menuItemsList.ContainerFromItem(item) as NavigationViewItem;
                itemContainer?.IsSelected = false;
            }
        }

        SelectedItem = args.SelectedItem;
        SelectionChanged?.Invoke(this, new NavigationViewSelectionChangedEventArgs
        {
            SelectedItem = args.SelectedItem,
            SelectedItemContainer = args.SelectedItemContainer
        });
    }

    internal void OnSelectionInvoke(NavigationViewList list, object selectedItem, NavigationViewItem container)
    {
        object? oldSelectedItem = m_menuItemsList?.SelectedItem ?? m_footerMenuItemsList?.SelectedItem;
        if (oldSelectedItem == selectedItem || oldSelectedItem is not null && list?.ContainerFromItem(oldSelectedItem) == container)
            return;

        if (oldSelectedItem is not null)
        {
            NavigationViewItem? oldContainer = (m_menuItemsList?.ContainerFromItem(oldSelectedItem) ?? m_footerMenuItemsList?.ContainerFromItem(oldSelectedItem)) as NavigationViewItem;
            if (oldContainer is not null)
            {
                if (oldContainer.GetSelectionIndicator() is UIElement selectionIndicator)
                    selectionIndicator.Opacity = 0.0f;
                oldContainer.IsSelected = false;
            }
        }

        AnimateSelectionChanged(container);
        container.IsSelected = true;
        SelectionChanged?.Invoke(this, new() { SelectedItem = selectedItem, SelectedItemContainer = container });
    }

    internal void OnItemInvoke(NavigationViewList list, object invokedItem, NavigationViewItem container)
    {
        ItemInvoked?.Invoke(this, new() { InvokedItem = invokedItem, InvokedItemContainer = container });
    }

    private void AnimateSelectionChanged(NavigationViewItem newContainer)
    {
        UIElement? prevIndicator = m_activeIndicator;
        UIElement? nextIndicator = newContainer.GetSelectionIndicator();

        bool haveValidAnimation = false;

        if (m_prevIndicator is not null || m_nextIndicator is not null)
        {
            if (nextIndicator is not null && m_nextIndicator == nextIndicator)
            {
                if (prevIndicator is not null && m_prevIndicator == prevIndicator)
                {
                    ResetElementAnimationProperties(prevIndicator, 0.0f);
                }

                haveValidAnimation = true;
            }
            else
            {
                OnAnimationComplete();
            }
        }

        if (!haveValidAnimation)
        {
            if (prevIndicator is not null && nextIndicator is not null && (prevIndicator != nextIndicator))
            {
                ResetElementAnimationProperties(prevIndicator, 1.0f);
                ResetElementAnimationProperties(nextIndicator, 1.0f);

                Point point = new(0, 0);
                float prevPos, nextPos;

                Point prevPosPoint = prevIndicator.TransformToVisual(this).TransformPoint(point);
                Point nextPosPoint = nextIndicator.TransformToVisual(this).TransformPoint(point);
                Size prevSize = prevIndicator.RenderSize;
                Size nextSize = nextIndicator.RenderSize;

                prevPos = (float)prevPosPoint.Y;
                nextPos = (float)nextPosPoint.Y;

                Visual visual = ElementCompositionPreview.GetElementVisual(this);
                CompositionScopedBatch scopedBatch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                float outgoingEndPosition = nextPos - prevPos;
                float incomingStartPosition = prevPos - nextPos;

                PlayIndicatorAnimations(prevIndicator,
                    0,
                    outgoingEndPosition,
                    prevSize,
                    nextSize,
                    true);
                PlayIndicatorAnimations(nextIndicator,
                    incomingStartPosition,
                    0,
                    prevSize,
                    nextSize,
                    false);

                scopedBatch.End();
                m_prevIndicator = prevIndicator;
                m_nextIndicator = nextIndicator;

                scopedBatch.Completed += (_, _) =>
                {
                    OnAnimationComplete();
                };
            }
            else if (prevIndicator != nextIndicator)
            {
                ResetElementAnimationProperties(prevIndicator, 0.0f);
                ResetElementAnimationProperties(nextIndicator, 1.0f);
            }
            m_activeIndicator = nextIndicator;
        }
    }

    static readonly Vector2 c_frame1point1 = new(0.9f, 0.1f);
    static readonly Vector2 c_frame1point2 = new(1.0f, 0.2f);
    static readonly Vector2 c_frame2point1 = new(0.1f, 0.9f);
    static readonly Vector2 c_frame2point2 = new(0.2f, 1.0f);

    private void PlayIndicatorAnimations(UIElement indicator, float from, float to, Size beginSize, Size endSize, bool isOutgoing)
    {
        Visual visual = ElementCompositionPreview.GetElementVisual(indicator);
        Compositor compositor = visual.Compositor;

        Size size = indicator.RenderSize;
        float dimension = (float)size.Height;

        float beginScale = 1.0f;
        float endScale = 1.0f;

        StepEasingFunction singleStep = compositor.CreateStepEasingFunction();
        singleStep.IsFinalStepSingleFrame = true;

        if (isOutgoing)
        {
            ScalarKeyFrameAnimation opacityAnim = compositor.CreateScalarKeyFrameAnimation();
            opacityAnim.InsertKeyFrame(0.0f, 1.0f);
            opacityAnim.InsertKeyFrame(0.333f, 1.0f, singleStep);
            opacityAnim.InsertKeyFrame(1.0f, 0.0f, compositor.CreateCubicBezierEasingFunction(c_frame2point1, c_frame2point2));
            opacityAnim.Duration = TimeSpan.FromMilliseconds(600);

            visual.StartAnimation("Opacity", opacityAnim);
        }

        ScalarKeyFrameAnimation posAnim = compositor.CreateScalarKeyFrameAnimation();
        posAnim.InsertKeyFrame(0.0f, from < to ? from : (from + dimension * (beginScale - 1)));
        posAnim.InsertKeyFrame(0.333f, from < to ? to + dimension * (endScale - 1) : to, singleStep);
        posAnim.Duration = TimeSpan.FromMilliseconds(600);

        ScalarKeyFrameAnimation scaleAnim = compositor.CreateScalarKeyFrameAnimation();
        scaleAnim.InsertKeyFrame(0.0f, beginScale);
        scaleAnim.InsertKeyFrame(0.333f, float.Abs(to - from) / dimension + (from < to ? endScale : beginScale), compositor.CreateCubicBezierEasingFunction(c_frame1point1, c_frame1point2));
        scaleAnim.InsertKeyFrame(1.0f, endScale, compositor.CreateCubicBezierEasingFunction(c_frame2point1, c_frame2point2));
        scaleAnim.Duration = TimeSpan.FromMilliseconds(600);

        ScalarKeyFrameAnimation centerAnim = compositor.CreateScalarKeyFrameAnimation();
        centerAnim.InsertKeyFrame(0.0f, from < to ? 0.0f : dimension);
        centerAnim.InsertKeyFrame(1.0f, from < to ? dimension : 0.0f, singleStep);
        centerAnim.Duration = TimeSpan.FromMilliseconds(200);

        visual.StartAnimation("Offset.Y", posAnim);
        visual.StartAnimation("Scale.Y", scaleAnim);
        visual.StartAnimation("CenterPoint.Y", centerAnim);
    }

    void OnAnimationComplete()
    {
        ResetElementAnimationProperties(m_prevIndicator, 0.0f);
        m_prevIndicator = null;

        ResetElementAnimationProperties(m_nextIndicator, 1.0f);
        m_nextIndicator = null;
    }

    void ResetElementAnimationProperties(UIElement? element, float desiredOpacity)
    {
        if (element is null)
            return;

        element.Opacity = desiredOpacity;
        Visual visual = ElementCompositionPreview.GetElementVisual(element);
        if (visual is not null)
        {
            visual.Offset = new Vector3(0, 0, 0);
            visual.Scale = new Vector3(1, 1, 1);
            visual.Opacity = desiredOpacity;
        }
    }
}