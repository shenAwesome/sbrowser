using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace sbrowser;

public partial class Form1 : Form {
    private WebView2 webView;
    private CartoonTextBox addressBar;
    private CartoonButton goButton;
    private Panel topPanel;
    private WebViewBridge bridge;

    public Form1() {
        InitializeComponent();
        InitializeCustomUI();
        InitializeAsync();
    }

    private void InitializeCustomUI() {
        // Main window
        Text = "Toon Browser";
        Size = new Size(1000, 700);
        BackColor = Color.WhiteSmoke;

        // WebView
        webView = new WebView2 {
            Dock = DockStyle.Fill
        };
        Controls.Add(webView);

        // Top panel
        topPanel = new Panel {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(10, 8, 10, 8),
            BackColor = Color.FromArgb(52, 152, 219)
        };
        Controls.Add(topPanel);

        // Layout container (PERFECT alignment)
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 2,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.RowStyles.Clear();
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));

        topPanel.Controls.Add(layout);

        // Address bar
        addressBar = new CartoonTextBox {
            Dock = DockStyle.Fill
        };
        addressBar.KeyDown += AddressBar_KeyDown;

        layout.Controls.Add(addressBar, 0, 0);

        // GO button
        goButton = new CartoonButton {
            Text = "GO",
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            BackColor = Color.FromArgb(241, 196, 15),
            ForeColor = Color.Black,
            Font = new Font("Comic Sans MS", 12, FontStyle.Bold)
        };
        goButton.Click += GoButton_Click;
        goButton.Margin = new Padding(2);

        layout.Controls.Add(goButton, 1, 0);
    }

    private async void InitializeAsync() {

        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: null,
            options: new CoreWebView2EnvironmentOptions {
                AdditionalBrowserArguments = "--no-proxy-server"
            }
        );
        await webView.EnsureCoreWebView2Async(env);
        bridge = new WebViewBridge(webView);
        bridge.AddOnLoad(async () => {
            Console.WriteLine("Page loaded");
            string jsPath = Path.Combine(AppContext.BaseDirectory, "bot.js");
            string botJs = File.ReadAllText(jsPath);
            await webView.CoreWebView2.ExecuteScriptAsync(botJs);

            /*

            await Task.Delay(1000 * 10);
            var ret = await bridge.CallJsAsync<string>("askAI", new {
                question = "how is your day"
            });

            MessageBox.Show(ret, "JS Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
            */
        });


        await bridge.InitializeAsync();
        webView.CoreWebView2.OpenDevToolsWindow();

        var url = "https://gemini.google.com/app?hl=en-AU";
        webView.Source = new Uri(url);
        //addressBar.Text = "https://www.google.com";

        webView.SourceChanged += (_, _) => {
            if (webView.CoreWebView2 != null)
                addressBar.Text = webView.Source.ToString();
        };
    }

    private void GoButton_Click(object? sender, EventArgs e) => Navigate();

    private void AddressBar_KeyDown(object? sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter) {
            Navigate();
            e.SuppressKeyPress = true;
        }
    }

    private void Navigate() {
        var url = addressBar.Text.Trim();

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        if (Uri.TryCreate(url, UriKind.Absolute, out _)) {
            webView.CoreWebView2?.Navigate(url);
        } else {
            MessageBox.Show("Invalid URL", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

// ------------------------------------------------------------
// Cartoon Button
// ------------------------------------------------------------
public class CartoonButton : Button {
    public CartoonButton() {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int borderSize = 1;
        var rect = ClientRectangle;
        var drawRect = new Rectangle(
            rect.X + borderSize / 2,
            rect.Y + borderSize / 2,
            rect.Width - borderSize,
            rect.Height - borderSize
        );

        using (var brush = new SolidBrush(BackColor))
            g.FillRectangle(brush, drawRect);

        using (var pen = new Pen(Color.Black, borderSize))
            g.DrawRectangle(pen, drawRect);

        TextRenderer.DrawText(
            g,
            Text,
            Font,
            ClientRectangle,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        );
    }
}

public class CartoonTextBox : UserControl {
    private TextBox inner;

    public CartoonTextBox() {
        Height = 44;
        BackColor = Color.White;

        inner = new TextBox {
            BorderStyle = BorderStyle.None,
            Font = new Font("Comic Sans MS", 14),
            Location = new Point(8, 4), // REAL vertical centering
            Width = Width - 16,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.Add(inner);
    }

    protected override void OnResize(EventArgs e) {
        base.OnResize(e);
        if (inner == null)
            return;
        inner.Width = Width - 16;
    }

    public override string Text {
        get => inner.Text;
        set => inner.Text = value;
    }

    public new event KeyEventHandler KeyDown {
        add => inner.KeyDown += value;
        remove => inner.KeyDown -= value;
    }
}
