using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;
using EventHandler = System.EventHandler;

namespace HASCore
{
    partial class TextToSpeechForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer Components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (Components != null))
            {
                Components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(TextToSpeechForm));
            this.Components = new Container();
            this.tbText = new TextBox();
            this.label1 = new Label();
            this.btnClose = new Button();
            this.tbKeys = new TextBox();
            this.label2 = new Label();
            this.label3 = new Label();
            this.tbWhereSave = new TextBox();
            this.btnBrowseFolderLoc = new Button();
            this.btnCreateWAV = new Button();
            this.btnCreateWAVAdd = new Button();
            this.SuspendLayout();
            // 
            // tbText
            // 
            this.tbText.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.tbText.Location = new Point(14, 25);
            this.tbText.Name = "tbText";
            this.tbText.Size = new Size(384, 20);
            this.tbText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(193, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(28, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Text";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = AnchorStyles.Bottom;
            this.btnClose.Location = new Point(170, 201);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.CloseButton_Click);
            // 
            // tbKeys
            // 
            this.tbKeys.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.tbKeys.Location = new Point(14, 163);
            this.tbKeys.Name = "tbKeys";
            this.tbKeys.ReadOnly = true;
            this.tbKeys.Size = new Size(235, 20);
            this.tbKeys.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new Point(122, 147);
            this.label2.Name = "label2";
            this.label2.Size = new Size(30, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Keys";
            // 
            // label3
            // 
            this.label3.Anchor = AnchorStyles.Top;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(131, 58);
            this.label3.Name = "label3";
            this.label3.Size = new Size(143, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Where to save perm TTS file";
            // 
            // tbWhereSave
            // 
            this.tbWhereSave.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.tbWhereSave.Location = new Point(14, 74);
            this.tbWhereSave.Name = "tbWhereSave";
            this.tbWhereSave.Size = new Size(353, 20);
            this.tbWhereSave.TabIndex = 1;
            // 
            // btnBrowseFolderLoc
            // 
            this.btnBrowseFolderLoc.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnBrowseFolderLoc.Location = new Point(373, 72);
            this.btnBrowseFolderLoc.Name = "btnBrowseFolderLoc";
            this.btnBrowseFolderLoc.Size = new Size(25, 23);
            this.btnBrowseFolderLoc.TabIndex = 2;
            this.btnBrowseFolderLoc.Text = "...";
            this.btnBrowseFolderLoc.UseVisualStyleBackColor = true;
            this.btnBrowseFolderLoc.Click += new EventHandler(this.BrowseFolderLocationButton_Click);
            // 
            // btnCreateWAV
            // 
            this.btnCreateWAV.Anchor = AnchorStyles.Top;
            this.btnCreateWAV.Location = new Point(162, 108);
            this.btnCreateWAV.Name = "btnCreateWAV";
            this.btnCreateWAV.Size = new Size(98, 23);
            this.btnCreateWAV.TabIndex = 12;
            this.btnCreateWAV.Text = "Only create WAV";
            this.btnCreateWAV.UseVisualStyleBackColor = true;
            this.btnCreateWAV.Click += new EventHandler(this.CreateWAVButton_Click);
            // 
            // btnCreateWAVAdd
            // 
            this.btnCreateWAVAdd.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnCreateWAVAdd.Location = new Point(255, 161);
            this.btnCreateWAVAdd.Name = "btnCreateWAVAdd";
            this.btnCreateWAVAdd.Size = new Size(143, 23);
            this.btnCreateWAVAdd.TabIndex = 4;
            this.btnCreateWAVAdd.Text = "Create WAV and add to list";
            this.btnCreateWAVAdd.UseVisualStyleBackColor = true;
            this.btnCreateWAVAdd.Click += new EventHandler(this.CreateWAVAddButton_Click);
            // 
            // TextToSpeechForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(414, 231);
            this.Controls.Add(this.btnCreateWAVAdd);
            this.Controls.Add(this.btnCreateWAV);
            this.Controls.Add(this.btnBrowseFolderLoc);
            this.Controls.Add(this.tbWhereSave);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbKeys);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbText);
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new Size(4000, 270);
            this.MinimumSize = new Size(422, 270);
            this.Name = "TextToSpeechForm";
            this.Text = "TTS";
            this.Load += new EventHandler(this.TTS_Load);
            this.FormClosing += TextToSpeechForm_FormClosing;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private TextBox tbText;
        private Label label1;
        private Button btnClose;
        private TextBox tbKeys;
        private Label label2;
        private Label label3;
        private TextBox tbWhereSave;
        private Button btnBrowseFolderLoc;
        private Button btnCreateWAV;
        private Button btnCreateWAVAdd;
    }
}