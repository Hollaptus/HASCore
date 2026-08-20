using System.Media;
using System.Diagnostics;
using NAudio.Wave;
using HASCore.Helpers;
using HASCore.Keyboard;
using HASCore.Soundboard;
// Declaring the using statement so we don't have to always prepend
// 'XMLSettings' to an already static fields of the class.
using static HASCore.Soundboard.XMLSettings;

namespace HASCore.Forms;

/// Description
/// <summary>
///     <see cref="MainForm"/> class part responsible for the main form's logic,
///     audio playback, loopback, keyboard hook handling, and user interactions.
/// </summary>
/// 
/// Additional information
/// <remarks>
///     This partial class works together with the designer‑generated code
///     (<see cref="InitializeComponent"/>) to provide the full main window.
///     It manages the soundboard's core functionality: loading XML presets,
///     playing sounds via hotkeys, handling push‑to‑talk, loopback recording,
///     and device management. The form also handles global keyboard input via
///     <see cref="GlobalKeyboardHook"/> and integrates with the
///     <see cref="AudioPlaybackEngine"/>.
/// </remarks>
public partial class MainForm : Form
{
    #region Private Fields

    /// Description
    /// <summary>
    ///     Temporary variable for random sound selection when multiple sounds
    ///     are assigned to the same hotkey.
    /// </summary>
    private Int32 _tempLastIndex = -1;

    /// Description
    /// <summary>
    ///     Random number generator for selecting a random sound from the list
    ///     when multiple sounds are mapped to the same hotkey.
    /// </summary>
    private readonly Random _random = new ();

    /// Description
    /// <summary>
    ///     Buffered wave provider used for loopback audio capture.
    /// </summary>
    private BufferedWaveProvider? _loopbackWaveProvider = null;

    /// Description
    /// <summary>
    ///     Wave input stream for capturing system audio (loopback).
    /// </summary>
    private WaveIn? _loopbackSourceStream = null;

    /// Description
    /// <summary>
    ///     Wave output device for playing the captured loopback audio.
    /// </summary>
    private WaveOut? _loopbackWaveOut = null;

    /// Description
    /// <summary>
    ///     The key assigned for the push‑to‑talk functionality.
    /// </summary>
    private Keys _pushToTalkKey;

    /// Description
    /// <summary>
    ///     Flag indicating whether the push‑to‑talk key is currently released
    ///     (used for simulating key press/release).
    /// </summary>
    private Boolean _keyUpPushToTalkKey = false;

    /// Description
    /// <summary>
    ///     Flag to prevent multiple message boxes from being shown simultaneously.
    /// </summary>
    private Boolean _showMsgBox = false;

    /// Description
    /// <summary>
    ///     Stores the last processed key combination to avoid duplicate handling.
    /// </summary>
    private HashSet<Keys>? _lastProcessedKeys = null;

    /// Description
    /// <summary>
    ///     Set of keys that are currently being held down for a repeating hotkey.
    /// </summary>
    private HashSet<Keys>? _holdKeys;

    /// Description
    /// <summary>
    ///     The hotkey that is currently being held (used for repeat functionality).
    /// </summary>
    private SoundHotkey? _currentHoldHotkey;

    #endregion

    #region Internal Fields

    /// Description
    /// <summary>
    ///     Full path to the currently loaded XML settings file.
    /// </summary>
    internal String XMLLocation = String.Empty;

    /// Description
    /// <summary>
    ///     List of sound hotkeys currently loaded into the soundboard.
    /// </summary>
    internal List<SoundHotkey> SoundHotkeys = [];

    #endregion

    #region Constructor

    /// Description
    /// <summary>
    ///     Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     This constructor performs the following steps:
    ///     <list type="number">
    ///         <item>
    ///             <term>Component initialization</term>
    ///             <description>Calls <see cref="InitializeComponent"/> to set up the UI.</description>
    ///         </item>
    ///         <item>
    ///             <term>Global hook setup</term>
    ///             <description>Initializes <see cref="GlobalKeyboardHook"/> and subscribes to <see cref="GlobalKeyboardHook.KeysChanged"/>.</description>
    ///         </item>
    ///         <item>
    ///             <term>Working directory</term>
    ///             <description>Sets the current directory to the executable's location.</description>
    ///         </item>
    ///         <item>
    ///             <term>Tooltips</term>
    ///             <description>Adds tooltips to certain buttons.</description>
    ///         </item>
    ///         <item>
    ///             <term>Load data</term>
    ///             <description>Populates device and window lists, loads sound settings, and initializes the audio engine.</description>
    ///         </item>
    ///         <item>
    ///             <term>Event subscription</term>
    ///             <description>Subscribes to the <see cref="AudioPlaybackEngine.AllInputEnded"/> event and sets the hold repeat timer interval.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public MainForm()
    {
        // Calling initialization procedure from another part of the class.
        InitializeComponent();

        // Initializing the global WinAPI keyboard hook for processing
        // the user input, so we can subscribe the event handler "KeysChanged"
        // to an event "OnKeysChanged" that we can do some actions on pressing
        // the hotkeys.
        GlobalKeyboardHook.Initialize();
        GlobalKeyboardHook.KeysChanged += OnKeysChanged;

        // Initializing the current working directory to the executable path.
        Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath)
            ?? throw new InvalidOperationException("Unable to determine the application directory. Aborting execution.");

