using System;
using System.Threading.Tasks;
using System.Windows.Forms; // ← Critical missing using
using System.Runtime.InteropServices;
using System.Drawing; // for Icon/SystemIcons

namespace sbrowser {
    public class FixForm : Form {
        private const int HOTKEY_ID = 1;
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_SPACE = 0x20;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;



        public GeminiAgent AI;
        public MainForm main;

        public FixForm() {
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            FormBorderStyle = FormBorderStyle.None;
            Opacity = 0; // Invisible but still has handle

            // Create context menu to allow exit
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            trayIcon = new NotifyIcon {
                Text = "SFixer (Ctrl+Space)",
                Icon = GetTrayIcon(),
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.Click += TrayIcon_Click;

            Load += (_, _) => Visible = false;
            FormClosed += (_, _) => {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            };
        }

        private void TrayIcon_Click(object? sender, EventArgs e) {
            main.Show();
            main.BringToFront();
            main.Activate();
        }

        private Icon GetTrayIcon() {
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            // Optionally draw a simple symbol (e.g., letter 'S')
            using var font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
            g.DrawString("S", font, Brushes.White, 0, 0);
            return Icon.FromHandle(bmp.GetHicon());
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL, VK_SPACE);
        }

        protected override void OnHandleDestroyed(EventArgs e) {
            UnregisterHotKey(Handle, HOTKEY_ID);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID) {
                _ = HandleHotkeyAsync(); // fire-and-forget
            }
            base.WndProc(ref m);
        }

        private async Task<string?> ReadSelection() {
            await InvokeAsync(() => {
                string? backup = Clipboard.ContainsText() ? Clipboard.GetText() : null;

                // Copy selection
                SendKeys.SendWait("^c");
                return backup;
            });

            // Small delay to let clipboard update
            await Task.Delay(60);

            string? selected = await InvokeAsync(() => {
                if (!Clipboard.ContainsText()) return null;
                return Clipboard.GetText();
            });
            return selected;
        }

        private async Task HandleHotkeyAsync() {
            try {
                // Ensure clipboard operations happen on the UI thread (STA)
                var toFix = await ReadSelection();

                if (string.IsNullOrWhiteSpace(toFix))
                    return;

                string modified = await ModifyAsync(toFix);

                if (toFix == await ReadSelection()) {
                    //all good paste it
                    await InvokeAsync(() => {
                        Clipboard.SetText(modified);
                    });
                    await Task.Delay(30);
                    SendKeys.SendWait("^v");
                }

            } catch (Exception ex) {
                // Log or show error if needed (avoid silent failure in production)
                MessageBox.Show($"Error: {ex.Message}", "Hotkey Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper to marshal calls back to UI thread (STA)
        private Task<T> InvokeAsync<T>(Func<T> func) {
            if (InvokeRequired) {
                return Task.FromResult((T)Invoke(func));
            }
            return Task.FromResult(func());
        }

        private Task InvokeAsync(Action action) {
            if (InvokeRequired) {
                Invoke(action);
                return Task.CompletedTask;
            }
            action();
            return Task.CompletedTask;
        }

        private async Task<string> ModifyAsync(string input) {
            string prompt = $"""
            return fixed english, no explaintaion, no formatting 
            ---start----
            {input}
            ---end---
            """;
            System.Media.SystemSounds.Exclamation.Play();

            string result = await AI.Ask(prompt);
            System.Media.SystemSounds.Exclamation.Play();
            return result;
        }
    }
}