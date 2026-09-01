// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;

namespace HASCore.Forms;

/// Description
/// <summary>
///     <see cref="SettingsForm"/> class part for implementing the initialization
///     of an object. Provides the settings dialog for configuring the soundboard
///     application, including key bindings, XML presets, and playback options.
/// </summary>
/// <remarks>
///     <para>
///         This form allows the user to view, add, edit, and remove XML‑based sound presets,
///         change global hotkeys (stop all sounds, toggle soundboard enabled), and adjust
///         playback behaviour (minimise to tray, overlapping sounds, repeat on hold).
///     </para>
///     <para>
///         The form is implemented as a <see langword="partial"/> class to separate the
///         design <see cref="InitializeComponent"/> code from the rest of the logic.
///     </para>
/// </remarks>
partial class SettingsForm
{
    /// Description
    /// <summary>
    ///     Container for components on this form.
    /// </summary>
    private Container? components = null;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="stopKeysTextBox">StopKeysTextBox</seealso>.
    /// </summary>
    private Label? stopKeysLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="toggleKeysTextBox">ToggleKeysTextBox</seealso>.
    /// </summary>
    private Label? toggleKeysLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="holdDelayNumeric">HoldDelayNumeric</seealso>.
    /// </summary>
    private Label? holdDelayLabel;
    /// Description
    /// <summary>
    ///     Button for adding new XML file location of <see cref="XMLSettings"/> inside of 
    ///     <seealso cref="KeyLocationsListView"/>, along with hotkeys to change to this preset to.
    /// </summary>
    private Button? addButton;
    /// Description
    /// <summary>
    ///     Button for editing existing <see cref="XMLSettings"/> inside of 
    ///     <seealso cref="KeyLocationsListView"/> and their hotkeys to change to this preset.
    /// </summary>
    private Button? editButton;
    /// Description
    /// <summary>
    ///     Button for removing existing <see cref="XMLSettings"/> inside of <seealso cref="KeyLocationsListView"/>.
    /// </summary>
    private Button? removeButton;
    /// Description
    /// <summary>
    ///     Button for accepting the changes made inside this form and saving them to disk.
    /// </summary>
    private Button? okButton;
    /// Description
    /// <summary>
    ///     Button for discarding the changes made inside this form.
    /// </summary>
    private Button? cancelButton;
    /// Description
    /// <summary>
    ///     Box for grouping the list view and their respective button controls.
    /// </summary>
    private GroupBox? groupBox;
    /// Description
    /// <summary>
    ///     Checkbox for enabling the setting so the form minimizes to tray instead of taskbar.
    /// </summary> 
    private CheckBox? minimizeToTrayCheckBox;
    /// Description
    /// <summary>
    ///     Checkbox for enabling the setting to play sounds over eachother instead of
    ///     stopping the previous sound and playing a new one.
    /// </summary> 
    private CheckBox? playOverEachotherCheckBox;
    /// Description
    /// <summary>
    ///     Checkbox for enabling the setting to repeat sounds on holding the hotkey.
    /// </summary> 
    private CheckBox? repeatOnHoldCheckBox;
    /// Description
    /// <summary>
    ///     Textbox for inputing the combination of keys to stop all sounds from playing.
    /// </summary>
    private TextBox? stopKeysTextBox;
    /// Description
    /// <summary>
    ///     Textbox for inputing the combination of keys to toggle the 'Enabled' flag.
    /// </summary>
    private TextBox? toggleKeysTextBox;
    /// Description
    /// <summary>
    ///     Spin box for inputing the delay in milliseconds for playing the new sound on holding the hotkey.
    /// </summary>
    private NumericUpDown? holdDelayNumeric;
    /// Description
    /// <summary>
    ///     A list view for all added <see cref="XMLSettings"/> as presets.
    /// </summary>
    internal ListView? KeysLocationsListView;
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
    ///     Clean up any resources being used.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="disposing">
    ///     <c>true</c> if managed resources should be disposed; otherwise, <c>false</c>.
    /// </param>
    protected override void Dispose(Boolean disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        
        base.Dispose(disposing);
    }

    /// Description
    /// <summary>
    ///     Component initialization on program startup.
    /// </summary>
    /// 
    /// Additional information
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
        ComponentResourceManager resources  = new (typeof(SettingsForm));
        // Component container
        this.components                     = new Container();
        // Labels
        this.stopKeysLabel                  = new Label();
        this.toggleKeysLabel                = new Label();
        this.holdDelayLabel                 = new Label();
        // Buttons
        this.addButton                      = new Button();
        this.editButton                     = new Button();
        this.removeButton                   = new Button();
        this.okButton                       = new Button();
        this.cancelButton                   = new Button();
        // Groupboxes
        this.groupBox                       = new GroupBox();
        // Checkboxes
        this.minimizeToTrayCheckBox         = new CheckBox();
        this.playOverEachotherCheckBox      = new CheckBox();
        this.repeatOnHoldCheckBox           = new CheckBox();
        // Textboxes
        this.stopKeysTextBox                = new TextBox();
        this.toggleKeysTextBox              = new TextBox();
        // Spinboxes
        this.holdDelayNumeric               = new NumericUpDown();
        // List views
        this.KeysLocationsListView          = new ListView();
        // Column headers
        this.KeysColumnHeader               = new ColumnHeader();
        this.XMLLocationsColumnHeader       = new ColumnHeader();

