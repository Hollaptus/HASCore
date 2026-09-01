// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;
// Also, declaring aliases for the same reason.
using Timer = System.Windows.Forms.Timer;

namespace HASCore.Forms;

/// <summary>
/// <see cref="MainForm"/> class part for implementing the initialization 
/// </summary>
public partial class MainForm
{
    /// Description
    /// <summary>
    ///     Container for components on this form.
    /// </summary>
    private Container? components = null;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="playbackDevicesComboBox">playbackDevicesComboBox</seealso>.
    /// </summary>
    private Label? playbackLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="loopbackDevicesComboBox">loopbackDevicesComboBox</seealso>.
    /// </summary>
    private Label? loopbackLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="pushToTalkKeyTextBox">pushToTalkKeyTextBox</seealso>.
    /// </summary>
    private Label? keyLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="windowsComboBox">windowsComboBox</seealso>.
    /// </summary>
    private Label? windowLabel;
    /// Description
    /// <summary>
    ///     Button for adding a new entry into the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
    /// </summary>
    private Button? addButton;
    /// Description
    /// <summary>
    ///     Button for editing a selected entry into the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
    /// </summary>
    private Button? editButton;
    /// Description
    /// <summary>
    ///     Button for removing a selected entry from the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
    /// </summary>
    private Button? removeButton;
    /// Description
    /// <summary>
    ///     Button for clearing the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
    /// </summary>
    private Button? clearButton;
    /// Description
    /// <summary>
    ///     Button for loading <see cref="XMLSettings"/> preset from a file on disk
    ///     into <see cref="KeySoundsListView">KeySoundsListView</see>.
    /// </summary>
    private Button? loadButton;
    /// Description
    /// <summary>
    ///     Button for saving the <see cref="KeySoundsListView">KeySoundsListView</see> contents
    ///     of a current preset into <see cref="XMLSettings"/> file on disk.
    /// </summary>
    private Button? saveButton;
    /// Description
    /// <summary>
    ///     Button for saving the <see cref="KeySoundsListView">KeySoundsListView</see> contents
    ///     of a current preset into a different <see cref="XMLSettings"/> file on disk.
    /// </summary>
    private Button? saveAsButton;
    /// Description
    /// <summary>
    ///     Button for playing a sound from the selected entry inside the <see cref="KeySoundsListView">KeySoundsListView</see>.
    /// </summary>
    private Button? playSelectedSoundButton;
    /// Description
    /// <summary>
    ///     Button for stopping all sounds from playing.
    /// </summary>
    private Button? stopAllSoundsButton;
    /// Description
    /// <summary>
    ///     Button for reloading a list of the audio devices currently presented on system.
    /// </summary>
    private Button? reloadDevicesButton;
    /// Description
    /// <summary>
    ///     Button for reloading a list of currently opened windows inside the OS.
    /// </summary>
    private Button? reloadWindowsButton;
    /// Description
    /// <summary>
    ///     Box for grouping the controls related to 'Push to talk' function.
    /// </summary>
    private GroupBox? pushToTalkGroupBox;
    /// Description
    /// <summary>
    ///     Box for grouping the controls related to selecting the audio playback and loopback devices.
    /// </summary>
    private GroupBox? audioDevicesGroupBox;
    /// Description
    /// <summary>
    ///     Checkbox for enabling or disabling the soundboard.
    /// </summary> 
    private CheckBox? enableCheckBox;
    /// Description
    /// <summary>
    ///     Checkbox for enabling or disabling 'Push to talk' functionality.
    /// </summary> 
    private CheckBox? enablePushToTalkCheckBox;
    /// Description
    /// <summary>
    ///     Combobox for selecting playback device on system.
    /// </summary> 
    private ComboBox? playbackDevicesComboBox;
    /// Description
    /// <summary>
    ///     Combobox for selecting loopback device on system.
    /// </summary> 
    private ComboBox? loopbackDevicesComboBox;
    /// Description
    /// <summary>
    ///     Combobox for selecting a window to restrict usage of soundboard to.
    /// </summary> 
    private ComboBox? windowsComboBox;
    /// Description
    /// <summary>
    ///     Textbox for inputing the combination of keys to toggle
    ///     the 'Enable' flag of 'Push to talk' functionality.
    /// </summary>
    private TextBox? pushToTalkKeyTextBox;
    /// Description
    /// <summary>
    ///     A tool strip on top of the <see cref="MainForm"/> for additional settings and functions. 
    /// </summary>
    private MenuStrip? menuStrip;
    /// Description
    /// <summary>
    ///     An item inside the <seealso cref="menuStrip">menuStrip</seealso> for opening the <seealso cref="SettingsForm"/>. 
    /// </summary>
    private ToolStripMenuItem? settingsToolStripMenuItem;
    /// Description
    /// <summary>
    ///     An item inside the <seealso cref="menuStrip">menuStrip</seealso> for opening the <seealso cref="TextToSpeechForm"/>.      
    /// </summary>
    private ToolStripMenuItem? ttsToolStripMenuItem;
    /// Description
    /// <summary>
    ///     An item inside the <seealso cref="menuStrip">menuStrip</seealso> for opening the 'Releases' page on GitHub repo.      
    /// </summary>
    private ToolStripMenuItem? updateToolStripMenuItem;
    /// Description
    /// <summary>
    ///     A notification icon on minimizing the soundboard to tray. 
    /// </summary>
    private NotifyIcon? notificationIcon;
    /// Description
    /// <summary>
    ///     A timer component used to poll for keyboard inputs 
    ///     to play sounds, stop sounds, enable certain features, etc.
    /// </summary>
    internal Timer? HoldRepeatTimer;
    /// Description
    /// <summary>
    ///     A list view for all the <seealso cref="XMLSettings.SoundHotkey">SoundHotkey</seealso>
    ///     settings of the current preset.
    /// </summary>
    internal ListView? KeySoundsListView;
    /// Description
    /// <summary>
    ///     A header for displaying the name of the column 'Keys'
    ///     inside <seealso cref="KeysSoundsListView">KeysSoundsListView</seealso>. 
    /// </summary>
    internal ColumnHeader? KeysColumnHeader;
    /// Description
    /// <summary>
    ///     A header for displaying the name of the column 'Sound location'
    ///     inside <seealso cref="KeysSoundsListView">KeysSoundsListView</seealso>.
    /// </summary>
    internal ColumnHeader? SoundLocationColumnHeader;
    /// Description
    /// <summary>
    ///     A header for displaying the name of the column 'Window'
    ///     inside <seealso cref="KeysSoundsListView">KeysSoundsListView</seealso>. 
    /// </summary>
    internal ColumnHeader? WindowColumnHeader;

