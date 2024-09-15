using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

using WinRT;
using System.Runtime.CompilerServices;

using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;

using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.SW;
using static TerraFX.Interop.Windows.SWP;
using static TerraFX.Interop.Windows.Windows;

namespace XamlIslandsNet9
{
    public partial class App : Application
    {
        private HWND _hwnd = default;
        private HWND _xamlHwnd = default;
        private HWND _coreHwnd = default;

        private bool _xamlInitialized = false;

        private DesktopWindowXamlSource _desktopWindowXamlSource = null;
        private WindowsXamlManager _xamlManager = null;
        private CoreWindow _coreWindow = null;

        private ComPtr<IDesktopWindowXamlSourceNative2> _nativeSource = default;

        internal Frame Frame = null;

        public App(HWND hwnd)
        {
            _hwnd = hwnd;
            InitializeXaml();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InternalLoadLibrary(string lib)
        {
            fixed (char* libName = lib)
                LoadLibraryW(libName);
        }

        private unsafe void InitializeXaml()
        {
            // Is this needed anymore? maybe for older builds?
            InternalLoadLibrary("twinapi.appcore.dll");
            InternalLoadLibrary("threadpoolwinrt.dll");

            _xamlManager = WindowsXamlManager.InitializeForCurrentThread();
            _desktopWindowXamlSource = new();

            ThrowIfFailed(((IUnknown*)((IWinRTObject)_desktopWindowXamlSource).NativeObject.ThisPtr)->QueryInterface(__uuidof<IDesktopWindowXamlSourceNative2>(), (void**)_nativeSource.GetAddressOf()));

            _nativeSource.Get()->AttachToWindow(_hwnd);
            _nativeSource.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _xamlHwnd));

            RECT wRect;
            GetClientRect(_hwnd, &wRect);
            SetWindowPos(_xamlHwnd, HWND.NULL, 0, 0, wRect.right - wRect.left, wRect.bottom - wRect.top, SWP_SHOWWINDOW | SWP_NOACTIVATE | SWP_NOZORDER);

            _coreWindow = CoreWindow.GetForCurrentThread();

            using ComPtr<ICoreWindowInterop> interop = default;
            ThrowIfFailed(((IUnknown*)((IWinRTObject)_coreWindow).NativeObject.ThisPtr)->QueryInterface(__uuidof<ICoreWindowInterop>(), (void**)interop.GetAddressOf()));
            interop.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _coreHwnd));

            Frame = new Frame();
            _desktopWindowXamlSource.Content = Frame;

            _xamlInitialized = true;
            OnXamlInitialized();
        }

        private void OnXamlInitialized()
        {
            Frame.Navigate(typeof(MainPage));
        }

        internal void OnResize(int x, int y)
        {
            if (_xamlHwnd != default)
                SetWindowPos(_xamlHwnd, HWND.NULL, 0, 0, x, y, SWP_SHOWWINDOW | SWP_NOACTIVATE | SWP_NOZORDER);

            if (_coreHwnd != default)
                SendMessageW(_coreHwnd, WM_SIZE, (WPARAM)x, y);
        }

        internal void ProcessCoreWindowMessage(uint message, WPARAM wParam, LPARAM lParam)
        {
            if (_coreHwnd != default)
                SendMessageW(_coreHwnd, message, wParam, lParam);
        }

        internal void OnSetFocus()
        {
            if (_xamlHwnd != default)
                SetFocus(_xamlHwnd);
        }

        internal unsafe bool PreTranslateMessage(MSG* msg)
        {
            BOOL result = false;

            if (_xamlInitialized)
                _nativeSource.Get()->PreTranslateMessage(msg, &result);

            return result;
        }
    }
}
