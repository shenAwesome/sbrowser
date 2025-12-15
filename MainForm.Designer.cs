namespace sbrowser {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            chatPanel = new ReaLTaiizor.Controls.MaterialRichTextBox();
            inputBox = new ReaLTaiizor.Controls.MaterialTextBoxEdit();
            SuspendLayout();
            // 
            // chatPanel
            // 
            chatPanel.BackColor = Color.FromArgb(255, 255, 255);
            chatPanel.BorderStyle = BorderStyle.None;
            chatPanel.Depth = 0;
            chatPanel.Dock = DockStyle.Fill;
            chatPanel.Font = new Font("Microsoft Sans Serif", 12F);
            chatPanel.ForeColor = Color.FromArgb(222, 0, 0, 0);
            chatPanel.Hint = "";
            chatPanel.Location = new Point(10, 75);
            chatPanel.Margin = new Padding(10);
            chatPanel.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.HOVER;
            chatPanel.Name = "chatPanel";
            chatPanel.ReadOnly = true;
            chatPanel.Size = new Size(780, 317);
            chatPanel.TabIndex = 3;
            chatPanel.Text = "";
            // 
            // inputBox
            // 
            inputBox.AnimateReadOnly = false;
            inputBox.AutoCompleteMode = AutoCompleteMode.None;
            inputBox.AutoCompleteSource = AutoCompleteSource.None;
            inputBox.BackgroundImageLayout = ImageLayout.None;
            inputBox.CharacterCasing = CharacterCasing.Normal;
            inputBox.Depth = 0;
            inputBox.Dock = DockStyle.Bottom;
            inputBox.Font = new Font("Microsoft Sans Serif", 12F);
            inputBox.HideSelection = true;
            inputBox.LeadingIcon = null;
            inputBox.Location = new Point(10, 392);
            inputBox.MaxLength = 32767;
            inputBox.MouseState = ReaLTaiizor.Helper.MaterialDrawHelper.MaterialMouseState.OUT;
            inputBox.Name = "inputBox";
            inputBox.PasswordChar = '\0';
            inputBox.PrefixSuffixText = null;
            inputBox.ReadOnly = false;
            inputBox.RightToLeft = RightToLeft.No;
            inputBox.SelectedText = "";
            inputBox.SelectionLength = 0;
            inputBox.SelectionStart = 0;
            inputBox.ShortcutsEnabled = true;
            inputBox.Size = new Size(780, 48);
            inputBox.TabIndex = 4;
            inputBox.TabStop = false;
            inputBox.TextAlign = HorizontalAlignment.Left;
            inputBox.TrailingIcon = null;
            inputBox.UseAccent = false;
            inputBox.UseSystemPasswordChar = false;
            inputBox.KeyDown += inputBox_KeyDown;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(chatPanel);
            Controls.Add(inputBox);
            DrawerAutoShow = true;
            Name = "MainForm";
            Padding = new Padding(10, 75, 10, 10);
            Text = "Chat with AI";
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ReaLTaiizor.Controls.MaterialRichTextBox chatPanel;
        private ReaLTaiizor.Controls.MaterialTextBoxEdit inputBox;
    }
}