// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;
// Also, declaring aliases for the same reason.
using Timer = System.Windows.Forms.Timer;
using EventHandler = System.EventHandler;

namespace JNSoundboardCore
{
    /// <summary>
    /// <see cref="SettingsForm"/> class part for implementing the initialization 
    /// </summary>
    partial class SettingsForm
    {
        /// Description
        /// <summary>
        ///     Container for components on this form.
        /// </summary>
        private Container? Components = null;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="ToggleKeysTextBox">ToggleKeysTextBox</seealso>.
        /// </summary>
        private Label? ToggleKeysLabel;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="StopKeysTextBox">StopKeysTextBox</seealso>.
        /// </summary>
        private Label? StopKeysLabel;
        /// Description
        /// <summary>
        ///     Button for adding new XML file location of <see cref="XMLSettings"/> inside of 
        ///     <seealso cref="KeyLocationsListView"/>, along with hotkeys to change to this preset to.
        /// </summary>
        private Button? AddButton;
        /// Description
        /// <summary>
        ///     Button for editing existing <see cref="XMLSettings"/> inside of 
        ///     <seealso cref="KeyLocationsListView"/> and their hotkeys to change to this preset.
        /// </summary>
        private Button? EditButton;
        /// Description
        /// <summary>
        ///     Button for removing existing <see cref="XMLSettings"/> inside of <seealso cref="KeyLocationsListView"/>.
        /// </summary>
        private Button? RemoveButton;
        /// Description
        /// <summary>
        ///     Button for accepting the changes made inside this form and saving them to disk.
        /// </summary>
        private Button? OKButton;
        /// Description
        /// <summary>
        ///     Button for discarding the changes made inside this form.
        /// </summary>
        /// <remarks>
        ///     Using the 'new' keyword here because of conflicting with an inherited member
        ///     from the <see cref="Form"/> class named the same, so we hide the base member.
        /// </remarks> 
        private new Button? CancelButton;
        /// Description
        /// <summary>
        ///     Box for grouping the list view and their respective button controls.
        /// </summary>
        private GroupBox? GroupBox;
        /// Description
        /// <summary>
        ///     Checkbox for enabling the setting so the form minimizes to tray instead of taskbar.
        /// </summary> 
        private CheckBox? MinimizeToTrayCheckBox;
        /// Description
        /// <summary>
        ///     Textbox for inputing the combination of keys to toggle the 'Enabled' flag.
        /// </summary>
        private TextBox? ToggleKeysTextBox;
        /// Description
        /// <summary>
        ///     Textbox for inputing the combination of keys to stop all sounds from playing.
        /// </summary>
        private TextBox? StopKeysTextBox;
        /// Description
        /// <summary>
        ///     A header for displaying the name of the column 'Keys'
        ///     inside <seealso cref="KeysLocationsListView">KeysLocationsListView</seealso>. 
        /// </summary>
        internal ColumnHeader? KeysColumnHeader;
        /// Description
        /// <summary>
        ///     A header for displaying the name of the column 'XML Locations' inside
        ///     <seealso cref="KeysLocationsListView">KeysLocationsListView</seealso>. 
        /// </summary>
        internal ColumnHeader? XMLLocationsColumnHeader;
        /// Description
        /// <summary>
        ///     A list view for all added <see cref="XMLSettings"/> as presets.
        /// </summary>
        internal ListView? KeysLocationsListView;
        /// Description
        /// <summary>
        ///     A timer component used to poll for keyboard inputs inside <see cref="TextBox"/> components.
        /// </summary>
        private Timer? MainTimer;

