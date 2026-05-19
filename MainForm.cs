using System.Media;
using System.Diagnostics;
using NAudio.Wave;
// Declaring the using statement so we don't have to always prepend
// 'XMLSettings' to an already static fields of the class.
using static JNSoundboardCore.XMLSettings;

namespace JNSoundboardCore
{
    /// Description
    /// <summary>
    ///     <see cref="MainForm"/> class part for initializing the Object and its event handlers.
    /// </summary>
    public partial class MainForm : Form
    {
        private WaveIn? LoopbackSourceStream = null;
        private BufferedWaveProvider? LoopbackWaveProvider = null;
        private WaveOut? LoopbackWaveOut = null;
        private readonly Random Rand = new();

        private Boolean KeyUpPushToTalkKey = false;
        private Keys PushToTalkKey;
        private List<Keys>? KeysJustPressed = null;
        private Boolean ShowMsgBox = false;
        private Int32 LastIndex = -1;

        internal List<SoundHotkey> SoundHotkeys = [];

        internal String XMLLocation = String.Empty;

        /// Description
        /// <summary>
        ///     <see cref="MainForm"/> constructor for initialization of class properties.
        /// </summary>
        public MainForm()
        {
            // Calling initialization procedure from another part of the class.
            InitializeComponent();

            // Dynamically creating the ToolTip object for displaying
            // tooltips for certain controls
            ToolTip tooltip = new();
            // Because we have already called the InitializeComponent procedure,
            // these buttons shouldn't be null, but we'll still check just in case
            if (ReloadDevicesButton is not null)
                tooltip.SetToolTip(ReloadDevicesButton, "Refresh sound devices");
            if (ReloadWindowsButton is not null)
                tooltip.SetToolTip(ReloadWindowsButton, "Reload windows");

            // Calling procedures to populate controls with relevant information
            // For PlaybackDevicesComboBox and LoopbackDevicesComboBox:
            LoadSoundDevices();
            // For WindowsComboBox:
            LoadWindows();
            // For KeySoundsListView:
            LoadSoundboardSettingsXML();

            // Also checking if the devices haven't changed since last launch: 
            if (PlaybackDevicesComboBox is not null && PlaybackDevicesComboBox.Items.Contains(CurrentSettings.LastPlaybackDevice))
            {
                // We select the item that has been as default playback device last time
                PlaybackDevicesComboBox.SelectedItem = CurrentSettings.LastPlaybackDevice;
                // Also adding the event handler for changes in the index of selected item in the combobox
                PlaybackDevicesComboBox.SelectedIndexChanged += PlaybackDevicesComboBox_SelectedIndexChanged;
            }
            if (LoopbackDevicesComboBox is not null && LoopbackDevicesComboBox.Items.Contains(CurrentSettings.LastLoopbackDevice)) 
            {
                LoopbackDevicesComboBox.SelectedItem = CurrentSettings.LastLoopbackDevice;
                LoopbackDevicesComboBox.SelectedIndexChanged += LoopbackDevicesComboBox_SelectedIndexChanged;
            }

            // After all the settings have been loaded, 
            // we start initializing the engine for audio playback
            InitAudioPlaybackEngine();

            // Also adding the event handler after input has ended
            // for 'Push to talk' functionality
            AudioPlaybackEngine.Instance.AllInputEnded += OnAllInputEnded;
        }

        private void OnAllInputEnded(Object? sender, EventArgs? e)
        {
            if (KeyUpPushToTalkKey)
            {
                KeyUpPushToTalkKey = false;
                Keyboard.SendKey(PushToTalkKey, false);
            }
        }