    /// Description
    /// <summary>
    ///     Clean up any resources being used.
    /// </summary>
    /// 
    /// Parameters
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
        ComponentResourceManager resources  = new (typeof(MainForm));
        // Component container            
        this.components                     = new Container();
        // Labels
        this.playbackLabel                  = new Label();
        this.loopbackLabel                  = new Label();
        this.keyLabel                       = new Label();
        this.windowLabel                    = new Label();
        // Buttons
        this.saveButton                     = new Button();
        this.removeButton                   = new Button();
        this.editButton                     = new Button();
        this.addButton                      = new Button();
        this.loadButton                     = new Button();
        this.reloadDevicesButton            = new Button();
        this.clearButton                    = new Button();
        this.saveAsButton                   = new Button();
        this.playSelectedSoundButton        = new Button();
        this.stopAllSoundsButton            = new Button();
        this.reloadWindowsButton            = new Button();
        // Groupboxes
        this.pushToTalkGroupBox             = new GroupBox();
        this.audioDevicesGroupBox           = new GroupBox();
        // Checkboxes
        this.enablePushToTalkCheckBox       = new CheckBox();
        this.enableCheckBox                 = new CheckBox();
        // Textboxes
        this.pushToTalkKeyTextBox           = new TextBox();
        // Comboboxes
        this.playbackDevicesComboBox        = new ComboBox();
        this.windowsComboBox                = new ComboBox();
        this.loopbackDevicesComboBox        = new ComboBox();
        // Icons
        this.notificationIcon               = new NotifyIcon(this.components);
        // Tool strips
        this.menuStrip                      = new MenuStrip();
        // Tool strip items
        this.settingsToolStripMenuItem      = new ToolStripMenuItem();
        this.ttsToolStripMenuItem           = new ToolStripMenuItem();
        this.updateToolStripMenuItem        = new ToolStripMenuItem();
        // Timers
        this.HoldRepeatTimer                = new Timer(this.components);
        // List views
        this.KeySoundsListView              = new ListView();
        // Column headers
        this.KeysColumnHeader               = new ColumnHeader();
        this.WindowColumnHeader             = new ColumnHeader();
        this.SoundLocationColumnHeader      = new ColumnHeader();

