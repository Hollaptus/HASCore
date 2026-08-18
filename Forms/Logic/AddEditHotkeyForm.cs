using System.Diagnostics;
using System.Text;
using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
using HASCore.Soundboard;

namespace HASCore.Forms;

public partial class AddEditHotkeyForm : Form
{
    private MainForm? _mainForm;
    private SettingsForm? _settingsForm;
    /// <summary>
    /// For tracking the last processed key combination.
    /// </summary>
    private HashSet<Keys>? _lastProcessedKeys = null;
    private IEnumerable<TextBox> _tbControls; 
    internal SoundHotkeyEditData? EditData = null;
    internal Int32 EditIndex = -1;

    public AddEditHotkeyForm()
    {
        // Calling initialization procedure from another part of the class.
        InitializeComponent();

        // Initializing the global WinAPI keyboard hook for processing
        // the user input, so we can subscribe the event handler "KeysChanged"
        // to an event "OnKeysChanged" that we can do some actions on pressing
        // the hotkeys.
        GlobalKeyboardHook.Initialize();
        GlobalKeyboardHook.KeysChanged += OnKeysChanged;
        
        // Usually, using this.Controls.OfType<T>() method would be fine,
        // but if/when we will add some GroupBox, Panel or other container,
        // and put a TextBox inside of it - this will break, so instead
        // we use the extension to get all the forms controls, even inside containers.
        _tbControls = this.GetAllControls().OfType<TextBox>();
    }

    private void AddEditHotkeyForm_FormClosing(Object? sender, FormClosingEventArgs e) =>
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;

    private void AddEditSoundKeys_Load(Object? sender, EventArgs e)
    {
        // Hide window restriction if we are adding/editing XML preset files.
        if (SettingsForm.EditLoadXMLFile)
        {
            // Remove visibility of the options to restrict 
            // hotkey triggering in a certain window.
            windowRestrictionGroupBox?.Visible = false;
            // Set the window size accordingly, since we don't
            // have a portion of the form components visible.
            this.MinimumSize = new Size(375, 205);
            this.Size = new Size(375, 205);

            // Getting the current open settings form. 
            _settingsForm = Application.OpenForms.OfType<SettingsForm>().FirstOrDefault();
            // Changing the window title for the current mode.
            this.Text = "Add/edit keys and XML location";

            // If the index of current item of ListView doesn't equal to
            // -1 (no entry selected), we get the current data from the item. 
            if (EditIndex != -1)
            {
                keysTextBox?.Text = EditData?.Keys;
                locationTextBox?.Text = EditData?.SoundLocation;
            }
        }
        // Otherwise we've opened the form from the context of adding the hotkey to play sound.
        else
        {
            // Get the main form of the app.
            _mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();

            // Add info to the labels text that we can put more than one file here.
            locationLabel?.Text += " (use a semi-colon (;) to seperate multiple locations)";

            // Load currently open windows into the combobox.
            LoadWindows();

            // If the index of current item of ListView doesn't equal to
            // -1 (no entry selected), we get the current data from the item.
            // For the context of adding the sound hotkey - also get the window
            // restriction settings. 
            if (EditIndex != -1)
            {
                // Get the hotkey for the current ListView item.
                keysTextBox?.Text = EditData?.Keys;

                // If the window restriction is filled - also fill the options.
                if (!String.IsNullOrEmpty(EditData?.WindowTitle))
                {
                    // Enable the checkbox for the restriction.
                    enableRestrictWindowCheckBox?.Checked = true;

                    // Get the current index of window by title. 
                    Int32 index = windowsComboBox?.Items.IndexOf(EditData?.WindowTitle) ?? -1;

                    // If the window still exists - select it from the combobox.
                    if (index != -1) 
                        windowsComboBox?.SelectedIndex = index;
                    // Otherwise add the entry and select it.
                    else
                    {
                        windowsComboBox?.Items.Add(EditData?.WindowTitle!);
                        windowsComboBox?.SelectedIndex = windowsComboBox.Items.Count - 1;
                    }
                }

                // Get the location(s) of the hotkey entry. 
                locationTextBox?.Text = EditData?.SoundLocation;
            }
        }
    }

    // Unfocusing all the controls so we can leave 
    // by pressing the "Escape" key upon opening the form. 
    private void AddEditHotkeyForm_Shown(Object? sender, EventArgs e)
        => this.ActiveControl = null;
    
    private void LoadWindows()
    {
        // Clear all the items.
        windowsComboBox?.Items.Clear();

        // Add all processes names in one go to the combobox.
        windowsComboBox?.Items.AddRange(
            [ "[No restrictions]",
              .. Process.GetProcesses()
                .Where(p => !String.IsNullOrEmpty(p.MainWindowTitle))
                .Select(p => p.MainWindowTitle)]
        );

        // Select the first option by default.
        windowsComboBox?.SelectedIndex = 0;
    }