        // Dynamically creating the ToolTip object for displaying
        // tooltips for certain controls
        ToolTip tooltip = new ();

        // Because we have already called the InitializeComponent procedure,
        // these buttons shouldn't be null, but we'll still check just in case
        if (reloadDevicesButton is not null)
            tooltip.SetToolTip(reloadDevicesButton, "Refresh sound devices");
        if (reloadWindowsButton is not null)
            tooltip.SetToolTip(reloadWindowsButton, "Reload windows");

        // Calling procedures to populate controls with relevant information
        // For PlaybackDevicesComboBox and LoopbackDevicesComboBox:
        LoadSoundDevices();
        // For WindowsComboBox:
        LoadWindows();
        // For KeySoundsListView:
        LoadSoundboardSettingsXML();

        // Also checking if the devices haven't changed since last launch: 
        if (playbackDevicesComboBox is not null)
        {
            // We select the item that has been as default playback device last time.
            if (playbackDevicesComboBox.Items.Contains(CurrentSettings.LastPlaybackDevice))
                playbackDevicesComboBox.SelectedItem = CurrentSettings.LastPlaybackDevice;
            // Also adding the event handler for changes in the index of selected item in the combobox.
            playbackDevicesComboBox.SelectedIndexChanged += PlaybackDevicesComboBox_SelectedIndexChanged;
        }
        // Same for loopback.
        if (loopbackDevicesComboBox is not null)
        {
            if (loopbackDevicesComboBox.Items.Contains(CurrentSettings.LastLoopbackDevice))
                loopbackDevicesComboBox.SelectedItem = CurrentSettings.LastLoopbackDevice;
            loopbackDevicesComboBox.SelectedIndexChanged += LoopbackDevicesComboBox_SelectedIndexChanged;
        }

        // After all the settings have been loaded, 
        // we start initializing the engine for audio playback
        InitAudioPlaybackEngine();

        // Also adding the event handler after input has ended
        // for 'Push to talk' functionality
        AudioPlaybackEngine.Instance.AllInputEnded += OnAllInputEnded;

