using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Fenceless.Util
{
    public class GlobalHotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private readonly MessageWindow messageWindow;
        private readonly Dictionary<int, Action> hotkeyActions = new Dictionary<int, Action>();
        private int nextHotkeyId = 1;
        private bool disposed = false;

        public GlobalHotkeyManager()
        {
            messageWindow = new MessageWindow(this);
        }

        public int RegisterHotkey(Keys key, bool ctrl = false, bool alt = false, bool shift = false, bool win = false, Action action = null)
        {
            uint modifiers = 0;
            if (ctrl) modifiers |= MOD_CONTROL;
            if (alt) modifiers |= MOD_ALT;
            if (shift) modifiers |= MOD_SHIFT;
            if (win) modifiers |= MOD_WIN;

            int id = nextHotkeyId++;
            if (RegisterHotKey(messageWindow.Handle, id, modifiers, (uint)key))
            {
                if (action != null)
                {
                    hotkeyActions[id] = action;
                }
                return id;
            }
            return -1;
        }

        public void UnregisterHotkey(int id)
        {
            UnregisterHotKey(messageWindow.Handle, id);
            hotkeyActions.Remove(id);
        }

        private void OnHotkeyPressed(int id)
        {
            if (hotkeyActions.TryGetValue(id, out Action action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing hotkey action: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var id in hotkeyActions.Keys)
                {
                    UnregisterHotKey(messageWindow.Handle, id);
                }
                hotkeyActions.Clear();
                messageWindow?.DestroyHandle();
                disposed = true;
            }
        }

        private class MessageWindow : NativeWindow
        {
            private readonly GlobalHotkeyManager manager;

            public MessageWindow(GlobalHotkeyManager manager)
            {
                this.manager = manager;
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    int id = m.WParam.ToInt32();
                    manager.OnHotkeyPressed(id);
                }
                base.WndProc(ref m);
            }
        }
    }
}