    private void OKButton_Click(Object? sender, EventArgs e)
    {
        // Using the builder for displaying multiple error messages,
        // if there are any.
        StringBuilder stringBuilder = new ();
        
        // If the location is empty
        if (String.IsNullOrWhiteSpace(locationTextBox?.Text))
            stringBuilder.AppendLine("Location is empty");

        if (SettingsForm.EditLoadXMLFile && String.IsNullOrWhiteSpace(keysTextBox?.Text))
            stringBuilder.Append("No keys entered");

        List<String>? soundLocations = null;

        if (locationTextBox is not null 
            && !SettingsForm.EditLoadXMLFile 
            && Conversions.SoundLocsArrayFromString(locationTextBox.Text, out soundLocations, out String? errorMessage))
        {
            if (soundLocations is not null && soundLocations.Any(x => String.IsNullOrWhiteSpace(x) || !File.Exists(x)))
            {
                MessageBox.Show("The file/one of the files does not exist");
                this.Close();
                return;
            }

            if (soundLocations == null)
            {
                stringBuilder.AppendLine(errorMessage);
                return;
            }
        }

        if (!Conversions.KeysArrayFromString(keysTextBox?.Text, out List<Keys>? keysList, out errorMessage))
        {
            keysList = [];
            stringBuilder.AppendLine(errorMessage);
        }

        if (stringBuilder.Length > 0)
        {
            MessageBox.Show(
                stringBuilder.ToString(), 
                "Errors have occured", 
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (SettingsForm.EditLoadXMLFile)
        {
            if (EditIndex != -1)
            {
                _settingsForm?.KeysLocationsListView?.Items[EditIndex].Text = keysTextBox?.Text;
                _settingsForm?.KeysLocationsListView?.Items[EditIndex].SubItems[1].Text = locationTextBox?.Text;

                _settingsForm?.LoadXMLFilesList?[EditIndex].Keys = keysList;
                _settingsForm?.LoadXMLFilesList?[EditIndex].XMLLocation = locationTextBox?.Text;
            }
            else
            {
                ListViewItem item = new (keysTextBox?.Text);
                item.SubItems.Add(locationTextBox?.Text);

                _settingsForm?.KeysLocationsListView?.Items.Add(item);

                _settingsForm?.LoadXMLFilesList?.Add(new XMLSettings.LoadXMLFile(keysList, locationTextBox?.Text));
            }
        }
        else
        {
            String? windowText = String.Empty;
            if (enableRestrictWindowCheckBox?.Checked == true && !String.IsNullOrEmpty(windowsComboBox?.SelectedItem as String)) windowText = windowsComboBox.SelectedItem as String;

            if (EditIndex > -1)
            {
                _mainForm?.KeySoundsListView?.Items[EditIndex].Text = keysTextBox?.Text;
                _mainForm?.KeySoundsListView?.Items[EditIndex].SubItems[1].Text = windowText;
                _mainForm?.KeySoundsListView?.Items[EditIndex].SubItems[2].Text = locationTextBox.Text;

                _mainForm?.SoundHotkeys[EditIndex] = new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!);
            }
            else
            {
                ListViewItem newItem = new (keysTextBox?.Text);
                newItem.SubItems.Add(windowText);
                newItem.SubItems.Add(locationTextBox.Text);

                _mainForm?.KeySoundsListView?.Items.Add(newItem);

                _mainForm?.SoundHotkeys.Add(new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!));
            }

            _mainForm?.KeySoundsListView?.ListViewItemSorter = new Comparers.ListViewItemComparer(0);
            _mainForm?.KeySoundsListView?.Sort();

            _mainForm?.SoundHotkeys.Sort(new Comparers.SoundHotkeyComparer());

            _mainForm?.KeysColumnHeader?.Width = -2;
            _mainForm?.SoundLocationColumnHeader?.Width = -2;
        }

        this.Close();
    }

    private void CancelButton_Click(Object? sender, EventArgs e)
    {
        this.Close();
    }

    private void BrowseSoundLocationButton_Click(Object? sender, EventArgs e)
    {
        OpenFileDialog diag = new ()
        {
            Multiselect = !SettingsForm.EditLoadXMLFile,
            Filter = SettingsForm.EditLoadXMLFile 
                ? "XML file containing keys and sounds|*.xml" 
                : "Supported audio formats|*.mp3;*.m4a;*.wav;*.wma;*.ac3;*.aiff;*.mp2|All files|*.*"
        };
        locationTextBox?.Text = diag.ShowDialog() == DialogResult.OK ? String.Join(';', diag.FileNames) : String.Empty;
    }

    private void EnableRestrictWindowCheckBox_CheckedChanged(Object? sender, EventArgs e)
    {
        windowsComboBox?.Enabled = enableRestrictWindowCheckBox?.Checked == true;
        reloadWindowsButton?.Enabled = enableRestrictWindowCheckBox?.Checked == true;
    }

    private void ReloadWindowsButton_Click(Object? sender, EventArgs e)
    {
        LoadWindows();
    }

    private void OnKeysChanged(Object? sender, HashSet<Keys> currentKeys)
    {
        // Saving the current selected textbox that is readonly
        // so we correctly handle the input for saving the hotkeys.
        TextBox? readOnlyTextBox = _tbControls.FirstOrDefault(tb => tb.ReadOnly && tb.Focused);

        // After all the keys are up - clear the set.
        if (currentKeys.Count == 0)
        {
            _lastProcessedKeys = null;
            return;
        }

        // On "Escape" we either unfocus everything or close the form gracefully.
        if (currentKeys.Contains(Keys.Escape))
        {
            if (this.ActiveControl is not null)
            {
                this.ActiveControl = null;
                return;
            }
            else this.Close();
        }

        // If the hotkey textbox is focused and is readonly - we remove the current keys
        // and unfocus the textbox so we don't capture the keys outside the focus.
        if (currentKeys.Contains(Keys.Back))
        {
            _lastProcessedKeys = null;
            readOnlyTextBox?.Text = String.Empty;
            return;
        }

        // If there are no pressed keys at the moment, the count of keys is more or equal
        // to the previous count, or if the currently pressed keys are not equal to previous
        // ones, then we update the keys, as long the textbox for hotkeys is focused.    
        if (_lastProcessedKeys == null 
            || (currentKeys.Count >= _lastProcessedKeys.Count && !_lastProcessedKeys.SetEquals(currentKeys)))
        {
            _lastProcessedKeys = [.. currentKeys];
            readOnlyTextBox?.Text = Conversions.KeysToString(_lastProcessedKeys);
        }
    }
}