        // Suspending layout logic before adding controls
        // for child objects to initialize without firing events
        this.SuspendLayout();
        // This has to be done on child objects as well, because
        // suspending layouts on the form itself doesn't suspend
        // the layout logic on the child components
        this.menuStrip.SuspendLayout();
        this.pushToTalkGroupBox.SuspendLayout();
        this.audioDevicesGroupBox.SuspendLayout();

        // ------------------------
        // Adding object properties
        // ------------------------

        // Labels

        // 
        // playbackLabel
        // 
        this.playbackLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.playbackLabel.AutoSize = true;
        this.playbackLabel.Location = new Point(6, 23);
        this.playbackLabel.Name = "playbackLabel";
        this.playbackLabel.Size = new Size(51, 13);
        this.playbackLabel.TabIndex = 5;
        this.playbackLabel.Text = "Playback";
        // 
        // loopbackLabel
        // 
        this.loopbackLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.loopbackLabel.AutoSize = true;
        this.loopbackLabel.Location = new Point(6, 50);
        this.loopbackLabel.Name = "loopbackLabel";
        this.loopbackLabel.Size = new Size(55, 13);
        this.loopbackLabel.TabIndex = 18;
        this.loopbackLabel.Text = "Loopback";
        // 
        // keyLabel
        // 
        this.keyLabel.AutoSize = true;
        this.keyLabel.Location = new Point(7, 22);
        this.keyLabel.Name = "keyLabel";
        this.keyLabel.Size = new Size(25, 13);
        this.keyLabel.TabIndex = 1;
        this.keyLabel.Text = "Key";
        // 
        // windowLabel
        // 
        this.windowLabel.AutoSize = true;
        this.windowLabel.Location = new Point(7, 48);
        this.windowLabel.Name = "windowLabel";
        this.windowLabel.Size = new Size(46, 13);
        this.windowLabel.TabIndex = 2;
        this.windowLabel.Text = "Window";

        // Buttons

