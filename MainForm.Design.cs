// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;
// Also, declaring aliases for the same reason.
using Timer = System.Windows.Forms.Timer;
using EventHandler = System.EventHandler;

namespace HASCore
{
    /// <summary>
    /// <see cref="MainForm"/> class part for implementing the initialization 
    /// </summary>
    partial class MainForm
    {
        /// Description
        /// <summary>
        ///     Container for components on this form.
        /// </summary>
        private Container? Components = null;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="PlaybackDevicesComboBox">PlaybackDevicesComboBox</seealso>.
        /// </summary>
        private Label? PlaybackLabel;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="LoopbackDevicesComboBox">LoopbackDevicesComboBox</seealso>.
        /// </summary>
        private Label? LoopbackLabel;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="PushToTalkKeyTextBox">PushToTalkKeyTextBox</seealso>.
        /// </summary>
        private Label? KeyLabel;
        /// Description
        /// <summary>
        ///     Label for <seealso cref="WindowsComboBox">WindowsComboBox</seealso>.
        /// </summary>
        private Label? WindowLabel;
        /// Description
        /// <summary>
        ///     Button for adding a new entry into the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
        /// </summary>
        private Button? AddButton;
        /// Description
        /// <summary>
        ///     Button for editing a selected entry into the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
        /// </summary>
        private Button? EditButton;
        /// Description
        /// <summary>
        ///     Button for removing a selected entry from the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
        /// </summary>
        private Button? RemoveButton;
        /// Description
        /// <summary>
        ///     Button for clearing the <see cref="KeySoundsListView">KeySoundsListView</see> of current preset.
        /// </summary>
        private Button? ClearButton;
        /// Description
        /// <summary>
        ///     Button for loading <see cref="XMLSettings"/> preset from a file on disk
        ///     into <see cref="KeySoundsListView">KeySoundsListView</see>.
        /// </summary>
        private Button? LoadButton;
        /// Description
        /// <summary>
        ///     Button for saving the <see cref="KeySoundsListView">KeySoundsListView</see> contents
        ///     of a current preset into <see cref="XMLSettings"/> file on disk.
        /// </summary>
        private Button? SaveButton;
        /// Description
        /// <summary>
        ///     Button for saving the <see cref="KeySoundsListView">KeySoundsListView</see> contents
        ///     of a current preset into a different <see cref="XMLSettings"/> file on disk.
        /// </summary>
        private Button? SaveAsButton;
        /// Description
        /// <summary>
        ///     Button for playing a sound from the selected entry inside the <see cref="KeySoundsListView">KeySoundsListView</see>.
        /// </summary>
        private Button? PlaySelectedSoundButton;
        /// Description
        /// <summary>
        ///     Button for stopping all sounds from playing.
        /// </summary>
        private Button? StopAllSoundsButton;
        /// Description
        /// <summary>
        ///     Button for reloading a list of the audio devices currently presented on system.
        /// </summary>
        private Button? ReloadDevicesButton;
        /// Description
        /// <summary>
        ///     Button for reloading a list of currently opened windows inside the OS.
        /// </summary>
        private Button? ReloadWindowsButton;
        /// Description
        /// <summary>
        ///     Box for grouping the controls related to 'Push to talk' function.
        /// </summary>
        private GroupBox? PushToTalkGroupBox;
        /// Description
        /// <summary>
        ///     Box for grouping the controls related to selecting the audio playback and loopback devices.
        /// </summary>
        private GroupBox? AudioDevicesGroupBox;
        /// Description
        /// <summary>
        ///     Checkbox for enabling or disabling the soundboard.
        /// </summary> 
        private CheckBox? EnableCheckBox;
        /// Description
        /// <summary>
        ///     Checkbox for enabling or disabling 'Push to talk' functionality.
        /// </summary> 
        private CheckBox? EnablePushToTalkCheckBox;
        /// Description
        /// <summary>
        ///     Combobox for selecting playback device on system.
        /// </summary> 
        private ComboBox? PlaybackDevicesComboBox;
        /// Description
        /// <summary>
        ///     Combobox for selecting loopback device on system.
        /// </summary> 
        private ComboBox? LoopbackDevicesComboBox;
        /// Description
        /// <summary>
        ///     Combobox for selecting a window to restrict usage of soundboard to.
        /// </summary> 
        private ComboBox? WindowsComboBox;
        /// Description
        /// <summary>
        ///     Textbox for inputing the combination of keys to toggle
        ///     the 'Enable' flag of 'Push to talk' functionality.
        /// </summary>
        private TextBox? PushToTalkKeyTextBox;
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
        ///     A list view for all the <seealso cref="XMLSettings.SoundHotkey">SoundHotkey</seealso>
        ///     settings of the current preset.
        /// </summary>
        internal ListView? KeySoundsListView;
        /// Description
        /// <summary>
        ///     An item inside the <seealso cref="MenuStrip">MenuStrip</seealso> for opening the <seealso cref="SettingsForm"/>. 
        /// </summary>
        private ToolStripMenuItem? SettingsToolStripMenuItem;
        /// Description
        /// <summary>
        ///     An item inside the <seealso cref="MenuStrip">MenuStrip</seealso> for opening the <seealso cref="TextToSpeechForm"/>.      
        /// </summary>
        private ToolStripMenuItem? TTSToolStripMenuItem;
        /// Description
        /// <summary>
        ///     An item inside the <seealso cref="MenuStrip">MenuStrip</seealso> for opening the 'Releases' page on GitHub repo.      
        /// </summary>
        private ToolStripMenuItem? UpdateToolStripMenuItem;
        /// Description
        /// <summary>
        ///     A tool strip on top of the <see cref="MainForm"/> for additional settings and functions. 
        /// </summary>
        private MenuStrip? MenuStrip;
        /// Description
        /// <summary>
        ///     A notification icon on minimizing the soundboard to tray. 
        /// </summary>
        private NotifyIcon? NotificationIcon;
        // /// Description
        // /// <summary>
        // ///     A timer component used to poll for keyboard inputs 
        // ///     to play sounds, stop sounds, enable certain features, etc.
        // /// </summary>
        internal Timer? HoldRepeatTimer;
        /// Description
        /// <summary>
        ///     A timer component used to poll for keyboard inputs 
        ///     while 'Push to talk' is enabled.
        /// </summary>
        private Timer? PushToTalkKeyTimer;

