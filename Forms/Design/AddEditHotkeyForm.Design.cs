// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;

namespace HASCore.Forms;

/// <summary>
/// <see cref="AddEditHotkeyForm"/> class part for implementing the initialization 
/// </summary>
public partial class AddEditHotkeyForm
{
    /// Description
    /// <summary>
    ///     Container for components on this form.
    /// </summary>
    private Container? components = null;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="keysTextBox">keysTextBox</seealso>.
    /// </summary>
    private Label? keyInputLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="locationTextBox">locationTextBox</seealso>.
    /// </summary>
    private Label? locationLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="windowsComboBox">windowsComboBox</seealso>.
    /// </summary>
    private Label? windowLabel;
    /// Description
    /// <summary>
    ///     Button to confirm and save the hotkey entry.
    /// </summary>
    private Button? okButton;
    /// Description
    /// <summary>
    ///     Button to cancel and discard changes.
    /// </summary>
    private Button? cancelButton;
    /// Description
    /// <summary>
    ///     Button to browse for a sound file.
    /// </summary>
    private Button? browseSoundButton;
    /// Description
    /// <summary>
    ///     Button to reload the list of windows.
    /// </summary>
    private Button? reloadWindowsButton;
    /// Description
    /// <summary>
    ///     Group box for window restriction options.
    /// </summary>
    private GroupBox? windowRestrictionGroupBox;
    /// Description
    /// <summary>
    ///     Checkbox to enable window restriction.
    /// </summary>
    private CheckBox? enableRestrictWindowCheckBox;
    /// Description
    /// <summary>
    ///     Combo box for selecting a window to restrict to.
    /// </summary>
    private ComboBox? windowsComboBox;
    /// Description
    /// <summary>
    ///     Text box for entering the key combination.
    /// </summary>
    private TextBox? keysTextBox;
    /// Description
    /// <summary>
    ///     Text box for the sound file location.
    /// </summary>
    private TextBox? locationTextBox;

    /// Description
    /// <summary>
    ///     Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

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
        ComponentResourceManager resources  = new (typeof(AddEditHotkeyForm));
        // Component container            
        this.components                     = new Container();
        // Labels
        this.keyInputLabel                  = new Label();
        this.locationLabel                  = new Label();
        this.windowLabel                    = new Label();
        // Buttons
        this.okButton                       = new Button();
        this.cancelButton                   = new Button();
        this.browseSoundButton              = new Button();
        this.reloadWindowsButton            = new Button();
        // Group boxes
        this.windowRestrictionGroupBox      = new GroupBox();
        // Check boxes
        this.enableRestrictWindowCheckBox   = new CheckBox();
        // Combo boxes
        this.windowsComboBox                = new ComboBox();
        // Text boxes
        this.keysTextBox                    = new TextBox();
        this.locationTextBox                = new TextBox();

        // Suspending layout logic before adding controls
        // for child objects to initialize without firing events
        this.SuspendLayout();
        // This has to be done on child objects as well, because
        // suspending layouts on the form itself doesn't suspend
        // the layout logic on the child components
        this.windowRestrictionGroupBox.SuspendLayout();

        // ------------------------
        // Adding object properties
        // ------------------------

        // Labels

        // 
        // keyInputLabel
        // 
        this.keyInputLabel.AutoSize = true;
        this.keyInputLabel.Location = new Point(12, 59);
        this.keyInputLabel.Name = "keyInputLabel";
        this.keyInputLabel.Size = new Size(230, 13);
        this.keyInputLabel.TabIndex = 4;
        this.keyInputLabel.Text = "Keys (click on text box then press desired keys)";
        // 
        // locationLabel
        // 
        this.locationLabel.AutoSize = true;
        this.locationLabel.Location = new Point(12, 8);
        this.locationLabel.Name = "locationLabel";
        this.locationLabel.Size = new Size(76, 13);
        this.locationLabel.TabIndex = 5;
        this.locationLabel.Text = "Location of file";
        // 
        // windowLabel
        // 
        this.windowLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.windowLabel.AutoSize = true;
        this.windowLabel.Location = new Point(11, 48);
        this.windowLabel.Name = "windowLabel";
        this.windowLabel.Size = new Size(46, 13);
        this.windowLabel.TabIndex = 17;
        this.windowLabel.Text = "Window";

        // Buttons