        // 
        // addButton
        // 
        this.addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.addButton.Location = new Point(553, 27);
        this.addButton.Name = "addButton";
        this.addButton.Size = new Size(75, 43);
        this.addButton.TabIndex = 1;
        this.addButton.Text = "Add";
        this.addButton.UseVisualStyleBackColor = true;
        this.addButton.Click += this.AddButton_Click;
        // 
        // editButton
        // 
        this.editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.editButton.Location = new Point(553, 76);
        this.editButton.Name = "editButton";
        this.editButton.Size = new Size(75, 43);
        this.editButton.TabIndex = 2;
        this.editButton.Text = "Edit";
        this.editButton.UseVisualStyleBackColor = true;
        this.editButton.Click += this.EditButton_Click;
        // 
        // removeButton
        // 
        this.removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.removeButton.Location = new Point(553, 125);
        this.removeButton.Name = "removeButton";
        this.removeButton.Size = new Size(75, 43);
        this.removeButton.TabIndex = 3;
        this.removeButton.Text = "Remove";
        this.removeButton.UseVisualStyleBackColor = true;
        this.removeButton.Click += this.RemoveButton_Click;
        // 
        // clearButton
        // 
        this.clearButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.clearButton.Location = new Point(553, 174);
        this.clearButton.Name = "clearButton";
        this.clearButton.Size = new Size(75, 43);
        this.clearButton.TabIndex = 4;
        this.clearButton.Text = "Clear";
        this.clearButton.UseVisualStyleBackColor = true;
        this.clearButton.Click += this.ClearButton_Click;
        // 
        // loadButton
        // 
        this.loadButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.loadButton.Location = new Point(12, 344);
        this.loadButton.Name = "loadButton";
        this.loadButton.Size = new Size(145, 23);
        this.loadButton.TabIndex = 7;
        this.loadButton.Text = "Load";
        this.loadButton.UseVisualStyleBackColor = true;
        this.loadButton.Click += this.LoadButton_Click;
        // 
        // saveButton
        // 
        this.saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.saveButton.Location = new Point(163, 344);
        this.saveButton.Name = "saveButton";
        this.saveButton.Size = new Size(145, 23);
        this.saveButton.TabIndex = 8;
        this.saveButton.Text = "Save";
        this.saveButton.UseVisualStyleBackColor = true;
        this.saveButton.Click += this.SaveButton_Click;
        // 
        // saveAsButton
        // 
        this.saveAsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.saveAsButton.Location = new Point(316, 344);
        this.saveAsButton.Name = "saveAsButton";
        this.saveAsButton.Size = new Size(145, 23);
        this.saveAsButton.TabIndex = 9;
        this.saveAsButton.Text = "Save As";
        this.saveAsButton.UseVisualStyleBackColor = true;
        this.saveAsButton.Click += this.SaveAsButton_Click;
        // 
        // playSelectedSoundButton
        // 
        this.playSelectedSoundButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.playSelectedSoundButton.Location = new Point(553, 246);
        this.playSelectedSoundButton.Name = "playSelectedSoundButton";
        this.playSelectedSoundButton.Size = new Size(75, 43);
        this.playSelectedSoundButton.TabIndex = 5;
        this.playSelectedSoundButton.Text = "Play sound";
        this.playSelectedSoundButton.UseVisualStyleBackColor = true;
        this.playSelectedSoundButton.Click += this.PlaySelectedSoundButton_Click;
        // 
        // stopAllSoundsButton
        // 
        this.stopAllSoundsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.stopAllSoundsButton.Location = new Point(553, 295);
        this.stopAllSoundsButton.Name = "stopAllSoundsButton";
        this.stopAllSoundsButton.Size = new Size(75, 43);
        this.stopAllSoundsButton.TabIndex = 6;
        this.stopAllSoundsButton.Text = "Stop all sounds";
        this.stopAllSoundsButton.UseVisualStyleBackColor = true;
        this.stopAllSoundsButton.Click += this.StopAllSoundsButton_Click;
        // 
        // reloadDevicesButton
        // 
        this.reloadDevicesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.reloadDevicesButton.Image = (Image?)resources.GetObject("ReloadButton.Image");
        this.reloadDevicesButton.Location = new Point(318, 47);
        this.reloadDevicesButton.Name = "reloadDevicesButton";
        this.reloadDevicesButton.Size = new Size(22, 22);
        this.reloadDevicesButton.TabIndex = 12;
        this.reloadDevicesButton.UseVisualStyleBackColor = true;
        this.reloadDevicesButton.Click += this.ReloadDevicesButton_Click;
        // 
        // reloadWindowsButton
        // 
        this.reloadWindowsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.reloadWindowsButton.Image = (Image?)resources.GetObject("ReloadButton.Image");
        this.reloadWindowsButton.Location = new Point(226, 45);
        this.reloadWindowsButton.Name = "reloadWindowsButton";
        this.reloadWindowsButton.Size = new Size(22, 22);
        this.reloadWindowsButton.TabIndex = 15;
        this.reloadWindowsButton.UseVisualStyleBackColor = true;
        this.reloadWindowsButton.Click += this.ReloadWindowsButton_Click;

        // Groupboxes

        // 
        // pushToTalkGroupBox
        // 
        this.pushToTalkGroupBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.pushToTalkGroupBox.Controls.Add(this.windowLabel);
        this.pushToTalkGroupBox.Controls.Add(this.keyLabel);
        this.pushToTalkGroupBox.Controls.Add(this.reloadWindowsButton);
        this.pushToTalkGroupBox.Controls.Add(this.enablePushToTalkCheckBox);
        this.pushToTalkGroupBox.Controls.Add(this.windowsComboBox);
        this.pushToTalkGroupBox.Controls.Add(this.pushToTalkKeyTextBox);
        this.pushToTalkGroupBox.Location = new Point(372, 393);
        this.pushToTalkGroupBox.Name = "pushToTalkGroupBox";
        this.pushToTalkGroupBox.Size = new Size(254, 94);
        this.pushToTalkGroupBox.TabIndex = 13;
        this.pushToTalkGroupBox.TabStop = false;
        this.pushToTalkGroupBox.Text = "Auto press push to talk key";
        // 
        // audioDevicesGroupBox
        // 
        this.audioDevicesGroupBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.audioDevicesGroupBox.Controls.Add(this.playbackLabel);
        this.audioDevicesGroupBox.Controls.Add(this.loopbackLabel);
        this.audioDevicesGroupBox.Controls.Add(this.reloadDevicesButton);
        this.audioDevicesGroupBox.Controls.Add(this.playbackDevicesComboBox);
        this.audioDevicesGroupBox.Controls.Add(this.loopbackDevicesComboBox);
        this.audioDevicesGroupBox.Location = new Point(12, 413);
        this.audioDevicesGroupBox.Name = "audioDevicesGroupBox";
        this.audioDevicesGroupBox.Size = new Size(354, 74);
        this.audioDevicesGroupBox.TabIndex = 10;
        this.audioDevicesGroupBox.TabStop = false;
        this.audioDevicesGroupBox.Text = "Audio devices";

