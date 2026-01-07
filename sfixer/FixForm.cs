using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using System.Diagnostics;

namespace sfixer {
    public class FixForm : Form {
        // --- Windows API Constants ---
        private const int HOTKEY_ID = 1;
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_SPACE = 0x20;

        // --- DLL Imports ---
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // --- UI & State Members ---
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private System.Windows.Forms.Timer focusTimer;
        private bool isHotkeyRegistered = false;

        public GeminiAgent AI;
        public MainForm main;

        public FixForm() {
            // Invisible Form Setup
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            FormBorderStyle = FormBorderStyle.None;
            Opacity = 0;

            // Tray Menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open Main", null, (s, e) => OpenMainWindow());
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            trayIcon = new NotifyIcon {
                Text = "SFixer (Ctrl+Space)",
                Icon = GetTrayIcon(),
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.Click += TrayIcon_Click;

            // Smart Focus Timer: Checks active app every 500ms
            focusTimer = new System.Windows.Forms.Timer();
            focusTimer.Interval = 500;
            focusTimer.Tick += FocusTimer_Tick;
            focusTimer.Start();

            Load += (_, _) => Visible = false;
            FormClosed += (_, _) => {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            };
        }

        private void FocusTimer_Tick(object sender, EventArgs e) {
            string appInfo = GetActiveAppName().ToLower();

            // Check if we are in an IDE where Ctrl+Space is needed
            bool isCodeApp = appInfo.Contains("devenv") || // Visual Studio
                             appInfo.Contains("code") ||   // VS Code
                             appInfo.Contains("visual studio");

            if (isCodeApp && isHotkeyRegistered) {
                UnregisterHotKey(Handle, HOTKEY_ID);
                isHotkeyRegistered = false;
                Debug.WriteLine("IDE Detected: Hotkey released.");
            } else if (!isCodeApp && !isHotkeyRegistered) {
                // Re-register if we move away from the IDE
                if (RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL, VK_SPACE)) {
                    isHotkeyRegistered = true;
                    Debug.WriteLine("IDE Lost Focus: Hotkey re-registered.");
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);
            if (RegisterHotKey(Handle, HOTKEY_ID, MOD_CONTROL, VK_SPACE)) {
                isHotkeyRegistered = true;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e) {
            UnregisterHotKey(Handle, HOTKEY_ID);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID) {
                _ = HandleHotkeyAsync();
            }
            base.WndProc(ref m);
        }

        private async Task HandleHotkeyAsync() {
            try {
                // Get source app for debugging/logging
                string sourceApp = GetActiveAppName();
                Debug.WriteLine($"[SFixer] Processing text from: {sourceApp}");

                var toFix = await ReadSelection(true);
                if (string.IsNullOrWhiteSpace(toFix)) return;

                string modified = await ModifyAsync(toFix);

                // Verify user hasn't changed selection during AI call
                if (toFix == await ReadSelection()) {
                    await InvokeAsync(() => {
                        Clipboard.SetText(modified);
                    });
                    await Task.Delay(50); // Small buffer for clipboard stability
                    SendKeys.SendWait("^v");
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}", "Hotkey Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string?> ReadSelection(bool mustChange = false) {
            string initialText = string.Empty;
            if (mustChange) {
                await InvokeAsync(() => Clipboard.Clear());
                initialText = await InvokeAsync(() =>
                    Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty);
            }

            SendKeys.SendWait("^c");
            await Task.Delay(150); // Increased delay for slower apps

            string newText = await InvokeAsync(() =>
                Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty);

            if (mustChange && newText.Trim() == initialText.Trim()) return string.Empty;
            return newText;
        }

        private string GetActiveAppName() {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return "Unknown";

            var titleBuilder = new StringBuilder(256);
            GetWindowText(hwnd, titleBuilder, 256);

            GetWindowThreadProcessId(hwnd, out uint processId);
            try {
                using (var proc = Process.GetProcessById((int)processId)) {
                    return $"{proc.ProcessName} ({titleBuilder})";
                }
            } catch { return titleBuilder.ToString(); }
        }

        private async Task<string> ModifyAsync(string input) {
            string prompt = $"return fixed english, no explaintaion, no formatting, no extra wording. \n---start---\n{input}\n---end---";
            System.Media.SystemSounds.Exclamation.Play(); // Start sound

            string result = await AI.Ask(prompt);

            System.Media.SystemSounds.Asterisk.Play(); // Finish sound
            return result;
        }

        private void OpenMainWindow() {
            main.Show();
            main.BringToFront();
            main.Activate();
        }

        // --- Utility Methods ---
        private void TrayIcon_Click(object sender, EventArgs e) {
            if (e is MouseEventArgs mouseArgs) {
                // Check if the button clicked was the Left button
                if (mouseArgs.Button == MouseButtons.Left) {
                    OpenMainWindow();
                }
            }
        }

        private Icon GetTrayIcon() {
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Blue);
            using var font = new Font("Arial", 10, FontStyle.Bold);
            g.DrawString("S", font, Brushes.White, -1, -1);
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Task<T> InvokeAsync<T>(Func<T> func) {
            return InvokeRequired ? (Task<T>)Invoke(new Func<Task<T>>(() => Task.FromResult(func()))) : Task.FromResult(func());
        }

        private Task InvokeAsync(Action action) {
            if (InvokeRequired) { Invoke(action); return Task.CompletedTask; }
            action(); return Task.CompletedTask;
        }
    }
}