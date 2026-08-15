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
///     <see cref="MainForm"/> class part for initializing the Object and its event handlers.
/// </summary>
public partial class MainForm : Form
{
    /// <summary>
    /// Temporary variable for playing random sounds.
    /// </summary>
    private Int32 _tempLastIndex = -1;
    /// <summary>
    /// Randomizer for getting the index so we could play sounds when
    /// there are multiple sound files listed on the same hotkey.
    /// </summary>
    private readonly Random _random = new ();
    /// <summary>
    /// A provider for the buffer of stored sounds in memory.
    /// </summary>
    private BufferedWaveProvider? _loopbackWaveProvider = null;
    /// <summary>
    /// A stream to which we should write our wave input for loopback.
    /// </summary>
    private WaveIn? _loopbackSourceStream = null;
    /// <summary>
    /// Device that we should output to from our stream and provider for loopback.
    /// </summary>
    private WaveOut? _loopbackWaveOut = null;
    /// <summary>
    /// A key that is used for the Push-To-Talk functionality.
    /// </summary>
    private Keys _pushToTalkKey;
    /// <summary>
    /// Variable for checking if the Push-To-Talk key is currently up.
    /// </summary>
    private Boolean _keyUpPushToTalkKey = false;
    /// <summary>
    /// Variable for checking if we should display a message box.
    /// Used when there is currently a message box active so we 
    /// don't spam the user with them.
    /// </summary>
    private Boolean _showMsgBox = false;
    /// <summary>
    /// For tracking the last processed key combination.
    /// </summary>
    private HashSet<Keys>? _lastProcessedKeys = null; 
    /// <summary>
    /// Combination keys that are being held.
    /// </summary>
    private HashSet<Keys>? _holdKeys;
    /// <summary>
    /// Respective hotkey that is currently being held.
    /// </summary>
    private SoundHotkey? _currentHoldHotkey;

    /// <summary>
    /// Location of the current XML file with settings that is loaded.
    /// </summary>
    internal String XMLLocation = String.Empty;
    /// <summary>
    /// List of sounds and their respective hotkeys assocciated with them.
    /// </summary>
    internal List<SoundHotkey> SoundHotkeys = [];

    /// Description
    /// <summary>
    ///     <see cref="MainForm"/> constructor for initialization of class properties.
    /// </summary>
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
        Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;

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

    private void MainForm_FormClosing(Object? sender, FormClosingEventArgs e)
    {
        HoldRepeatTimer?.Stop();
        HoldRepeatTimer?.Dispose();
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
        GlobalKeyboardHook.Shutdown();
    }

    private void OnAllInputEnded(Object? sender, EventArgs? e)
    {
        if (_keyUpPushToTalkKey)
        {
            _keyUpPushToTalkKey = false;
            KeyboardEmulator.SendKey(_pushToTalkKey, false);
        }
    }

    private void InitAudioPlaybackEngine()
    {
        try
        {
            if (playbackDevicesComboBox?.SelectedIndex is not null)
                AudioPlaybackEngine.Instance.Init(playbackDevicesComboBox.SelectedIndex);
            else throw new NullReferenceException("No audio device has been selected");
        }
        catch (Exception ex)
        {
            SystemSounds.Beep.Play();
            String msg = ex.ToString();
            if (msg.Contains("AlreadyAllocated calling waveOutOpen")) {
                msg = "Failed to open device. Already in exclusive use by another application? \n\n" + msg;
            }
            MessageBox.Show($"Initialization of audio engine has failed: {msg}");
        }
    }

    private void LoadWindows()
    {
        try
        {
            // We are checking if the combobox has been initialized just in case,
            // the controls should be initialized by now.
            if (windowsComboBox is not null)
            {
                // Clearing the items and adding the 'Any window' option.
                windowsComboBox.Items.Clear();
                windowsComboBox.Items.Add("[Any window]");
                windowsComboBox.SelectedIndex = 0;

                // Getting all the processes currently running and adding to the list.
                foreach (Process process in Process.GetProcesses())
                    if (!String.IsNullOrEmpty(process.MainWindowTitle))
                        windowsComboBox.Items.Add(process.MainWindowTitle);
            }
            else throw new NullReferenceException("Windows list hasn't been initialized");
        }
        catch (Exception ex)
        {
            SystemSounds.Beep.Play();
            String msg = ex.ToString();
            MessageBox.Show($"Initialization of windows list has failed: {msg}");
        }
    }

