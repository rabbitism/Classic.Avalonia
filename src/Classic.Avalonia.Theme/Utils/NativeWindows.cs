using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;

namespace Classic.Avalonia.Theme.Utils;

public class NativeWindows
{
    public static readonly AttachedProperty<bool> RemoveCornerProperty =
        AvaloniaProperty.RegisterAttached<NativeWindows, Window, bool>("RemoveCorner");

    static NativeWindows()
    {
        RemoveCornerProperty.Changed.AddClassHandler<Window>((window, e) => OnCornerRemove(window, e));
    }

    [DllImport("dwmapi.dll")]
    private static extern unsafe int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, void* pvAttribute,
        int cbAttribute);

    public static void SetRemoveCorner(Window obj, bool value)
    {
        obj.SetValue(RemoveCornerProperty, value);
    }

    public static bool GetRemoveCorner(Window obj)
    {
        return obj.GetValue(RemoveCornerProperty);
    }

    private static void OnCornerRemove(Window window, AvaloniaPropertyChangedEventArgs avaloniaPropertyChangedEventArgs)
    {
        window.GetObservable(Window.IsExtendedIntoWindowDecorationsProperty).Subscribe(new AnonymousObserver<bool>(a =>
        {
            if (a)
                if (window.TryGetPlatformHandle() is { } handle)
                    unsafe
                    {
                        var cornerPreference = (int)DwmWindowCornerPreference.DWMWCP_DONOTROUND;
                        DwmSetWindowAttribute(handle.Handle, (int)DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE,
                            &cornerPreference, sizeof(int));
                    }
        }));
    }

    private enum DwmWindowCornerPreference : uint
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND,
        DWMWCP_ROUND,
        DWMWCP_ROUNDSMALL
    }

    private enum DwmWindowAttribute : uint
    {
        DWMWA_NCRENDERING_ENABLED = 1,
        DWMWA_NCRENDERING_POLICY,
        DWMWA_TRANSITIONS_FORCEDISABLED,
        DWMWA_ALLOW_NCPAINT,
        DWMWA_CAPTION_BUTTON_BOUNDS,
        DWMWA_NONCLIENT_RTL_LAYOUT,
        DWMWA_FORCE_ICONIC_REPRESENTATION,
        DWMWA_FLIP3D_POLICY,
        DWMWA_EXTENDED_FRAME_BOUNDS,
        DWMWA_HAS_ICONIC_BITMAP,
        DWMWA_DISALLOW_PEEK,
        DWMWA_EXCLUDED_FROM_PEEK,
        DWMWA_CLOAK,
        DWMWA_CLOAKED,
        DWMWA_FREEZE_REPRESENTATION,
        DWMWA_PASSIVE_UPDATE_MODE,
        DWMWA_USE_HOSTBACKDROPBRUSH,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_BORDER_COLOR,
        DWMWA_CAPTION_COLOR,
        DWMWA_TEXT_COLOR,
        DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
        DWMWA_LAST
    }
}