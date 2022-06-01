using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SystemMute
{

    public class GlobleKeyboardShotcut
    {
        public delegate void MuteShutcutPressedEventHandler();
        public event MuteShutcutPressedEventHandler MuteShutcutPressedEvent;
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        public void OnMuteShutcutPressedEvent()
        {
            MuteShutcutPressedEvent?.Invoke();
        }
        internal void UnRegister()
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }

        //CAPS LOCK:
        private const uint VK_CAPITAL = 0x14;
        private const uint M_Key = 0x4D;

        private IntPtr _windowHandle;
        private HwndSource _source;
        public void OnSourceInitialized(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, M_Key); //CTRL + CAPS_LOCK
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == M_Key)
                            {
                                OnMuteShutcutPressedEvent();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
