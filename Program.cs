namespace sbrowser {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {

            ApplicationConfiguration.Initialize();

            var webviewForm = new WebViewForm {
                ShowInTaskbar = false,
                WindowState = FormWindowState.Minimized,
                Opacity = 0
            };
            webviewForm.Show();
            webviewForm.Location = new Point(-2000, -2000);
            //webviewForm.Visible = false; 
            var mainForm = new MainForm {
                AI = webviewForm
            };

            Application.Run(mainForm);
        }
    }
}