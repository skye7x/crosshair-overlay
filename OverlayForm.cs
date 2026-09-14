using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrosshairOverlay
{
    public class OverlayForm : Form
    {
        // ---- Crosshair appearance (tweak to taste) ----
        private const int ArmLength = 7;    // length of each arm, in pixels
        private const int GapSize = 0;      // empty gap around the center (0 = solid plus)
        private const int Thickness = 1;    // line thickness
        private const int DotRadius = 0;    // center dot radius (0 = no dot, just the plus)

        // ---- Win32 interop ----
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_TOGGLE = 1;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_H = 0x48;

        private NotifyIcon trayIcon;
        private bool crosshairVisible = true;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen.Bounds; // full primary monitor
            BackColor = Color.Magenta;            // arbitrary "key" color
            TransparencyKey = Color.Magenta;       // becomes fully transparent
            DoubleBuffered = true;

            SetupTrayIcon();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // Layered + transparent => click-through. ToolWindow => no taskbar/alt-tab entry.
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Exclude this window from screen capture / streaming.
            // Requires Windows 10 version 2004 (build 19041) or later.
            bool ok = SetWindowDisplayAffinity(Handle, WDA_EXCLUDEFROMCAPTURE);
            if (!ok)
            {
                // Falls back gracefully: window still works, just not capture-excluded.
                // (Older Windows builds will fail this call.)
            }

            RegisterHotKey(Handle, HOTKEY_ID_TOGGLE, MOD_CONTROL | MOD_ALT, VK_H);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID_TOGGLE)
            {
                crosshairVisible = !crosshairVisible;
                Invalidate();
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!crosshairVisible) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            using (var pen = new Pen(Color.White, Thickness))
            {
                e.Graphics.DrawLine(pen, cx - GapSize - ArmLength, cy, cx - GapSize, cy); // left
                e.Graphics.DrawLine(pen, cx + GapSize, cy, cx + GapSize + ArmLength, cy); // right
                e.Graphics.DrawLine(pen, cx, cy - GapSize - ArmLength, cx, cy - GapSize); // top
                e.Graphics.DrawLine(pen, cx, cy + GapSize, cx, cy + GapSize + ArmLength); // bottom
            }

            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillEllipse(brush, cx - DotRadius, cy - DotRadius, DotRadius * 2, DotRadius * 2);
            }
        }

        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Crosshair Overlay (Ctrl+Alt+H to toggle)"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Toggle Crosshair (Ctrl+Alt+H)", null, (s, e) =>
            {
                crosshairVisible = !crosshairVisible;
                Invalidate();
            });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            });
            trayIcon.ContextMenuStrip = menu;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterHotKey(Handle, HOTKEY_ID_TOGGLE);
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}