using MicaForEveryone.App.ViewModels;
using MicaForEveryone.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;
using WinUIEx;
using static MicaForEveryone.PInvoke.Messaging;
using static MicaForEveryone.PInvoke.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaForEveryone.App.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public unsafe sealed partial class SettingsWindow : WindowEx
{
    private static delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> oldWndProc;
    private SettingsViewModel ViewModel { get; }

    public SettingsWindow()
    {
        InitializeComponent();

        ViewModel = App.Services.GetRequiredService<SettingsViewModel>();
        ExtendsContentIntoTitleBar = true;

        oldWndProc = (delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT>)SetWindowLongPtrW(new HWND((void*)WindowNative.GetWindowHandle(this)), WindowLongIndex.GWL_WNDPROC, (nint)(delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT>)&WindowProc);
    }

    [UnmanagedCallersOnly]
    private static LRESULT WindowProc(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam)
    {
        if (Msg == WM_DESTROY)
        {
            LRESULT result = CallWindowProcW(oldWndProc, hWnd, Msg, wParam, lParam);
            MSG msg;
            while (PeekMessageW(&msg, default, 0, 0, 1))
            {
                if (msg.message != 18)
                {
                    TranslateMessage(&msg);
                    DispatchMessageW(&msg);
                    continue;
                }
                break;
            }
            return result;
        }
        return CallWindowProcW(oldWndProc, hWnd, Msg, wParam, lParam);
    }

    private void Window_Activated(object _, WindowActivatedEventArgs args)
    {
        // TODO: Add code to deal with title bar color change.
    }

    public static string GetIconForRule(Rule rule)
    {
        if (rule is GlobalRule)
            return "\uED35";
        if (rule is ProcessRule)
            return "\uECAA";
        if (rule is ClassRule)
            return "\uE737";
        throw new ArgumentException("Invalid rule type.", nameof(rule));
    }
}

public class SettingsNavigationItem
{
    public string? Uid { get; set; }

    public string? Tag { get; set; }

    public IconElement? Icon { get; set; }
}