using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace sbrowser {
    public partial class MainForm : MaterialForm {

        public WebViewForm AI;

        public MainForm() {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void MetroTextBox1_KeyPress(object? sender, KeyPressEventArgs e) {
            throw new NotImplementedException();
        }

        private async void textBox1_KeyDown(object sender, KeyEventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void metroTextBox1_Click(object sender, EventArgs e) {

        }

        private async void metroTextBox1_KeyDown(object sender, KeyEventArgs e) {

        }

        private async void inputBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                string userInput = inputBox.Text;
                inputBox.Text = string.Empty;
                e.SuppressKeyPress = true;
                e.Handled = true;

                chatPanel.Text = "";
                //metroRichTextBox1.SelectionStart = richTextBox1.TextLength;
                //metroRichTextBox1.SelectionLength = 0;
                //metroRichTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                chatPanel.AppendText(userInput + Environment.NewLine + Environment.NewLine);

                // Reset font to regular for AI response
                //richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                string aiResponse = await AI.Ask(userInput);
                chatPanel.AppendText(aiResponse + Environment.NewLine);
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {

        }
    }
}