        /// Description
        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
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
            ComponentResourceManager resources = new(typeof(MainForm));
            // Component container            
            this.Components                 = new Container();
            // Labels
            this.PlaybackLabel              = new Label();
            this.LoopbackLabel              = new Label();
            this.KeyLabel                   = new Label();
            this.WindowLabel                = new Label();
            // Buttons
            this.SaveButton                 = new Button();
            this.RemoveButton               = new Button();
            this.EditButton                 = new Button();
            this.AddButton                  = new Button();
            this.LoadButton                 = new Button();
            this.ReloadDevicesButton        = new Button();
            this.ClearButton                = new Button();
            this.SaveAsButton               = new Button();
            this.PlaySelectedSoundButton    = new Button();
            this.StopAllSoundsButton        = new Button();
            this.ReloadWindowsButton        = new Button();
            // Groupboxes
            this.PushToTalkGroupBox         = new GroupBox();
            this.AudioDevicesGroupBox       = new GroupBox();
            // Checkboxes
            this.EnablePushToTalkCheckBox   = new CheckBox();
            this.EnableCheckBox             = new CheckBox();
            // Textboxes
            this.PushToTalkKeyTextBox       = new TextBox();
            // Comboboxes
            this.PlaybackDevicesComboBox    = new ComboBox();
            this.WindowsComboBox            = new ComboBox();
            this.LoopbackDevicesComboBox    = new ComboBox();
            // Column headers
            this.KeysColumnHeader           = new ColumnHeader();
            this.WindowColumnHeader         = new ColumnHeader();
            this.SoundLocationColumnHeader  = new ColumnHeader();
            // List views
            this.KeySoundsListView          = new ListView();
            // Tool strip items
            this.SettingsToolStripMenuItem  = new ToolStripMenuItem();
            this.TTSToolStripMenuItem       = new ToolStripMenuItem();
            this.UpdateToolStripMenuItem    = new ToolStripMenuItem();
            // Tool strips
            this.MenuStrip                  = new MenuStrip();
            // Icons
            this.NotificationIcon           = new NotifyIcon(this.Components);
            // Timers
            this.HoldRepeatTimer            = new Timer(this.Components);
            this.PushToTalkKeyTimer         = new Timer(this.Components);