        // Checkboxes

        // 
        // enableCheckBox
        // 
        this.enableCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.enableCheckBox.AutoSize = true;
        this.enableCheckBox.Location = new Point(567, 350);
        this.enableCheckBox.Name = "enableCheckBox";
        this.enableCheckBox.Size = new Size(59, 17);
        this.enableCheckBox.TabIndex = 17;
        this.enableCheckBox.Text = "Enable";
        this.enableCheckBox.UseVisualStyleBackColor = true;
        this.enableCheckBox.CheckedChanged += this.EnableCheckBox_CheckedChanged;
        // 
        // enablePushToTalkCheckBox
        // 
        this.enablePushToTalkCheckBox.AutoSize = true;
        this.enablePushToTalkCheckBox.Location = new Point(10, 72);
        this.enablePushToTalkCheckBox.Name = "enablePushToTalkCheckBox";
        this.enablePushToTalkCheckBox.Size = new Size(59, 17);
        this.enablePushToTalkCheckBox.TabIndex = 16;
        this.enablePushToTalkCheckBox.Text = "Enable";
        this.enablePushToTalkCheckBox.UseVisualStyleBackColor = true;
        this.enablePushToTalkCheckBox.CheckedChanged += this.EnablePushToTalkCheckBox_CheckedChanged;

        // Textboxes

        // 
        // pushToTalkKeyTextBox
        // 
        this.pushToTalkKeyTextBox.Location = new Point(59, 19);
        this.pushToTalkKeyTextBox.Name = "pushToTalkKeyTextBox";
        this.pushToTalkKeyTextBox.ReadOnly = true;
        this.pushToTalkKeyTextBox.Size = new Size(161, 20);
        this.pushToTalkKeyTextBox.TabIndex = 13;

        // Comboboxes

        // 
        // playbackDevicesComboBox
        // 
        this.playbackDevicesComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.playbackDevicesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        this.playbackDevicesComboBox.FormattingEnabled = true;
        this.playbackDevicesComboBox.Location = new Point(72, 20);
        this.playbackDevicesComboBox.Name = "playbackDevicesComboBox";
        this.playbackDevicesComboBox.Size = new Size(240, 21);
        this.playbackDevicesComboBox.TabIndex = 10;
        // 
        // loopbackDevicesComboBox
        // 
        this.loopbackDevicesComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.loopbackDevicesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        this.loopbackDevicesComboBox.FormattingEnabled = true;
        this.loopbackDevicesComboBox.Location = new Point(72, 47);
        this.loopbackDevicesComboBox.Name = "loopbackDevicesComboBox";
        this.loopbackDevicesComboBox.Size = new Size(240, 21);
        this.loopbackDevicesComboBox.TabIndex = 11;
        // 
        // windowsComboBox
        // 
        this.windowsComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        this.windowsComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        this.windowsComboBox.FormattingEnabled = true;
        this.windowsComboBox.Location = new Point(59, 45);
        this.windowsComboBox.Name = "windowsComboBox";
        this.windowsComboBox.Size = new Size(161, 21);
        this.windowsComboBox.TabIndex = 14;        
        
        // Tool strips

        // 
        // MenuStrip
        // 
        this.menuStrip.Location = new Point(0, 0);
        this.menuStrip.Name = "MenuStrip";
        this.menuStrip.Size = new Size(638, 24);
        this.menuStrip.TabIndex = 17;
        this.menuStrip.Text = "Menu";
        this.menuStrip.Items.AddRange(
            [
                this.settingsToolStripMenuItem,
                this.ttsToolStripMenuItem,
                this.updateToolStripMenuItem
            ]
        );
 
        // Tool strip items

        // 
        // SettingsToolStripMenuItem
        // 
        this.settingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
        this.settingsToolStripMenuItem.Size = new Size(61, 20);
        this.settingsToolStripMenuItem.Text = "Settings";
        this.settingsToolStripMenuItem.Click += this.SettingsToolStripMenuItem_Click;
        // 
        // TTSToolStripMenuItem
        // 
        this.ttsToolStripMenuItem.Name = "TTSToolStripMenuItem";
        this.ttsToolStripMenuItem.Size = new Size(99, 20);
        this.ttsToolStripMenuItem.Text = "Text-to-speech";
        this.ttsToolStripMenuItem.Click += this.TTSToolStripMenuItem_Click;
        // 
        // UpdateToolStripMenuItem
        // 
        this.updateToolStripMenuItem.Name = "UpdateToolStripMenuItem";
        this.updateToolStripMenuItem.Size = new Size(110, 20);
        this.updateToolStripMenuItem.Text = "Check for update";
        this.updateToolStripMenuItem.Click += this.UpdateToolStripMenuItem_Click;