        // 
        // okButton
        // 
        this.okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.okButton.Location = new Point(199, 200);
        this.okButton.Name = "okButton";
        this.okButton.Size = new Size(75, 23);
        this.okButton.TabIndex = 6;
        this.okButton.Text = "OK";
        this.okButton.UseVisualStyleBackColor = true;
        this.okButton.Click += this.OKButton_Click;
        // 
        // cancelButton
        // 
        this.cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.cancelButton.DialogResult = DialogResult.Cancel;
        this.cancelButton.Location = new Point(280, 199);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new Size(75, 23);
        this.cancelButton.TabIndex = 7;
        this.cancelButton.Text = "Cancel";
        this.cancelButton.UseVisualStyleBackColor = true;
        this.cancelButton.Click += this.CancelButton_Click;
        // 
        // browseSoundButton
        // 
        this.browseSoundButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.browseSoundButton.Location = new Point(327, 22);
        this.browseSoundButton.Name = "browseSoundButton";
        this.browseSoundButton.Size = new Size(28, 23);
        this.browseSoundButton.TabIndex = 1;
        this.browseSoundButton.Text = "...";
        this.browseSoundButton.UseVisualStyleBackColor = true;
        this.browseSoundButton.Click += this.BrowseSoundLocationButton_Click;
        // 
        // reloadWindowsButton
        // 
        this.reloadWindowsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.reloadWindowsButton.Enabled = false;
        this.reloadWindowsButton.Image = (Image?)resources.GetObject("reloadWindowsButton.Image");
        this.reloadWindowsButton.Location = new Point(246, 44);
        this.reloadWindowsButton.Name = "reloadWindowsButton";
        this.reloadWindowsButton.Size = new Size(22, 22);
        this.reloadWindowsButton.TabIndex = 5;
        this.reloadWindowsButton.UseVisualStyleBackColor = true;
        this.reloadWindowsButton.Click += this.ReloadWindowsButton_Click;

        // Group boxes

        // 
        // windowRestrictionGroupBox
        // 
        this.windowRestrictionGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.windowRestrictionGroupBox.Controls.Add(this.windowLabel);
        this.windowRestrictionGroupBox.Controls.Add(this.reloadWindowsButton);
        this.windowRestrictionGroupBox.Controls.Add(this.enableRestrictWindowCheckBox);
        this.windowRestrictionGroupBox.Controls.Add(this.windowsComboBox);
        this.windowRestrictionGroupBox.Location = new Point(15, 109);
        this.windowRestrictionGroupBox.Name = "windowRestrictionGroupBox";
        this.windowRestrictionGroupBox.Size = new Size(279, 73);
        this.windowRestrictionGroupBox.TabIndex = 3;
        this.windowRestrictionGroupBox.TabStop = false;
        this.windowRestrictionGroupBox.Text = "Restrict to certain window";

        // Check boxes

        // 
        // enableRestrictWindowCheckBox
        // 
        this.enableRestrictWindowCheckBox.AutoSize = true;
        this.enableRestrictWindowCheckBox.Location = new Point(14, 19);
        this.enableRestrictWindowCheckBox.Name = "enableRestrictWindowCheckBox";
        this.enableRestrictWindowCheckBox.Size = new Size(59, 17);
        this.enableRestrictWindowCheckBox.TabIndex = 3;
        this.enableRestrictWindowCheckBox.Text = "Enable";
        this.enableRestrictWindowCheckBox.UseVisualStyleBackColor = true;
        this.enableRestrictWindowCheckBox.CheckedChanged += this.EnableRestrictWindowCheckBox_CheckedChanged;

        // Combo boxes

        // 
        // windowsComboBox
        // 
        this.windowsComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.windowsComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        this.windowsComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        this.windowsComboBox.Enabled = false;
        this.windowsComboBox.FormattingEnabled = true;
        this.windowsComboBox.Location = new Point(63, 45);
        this.windowsComboBox.Name = "windowsComboBox";
        this.windowsComboBox.Size = new Size(177, 21);
        this.windowsComboBox.TabIndex = 4;
        
        // Text boxes

        // 
        // keysTextBox
        // 
        this.keysTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.keysTextBox.Location = new Point(15, 75);
        this.keysTextBox.Name = "keysTextBox";
        this.keysTextBox.ReadOnly = true;
        this.keysTextBox.Size = new Size(346, 20);
        this.keysTextBox.TabIndex = 2;
        // 
        // locationTextBox
        // 
        this.locationTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.locationTextBox.Location = new Point(15, 24);
        this.locationTextBox.Name = "locationTextBox";
        this.locationTextBox.Size = new Size(306, 20);
        this.locationTextBox.TabIndex = 0;

        // Form

        // 
        // AddEditHotkeyForm
        // 
        this.AcceptButton = this.okButton;
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(367, 231);
        this.Icon = (Icon?)resources.GetObject("$this.Icon");
        this.MaximizeBox = false;
        this.MaximumSize = new Size(4000, 270);
        this.MinimumSize = new Size(375, 270);
        this.Name = "AddEditHotkeyForm";
        this.Text = "Add/edit sound";
        this.FormClosing += this.AddEditHotkeyForm_FormClosing;
        this.Load += this.AddEditSoundKeys_Load;
        this.Shown += this.AddEditHotkeyForm_Shown;

        // Adding the controls to the form
        this.Controls.Add(this.keyInputLabel);
        this.Controls.Add(this.locationLabel);
        this.Controls.Add(this.keysTextBox);
        this.Controls.Add(this.locationTextBox);
        this.Controls.Add(this.browseSoundButton);
        this.Controls.Add(this.okButton);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.windowRestrictionGroupBox);

        // After initializing all the objects and their properties,
        // we need to resume layout logic and apply it forcibly
        // Remark: this doesn't equal to ResumeLayout(true)
        this.ResumeLayout(false);
        this.PerformLayout();
        this.windowRestrictionGroupBox.ResumeLayout(false);
        this.windowRestrictionGroupBox.PerformLayout();
    }
}