            // Suspending layout logic before adding controls
            // for child objects to initialize without firing events
            this.SuspendLayout();
            // This has to be done on child objects as well, because
            // suspending layouts on the form itself doesn't suspend
            // the layout logic on the child components
            this.MenuStrip.SuspendLayout();
            this.PushToTalkGroupBox.SuspendLayout();
            this.AudioDevicesGroupBox.SuspendLayout();

            // ------------------------
            // Adding object properties
            // ------------------------

            // Labels

            // 
            // PlaybackLabel
            // 
            this.PlaybackLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.PlaybackLabel.AutoSize = true;
            this.PlaybackLabel.Location = new Point(6, 23);
            this.PlaybackLabel.Name = "PlaybackLabel";
            this.PlaybackLabel.Size = new Size(51, 13);
            this.PlaybackLabel.TabIndex = 5;
            this.PlaybackLabel.Text = "Playback";
            // 
            // LoopbackLabel
            // 
            this.LoopbackLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.LoopbackLabel.AutoSize = true;
            this.LoopbackLabel.Location = new Point(6, 50);
            this.LoopbackLabel.Name = "LoopbackLabel";
            this.LoopbackLabel.Size = new Size(55, 13);
            this.LoopbackLabel.TabIndex = 18;
            this.LoopbackLabel.Text = "Loopback";
            // 
            // KeyLabel
            // 
            this.KeyLabel.AutoSize = true;
            this.KeyLabel.Location = new Point(7, 22);
            this.KeyLabel.Name = "KeyLabel";
            this.KeyLabel.Size = new Size(25, 13);
            this.KeyLabel.TabIndex = 1;
            this.KeyLabel.Text = "Key";
            // 
            // WindowLabel
            // 
            this.WindowLabel.AutoSize = true;
            this.WindowLabel.Location = new Point(7, 48);
            this.WindowLabel.Name = "WindowLabel";
            this.WindowLabel.Size = new Size(46, 13);
            this.WindowLabel.TabIndex = 2;
            this.WindowLabel.Text = "Window";

            // Buttons