        private void InitAudioPlaybackEngine()
        {
            try
            {
                if (PlaybackDevicesComboBox?.SelectedIndex is not null)
                    AudioPlaybackEngine.Instance.Init(PlaybackDevicesComboBox.SelectedIndex);
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
                // the controls should be initialized by now
                if (WindowsComboBox is not null)
                {
                    // Clearing the items and adding the 'Any window' option
                    WindowsComboBox.Items.Clear();
                    WindowsComboBox.Items.Add("[Any window]");
                    WindowsComboBox.SelectedIndex = 0;

                    // Getting all the processes currently running and adding to the list
                    foreach (Process process in Process.GetProcesses())
                        if (!String.IsNullOrEmpty(process.MainWindowTitle))
                            WindowsComboBox.Items.Add(process.MainWindowTitle);
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
                // the controls should be initialized by now
                if (PlaybackDevicesComboBox?.Items is not null && LoopbackDevicesComboBox?.Items is not null)
                {
                    List<WaveOutCapabilities> playbackSources = [];
                    List<WaveInCapabilities> loopbackSources = [];
                    
                    // Iterating through audio devices and 
                    // adding them to their respective lists
                    for (Int32 i = 0; i < WaveOut.DeviceCount; i++)
                        playbackSources.Add(WaveOut.GetCapabilities(i));

                    for (Int32 i = 0; i < WaveIn.DeviceCount; i++)
                        loopbackSources.Add(WaveIn.GetCapabilities(i));
                    
                    // Clearing the list of items inside the comboboxes
                    PlaybackDevicesComboBox.Items.Clear();
                    LoopbackDevicesComboBox.Items.Clear();

                    // Adding the playback devices from audio devices capabilities list
                    foreach (WaveOutCapabilities source in playbackSources)
                        PlaybackDevicesComboBox.Items.Add(source.ProductName);
                    
                    // Setting the index if there are any items
                    if (PlaybackDevicesComboBox.Items.Count > 0)
                        PlaybackDevicesComboBox.SelectedIndex = 0;
                    // And adding an empty entry
                    LoopbackDevicesComboBox.Items.Add(String.Empty);

                    // Doing the same for loopback devices
                    foreach (WaveInCapabilities source in loopbackSources)
                        LoopbackDevicesComboBox.Items.Add(source.ProductName);

                    LoopbackDevicesComboBox.SelectedIndex = 0;
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
                // Stopping loopback if it is used right now
                StopLoopback();
                // We are checking if the combobox has been initialized just in case,
                // the controls should be initialized by now
                if (LoopbackDevicesComboBox is not null)
                {
                    Int32 deviceNumber = LoopbackDevicesComboBox.SelectedIndex - 1;

                    // Setting the parameters of the loopback stream
                    LoopbackSourceStream ??= new WaveIn();
                    LoopbackSourceStream.DeviceNumber = deviceNumber;
                    LoopbackSourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);
                    LoopbackSourceStream.BufferMilliseconds = 25;
                    LoopbackSourceStream.NumberOfBuffers = 5;
                    LoopbackSourceStream.DataAvailable += LoopbackSourceStream_DataAvailable;
                    // Setting the parameters of the provider
                    LoopbackWaveProvider = new BufferedWaveProvider(LoopbackSourceStream.WaveFormat)
                    {
                        DiscardOnBufferOverflow = true
                    };
                    // Setting the parameters of the output
                    LoopbackWaveOut ??= new WaveOut();
                    LoopbackWaveOut.DeviceNumber = LoopbackDevicesComboBox.SelectedIndex;
                    LoopbackWaveOut.DesiredLatency = 125;
                    // Initialize output based on the provider
                    LoopbackWaveOut.Init(LoopbackWaveProvider);
                    // Record what is gonna be looped backed
                    LoopbackSourceStream.StartRecording();
                    // Play it out on the output
                    LoopbackWaveOut.Play();
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
                // Clearing resources
                LoopbackWaveOut?.Stop();
                LoopbackWaveOut?.Dispose();
                LoopbackWaveProvider?.ClearBuffer();
                LoopbackSourceStream?.StopRecording();
                LoopbackSourceStream?.Dispose();
                // Setting the values of objects to null reference
                LoopbackWaveOut = null;
                LoopbackWaveProvider = null;
                LoopbackSourceStream = null;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private static void StopPlayback() => AudioPlaybackEngine.Instance.StopAllSounds();
        
        private static void PlaySound(String file)
        {
            // StopPlayback();

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
            // contents are not empty, we parse the entries and load them
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
                    String keys = Helper.KeysToString(hotkey.Keys ?? []);

                    // Checking for repeating keys and building the error message.
                    if (!String.IsNullOrEmpty(keys) && items.Count > 0 && items[^1].Text == keys && !sameKeys.Contains(keys))
                        sameKeys += (sameKeys != String.Empty ? ", " : String.Empty) + keys;

                    // Adding a new item for a ListView
                    ListViewItem tempItem = new(keys);
                    tempItem.SubItems.Add(hotkey.WindowTitle);
                    tempItem.SubItems.Add(Helper.SoundLocsArrayToString(hotkey.SoundLocations));
                    
                    // Then appending to the end of the list of items
                    items.Add(tempItem);
                }

                // If there are any items, we add them to the ListView
                if (items.Count > 0)
                {
                    // Also if there were any errors, we show them to the user
                    if (!String.IsNullOrEmpty(errors))
                        MessageBox.Show(errors);

                    if (!String.IsNullOrEmpty(sameKeys))
                        MessageBox.Show("Multiple entries using the same keys. The keys being used multiple times are: " + sameKeys);

                    // Clearing the lists and adding the new values
                    SoundHotkeys.Clear();
                    SoundHotkeys.AddRange(settings.SoundHotkeys);
                    KeySoundsListView?.Items.Clear();
                    KeySoundsListView?.Items.AddRange([.. items]);

                    // We set the width of the column headers to this specific number
                    // so we autosize to the width of the heading, according to .NET docs:
                    // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.columnheader.width
                    KeysColumnHeader?.Width = -2;
                    SoundLocationColumnHeader?.Width = -2;

                    // Setting the field to current XML file path for saving the changes
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
            // of the contents and index of selected item
            if (KeySoundsListView?.SelectedItems.Count > 0)
            {
                // Get the first available item
                ListViewItem item = KeySoundsListView.SelectedItems[0];

                // Create a new instance of the form and run dialog
                new AddEditHotkeyForm
                {
                    // Get its contents and index and write their values
                    // to the fields of what we are editing
                    EditStrings = [item.Text, item.SubItems[1].Text, item.SubItems[2].Text],
                    EditIndex = KeySoundsListView.SelectedIndices[0]
                }.ShowDialog();
            }
        }

        private void LoopbackSourceStream_DataAvailable(Object? sender, WaveInEventArgs? e)
        { 
            if (LoopbackWaveProvider != null && LoopbackWaveProvider.BufferedDuration.TotalMilliseconds <= 100)
                LoopbackWaveProvider.AddSamples(e?.Buffer, 0, e?.BytesRecorded ?? 0);
        }

        private void SettingsToolStripMenuItem_Click(Object? sender, EventArgs? e) => new SettingsForm().ShowDialog();

        private void TTSToolStripMenuItem_Click(Object? sender, EventArgs? e) => new TextToSpeechForm().ShowDialog();

        private void UpdateToolStripMenuItem_Click(Object? sender, EventArgs? e)
        {
            using Process process = new();
            process.StartInfo.FileName = "https://github.com/Hollaptus/JNSoundboardCore/releases";
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

                if (KeySoundsListView.Items.Count == 0) EnableCheckBox?.Checked = false;
            }
        }

        private void ClearButton_Click(Object? sender, EventArgs? e)
        {
            if (MessageBox.Show("Are you sure you want to clear all items?", "Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SoundHotkeys.Clear();
                KeySoundsListView?.Items.Clear();

                EnableCheckBox?.Checked = false;
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
            OpenFileDialog diag = new() { Filter = "XML file containing keys and sounds|*.xml" };
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
                XMLLocation = Helper.UserGetXMLLocation();

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
            XMLLocation = Helper.UserGetXMLLocation();
            // If the location was empty and we have a previous one - save to that one.
            if (String.IsNullOrEmpty(XMLLocation) && !String.IsNullOrEmpty(lastLocation))
                XMLLocation = lastLocation;
            
            // If the location is not empty - we write the XML file to that location.
            if (!String.IsNullOrEmpty(XMLLocation))
            {
                WriteXML(new Settings(SoundHotkeys), XMLLocation);
                MessageBox.Show($"Saved as:\n{XMLLocation}");
            }
            // Otherwise, we show a message that file hasn't been saved
            else MessageBox.Show("Location was empty, file hasn't been saved!");
        }

        private void ReloadDevicesButton_Click(Object? sender, EventArgs? e)
        {
            // Stopping all current sounds and reloading the devices
            StopPlayback();
            StopLoopback();
            LoadSoundDevices();
        }

        private void EnableCheckBox_CheckedChanged(Object? sender, EventArgs? e)
        {
            // If the checkbox is enabled - set the timer and start loopback 
            if (EnableCheckBox?.Checked == true)
            {
                // If there are any settings or hotkeys - enable the timer
                if ((SoundHotkeys != null && SoundHotkeys.Count > 0) 
                || (CurrentSettings.LoadXMLFiles != null && CurrentSettings.LoadXMLFiles.Count > 0))
                    MainTimer?.Enabled = true;
                // Otherwise, disable it
                else EnableCheckBox.Checked = false;

                // Start the loopback if there are any devices and soundboard is enabled
                if (EnableCheckBox.Checked && PlaybackDevicesComboBox?.Items.Count > 0 && LoopbackDevicesComboBox?.SelectedIndex > 0)
                    StartLoopback();
            }
            // Otherwise, stop all sounds and dispose of objects
            else
            {
                StopPlayback();
                StopLoopback();
            }
        }

        private void KeySoundsListView_MouseDoubleClick(Object? sender, MouseEventArgs? e) => EditSelectedSoundHotkey();
        
        private void MainTimer_Tick(Object? sender, EventArgs? e)
        {
            List<Keys> keysPressed = Keyboard.GetPressedKeys();
            Int32 keysPressedCount = 0;

            // Check that required keys are pressed to enable soundboard.
            if (CurrentSettings.EnableSoundboardKeys?.Count > 0) 
            {
                foreach (Keys key in CurrentSettings.EnableSoundboardKeys)
                    if (keysPressed.Contains(key)) keysPressedCount++;
                
                // for (Int32 i = 0; i < CurrentSettings.EnableSoundboardKeys.Count; i++)
                //     if (Keyboard.IsKeyDown(CurrentSettings.EnableSoundboardKeys[i])) keysPressedCount++;
                
                if (keysPressedCount == CurrentSettings.EnableSoundboardKeys.Count 
                && (KeysJustPressed == null || !KeysJustPressed.Intersect(CurrentSettings.EnableSoundboardKeys).Any()))
                {
                    EnableCheckBox?.Checked ^= true;

                    KeysJustPressed = CurrentSettings.EnableSoundboardKeys;

                    return;
                }
                else if (KeysJustPressed == CurrentSettings.EnableSoundboardKeys)
                    KeysJustPressed = null;

                keysPressedCount = 0;
            }

            if (EnableCheckBox?.Checked == true)
            {
                if (SoundHotkeys.Count > 0) //check that required keys are pressed to play sound
                {
                    IntPtr foregroundWindow = Helper.GetForegroundWindow();

                    for (Int32 i = 0; i < SoundHotkeys.Count; i++)
                    {
                        keysPressedCount = 0;

                        if (SoundHotkeys[i].Keys?.Count == 0
                            || (SoundHotkeys[i].WindowTitle != String.Empty 
                            && !Helper.IsForegroundWindow(SoundHotkeys[i].WindowTitle, foregroundWindow)))
                            continue;

                        // for (Int32 j = 0; j < SoundHotkeys[i].Keys?.Count; j++)
                        //     if (Keyboard.IsKeyDown(SoundHotkeys[i].Keys![j]))
                        //         keysPressedCount++;
                        if (SoundHotkeys[i].Keys != null)
                            foreach (Keys key in SoundHotkeys[i]?.Keys!)
                                if (keysPressed.Contains(key)) keysPressedCount++;

                        if (keysPressedCount == SoundHotkeys[i].Keys?.Count)
                        {
                            if (KeysJustPressed == SoundHotkeys[i].Keys) continue;

                            if (SoundHotkeys[i].Keys?.Count > 0 
                                && SoundHotkeys[i].Keys!.All(x => x != 0) 
                                && SoundHotkeys[i].SoundLocations?.Count > 0 
                                && SoundHotkeys[i].SoundLocations!.Any(x => File.Exists(x)))
                            {
                                if (EnablePushToTalkCheckBox?.Checked == true 
                                    && !KeyUpPushToTalkKey 
                                    && !Keyboard.IsKeyDown(PushToTalkKey)
                                    && (WindowsComboBox?.SelectedIndex == 0 
                                    || Helper.IsForegroundWindow(WindowsComboBox?.SelectedItem as String)))
                                {
                                    KeyUpPushToTalkKey = true;
                                    Boolean result = Keyboard.SendKey(PushToTalkKey, true);
                                    Thread.Sleep(100);
                                }

                                PlayKeySound(SoundHotkeys[i]);
                                return;
                            }
                        }
                        else if (KeysJustPressed == SoundHotkeys[i].Keys) KeysJustPressed = null;
                    }

                    keysPressedCount = 0;
                }

                if (CurrentSettings.StopSoundKeys != null && CurrentSettings.StopSoundKeys.Count > 0) //check that required keys are pressed to stop all sounds
                {
                    for (Int32 i = 0; i < CurrentSettings.StopSoundKeys.Count; i++)
                        if (Keyboard.IsKeyDown(CurrentSettings.StopSoundKeys[i])) keysPressedCount++;
                    
                    if (keysPressedCount == CurrentSettings.StopSoundKeys.Count)
                        if (KeysJustPressed == null || !KeysJustPressed.Intersect(CurrentSettings.StopSoundKeys).Any())
                        {
                            StopPlayback();

                            KeysJustPressed = CurrentSettings.StopSoundKeys;

                            return;
                        }
                    else if (KeysJustPressed == CurrentSettings.StopSoundKeys)
                        KeysJustPressed = null;

                    keysPressedCount = 0;
                }

                if (CurrentSettings.LoadXMLFiles != null && CurrentSettings.LoadXMLFiles.Count > 0) //check that required keys are pressed to load XML file
                {
                    for (Int32 i = 0; i < CurrentSettings.LoadXMLFiles.Count; i++)
                    {
                        if (CurrentSettings.LoadXMLFiles[i].Keys?.Count == 0) continue;

                        keysPressedCount = 0;

                        for (Int32 j = 0; j < CurrentSettings.LoadXMLFiles[i].Keys?.Count; j++)
                            if (Keyboard.IsKeyDown(CurrentSettings.LoadXMLFiles[i].Keys![j])) keysPressedCount++;
                        
                        if (keysPressedCount == CurrentSettings.LoadXMLFiles[i].Keys?.Count)
                        {
                            if (KeysJustPressed == null || !KeysJustPressed.Intersect(CurrentSettings.LoadXMLFiles[i].Keys!).Any())
                            {
                                if (!String.IsNullOrWhiteSpace(CurrentSettings.LoadXMLFiles[i].XMLLocation) && File.Exists(CurrentSettings.LoadXMLFiles[i].XMLLocation))
                                {
                                    KeysJustPressed = CurrentSettings.LoadXMLFiles[i].Keys;

                                    LoadXMLFile(CurrentSettings.LoadXMLFiles[i].XMLLocation!);
                                }

                                return;
                            }
                        }
                        else if (KeysJustPressed == CurrentSettings.LoadXMLFiles[i].Keys)
                        {
                            KeysJustPressed = null;
                        }
                    }

                    keysPressedCount = 0;
                }

                if (KeyUpPushToTalkKey)
                {
                    if (!Keyboard.IsKeyDown(PushToTalkKey)) KeyUpPushToTalkKey = false;

                    if (WindowsComboBox?.SelectedIndex != 0 && !Helper.IsForegroundWindow(WindowsComboBox?.SelectedItem as String))
                    {
                        KeyUpPushToTalkKey = false;
                        Keyboard.SendKey(PushToTalkKey, false);
                    }
                }
            }
        }

        private void PlayKeySound(SoundHotkey currentKeysSounds)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath)!;

            String path = String.Empty;

            if (currentKeysSounds.SoundLocations.Count > 1)
            {
                //get random sound
                Int32 temp;

                while (true)
                {
                    temp = Rand.Next(0, currentKeysSounds.SoundLocations.Count);

                    if (temp != LastIndex && File.Exists(currentKeysSounds.SoundLocations[temp])) break;
                    Thread.Sleep(1);
                }

                LastIndex = temp;

                path = currentKeysSounds.SoundLocations[LastIndex];
            }
            else if (currentKeysSounds.SoundLocations.Count == 1)
                path = currentKeysSounds.SoundLocations.First(); //get first sound

            if (File.Exists(path))
            {
                PlaySound(path);
                KeysJustPressed = currentKeysSounds.Keys;
            }
            else if (!ShowMsgBox) //dont run when already showing messagebox (don't want a bunch of these on your screen, do you?)
            {
                SystemSounds.Beep.Play();
                ShowMsgBox = true;
                MessageBox.Show("File " + path + " does not exist");
                ShowMsgBox = false;
            }
        }

        private void LoopbackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
        {
            if (LoopbackDevicesComboBox?.SelectedIndex > 0)
            {
                if (EnableCheckBox?.Checked == true) //start loopback on new device, or stop loopback
                {
                    if (String.IsNullOrEmpty(LoopbackDevicesComboBox.SelectedItem?.ToString())) StopLoopback();
                    else StartLoopback();
                }
                else
                    StopLoopback();
            }

            CurrentSettings.LastLoopbackDevice = LoopbackDevicesComboBox?.SelectedItem as String;

            SaveSoundboardSettingsXML();
        }

        private void PlaybackDevicesComboBox_SelectedIndexChanged(Object? sender, EventArgs? e)
        {
            //start loopback on new device and stop all sounds playing
            if (LoopbackWaveOut != null && LoopbackSourceStream != null && EnableCheckBox?.Checked == true)
                StartLoopback();

            StopPlayback();

            InitAudioPlaybackEngine();
            
            // String deviceName = PlaybackDevicesComboBox.SelectedItem.ToString();
            CurrentSettings.LastPlaybackDevice = PlaybackDevicesComboBox?.SelectedItem as String;

            SaveSoundboardSettingsXML();
        }

        private void MainForm_Resize(Object? sender, EventArgs? e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                NotificationIcon?.Visible = true;

                this.Hide();
            }
        }

        private void NotificationIcon_MouseClick(Object? sender, MouseEventArgs? e)
        {
            NotificationIcon?.Visible = false;

            //show form and give focus
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void PushToTalkKeyTextBox_Enter(Object? sender, EventArgs? e)
        {
            if (!EnablePushToTalkCheckBox?.Checked == true)
            {
                EnableCheckBox?.Checked = false;
                PushToTalkKeyTimer?.Enabled = true;
            }
        }

        private void PushToTalkKeyTextBox_Leave(Object? sender, EventArgs? e) => PushToTalkKeyTimer?.Enabled = false;
        
        private void PushToTalkKeyTimer_Tick(Object? sender, EventArgs? e)
        {
            if (Keyboard.IsKeyDown(Keys.Escape))
            {
                PushToTalkKeyTextBox?.Text = String.Empty;
                PushToTalkKey = default;
            }
            else
            {
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (Keyboard.IsKeyDown(key))
                    {
                        PushToTalkKeyTextBox?.Text = Helper.KeysToString(key);
                        PushToTalkKey = key;
                        break;
                    }
                }
            }
        }

        private void EnablePushToTalkCheckBox_CheckedChanged(Object? sender, EventArgs? e)
        {
            if (EnablePushToTalkCheckBox?.Checked == true)
            {
                if (String.IsNullOrEmpty(PushToTalkKeyTextBox?.Text))
                {
                    EnablePushToTalkCheckBox.Checked = false;
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
}