        // Suspending layout logic before adding controls
        // for child objects to initialize without firing events
        this.SuspendLayout();
        // This has to be done on child objects as well, because
        // suspending layouts on the form itself doesn't suspend
        // the layout logic on the child components
        this.groupBox.SuspendLayout();

        // ------------------------
        // Adding object properties
        // ------------------------

        // Labels

        // 
        // stopKeysLabel
        // 
        this.stopKeysLabel.Name = "stopKeysLabel";
        this.stopKeysLabel.Location = new Point(15, 15);
        this.stopKeysLabel.Size = new Size(104, 13);
        this.stopKeysLabel.AutoSize = true;
        this.stopKeysLabel.Text = "Stop all sounds keys";
        // 
        // toggleKeysLabel
        // 
        this.toggleKeysLabel.Location = new Point(15, 45);
        this.toggleKeysLabel.Name = "toggleKeysLabel";
        this.toggleKeysLabel.Size = new Size(104, 16);
        this.toggleKeysLabel.Text = "Enable Soundboard Hotkeys";
        // 
        // holdDelayLabel
        // 
        this.holdDelayLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.holdDelayLabel.Location = new Point(310, 342);
        this.holdDelayLabel.Name = "holdDelayLabel";
        this.holdDelayLabel.Size = new Size(50, 16);
        this.holdDelayLabel.Text = "ms delay";

        // Buttons

        // 
        // addButton
        // 
        this.addButton.Name = "addButton";
        this.addButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.addButton.Location = new Point(6, 162);
        this.addButton.Size = new Size(75, 23);
        this.addButton.TabIndex = 1;
        this.addButton.Text = "Add";
        this.addButton.UseVisualStyleBackColor = true;
        this.addButton.Click += this.AddButton_Click;
        // 
        // editButton
        // 
        this.editButton.Name = "editButton";
        this.editButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.editButton.Location = new Point(87, 162);
        this.editButton.Size = new Size(75, 23);
        this.editButton.TabIndex = 2;
        this.editButton.Text = "Edit";
        this.editButton.UseVisualStyleBackColor = true;
        this.editButton.Click += this.EditButton_Click;
        // 
        // removeButton
        // 
        this.removeButton.Name = "removeButton";
        this.removeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.removeButton.Location = new Point(168, 162);
        this.removeButton.Size = new Size(75, 23);
        this.removeButton.TabIndex = 3;
        this.removeButton.Text = "Remove";
        this.removeButton.UseVisualStyleBackColor = true;
        this.removeButton.Click += this.RemoveButton_Click;
        // 
        // okButton
        // 
        this.okButton.Name = "okButton";
        this.okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.okButton.Location = new Point(325, 370);
        this.okButton.Size = new Size(75, 23);
        this.okButton.TabIndex = 7;
        this.okButton.Text = "OK";
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.Click += this.OKButton_Click;
        // 
        // cancelButton
        // 
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.cancelButton.Location = new Point(410, 370);
        this.cancelButton.Size = new Size(75, 23);
        this.cancelButton.TabIndex = 8;
        this.cancelButton.Text = "Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.DialogResult = DialogResult.Cancel;
        this.cancelButton.Click += this.CancelButton_Click;
        
        // Groupboxes

        // 
        // groupBox
        // 
        this.groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.groupBox.Name = "groupBox";
        this.groupBox.Location = new Point(13, 70);
        this.groupBox.Size = new Size(472, 200);
        this.groupBox.TabIndex = 2;
        this.groupBox.TabStop = false;
        this.groupBox.Text = "Load XML file with keys";
        this.groupBox.Controls.Add(this.KeysLocationsListView);
        this.groupBox.Controls.Add(this.addButton);
        this.groupBox.Controls.Add(this.removeButton);
        this.groupBox.Controls.Add(this.editButton);

        // Checkboxes