            // 
            // AddButton
            // 
            this.AddButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.AddButton.Location = new Point(553, 27);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new Size(75, 43);
            this.AddButton.TabIndex = 1;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new EventHandler(this.AddButton_Click);
            // 
            // EditButton
            // 
            this.EditButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.EditButton.Location = new Point(553, 76);
            this.EditButton.Name = "EditButton";
            this.EditButton.Size = new Size(75, 43);
            this.EditButton.TabIndex = 2;
            this.EditButton.Text = "Edit";
            this.EditButton.UseVisualStyleBackColor = true;
            this.EditButton.Click += new EventHandler(this.EditButton_Click);
            // 
            // RemoveButton
            // 
            this.RemoveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.RemoveButton.Location = new Point(553, 125);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new Size(75, 43);
            this.RemoveButton.TabIndex = 3;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            this.RemoveButton.Click += new EventHandler(this.RemoveButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.ClearButton.Location = new Point(553, 174);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new Size(75, 43);
            this.ClearButton.TabIndex = 4;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.LoadButton.Location = new Point(12, 344);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new Size(145, 23);
            this.LoadButton.TabIndex = 7;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new EventHandler(this.LoadButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.SaveButton.Location = new Point(163, 344);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new Size(145, 23);
            this.SaveButton.TabIndex = 8;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new EventHandler(this.SaveButton_Click);
            // 
            // SaveAsButton
            // 
            this.SaveAsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.SaveAsButton.Location = new Point(316, 344);
            this.SaveAsButton.Name = "SaveAsButton";
            this.SaveAsButton.Size = new Size(145, 23);
            this.SaveAsButton.TabIndex = 9;
            this.SaveAsButton.Text = "Save As";
            this.SaveAsButton.UseVisualStyleBackColor = true;
            this.SaveAsButton.Click += new EventHandler(this.SaveAsButton_Click);
            // 
            // PlaySelectedSoundButton
            // 
            this.PlaySelectedSoundButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.PlaySelectedSoundButton.Location = new Point(553, 246);
            this.PlaySelectedSoundButton.Name = "PlaySelectedSoundButton";
            this.PlaySelectedSoundButton.Size = new Size(75, 43);
            this.PlaySelectedSoundButton.TabIndex = 5;
            this.PlaySelectedSoundButton.Text = "Play sound";
            this.PlaySelectedSoundButton.UseVisualStyleBackColor = true;
            this.PlaySelectedSoundButton.Click += new EventHandler(this.PlaySelectedSoundButton_Click);
            // 
            // StopAllSoundsButton
            // 
            this.StopAllSoundsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.StopAllSoundsButton.Location = new Point(553, 295);
            this.StopAllSoundsButton.Name = "StopAllSoundsButton";
            this.StopAllSoundsButton.Size = new Size(75, 43);
            this.StopAllSoundsButton.TabIndex = 6;
            this.StopAllSoundsButton.Text = "Stop all sounds";
            this.StopAllSoundsButton.UseVisualStyleBackColor = true;
            this.StopAllSoundsButton.Click += new EventHandler(this.StopAllSoundsButton_Click);
            // 
            // ReloadDevicesButton
            // 
            this.ReloadDevicesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.ReloadDevicesButton.Image = (Image?)resources.GetObject("ReloadButton.Image");
            this.ReloadDevicesButton.Location = new Point(318, 47);
            this.ReloadDevicesButton.Name = "ReloadDevicesButton";
            this.ReloadDevicesButton.Size = new Size(22, 22);
            this.ReloadDevicesButton.TabIndex = 12;
            this.ReloadDevicesButton.UseVisualStyleBackColor = true;
            this.ReloadDevicesButton.Click += new EventHandler(this.ReloadDevicesButton_Click);
            // 
            // ReloadWindowsButton
            // 
            this.ReloadWindowsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.ReloadWindowsButton.Image = (Image?)resources.GetObject("ReloadButton.Image");
            this.ReloadWindowsButton.Location = new Point(226, 45);
            this.ReloadWindowsButton.Name = "ReloadWindowsButton";
            this.ReloadWindowsButton.Size = new Size(22, 22);
            this.ReloadWindowsButton.TabIndex = 15;
            this.ReloadWindowsButton.UseVisualStyleBackColor = true;
            this.ReloadWindowsButton.Click += new EventHandler(this.ReloadWindowsButton_Click);

            // Groupboxes

            // 
            // PushToTalkGroupBox
            // 
            this.PushToTalkGroupBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.PushToTalkGroupBox.Controls.Add(this.WindowLabel);
            this.PushToTalkGroupBox.Controls.Add(this.WindowsComboBox);
            this.PushToTalkGroupBox.Controls.Add(this.KeyLabel);
            this.PushToTalkGroupBox.Controls.Add(this.PushToTalkKeyTextBox);
            this.PushToTalkGroupBox.Controls.Add(this.ReloadWindowsButton);
            this.PushToTalkGroupBox.Controls.Add(this.EnablePushToTalkCheckBox);
            this.PushToTalkGroupBox.Location = new Point(372, 393);
            this.PushToTalkGroupBox.Name = "PushToTalkGroupBox";
            this.PushToTalkGroupBox.Size = new Size(254, 94);
            this.PushToTalkGroupBox.TabIndex = 13;
            this.PushToTalkGroupBox.TabStop = false;
            this.PushToTalkGroupBox.Text = "Auto press push to talk key";
            // 
            // AudioDevicesGroupBox
            // 
            this.AudioDevicesGroupBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.AudioDevicesGroupBox.Controls.Add(this.PlaybackLabel);
            this.AudioDevicesGroupBox.Controls.Add(this.PlaybackDevicesComboBox);
            this.AudioDevicesGroupBox.Controls.Add(this.LoopbackLabel);
            this.AudioDevicesGroupBox.Controls.Add(this.LoopbackDevicesComboBox);
            this.AudioDevicesGroupBox.Controls.Add(this.ReloadDevicesButton);
            this.AudioDevicesGroupBox.Location = new Point(12, 413);
            this.AudioDevicesGroupBox.Name = "AudioDevicesGroupBox";
            this.AudioDevicesGroupBox.Size = new Size(354, 74);
            this.AudioDevicesGroupBox.TabIndex = 10;
            this.AudioDevicesGroupBox.TabStop = false;
            this.AudioDevicesGroupBox.Text = "Audio devices";

            // Checkboxes

            // 
            // EnableCheckBox
            // 
            this.EnableCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.EnableCheckBox.AutoSize = true;
            this.EnableCheckBox.Location = new Point(567, 350);
            this.EnableCheckBox.Name = "EnableCheckbox";
            this.EnableCheckBox.Size = new Size(59, 17);
            this.EnableCheckBox.TabIndex = 17;
            this.EnableCheckBox.Text = "Enable";
            this.EnableCheckBox.UseVisualStyleBackColor = true;
            this.EnableCheckBox.CheckedChanged += new EventHandler(this.EnableCheckBox_CheckedChanged);
            // 
            // EnablePushToTalkCheckBox
            // 
            this.EnablePushToTalkCheckBox.AutoSize = true;
            this.EnablePushToTalkCheckBox.Location = new Point(10, 72);
            this.EnablePushToTalkCheckBox.Name = "EnablePushToTalkCheckBox";
            this.EnablePushToTalkCheckBox.Size = new Size(59, 17);
            this.EnablePushToTalkCheckBox.TabIndex = 16;
            this.EnablePushToTalkCheckBox.Text = "Enable";
            this.EnablePushToTalkCheckBox.UseVisualStyleBackColor = true;
            this.EnablePushToTalkCheckBox.CheckedChanged += new EventHandler(this.EnablePushToTalkCheckBox_CheckedChanged);

            // Textboxes

            // 
            // PushToTalkKeyTextBox
            // 
            this.PushToTalkKeyTextBox.Location = new Point(59, 19);
            this.PushToTalkKeyTextBox.Name = "PushToTalkKeyTextBox";
            this.PushToTalkKeyTextBox.ReadOnly = true;
            this.PushToTalkKeyTextBox.Size = new Size(161, 20);
            this.PushToTalkKeyTextBox.TabIndex = 13;
            this.PushToTalkKeyTextBox.Enter += new EventHandler(this.PushToTalkKeyTextBox_Enter);
            this.PushToTalkKeyTextBox.Leave += new EventHandler(this.PushToTalkKeyTextBox_Leave);

            // Comboboxes

            // 
            // PlaybackDevicesComboBox
            // 
            this.PlaybackDevicesComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.PlaybackDevicesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PlaybackDevicesComboBox.FormattingEnabled = true;
            this.PlaybackDevicesComboBox.Location = new Point(72, 20);
            this.PlaybackDevicesComboBox.Name = "PlaybackDevicesComboBox";
            this.PlaybackDevicesComboBox.Size = new Size(240, 21);
            this.PlaybackDevicesComboBox.TabIndex = 10;
            // this.PlaybackDevicesComboBox.SelectedIndexChanged += PlaybackDevicesComboBox_SelectedIndexChanged;
            // 
            // LoopbackDevicesComboBox
            // 
            this.LoopbackDevicesComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.LoopbackDevicesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.LoopbackDevicesComboBox.FormattingEnabled = true;
            this.LoopbackDevicesComboBox.Location = new Point(72, 47);
            this.LoopbackDevicesComboBox.Name = "LoopbackDevicesComboBox";
            this.LoopbackDevicesComboBox.Size = new Size(240, 21);
            this.LoopbackDevicesComboBox.TabIndex = 11;
            // this.LoopbackDevicesComboBox.SelectedIndexChanged += LoopbackDevicesComboBox_SelectedIndexChanged;
            // 
            // WindowsComboBox
            // 
            this.WindowsComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.WindowsComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.WindowsComboBox.FormattingEnabled = true;
            this.WindowsComboBox.Location = new Point(59, 45);
            this.WindowsComboBox.Name = "WindowsComboBox";
            this.WindowsComboBox.Size = new Size(161, 21);
            this.WindowsComboBox.TabIndex = 14;

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
            this.KeySoundsListView.MouseDoubleClick += new MouseEventHandler(this.KeySoundsListView_MouseDoubleClick);
            this.KeySoundsListView.Columns.AddRange(
                [
                    this.KeysColumnHeader,
                    this.WindowColumnHeader,
                    this.SoundLocationColumnHeader
                ]
            );
            
            // Tool strip items

            // 
            // SettingsToolStripMenuItem
            // 
            this.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            this.SettingsToolStripMenuItem.Size = new Size(61, 20);
            this.SettingsToolStripMenuItem.Text = "Settings";
            this.SettingsToolStripMenuItem.Click += new EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // TTSToolStripMenuItem
            // 
            this.TTSToolStripMenuItem.Name = "TTSToolStripMenuItem";
            this.TTSToolStripMenuItem.Size = new Size(99, 20);
            this.TTSToolStripMenuItem.Text = "Text-to-speech";
            this.TTSToolStripMenuItem.Click += new EventHandler(this.TTSToolStripMenuItem_Click);
            // 
            // UpdateToolStripMenuItem
            // 
            this.UpdateToolStripMenuItem.Name = "UpdateToolStripMenuItem";
            this.UpdateToolStripMenuItem.Size = new Size(110, 20);
            this.UpdateToolStripMenuItem.Text = "Check for update";
            this.UpdateToolStripMenuItem.Click += new EventHandler(this.UpdateToolStripMenuItem_Click);

            // Tool strips

            // 
            // MenuStrip
            // 
            this.MenuStrip.Location = new Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new Size(638, 24);
            this.MenuStrip.TabIndex = 17;
            this.MenuStrip.Text = "Menu";
            this.MenuStrip.Items.AddRange(
                [
                    this.SettingsToolStripMenuItem,
                    this.TTSToolStripMenuItem,
                    this.UpdateToolStripMenuItem
                ]
            );

            // Icons

            // 
            // NotificationIcon
            // 
            this.NotificationIcon.BalloonTipIcon = ToolTipIcon.Info;
            this.NotificationIcon.BalloonTipText = "Minimized to the tray.";
            this.NotificationIcon.BalloonTipTitle = "HAS Core";
            this.NotificationIcon.Icon = (Icon?)resources.GetObject("Notification.Icon");
            this.NotificationIcon.Text = "HAS Core";
            this.NotificationIcon.MouseClick += new MouseEventHandler(this.NotificationIcon_MouseClick);
            
            // Timers

            // 
            // HoldRepeatTimer
            // 
            this.HoldRepeatTimer.Interval = 50; // Default value
            this.HoldRepeatTimer.Enabled = true;
            this.HoldRepeatTimer.Tick += new EventHandler(this.HoldRepeatTimer_Tick);
            // 
            // PushToTalkKeyTimer
            // 
            this.PushToTalkKeyTimer.Tick += new EventHandler(this.PushToTalkKeyTimer_Tick);

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
            this.MainMenuStrip = this.MenuStrip;
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
            
            // Adding the controls to the form
            this.Controls.Add(this.AudioDevicesGroupBox);
            this.Controls.Add(this.PushToTalkGroupBox);
            this.Controls.Add(this.StopAllSoundsButton);
            this.Controls.Add(this.PlaySelectedSoundButton);
            this.Controls.Add(this.SaveAsButton);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.EditButton);
            this.Controls.Add(this.AddButton);
            this.Controls.Add(this.KeySoundsListView);
            this.Controls.Add(this.EnableCheckBox);
            this.Controls.Add(this.MenuStrip);
            
            // Assigning an event handler for resizing the window 
            this.Resize += new EventHandler(this.MainForm_Resize);

            // After initializing all the objects and their properties,
            // we need to resume layout logic and apply it forcibly
            // Remark: this doesn't equal to ResumeLayout(true)
            this.ResumeLayout(false);
            this.PerformLayout();
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.PushToTalkGroupBox.ResumeLayout(false);
            this.PushToTalkGroupBox.PerformLayout();
            this.AudioDevicesGroupBox.ResumeLayout(false);
            this.AudioDevicesGroupBox.PerformLayout();
        }
    }
}