        // Icons

        // 
        // NotificationIcon
        // 
        this.notificationIcon.BalloonTipIcon = ToolTipIcon.Info;
        this.notificationIcon.BalloonTipText = "Minimized to the tray.";
        this.notificationIcon.BalloonTipTitle = "HAS Core";
        this.notificationIcon.Icon = (Icon?)resources.GetObject("Notification.Icon");
        this.notificationIcon.Text = "HAS Core";
        this.notificationIcon.MouseClick += this.NotificationIcon_MouseClick;
        
        // Timers

        // 
        // HoldRepeatTimer
        // 
        this.HoldRepeatTimer.Interval = 50; // Default value
        this.HoldRepeatTimer.Enabled = true;
        this.HoldRepeatTimer.Tick += this.HoldRepeatTimer_Tick;

        // List views

        // 
        // KeySoundsListView
        // 
        this.KeySoundsListView.Alignment = ListViewAlignment.Default;
        this.KeySoundsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.KeySoundsListView.FullRowSelect = true;
        this.KeySoundsListView.GridLines = true;
        this.KeySoundsListView.Location = new Point(12, 27);
        this.KeySoundsListView.MultiSelect = false;
        this.KeySoundsListView.Name = "KeySoundsListView";
        this.KeySoundsListView.Size = new Size(535, 311);
        this.KeySoundsListView.TabIndex = 0;
        this.KeySoundsListView.UseCompatibleStateImageBehavior = false;
        this.KeySoundsListView.View = View.Details;
        this.KeySoundsListView.MouseDoubleClick += this.KeySoundsListView_MouseDoubleClick;
        this.KeySoundsListView.Columns.AddRange(
            [
                this.KeysColumnHeader,
                this.WindowColumnHeader,
                this.SoundLocationColumnHeader
            ]
        );

        // Column headers

        // 
        // KeysColumnHeader
        // 
        this.KeysColumnHeader.Text = "Keys";
        this.KeysColumnHeader.Width = 150;
        // 
        // WindowColumnHeader
        // 
        this.WindowColumnHeader.Text = "Window";
        // 
        // SoundLocationColumnHeader
        // 
        this.SoundLocationColumnHeader.Text = "Sound location";
        this.SoundLocationColumnHeader.Width = 300;

        // Form

        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(638, 499);
        this.Icon = (Icon?)resources.GetObject("$this.Icon");
        this.MinimumSize = new Size(610, 530);
        this.Name = "MainForm";
        this.Text = "HAS Core";
        this.MainMenuStrip = this.menuStrip;
        this.FormClosing += this.MainForm_FormClosing;
        
        // Adding the controls to the form
        this.Controls.Add(this.saveAsButton);
        this.Controls.Add(this.clearButton);
        this.Controls.Add(this.loadButton);
        this.Controls.Add(this.saveButton);
        this.Controls.Add(this.removeButton);
        this.Controls.Add(this.editButton);
        this.Controls.Add(this.addButton);
        this.Controls.Add(this.playSelectedSoundButton);
        this.Controls.Add(this.stopAllSoundsButton);
        this.Controls.Add(this.audioDevicesGroupBox);
        this.Controls.Add(this.pushToTalkGroupBox);
        this.Controls.Add(this.enableCheckBox);
        this.Controls.Add(this.menuStrip);
        this.Controls.Add(this.KeySoundsListView);
        
        // Assigning an event handler for resizing the window 
        this.Resize += this.MainForm_Resize;

        // After initializing all the objects and their properties,
        // we need to resume layout logic and apply it forcibly
        // Remark: this doesn't equal to ResumeLayout(true)
        this.ResumeLayout(false);
        this.PerformLayout();
        this.menuStrip.ResumeLayout(false);
        this.menuStrip.PerformLayout();
        this.pushToTalkGroupBox.ResumeLayout(false);
        this.pushToTalkGroupBox.PerformLayout();
        this.audioDevicesGroupBox.ResumeLayout(false);
        this.audioDevicesGroupBox.PerformLayout();
    }
}