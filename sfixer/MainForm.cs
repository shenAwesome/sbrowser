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

namespace sfixer {
    public partial class MainForm : MaterialForm {

        public GeminiAgent AI;

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
                //chatPanel.Text = "";


                if (userInput == "exit" || userInput == "close") {
                    Application.Exit();
                    return;
                }

                if (userInput == "clear") {
                    chatPanel.Text = "";
                    return;
                }
                chatPanel.SelectionIndent = 10;

                chatPanel.SelectionStart = chatPanel.TextLength;
                chatPanel.SelectionLength = 0;
                chatPanel.SelectionFont = new Font(chatPanel.Font, FontStyle.Bold);
                chatPanel.AppendText(userInput + Environment.NewLine + Environment.NewLine);

                //chatPanel.AddItem(userInput + Environment.NewLine + Environment.NewLine);

                // Reset font to regular for AI response
                chatPanel.SelectionFont = new Font(chatPanel.Font, FontStyle.Regular);
                string aiResponse = await AI.Ask(userInput);
                chatPanel.AppendText(aiResponse + Environment.NewLine + Environment.NewLine);
                chatPanel.SelectionStart = chatPanel.Text.Length;
                chatPanel.ScrollToCaret();

                //chatPanel.AddItem(aiResponse); 
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                // 1. Cancel the actual closing operation. 
                // This stops the form from being disposed.
                e.Cancel = true;

                // 2. Hide the form.
                this.Hide();
            }
        }

        private void inputBox_Click(object sender, EventArgs e) {

        }
    }
}