        // Initializing the "Interval" property of "HoldRepeatTimer"
        // so we can set the delay between repeats on hold of hotkeys.
        HoldRepeatTimer?.Interval = CurrentSettings.DelayInMs ?? 50;
    }

    #endregion

    #region Form Event Handlers

    /// Description
    /// <summary>
    ///     Handles the <see cref="Form.FormClosing"/> event to clean up resources.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event (the form itself).</param>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    private void MainForm_FormClosing(Object? sender, FormClosingEventArgs e)
    {
        // Stopping the timer and disposing
        // of the memory allocated to it.
        HoldRepeatTimer?.Stop();
        HoldRepeatTimer?.Dispose();
        // Unsubscribing from the event and
        // calling the "Shutdown" procedure
        // to dispose of the global hook.
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
        GlobalKeyboardHook.Shutdown();
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Form.Resize"/> event to minimize the form to the system tray.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void MainForm_Resize(Object? sender, EventArgs? e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            notificationIcon?.Visible = true;
            this.Hide();
        }
    }

    #endregion

    #region Notification Icon Event Handlers

    /// Description
    /// <summary>
    ///     Handles the <see cref="NotifyIcon.MouseClick"/> event to restore the form from the tray.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    private void NotificationIcon_MouseClick(Object? sender, MouseEventArgs? e)
    {
        notificationIcon?.Visible = false;
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
    }

    #endregion

    #region Audio Engine Events

    /// Description
    /// <summary>
    ///     Handles the <see cref="AudioPlaybackEngine.AllInputEnded"/> event to release the push‑to‑talk key.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void OnAllInputEnded(Object? sender, EventArgs? e)
    {
        if (_keyUpPushToTalkKey)
        {
            _keyUpPushToTalkKey = false;
            KeyboardEmulator.SendKey(_pushToTalkKey, false);
        }
    }

    #endregion

    #region Initialization Helpers

    /// Description
    /// <summary>
    ///     Initializes the audio playback engine with the selected playback device.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     Retrieves the selected index from <see cref="playbackDevicesComboBox"/> and
    ///     calls <see cref="AudioPlaybackEngine.Init"/> to set up the engine.
    ///     If an error occurs, a message box is shown and a system beep is played.
    /// </remarks>
    private void InitAudioPlaybackEngine()
    {
        try
        {
            // If the index of the selected playback device isn't null
            // try to initialize the audio playback engine.
            if (playbackDevicesComboBox?.SelectedIndex is not null)
                AudioPlaybackEngine.Instance.Init(playbackDevicesComboBox.SelectedIndex);

            // Otherwise throw the error that we try to reference a null.
            else throw new NullReferenceException("No audio device has been selected");
        }
        catch (Exception ex)
        {
            // Getting the exception message.
            String exceptionMessage = ex.ToString();

            // If the exception contains error about allocation
            // when calling waveOutOpen, append information
            // specifying the error that has appeared.
            if (exceptionMessage.Contains("AlreadyAllocated calling waveOutOpen"))
                exceptionMessage = $"Failed to open device. Already in exclusive use by another application?\n\n{exceptionMessage}";
            
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            MessageBox.Show(
                caption: "Cannot initialize the audio engine",
                text: $"Initialization of audio engine has failed:\n{exceptionMessage}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    /// Description
    /// <summary>
    ///     Loads the list of running windows and populates <see cref="windowsComboBox"/>.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     The first item is always "[Any window]". Subsequent items are the
    ///     <see cref="Process.MainWindowTitle"/> of each running process.
    ///     If an error occurs, a message box is shown.
    /// </remarks>
    private void LoadWindows()
    {
        try
        {
            // Combobox should be initialized by now,
            // but we check just in case.
            if (windowsComboBox is not null)
            {
                // Repopulate the items of the combobox and 
                // add the value for not specifying a specific
                // window for playing the sounds in, set the
                // currently selected index to it.
                windowsComboBox.Items.Clear();
                windowsComboBox.Items.Add("[Any window]");
                windowsComboBox.SelectedIndex = 0;
                // Add all window titles in one go.
                windowsComboBox.Items.AddRange(Process.GetProcesses().Select(p => p.MainWindowTitle));
            }
            else throw new NullReferenceException("Windows list hasn't been initialized");
        }
        catch (Exception ex)
        {
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            MessageBox.Show(
                caption: "Cannot load currently opened windows",
                text: $"Initialization of windows list has failed: {ex}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    /// Description
    /// <summary>
    ///     Loads available audio playback and loopback devices and populates
    ///     <see cref="playbackDevicesComboBox"/> and <see cref="loopbackDevicesComboBox"/>.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     Uses NAudio's <see cref="WaveOut.GetCapabilities"/> and <see cref="WaveIn.GetCapabilities"/>
    ///     to enumerate devices. The loopback list includes an empty entry at index 0.
    ///     If an error occurs, a message box is shown.
    /// </remarks>
    private void LoadSoundDevices()
    {
        try
        {
            // Comboboxes should be initialized by now, but we check just in case.
            if (playbackDevicesComboBox?.Items is not null 
                && loopbackDevicesComboBox?.Items is not null)
            {
                List<WaveOutCapabilities> playbackSources = [];
                List<WaveInCapabilities> loopbackSources = [];

                // Iterating through audio devices and 
                // adding them to their respective lists.
                for (Int32 i = 0; i < WaveOut.DeviceCount; i++)
                    playbackSources.Add(WaveOut.GetCapabilities(i));

                for (Int32 i = 0; i < WaveIn.DeviceCount; i++)
                    loopbackSources.Add(WaveIn.GetCapabilities(i));
                
                // Clearing the list of items inside the comboboxes.
                playbackDevicesComboBox.Items.Clear();
                loopbackDevicesComboBox.Items.Clear();

                // Adding the playback devices from audio devices capabilities list.
                foreach (WaveOutCapabilities source in playbackSources)
                    playbackDevicesComboBox.Items.Add(source.ProductName);
                
                // Setting the index if there are any items.
                if (playbackDevicesComboBox.Items.Count > 0)
                    playbackDevicesComboBox.SelectedIndex = 0;

                // And adding an empty entry.
                loopbackDevicesComboBox.Items.Add(String.Empty);

                // Doing the same for loopback devices.
                foreach (WaveInCapabilities source in loopbackSources)
                    loopbackDevicesComboBox.Items.Add(source.ProductName);

                loopbackDevicesComboBox.SelectedIndex = 0;
            }
            else throw new NullReferenceException("Lists haven't been initialized");
        }
        catch (Exception ex)
        {
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            MessageBox.Show(
                caption: "Cannot load playback sound devices",
                text: $"Initialization of playback audio devices lists has failed: {ex}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    #endregion

    #region Loopback Control

    /// Description
    /// <summary>
    ///     Starts loopback audio capture and playback.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     Creates a <see cref="WaveIn"/> stream on the selected loopback device,
    ///     wraps it with a <see cref="BufferedWaveProvider"/>, and plays the captured
    ///     audio on the output device selected in <see cref="playbackDevicesComboBox"/>.
    ///     If an error occurs, a message box is shown.
    /// </remarks>
    private void StartLoopback()
    {
        try
        {
            // Stopping loopback if it is used right now.
            StopLoopback();
            // We are checking if the combobox has been initialized just in case,
            // the controls should be initialized by now.
            if (loopbackDevicesComboBox is not null)
            {
                Int32 deviceNumber = loopbackDevicesComboBox.SelectedIndex - 1;

                // Setting the parameters of the loopback stream.
                _loopbackSourceStream ??= new ();
                _loopbackSourceStream.DeviceNumber = deviceNumber;
                _loopbackSourceStream.WaveFormat = new (44100, WaveIn.GetCapabilities(deviceNumber).Channels);
                _loopbackSourceStream.BufferMilliseconds = 25;
                _loopbackSourceStream.NumberOfBuffers = 5;
                _loopbackSourceStream.DataAvailable += LoopbackSourceStream_DataAvailable;
                
                // Setting the parameters of the provider.
                _loopbackWaveProvider = new (_loopbackSourceStream.WaveFormat)
                {
                    DiscardOnBufferOverflow = true
                };
                
                // Setting the parameters of the output.
                _loopbackWaveOut ??= new ();
                _loopbackWaveOut.DeviceNumber = loopbackDevicesComboBox.SelectedIndex;
                _loopbackWaveOut.DesiredLatency = 125;
                // Initialize output based on the provider.
                _loopbackWaveOut.Init(_loopbackWaveProvider);
                // Record what is gonna be looped back.
                _loopbackSourceStream.StartRecording();
                // Play it out on the output.
                _loopbackWaveOut.Play();
            }
            else throw new NullReferenceException("Loopback devices list hasn't been initialized");
        }
        catch (Exception ex)
        {
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            MessageBox.Show(
                caption: "Cannot start loopback",
                text: $"Initialization of loopback devices has failed: {ex}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    /// Description
    /// <summary>
    ///     Stops loopback audio capture and playback, and releases resources.
    /// </summary>
    private void StopLoopback()
    {
        try
        {
            // Clearing resources.
            _loopbackWaveOut?.Stop();
            _loopbackWaveOut?.Dispose();
            _loopbackWaveProvider?.ClearBuffer();
            _loopbackSourceStream?.StopRecording();
            _loopbackSourceStream?.Dispose();
            // Setting the values of objects to null reference.
            _loopbackWaveOut = null;
            _loopbackWaveProvider = null;
            _loopbackSourceStream = null;
        }
        catch (Exception ex) 
        {
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            MessageBox.Show(
                caption: "Cannot stop loopback",
                text: $"Disposing of the loopback has failed: {ex}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    /// Description
    /// <summary>
    ///     Event handler for the <see cref="WaveIn.DataAvailable"/> event.
    ///     Adds received audio data to the <see cref="BufferedWaveProvider"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event (the <see cref="WaveIn"/> stream).</param>
    /// <param name="e">A <see cref="WaveInEventArgs"/> containing the audio buffer.</param>
    private void LoopbackSourceStream_DataAvailable(Object? sender, WaveInEventArgs? e)
    {
        if (_loopbackWaveProvider is not null && _loopbackWaveProvider.BufferedDuration.TotalMilliseconds <= 100)
            _loopbackWaveProvider.AddSamples(e?.Buffer, 0, e?.BytesRecorded ?? 0);
    }

    #endregion

    #region Static Playback Helpers

    /// Description
    /// <summary>
    ///     Stops all currently playing sounds via the <see cref="AudioPlaybackEngine"/>.
    /// </summary>
    private static void StopPlayback() => AudioPlaybackEngine.Instance.StopAllSounds();

    /// Description
    /// <summary>
    ///     Plays a single sound file using the <see cref="AudioPlaybackEngine"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="file">Full path to the sound file to play.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     If an error occurs, a system beep is played and a message box with details is shown.
    /// </remarks>
    private static void PlaySound(String file)
    {
        try
        {
            // Calling a function from the instance of audio engine
            // to play a specified sound from a file.
            AudioPlaybackEngine.Instance.PlaySound(file);
        }
        catch (Exception ex)
        {
            // Play a sound on the device and show a message box
            // containing the current error.
            SystemSounds.Beep.Play();
            String exceptionMessage = ex.ToString();
            
            // If the exception contains an unspecified error
            // when calling waveOutOpen, append additional info.
            if (exceptionMessage.Contains("UnspecifiedError calling waveOutOpen"))
                exceptionMessage = $"Something is wrong with either your sound card driver or the sound you tried to play: \"{Path.GetFileName(file)}\", try converting it to another format.\n\n{exceptionMessage}";

            MessageBox.Show(
                caption: "Cannot play sound",
                text: $"Playing a sound has failed: {exceptionMessage}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    #endregion

    #region XML File Loading

    /// Description
    /// <summary>
    ///     Loads an XML settings file and populates the soundboard with its contents.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="path">Full path to the XML file.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     The XML file is deserialized into a <see cref="Settings"/> object.
    ///     Entries with missing keys, empty sound locations, or missing files are reported
    ///     as errors. Duplicate key combinations are also reported. If the file is valid,
    ///     the <see cref="SoundHotkeys"/> list and <see cref="KeySoundsListView"/> are updated.
    /// </remarks>
    private void LoadXMLFile(String path)
    {
        if (ReadXML(typeof(Settings), path) is Settings settings
            && settings.SoundHotkeys is not null
            && settings.SoundHotkeys.Count > 0)
        {
            List<ListViewItem> items = [];
            String errors = String.Empty;
            String sameKeys = String.Empty;

            foreach (SoundHotkey hotkey in settings.SoundHotkeys)
            {
                // Getting the count of items inside lists.
                Int32 keysCount = hotkey.Keys?.Count ?? 0;
                Int32 slCount = hotkey.SoundLocations.Count;
                // Checking if there are any entries and their files exist.
                Boolean keysNull = keysCount > 0 && (!hotkey.Keys?.Any(x => x != 0) ?? true);
                Boolean soundsNotEmpty = hotkey.SoundLocations.All(x => !String.IsNullOrWhiteSpace(x));
                Boolean filesExist = hotkey.SoundLocations.All(x => File.Exists(x));
                // We suppress nullability warning because the executable path is never null.
                Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;

                // If there are any errors in the XML, we build the error message,
                // but we don't throw an error so that the user can fix the errors 
                // from within the app.
                if (keysNull || slCount < 1 || !soundsNotEmpty || !filesExist)
                {
                    String tempErr = String.Empty;
                    if (keysCount == 0 && (slCount == 0 || !soundsNotEmpty)) tempErr = "entry is empty";
                    else if (!keysNull) tempErr = "one or more keys are null";
                    else if (slCount == 0) tempErr = "no sounds provided";
                    else if (!filesExist) tempErr = "one or more sounds do not exist";

                    errors += $"Entry #{settings.SoundHotkeys.IndexOf(hotkey)} has an error: {tempErr}\r\n";
                }

                // Trying to get the keys for the entry if there are any.
                String keys = Conversions.KeysToString(hotkey.Keys ?? []);

                // Checking for repeating keys and building the error message.
                if (!String.IsNullOrEmpty(keys) && items.Count > 0 && items[^1].Text == keys && !sameKeys.Contains(keys))
                    sameKeys += (sameKeys != String.Empty ? ", " : String.Empty) + keys;

                // Adding a new item for a ListView.
                ListViewItem tempItem = new (keys);
                tempItem.SubItems.Add(hotkey.WindowTitle);
                tempItem.SubItems.Add(Conversions.SoundLocsArrayToString(hotkey.SoundLocations));
                
                // Then appending to the end of the list of items.
                items.Add(tempItem);
            }

            // If there are any items, we add them to the ListView.
            if (items.Count > 0)
            {
                // Also if there were any errors, we show them to the user.
                if (!String.IsNullOrEmpty(errors))
                    MessageBox.Show(errors);

                if (!String.IsNullOrEmpty(sameKeys))
                    MessageBox.Show("Multiple entries using the same keys. The keys being used multiple times are: " + sameKeys);

                // Clearing the lists and adding the new values.
                SoundHotkeys.Clear();
                SoundHotkeys.AddRange(settings.SoundHotkeys);
                KeySoundsListView?.Items.Clear();
                KeySoundsListView?.Items.AddRange([.. items]);

                // We set the width of the column headers to this specific number
                // so we autosize to the width of the heading, according to .NET docs:
                // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.columnheader.width
                KeysColumnHeader?.Width = -2;
                SoundLocationColumnHeader?.Width = -2;

                // Setting the field to current XML file path for saving the changes.
                XMLLocation = path;
            }
            else
            {
                // Play a sound on the device and show a message box
                // containing the current error.
                SystemSounds.Beep.Play();
                MessageBox.Show("No entries found, or all entries had errors in them (key being 'None', sound location behind empty or non-existant)");
            }
        }
        else
        {
            SystemSounds.Beep.Play();
            MessageBox.Show("No entries found, or there was an error reading the settings file");
        }
    }

    #endregion

    #region Hotkey Editing Helpers

    /// Description
    /// <summary>
    ///     Opens the <see cref="AddEditHotkeyForm"/> to edit the selected sound hotkey.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     The method retrieves the selected item from <see cref="KeySoundsListView"/>,
    ///     creates a <see cref="SoundHotkeyEditData"/> record, and passes it to the
    ///     edit form. If no item is selected, nothing happens.
    /// </remarks>
    private void EditSelectedSoundHotkey()
    {
        // If there are any selected items in the ListView,
        // we create the edit form and pass the parameters
        // of the contents and index of selected item.
        if (KeySoundsListView?.SelectedItems.Count > 0)
        {
            // Get the first available item
            ListViewItem item = KeySoundsListView.SelectedItems[0];

            // Build the record with values from ListViewItem
            SoundHotkeyEditData editData = new (
                Keys: item.Text,
                WindowTitle: item.SubItems[1].Text,
                SoundLocation: item.SubItems[2].Text
            );

            // Create a new instance of the form and run dialog.
            new AddEditHotkeyForm
            {
                // Get its contents and index and write their values
                // to the fields of what we are editing.
                EditData = editData,
                EditIndex = KeySoundsListView.SelectedIndices[0]
            }.ShowDialog();
        }
    }

    #endregion

    #region UI Event Handlers (Menu Items and Buttons)

    /// Description
    /// <summary>
    ///     Handles the <see cref="ToolStripMenuItem.Click"/> event of the settings menu item.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void SettingsToolStripMenuItem_Click(Object? sender, EventArgs? e) => new SettingsForm().ShowDialog();

    /// Description
    /// <summary>
    ///     Handles the <see cref="ToolStripMenuItem.Click"/> event of the TTS menu item.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void TTSToolStripMenuItem_Click(Object? sender, EventArgs? e) => new TextToSpeechForm().ShowDialog();

    /// Description
    /// <summary>
    ///     Handles the <see cref="ToolStripMenuItem.Click"/> event of the update menu item.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Opens the GitHub releases page in the default web browser.
    /// </remarks>
    private void UpdateToolStripMenuItem_Click(Object? sender, EventArgs? e)
    {
        using Process process = new ();
        process.StartInfo.FileName = "https://github.com/Hollaptus/HASCore/releases";
        process.StartInfo.UseShellExecute = true;
        process.Start();
    }

    #endregion

    #region Control Event Handlers (Buttons, CheckBoxes, etc.)

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="addButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void AddButton_Click(Object? sender, EventArgs? e) => new AddEditHotkeyForm().ShowDialog();

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="editButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void EditButton_Click(Object? sender, EventArgs? e) => EditSelectedSoundHotkey();

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="removeButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     After confirmation, removes the selected hotkey from both the
    ///     <see cref="SoundHotkeys"/> list and the <see cref="KeySoundsListView"/>.
    /// </remarks>
    private void RemoveButton_Click(Object? sender, EventArgs? e)
    {
        if (KeySoundsListView?.SelectedItems.Count > 0 && MessageBox.Show(
            caption: "Removing an item", 
            text: "Are you sure remove that item?", 
            buttons: MessageBoxButtons.YesNo,
            icon: MessageBoxIcon.Question
        ) == DialogResult.Yes)
        {
            SoundHotkeys.RemoveAt(KeySoundsListView.SelectedIndices[0]);
            KeySoundsListView.Items.Remove(KeySoundsListView.SelectedItems[0]);

            if (KeySoundsListView.Items.Count == 0) enableCheckBox?.Checked = false;
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="clearButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Clears all hotkeys and disables the soundboard.
    /// </remarks>
    private void ClearButton_Click(Object? sender, EventArgs? e)
    {
        if (MessageBox.Show(
            caption: "Clearing all items", 
            text: "Are you sure you want to clear all items?", 
            buttons: MessageBoxButtons.YesNo,
            icon: MessageBoxIcon.Question
        ) == DialogResult.Yes)
        {
            SoundHotkeys.Clear();
            KeySoundsListView?.Items.Clear();
            enableCheckBox?.Checked = false;
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="playSelectedSoundButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void PlaySelectedSoundButton_Click(Object? sender, EventArgs? e)
    {
        if (KeySoundsListView?.SelectedItems.Count > 0)
            PlayKeySound(SoundHotkeys[KeySoundsListView.SelectedIndices[0]]);
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="stopAllSoundsButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void StopAllSoundsButton_Click(Object? sender, EventArgs? e) => StopPlayback();

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="loadButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Opens a file dialog for selecting an XML file, then calls <see cref="LoadXMLFile"/>.
    /// </remarks>
    private void LoadButton_Click(Object? sender, EventArgs? e)
    {
        // Creating a new instance of user dialog.
        OpenFileDialog diag = new () { Filter = "XML file containing keys and sounds|*.xml" };
        if (diag.ShowDialog() == DialogResult.OK)
            LoadXMLFile(diag.FileName);
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="saveButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Saves the current hotkey list to the already‑loaded XML file.
    ///     If no file is loaded, prompts the user for a location.
    /// </remarks>
    private void SaveButton_Click(Object? sender, EventArgs? e)
    {
        if (!File.Exists(XMLLocation))
            XMLLocation = Files.UserGetXMLLocation();

        if (!String.IsNullOrEmpty(XMLLocation))
        {
            WriteXML(new Settings(SoundHotkeys), XMLLocation);
            MessageBox.Show($"Saved as:\n{XMLLocation}");
        }
        else 
        {
            // Show a message box containing the current error.
            MessageBox.Show("Location was empty, file hasn't been saved!");
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="saveAsButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Prompts the user for a new save location and saves the current hotkey list.
    /// </remarks>
    private void SaveAsButton_Click(Object? sender, EventArgs? e)
    {
        String lastLocation = XMLLocation;
        XMLLocation = Files.UserGetXMLLocation();
        if (String.IsNullOrEmpty(XMLLocation) && !String.IsNullOrEmpty(lastLocation))
            XMLLocation = lastLocation;

        if (!String.IsNullOrEmpty(XMLLocation))
        {
            WriteXML(new Settings(SoundHotkeys), XMLLocation);
            MessageBox.Show($"Saved as:\n{XMLLocation}");
        }
        else 
        {
            // Show a message box containing the current error.
            MessageBox.Show("Location was empty, file hasn't been saved!");
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="reloadDevicesButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void ReloadDevicesButton_Click(Object? sender, EventArgs? e)
    {
        StopPlayback();
        StopLoopback();
        LoadSoundDevices();
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="CheckBox.CheckedChanged"/> event of <see cref="enableCheckBox"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Enables or disables the soundboard. When enabled, starts loopback if devices are available;
    ///     when disabled, stops all sounds and loopback.
    /// </remarks>
    private void EnableCheckBox_CheckedChanged(Object? sender, EventArgs? e)
    {
        if (enableCheckBox?.Checked == true)
        {
            if (enableCheckBox.Checked && playbackDevicesComboBox?.Items.Count > 0 && loopbackDevicesComboBox?.SelectedIndex > 0)
                StartLoopback();
        }
        else
        {
            StopPlayback();
            StopLoopback();
            HoldRepeatTimer?.Stop();
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Control.MouseDoubleClick"/> event of <see cref="KeySoundsListView"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    private void KeySoundsListView_MouseDoubleClick(Object? sender, MouseEventArgs? e) => EditSelectedSoundHotkey();

    /// Description
    /// <summary>
    ///     Handles the <see cref="CheckBox.CheckedChanged"/> event of <see cref="enablePushToTalkCheckBox"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Validates that a push‑to‑talk key has been assigned. If not, unchecks the checkbox.
    /// </remarks>
    private void EnablePushToTalkCheckBox_CheckedChanged(Object? sender, EventArgs? e)
    {
        if (enablePushToTalkCheckBox?.Checked == true)
        {
            if (String.IsNullOrEmpty(pushToTalkKeyTextBox?.Text))
            {
                enablePushToTalkCheckBox.Checked = false;
                MessageBox.Show("There is no push to talk key entered");
                return;
            }
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="reloadWindowsButton"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    private void ReloadWindowsButton_Click(Object? sender, EventArgs? e) => LoadWindows();

    #endregion

    #region ComboBox Events

    /// Description
    /// <summary>
    ///     Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of <see cref="loopbackDevicesComboBox"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Updates the loopback device selection and restarts loopback if the soundboard is enabled.
    ///     Saves the selected device to <see cref="CurrentSettings"/>.
    /// </remarks>
    private void LoopbackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
    {
        if (loopbackDevicesComboBox?.SelectedIndex > 0)
        {
            if (enableCheckBox?.Checked == true)
            {
                if (String.IsNullOrEmpty(loopbackDevicesComboBox.SelectedItem?.ToString())) StopLoopback();
                else StartLoopback();
            }
            else
                StopLoopback();
        }

        CurrentSettings.LastLoopbackDevice = loopbackDevicesComboBox?.SelectedItem as String;
        SaveSoundboardSettingsXML();
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of <see cref="playbackDevicesComboBox"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Restarts loopback, stops all sounds, reinitializes the audio engine,
    ///     and saves the selected device to <see cref="CurrentSettings"/>.
    /// </remarks>
    private void PlaybackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
    {
        if (_loopbackWaveOut != null && _loopbackSourceStream != null && enableCheckBox?.Checked == true)
            StartLoopback();

        StopPlayback();
        InitAudioPlaybackEngine();

        CurrentSettings.LastPlaybackDevice = playbackDevicesComboBox?.SelectedItem as String;
        SaveSoundboardSettingsXML();
    }

    #endregion

    #region Keyboard Hook Handling

    /// Description
    /// <summary>
    ///     Handles the <see cref="GlobalKeyboardHook.KeysChanged"/> event.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event (the global hook).</param>
    /// <param name="currentKeys">A <see cref="HashSet{Keys}"/> containing all currently pressed keys.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     This method processes global keyboard input and performs the following actions:
    ///     <list type="bullet">
    ///         <item>Captures the push‑to‑talk key when the corresponding text box is focused.</item>
    ///         <item>Toggles the soundboard enable state using the keys defined in <see cref="CurrentSettings.EnableSoundboardKeys"/>.</item>
    ///         <item>Stops all sounds using the keys defined in <see cref="CurrentSettings.StopSoundKeys"/>.</item>
    ///         <item>Matches pressed keys against loaded hotkeys and plays the corresponding sound(s).</item>
    ///         <item>Handles repeat‑on‑hold behavior.</item>
    ///         <item>Loads an XML preset if the pressed keys match a <see cref="LoadXMLFile"/> entry.</item>
    ///     </list>
    /// </remarks>
    private void OnKeysChanged(Object? sender, HashSet<Keys> currentKeys)
    {
        // PTT key capture.
        this.Invoke((MethodInvoker)(() =>
        {
            if (pushToTalkKeyTextBox?.Focused == true)
            {
                // Cancel with Escape
                if (currentKeys.Count == 1 && currentKeys.Contains(Keys.Escape))
                {
                    pushToTalkKeyTextBox.Text = String.Empty;
                    _pushToTalkKey = Keys.None;
                    // Move focus away from the textbox, capture is done.
                    this.Focus();
                    // Early exit from the Invoke lambda.
                    return;
                }

                // Only assign if exactly one key is pressed.
                if (currentKeys.Count == 1)
                {
                    Keys pressedKey = currentKeys.First();
                    pushToTalkKeyTextBox.Text = Conversions.KeysToString(pressedKey);
                    _pushToTalkKey = pressedKey;
                    this.Focus();
                    return;
                }
                // If multiple keys or no keys, do nothing and return.
                return;
            }
        }));

        if (CurrentSettings.EnableSoundboardKeys?.Count > 0 &&
            currentKeys.SetEquals(CurrentSettings.EnableSoundboardKeys))
        {
            this.Invoke((MethodInvoker)(() => enableCheckBox?.Checked ^= true));
            return;
        }

        Boolean isEnabled = false;
        this.Invoke((MethodInvoker)(() => isEnabled = enableCheckBox?.Checked == true));
        if (!isEnabled) return;

        if (currentKeys.Count == 0)
        {
            _lastProcessedKeys = null;
            return;
        }

        if (HoldRepeatTimer != null && HoldRepeatTimer.Enabled && _holdKeys != null)
        {
            if (!_holdKeys.All(currentKeys.Contains))
            {
                HoldRepeatTimer.Stop();
                _holdKeys = null;
                _currentHoldHotkey = null;
            }
        }

        if (_lastProcessedKeys != null && _lastProcessedKeys.SetEquals(currentKeys))
            return;

        _lastProcessedKeys = [.. currentKeys];

        Boolean pttEnabled = false;
        Keys pttKey = Keys.None;
        Int32 windowsSelectedIndex = 0;
        String windowsSelectedItem = String.Empty;

        this.Invoke((MethodInvoker)(() =>
        {
            pttEnabled = enablePushToTalkCheckBox?.Checked == true;
            pttKey = _pushToTalkKey;
            windowsSelectedIndex = windowsComboBox?.SelectedIndex ?? 0;
            windowsSelectedItem = windowsComboBox?.SelectedItem as String ?? String.Empty;
        }));

        if (SoundHotkeys.Count > 0)
        {
            IntPtr foregroundWindow = WindowInterop.GetForegroundWindow();

            foreach (SoundHotkey hotkey in SoundHotkeys)
            {
                if (hotkey.Keys?.Count == 0
                    || (hotkey.WindowTitle != String.Empty
                    && !WindowInterop.IsForegroundWindow(hotkey.WindowTitle, foregroundWindow)))
                    continue;

                if (currentKeys.Count > 0 && currentKeys.Count == hotkey.Keys?.Count)
                {
                    if (currentKeys.Except(hotkey.Keys).Any()) continue;

                    if (hotkey.Keys.All(x => x != 0) && hotkey.SoundLocations.Any(x => File.Exists(x)))
                    {
                        if (pttEnabled
                            && !_keyUpPushToTalkKey
                            && !currentKeys.Contains(pttKey)
                            && (windowsSelectedIndex == 0
                            || WindowInterop.IsForegroundWindow(windowsSelectedItem)))
                        {
                            _keyUpPushToTalkKey = true;
                            KeyboardEmulator.SendKey(pttKey, true);
                            Thread.Sleep(100);
                        }

                        if (CurrentSettings.PlayOverEachother == false)
                            StopPlayback();
                        PlayKeySound(hotkey);

                        if (CurrentSettings.RepeatOnHold == true)
                        {
                            HoldRepeatTimer?.Stop();

                            _holdKeys = [.. hotkey.Keys ?? []];
                            _currentHoldHotkey = hotkey;

                            HoldRepeatTimer?.Interval = CurrentSettings.DelayInMs ?? 50;

                            HoldRepeatTimer?.Start();
                        }

                        return;
                    }
                }
            }
        }

        if (CurrentSettings.StopSoundKeys?.Count > 0
            && currentKeys.SetEquals(CurrentSettings.StopSoundKeys))
        {
            StopPlayback();
            return;
        }

        if (CurrentSettings.LoadXMLFiles?.Count > 0)
        {
            foreach (LoadXMLFile file in CurrentSettings.LoadXMLFiles)
            {
                if (file.Keys?.Count == 0 || file.Keys is null) continue;
                else if (currentKeys.SetEquals(file.Keys))
                {
                    if (File.Exists(file.XMLLocation))
                        LoadXMLFile(file.XMLLocation);
                    return;
                }
            }
        }

        if (_keyUpPushToTalkKey)
        {
            if (!currentKeys.Contains(pttKey)) _keyUpPushToTalkKey = false;

            if (windowsSelectedIndex != 0 && !WindowInterop.IsForegroundWindow(windowsSelectedItem))
            {
                _keyUpPushToTalkKey = false;
                KeyboardEmulator.SendKey(pttKey, false);
            }
        }
    }

    #endregion

    #region Hold Repeat Timer

    /// Description
    /// <summary>
    ///     Handles the <see cref="Timer.Tick"/> event of <see cref="HoldRepeatTimer"/>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Repeatedly plays the currently held hotkey's sound as long as the keys are still pressed.
    ///     The repeat interval is taken from <see cref="CurrentSettings.DelayInMs"/>.
    /// </remarks>
    private void HoldRepeatTimer_Tick(Object? sender, EventArgs e)
    {
        // If there is no active combination - we just exit.
        if (_holdKeys == null || _currentHoldHotkey == null)
        {
            HoldRepeatTimer?.Stop();
            return;
        }

        // Checking if the keys for the sound are still pressed.
        Boolean allPressed = _holdKeys.All(GlobalKeyboardHook.IsKeyDown);
        if (!allPressed)
        {
            // Keys down - stop repeating sounds.
            HoldRepeatTimer?.Stop();
            _holdKeys = null;
            _currentHoldHotkey = null;
            return;
        }

        // If keys are still pressed - then we check how we should 
        // approach playing them again.

        // If the setting for playing sounds over eachother is disabled
        // then we first stop previous playback.
        if (CurrentSettings.PlayOverEachother == false)
            StopPlayback();

        PlayKeySound(_currentHoldHotkey);
    }

    #endregion

    #region Sound Playback Helpers

    /// Description
    /// <summary>
    ///     Plays a sound associated with the given hotkey, selecting a random file if multiple are available.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="currentKeysSounds">The <see cref="SoundHotkey"/> containing the sound locations.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     If multiple sound files are listed, a random one is chosen (avoiding the last played file).
    ///     If the file does not exist, a system beep is played and an error message is shown once.
    /// </remarks>
    private void PlayKeySound(SoundHotkey currentKeysSounds)
    {
        String path = String.Empty;

        if (currentKeysSounds.SoundLocations.Count > 1)
        {
            Int32 temp;
            while (true)
            {
                temp = _random.Next(0, currentKeysSounds.SoundLocations.Count);
                if (temp != _tempLastIndex && File.Exists(currentKeysSounds.SoundLocations[temp])) break;
                Thread.Sleep(1);
            }
            _tempLastIndex = temp;
            path = currentKeysSounds.SoundLocations[_tempLastIndex];
        }
        else if (currentKeysSounds.SoundLocations.Count == 1)
            path = currentKeysSounds.SoundLocations.First();

        if (File.Exists(path))
        {
            PlaySound(path);
        }
        else if (!_showMsgBox)
        {
            SystemSounds.Beep.Play();
            _showMsgBox = true;
            MessageBox.Show("File " + path + " does not exist");
            _showMsgBox = false;
        }
    }

    #endregion
}