using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace sfixer {
    public class GeminiAgent : Form {
        private readonly WebView2 webView;
        private WebViewBridge bridge;
        public bool IsDebug = false;
        /// <summary>
        /// Initializes a new instance of the WebViewForm class.
        /// </summary>
        public GeminiAgent() {
            // Set up the form properties
            this.Text = "WebView2 Host Form";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. Initialize the WebView2 control 

            webView = new WebView2();

            // Make the WebView2 fill the entire form
            webView.Dock = DockStyle.Fill;

            // Add the control to the form
            this.Controls.Add(webView);

            // 2. Start the initialization process when the form loads
            this.Load += WebViewForm_Load;
        }

        /// <summary>
        /// Event handler for when the form loads. It ensures the WebView2 environment is ready.
        /// </summary>
        private async void WebViewForm_Load(object sender, EventArgs e) {
            try {

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string customUserDataFolder = Path.Combine(appDataPath, "aslib", "sfixer", "WebView2Data");
                Directory.CreateDirectory(customUserDataFolder);


                // Ensure the CoreWebView2 environment is initialized
                // This prepares the underlying browser process. 
                await webView.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync(
                      userDataFolder: customUserDataFolder,
                      options: new CoreWebView2EnvironmentOptions {
                          AdditionalBrowserArguments = "--no-proxy-server"
                      }
                ));
                bridge = new WebViewBridge(webView);
                bridge.AddOnLoad(async () => {
                    Console.WriteLine("Page loaded");
                    string jsPath = Path.Combine(AppContext.BaseDirectory, "bots/gemini.js");
                    string botJs = File.ReadAllText(jsPath);
                    await webView.CoreWebView2.ExecuteScriptAsync(botJs);
                    AskImplementation = async (string question) => {
                        try {
                            var ret = await bridge.CallJsAsync<string>("askAI", new { question });
                            return ret;
                        } catch (Exception e) {
                            webView.Refresh();
                            return "something wrong, rebooting...";
                        }
                    };
                    //MessageBox.Show(ret, "JS Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });


                await bridge.InitializeAsync();
                var url = "https://gemini.google.com/app?hl=en-AU";
                webView.Source = new Uri(url);

                if (IsDebug) webView.CoreWebView2.OpenDevToolsWindow();

            } catch (Exception ex) {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //await Task.Delay(15000);
            //var ret = await this.Ask("fix english: this is an box");
            //MessageBox.Show(ret);
        }

        public Func<string, Task<string>> AskImplementation = async (q) => {
            // Default implementation returns a completed Task<string>
            return await Task.FromResult("Loading...Ask again :)");
        };

        /// <summary>
        /// FIX: The return type is changed from 'string' to 'Task<string>'.
        /// </summary>
        public async Task<string> Ask(string question) {
            Task<string> taskResult = AskImplementation(question);
            string answer = await taskResult;
            return answer;
        }

        public void Reresh() {
            this.webView.Reload();
        }

        /// <summary>
        /// Optional: Public property to allow external code to interact with the WebView2 control.
        /// </summary>
        public WebView2 WebViewControl => webView;
    }
}