        // 
        // minimizeToTrayCheckBox
        // 
        this.minimizeToTrayCheckBox.Name = "minimizeToTrayCheckBox";
        this.minimizeToTrayCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.minimizeToTrayCheckBox.Location = new Point(15, 280);
        this.minimizeToTrayCheckBox.Size = new Size(215, 17);
        this.minimizeToTrayCheckBox.AutoSize = true;
        this.minimizeToTrayCheckBox.TabIndex = 3;
        this.minimizeToTrayCheckBox.Text = "Minimize button sends application to tray";
        this.minimizeToTrayCheckBox.UseVisualStyleBackColor = true;
        // 
        // playOverEachotherCheckBox
        // 
        this.playOverEachotherCheckBox.Name = "playOverEachotherCheckBox";
        this.playOverEachotherCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.playOverEachotherCheckBox.Location = new Point(15, 310);
        this.playOverEachotherCheckBox.Size = new Size(215, 17);
        this.playOverEachotherCheckBox.AutoSize = true;
        this.playOverEachotherCheckBox.TabIndex = 4;
        this.playOverEachotherCheckBox.Text = "Allow playing sounds over eachother";
        this.playOverEachotherCheckBox.UseVisualStyleBackColor = true;
        // 
        // repeatOnHoldCheckBox
        // 
        this.repeatOnHoldCheckBox.Name = "repeatOnHoldCheckBox";
        this.repeatOnHoldCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.repeatOnHoldCheckBox.Location = new Point(15, 340);
        this.repeatOnHoldCheckBox.Size = new Size(215, 17);
        this.repeatOnHoldCheckBox.AutoSize = true;
        this.repeatOnHoldCheckBox.TabIndex = 5;
        this.repeatOnHoldCheckBox.Text = "Allow repeating sounds on holding the hotkey";
        this.repeatOnHoldCheckBox.UseVisualStyleBackColor = true;

        // Textboxes
        
        // 
        // stopKeysTextBox
        // 
        this.stopKeysTextBox.Name = "stopKeysTextBox";
        this.stopKeysTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.stopKeysTextBox.Location = new Point(122, 12);
        this.stopKeysTextBox.Size = new Size(365, 20);
        this.stopKeysTextBox.ReadOnly = true;
        this.stopKeysTextBox.TabIndex = 0;
        // 
        // toggleKeysTextBox
        //
        this.toggleKeysTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; 
        this.toggleKeysTextBox.Location = new Point(122, 42);
        this.toggleKeysTextBox.Name = "toggleKeysTextBox";
        this.toggleKeysTextBox.ReadOnly = true;
        this.toggleKeysTextBox.Size = new Size(365, 20);
        this.toggleKeysTextBox.TabIndex = 1;

        // Spinboxes

        // 
        // holdDelayNumeric
        //
        this.holdDelayNumeric.Anchor = AnchorStyles.Bottom | AnchorStyles.Left; 
        this.holdDelayNumeric.Location = new Point(250, 340);
        this.holdDelayNumeric.Name = "holdDelayNumeric";
        this.holdDelayNumeric.Size = new Size(60, 24);
        this.holdDelayNumeric.TabIndex = 6;
        this.holdDelayNumeric.Minimum = 50;
        this.holdDelayNumeric.Maximum = 1000;
        this.holdDelayNumeric.Value = 500;
        this.holdDelayNumeric.Enabled = false;

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
        this.KeysLocationsListView.TabIndex = 0;
        this.KeysLocationsListView.UseCompatibleStateImageBehavior = false;
        this.KeysLocationsListView.MouseDoubleClick += this.KeysLocationsListView_MouseDoubleClick;
        this.KeysLocationsListView.Columns.AddRange(
            [
                this.KeysColumnHeader,
                this.XMLLocationsColumnHeader
            ]
        );

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

        // Form
        
        // 
        // SettingsForm
        // 
        this.AcceptButton = this.okButton;
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.Size = new Size(525, 425);
        this.ClientSize = new Size(495, 395);
        this.MinimumSize = new Size(390, 425);
        this.Icon = (Icon?)resources.GetObject("$this.Icon");
        this.Name = "SettingsForm";
        this.Text = "Soundboard Settings";
        this.FormClosing += this.SettingsForm_FormClosing;

        // Adding the controls to the form
        this.Controls.Add(this.toggleKeysLabel);
        this.Controls.Add(this.stopKeysLabel);
        this.Controls.Add(this.holdDelayLabel);
        this.Controls.Add(this.okButton);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.groupBox);
        this.Controls.Add(this.minimizeToTrayCheckBox);
        this.Controls.Add(this.playOverEachotherCheckBox);
        this.Controls.Add(this.repeatOnHoldCheckBox);
        this.Controls.Add(this.toggleKeysTextBox);
        this.Controls.Add(this.stopKeysTextBox);
        this.Controls.Add(this.holdDelayNumeric);

        // After initializing all the objects and their properties,
        // we need to resume layout logic and apply it forcibly
        // Remark: this doesn't equal to ResumeLayout(true)
        this.groupBox.ResumeLayout(false);
        this.groupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}