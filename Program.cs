namespace sbrowser {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ApplicationConfiguration.Initialize();

            var webviewForm = new GeminiAgent();

            webviewForm.IsDebug = false;

            if (!webviewForm.IsDebug) {
                webviewForm.Opacity = 0;
                webviewForm.ShowInTaskbar = false;
            }
            webviewForm.Show();
            if (!webviewForm.IsDebug) {
                webviewForm.Location = new Point(-2000, -2000);
            }
            //webviewForm.Visible = false; 
            var mainForm = new MainForm {
                AI = webviewForm,
                ShowInTaskbar = false
            };

            var fixForm = new FixForm();
            fixForm.main = mainForm;
            fixForm.AI = webviewForm;

            Application.Run(fixForm);
        }
    }
}