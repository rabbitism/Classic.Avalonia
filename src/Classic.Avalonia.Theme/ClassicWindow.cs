using System;
using System.Runtime.InteropServices;
using Classic.CommonControls.Utils;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Classic.Avalonia.Theme;

public class ClassicWindow : Window
{
    protected override Type StyleKeyOverride => typeof(ClassicWindow);

    private static class NativeMacOs
    {
        private const string ObjCLibrary = "/usr/lib/libobjc.dylib";
        [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string className);

        [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string selector);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, ulong arg);

        public static void EnableResizable(IntPtr windowPtr)
        {
            // Get styleMask
            IntPtr styleMaskSel = sel_registerName("styleMask");
            IntPtr styleMask = objc_msgSend(windowPtr, styleMaskSel);

            // Assuming NSResizableWindowMask is the bit mask for resizable windows
            const ulong nsWindowStyleMaskResizable = 1 << 3; // Correct mask for resizable windows
            ulong currentMask = (ulong)styleMask.ToInt64();

            // Set styleMask to make window resizable
            currentMask |= nsWindowStyleMaskResizable;

            // Set new styleMask
            IntPtr setStyleMaskSel = sel_registerName("setStyleMask:");
            objc_msgSend_void(windowPtr, setStyleMaskSel, currentMask);
        }
    }
    

    public ClassicWindow()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Activated += OnActivated;
        }
    }

    private void OnActivated(object sender, EventArgs e)
    {
        // Fix for macOS: enable resizing despite SystemDecorations = None
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && CanResize)
        {
            DispatcherTimer.RunOnce(() =>
            {
                if (CanResize && TryGetPlatformHandle() is { } handle)
                {
                    NativeMacOs.EnableResizable(handle.Handle);
                }
            }, TimeSpan.FromMilliseconds(1));
        }
    }
}

internal class ClassicWindowFactory : IWindowFactory
{
    public Window Create() => new ClassicWindow();
}