        /// Description
        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing && (Components != null))
            {
                Components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// Description
        /// <summary>
        ///     Component initialization on program startup.
        /// </summary>
        /// <remarks>
        ///     This procedure is needed for two things:
        ///     <list type="number">
        ///         <item>
        ///             <term>Construction of a class</term>
        ///             <description>procedure is called upon constructing a class</description>
        ///         </item>
        ///         <item>
        ///             <term>Class properties initialization</term>
        ///             <description>procedure is initializing components that will be used in a form</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        private void InitializeComponent()
        {
            // ------------------------
            // Initializing new objects
            // ------------------------

            // Component resource manager
            ComponentResourceManager resources = new(typeof(SettingsForm));
            // Component container
            this.Components                 = new Container();
            // Labels
            this.ToggleKeysLabel            = new Label();
            this.StopKeysLabel              = new Label();
            // Buttons
            this.AddButton                  = new Button();
            this.EditButton                 = new Button();
            this.RemoveButton               = new Button();
            this.OKButton                   = new Button();
            this.CancelButton               = new Button();
            // Groupboxes
            this.GroupBox                   = new GroupBox();
            // Checkboxes
            this.MinimizeToTrayCheckBox     = new CheckBox();
            // Textboxes
            this.ToggleKeysTextBox          = new TextBox();
            this.StopKeysTextBox            = new TextBox();
            // Column headers
            this.KeysColumnHeader           = new ColumnHeader();
            this.XMLLocationsColumnHeader   = new ColumnHeader();
            // List views
            this.KeysLocationsListView      = new ListView();
            // Timers
            this.MainTimer                  = new Timer(this.Components);

            // Suspending layout logic before adding controls
            // for child objects to initialize without firing events
            this.SuspendLayout();
            // This has to be done on child objects as well, because
            // suspending layouts on the form itself doesn't suspend
            // the layout logic on the child components
            this.GroupBox.SuspendLayout();

            // ------------------------
            // Adding object properties
            // ------------------------

            // Labels

            // 
            // ToggleKeysLabel
            // 
            this.ToggleKeysLabel.Location = new Point(8, 56);
            this.ToggleKeysLabel.Name = "ToggleKeysLabel";
            this.ToggleKeysLabel.Size = new Size(104, 16);
            this.ToggleKeysLabel.TabIndex = 2;
            this.ToggleKeysLabel.Text = "Enable Soundboard Hotkeys";
            // 
            // StopKeysLabel
            // 
            this.StopKeysLabel.Name = "StopKeysLabel";
            this.StopKeysLabel.Location = new Point(12, 15);
            this.StopKeysLabel.Size = new Size(104, 13);
            this.StopKeysLabel.AutoSize = true;
            this.StopKeysLabel.TabIndex = 0;
            this.StopKeysLabel.Text = "Stop all sounds keys";

            // Buttons

            // 
            // AddButton
            // 
            this.AddButton.Name = "AddButton";
            this.AddButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.AddButton.Location = new Point(6, 162);
            this.AddButton.Size = new Size(75, 23);
            this.AddButton.TabIndex = 2;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new EventHandler(this.AddButton_Click);
            // 
            // EditButton
            // 
            this.EditButton.Name = "EditButton";
            this.EditButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.EditButton.Location = new Point(87, 162);
            this.EditButton.Size = new Size(75, 23);
            this.EditButton.TabIndex = 3;
            this.EditButton.Text = "Edit";
            this.EditButton.UseVisualStyleBackColor = true;
            this.EditButton.Click += new EventHandler(this.EditButton_Click);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.RemoveButton.Location = new Point(168, 162);
            this.RemoveButton.Size = new Size(75, 23);
            this.RemoveButton.TabIndex = 4;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new EventHandler(this.RemoveButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Name = "OKButton";
            this.OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.OKButton.Location = new Point(337, 361);
            this.OKButton.Size = new Size(75, 23);
            this.OKButton.TabIndex = 7;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.CancelButton.Location = new Point(424, 361);
            this.CancelButton.Size = new Size(75, 23);
            this.CancelButton.TabIndex = 8;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.DialogResult = DialogResult.Cancel;
            this.CancelButton.Click += new EventHandler(this.CancelButton_Click);
            
            // Groupboxes

            // 
            // GroupBox
            // 
            this.GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.GroupBox.Name = "GroupBox";
            this.GroupBox.Location = new Point(15, 120);
            this.GroupBox.Size = new Size(472, 191);
            this.GroupBox.TabIndex = 1;
            this.GroupBox.TabStop = false;
            this.GroupBox.Text = "Load XML file with keys";
            this.GroupBox.Controls.Add(this.KeysLocationsListView);
            this.GroupBox.Controls.Add(this.AddButton);
            this.GroupBox.Controls.Add(this.RemoveButton);
            this.GroupBox.Controls.Add(this.EditButton);

            // Checkboxes

            // 
            // MinimizeToTrayCheckBox
            // 
            this.MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
            this.MinimizeToTrayCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.MinimizeToTrayCheckBox.Location = new Point(15, 344);
            this.MinimizeToTrayCheckBox.Size = new Size(216, 17);
            this.MinimizeToTrayCheckBox.AutoSize = true;
            this.MinimizeToTrayCheckBox.TabIndex = 5;
            this.MinimizeToTrayCheckBox.Text = "Minimize button sends application to tray";
            this.MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;

            // Textboxes
            
            // 
            // ToggleKeysTextBox
            //
            this.ToggleKeysTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; 
            this.ToggleKeysTextBox.Location = new Point(120, 48);
            this.ToggleKeysTextBox.Name = "ToggleKeysTextBox";
            this.ToggleKeysTextBox.ReadOnly = true;
            this.ToggleKeysTextBox.Size = new Size(368, 24);
            this.ToggleKeysTextBox.TabIndex = 1;
            this.ToggleKeysTextBox.Enter += new EventHandler(this.ToggleKeysTextBox_Enter);
            this.ToggleKeysTextBox.Leave += new EventHandler(this.ToggleKeysTextBox_Leave);
            // 
            // StopKeysTextBox
            // 
            this.StopKeysTextBox.Name = "StopKeysTextBox";
            this.StopKeysTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.StopKeysTextBox.Location = new Point(122, 12);
            this.StopKeysTextBox.Size = new Size(365, 20);
            this.StopKeysTextBox.ReadOnly = true;
            this.StopKeysTextBox.TabIndex = 0;
            this.StopKeysTextBox.Enter += new EventHandler(this.StopKeysTextBox_Enter);
            this.StopKeysTextBox.Leave += new EventHandler(this.StopKeysTextBox_Leave);

            // Column headers

            // 
            // KeysColumnHeader
            // 
            this.KeysColumnHeader.Text = "Keys";
            this.KeysColumnHeader.Width = 150;
            // 
            // XMLLocationsColumnHeader
            // 
            this.XMLLocationsColumnHeader.Text = "XML location";
            this.XMLLocationsColumnHeader.Width = 300;

            // List views

            // 
            // KeysLocationsListView
            // 
            this.KeysLocationsListView.Name = "KeysLocationsListView";
            this.KeysLocationsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.KeysLocationsListView.Location = new Point(6, 19);
            this.KeysLocationsListView.Size = new Size(460, 137);
            this.KeysLocationsListView.Alignment = ListViewAlignment.Default;
            this.KeysLocationsListView.View = View.Details;
            this.KeysLocationsListView.FullRowSelect = true;
            this.KeysLocationsListView.GridLines = true;
            this.KeysLocationsListView.MultiSelect = false;
            this.KeysLocationsListView.TabIndex = 1;
            this.KeysLocationsListView.UseCompatibleStateImageBehavior = false;
            this.KeysLocationsListView.MouseDoubleClick += new MouseEventHandler(this.KeysLocationsListView_MouseDoubleClick);
            this.KeysLocationsListView.Columns.AddRange(
                [
                    this.KeysColumnHeader,
                    this.XMLLocationsColumnHeader
                ]
            );

            // Timers

            // 
            // MainTimer
            // 
            this.MainTimer.Tick += new EventHandler(this.MainTimer_Tick);
            
            // Form
            
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(499, 304);
            this.Icon = (Icon?)resources.GetObject("$this.Icon");
            this.MinimumSize = new Size(296, 277);
            this.Name = "SettingsForm";
            this.Text = "Soundboard Settings";
            this.Size = new Size(512, 424);

            // Adding the controls to the form
            this.Controls.Add(this.ToggleKeysLabel);
            this.Controls.Add(this.StopKeysLabel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.GroupBox);
            this.Controls.Add(this.MinimizeToTrayCheckBox);
            this.Controls.Add(this.ToggleKeysTextBox);
            this.Controls.Add(this.StopKeysTextBox);

            // After initializing all the objects and their properties,
            // we need to resume layout logic and apply it forcibly
            // Remark: this doesn't equal to ResumeLayout(true)
            this.GroupBox.ResumeLayout(false);
            this.GroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

    }
}