    private void LoadSoundDevices()
    {
        try
        {
            // We are checking if the comboboxes have been initialized just in case,
            // the controls should be initialized by now.
            if (playbackDevicesComboBox?.Items is not null && loopbackDevicesComboBox?.Items is not null)
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
            SystemSounds.Beep.Play();
            String msg = ex.ToString();
            MessageBox.Show($"Initialization of audio devices lists has failed: {msg}");
        }
    }

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
                _loopbackSourceStream ??= new WaveIn();
                _loopbackSourceStream.DeviceNumber = deviceNumber;
                _loopbackSourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);
                _loopbackSourceStream.BufferMilliseconds = 25;
                _loopbackSourceStream.NumberOfBuffers = 5;
                _loopbackSourceStream.DataAvailable += LoopbackSourceStream_DataAvailable;
                // Setting the parameters of the provider.
                _loopbackWaveProvider = new BufferedWaveProvider(_loopbackSourceStream.WaveFormat)
                {
                    DiscardOnBufferOverflow = true
                };
                // Setting the parameters of the output.
                _loopbackWaveOut ??= new WaveOut();
                _loopbackWaveOut.DeviceNumber = loopbackDevicesComboBox.SelectedIndex;
                _loopbackWaveOut.DesiredLatency = 125;
                // Initialize output based on the provider.
                _loopbackWaveOut.Init(_loopbackWaveProvider);
                // Record what is gonna be looped backed.
                _loopbackSourceStream.StartRecording();
                // Play it out on the output.
                _loopbackWaveOut.Play();
            }
            else throw new NullReferenceException("Loopback devices list hasn't been initialized");
        }
        catch (Exception ex)
        {
            SystemSounds.Beep.Play();
            String msg = ex.ToString();
            MessageBox.Show($"Initialization of loopback devices has failed: {msg}");
        }
    }

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
        catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }

    private static void StopPlayback() => AudioPlaybackEngine.Instance.StopAllSounds();
    
    private static void PlaySound(String file)
    {
        try
        {
            AudioPlaybackEngine.Instance.PlaySound(file);
        }
        catch (Exception ex)
        {
            SystemSounds.Beep.Play();
            String msg = ex.ToString().Contains("UnspecifiedError calling waveOutOpen")
                ? $"Something is wrong with either the sound you tried to play ({file[(file.LastIndexOf('\\') + 1)..]}) (try converting it to another format) or your sound card driver\n\n{ex}"
                : ex.ToString();
            MessageBox.Show(msg);
        }
    }

    private void LoadXMLFile(String path)
    {
        // We try to read the settings in the specified path, if the file
        // is an XML file that can be serialized as Settings class and its
        // contents are not empty, we parse the entries and load them.
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

    private void LoopbackSourceStream_DataAvailable(Object? sender, WaveInEventArgs? e)
    { 
        if (_loopbackWaveProvider != null && _loopbackWaveProvider.BufferedDuration.TotalMilliseconds <= 100)
            _loopbackWaveProvider.AddSamples(e?.Buffer, 0, e?.BytesRecorded ?? 0);
    }

    private void SettingsToolStripMenuItem_Click(Object? sender, EventArgs? e) => new SettingsForm().ShowDialog();

    private void TTSToolStripMenuItem_Click(Object? sender, EventArgs? e) => new TextToSpeechForm().ShowDialog();

    private void UpdateToolStripMenuItem_Click(Object? sender, EventArgs? e)
    {
        using Process process = new ();
        process.StartInfo.FileName = "https://github.com/Hollaptus/HASCore/releases";
        process.StartInfo.UseShellExecute = true;
        process.Start();
    }

    private void AddButton_Click(Object? sender, EventArgs? e) => new AddEditHotkeyForm().ShowDialog();

    private void EditButton_Click(Object? sender, EventArgs? e) => EditSelectedSoundHotkey();

    private void RemoveButton_Click(Object? sender, EventArgs? e)
    {
        if (KeySoundsListView?.SelectedItems.Count > 0 
        && MessageBox.Show("Are you sure remove that item?", "Remove", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            SoundHotkeys.RemoveAt(KeySoundsListView.SelectedIndices[0]);
            KeySoundsListView.Items.Remove(KeySoundsListView.SelectedItems[0]);

            if (KeySoundsListView.Items.Count == 0) enableCheckBox?.Checked = false;
        }
    }

    private void ClearButton_Click(Object? sender, EventArgs? e)
    {
        if (MessageBox.Show("Are you sure you want to clear all items?", "Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            SoundHotkeys.Clear();
            KeySoundsListView?.Items.Clear();

            enableCheckBox?.Checked = false;
        }
    }

    private void PlaySelectedSoundButton_Click(Object? sender, EventArgs? e)
    {
        if (KeySoundsListView?.SelectedItems.Count > 0)
            PlayKeySound(SoundHotkeys[KeySoundsListView.SelectedIndices[0]]);
    }

    private void StopAllSoundsButton_Click(Object? sender, EventArgs? e) => StopPlayback();
    
    private void LoadButton_Click(Object? sender, EventArgs? e)
    {
        // Creating a new instance of user dialog.
        OpenFileDialog diag = new () { Filter = "XML file containing keys and sounds|*.xml" };
        // If the result is OK, trying to load the file 
        // that has been specified in the dialog as XML settings
        if (diag.ShowDialog() == DialogResult.OK)
            LoadXMLFile(diag.FileName);
    }

    private void SaveButton_Click(Object? sender, EventArgs? e)
    {
        // We check if there is a saved XML file location, if there is,
        // we skip this check. Otherwise, we ask the user the location
        // where we shoud save this XML file.
        if (!File.Exists(XMLLocation))
            XMLLocation = Files.UserGetXMLLocation();

        // If the location is not empty - we write the XML file to that location.
        if (!String.IsNullOrEmpty(XMLLocation))
        {
            WriteXML(new Settings(SoundHotkeys), XMLLocation);
            MessageBox.Show($"Saved as:\n{XMLLocation}");
        }
        // Otherwise, we show a message that file hasn't been saved
        else MessageBox.Show("Location was empty, file hasn't been saved!");
    }

    private void SaveAsButton_Click(Object? sender, EventArgs? e)
    {
        // Saving the previous location for reference.
        String lastLocation = XMLLocation;
        // Asking the user to tell us where to save the file.
        XMLLocation = Files.UserGetXMLLocation();
        // If the location was empty and we have a previous one - save to that one.
        if (String.IsNullOrEmpty(XMLLocation) && !String.IsNullOrEmpty(lastLocation))
            XMLLocation = lastLocation;
        
        // If the location is not empty - we write the XML file to that location.
        if (!String.IsNullOrEmpty(XMLLocation))
        {
            WriteXML(new Settings(SoundHotkeys), XMLLocation);
            MessageBox.Show($"Saved as:\n{XMLLocation}");
        }
        // Otherwise, we show a message that file hasn't been saved.
        else MessageBox.Show("Location was empty, file hasn't been saved!");
    }

    private void ReloadDevicesButton_Click(Object? sender, EventArgs? e)
    {
        // Stopping all current sounds and reloading the devices.
        StopPlayback();
        StopLoopback();
        LoadSoundDevices();
    }

    private void EnableCheckBox_CheckedChanged(Object? sender, EventArgs? e)
    {
        // If the checkbox is enabled - set the timer and start loopback.
        if (enableCheckBox?.Checked == true)
        {
            // Start the loopback if there are any devices and soundboard is enabled.
            if (enableCheckBox.Checked && playbackDevicesComboBox?.Items.Count > 0 && loopbackDevicesComboBox?.SelectedIndex > 0)
                StartLoopback();
        }
        // Otherwise, stop all sounds and dispose of objects.
        else
        {
            StopPlayback();
            StopLoopback();
            HoldRepeatTimer?.Stop();
        }
    }

    private void KeySoundsListView_MouseDoubleClick(Object? sender, MouseEventArgs? e) => EditSelectedSoundHotkey();
    
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

        // Play the sound based on the hotkey provided.
        PlayKeySound(_currentHoldHotkey);
    }

    private void PlayKeySound(SoundHotkey currentKeysSounds)
    {
        String path = String.Empty;

        if (currentKeysSounds.SoundLocations.Count > 1)
        {
            //get random sound
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
            path = currentKeysSounds.SoundLocations.First(); //get first sound

        if (File.Exists(path))
        {
            PlaySound(path);
        }
        else if (!_showMsgBox) //dont run when already showing messagebox (don't want a bunch of these on your screen, do you?)
        {
            SystemSounds.Beep.Play();
            _showMsgBox = true;
            MessageBox.Show("File " + path + " does not exist");
            _showMsgBox = false;
        }
    }

    private void LoopbackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
    {
        if (loopbackDevicesComboBox?.SelectedIndex > 0)
        {
            if (enableCheckBox?.Checked == true) //start loopback on new device, or stop loopback
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

    private void PlaybackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
    {
        //start loopback on new device and stop all sounds playing
        if (_loopbackWaveOut != null && _loopbackSourceStream != null && enableCheckBox?.Checked == true)
            StartLoopback();

        StopPlayback();

        InitAudioPlaybackEngine();
        
        // String deviceName = PlaybackDevicesComboBox.SelectedItem.ToString();
        CurrentSettings.LastPlaybackDevice = playbackDevicesComboBox?.SelectedItem as String;

        SaveSoundboardSettingsXML();
    }

    private void MainForm_Resize(Object? sender, EventArgs? e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            notificationIcon?.Visible = true;

            this.Hide();
        }
    }

    private void NotificationIcon_MouseClick(Object? sender, MouseEventArgs? e)
    {
        notificationIcon?.Visible = false;

        //show form and give focus
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
    }

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

    private void ReloadWindowsButton_Click(Object? sender, EventArgs? e)
    {
        LoadWindows();
    }
}