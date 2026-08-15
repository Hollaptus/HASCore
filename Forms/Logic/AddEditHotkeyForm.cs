using System.Diagnostics;
using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
using HASCore.Soundboard;

namespace HASCore.Forms;

public partial class AddEditHotkeyForm : Form
{
    private MainForm? mainForm;
    private SettingsForm? settingsForm;
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
        if (SettingsForm.EditLoadXMLFile)
        {
            // Hide window restriction if we are adding/editing XML files.
            windowRestrictionGroupBox?.Visible = false;
            this.MinimumSize = new Size(375, 205);
            this.Size = new Size(375, 205);

            settingsForm = Application.OpenForms.OfType<SettingsForm>().FirstOrDefault();
            this.Text = "Add/edit keys and XML location";

            if (EditIndex != -1)
            {
                keysTextBox?.Text = EditData?.Keys;
                locationTextBox?.Text = EditData?.SoundLocation;
            }
        }
        else
        {
            mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();

            locationLabel?.Text += " (use a semi-colon (;) to seperate multiple locations)";

            LoadWindows();

            if (EditIndex != -1)
            {
                keysTextBox?.Text = EditData?.Keys;

                if (!String.IsNullOrEmpty(EditData?.WindowTitle))
                {
                    enableRestrictWindowCheckBox?.Checked = true;

                    Int32 index = windowsComboBox?.Items.IndexOf(EditData?.WindowTitle) ?? -1;

                    if (index != -1) windowsComboBox?.SelectedIndex = index;
                    else
                    {
                        windowsComboBox?.Items.Add(EditData?.WindowTitle!);
                        windowsComboBox?.SelectedIndex = windowsComboBox.Items.Count - 1;
                    }
                }

                locationTextBox?.Text = EditData?.SoundLocation;
            }
        }
    }

    private void AddEditHotkeyForm_Shown(Object? sender, EventArgs e)
    {
        // Unfocusing all the controls so we can leave 
        // by pressing the "Escape" key upon opening the form. 
        this.ActiveControl = null;
    }

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
        if (String.IsNullOrWhiteSpace(locationTextBox?.Text))
        {
            MessageBox.Show("Location is empty");
            return;
        }

        if (SettingsForm.EditLoadXMLFile && String.IsNullOrWhiteSpace(keysTextBox?.Text))
        {
            MessageBox.Show("No keys entered");
            return;
        }

        List<String>? soundLocations = null;
        String? errorMessage = String.Empty;

        if (!SettingsForm.EditLoadXMLFile 
            && Conversions.SoundLocsArrayFromString(locationTextBox.Text, out soundLocations, out errorMessage))
        {
            if (soundLocations is not null && soundLocations.Any(x => String.IsNullOrWhiteSpace(x) || !File.Exists(x)))
            {
                MessageBox.Show("The file/one of the files does not exist");
                this.Close();
                return;
            }

            if (soundLocations == null)
            {
                MessageBox.Show(errorMessage);
                return;
            }
        }

        if (!Conversions.KeysArrayFromString(keysTextBox?.Text, out List<Keys>? keysList, out errorMessage)) 
            keysList = [];

        if (SettingsForm.EditLoadXMLFile)
        {
            if (EditIndex != -1)
            {
                settingsForm?.KeysLocationsListView?.Items[EditIndex].Text = keysTextBox?.Text;
                settingsForm?.KeysLocationsListView?.Items[EditIndex].SubItems[1].Text = locationTextBox.Text;

                settingsForm?.LoadXMLFilesList?[EditIndex].Keys = keysList;
                settingsForm?.LoadXMLFilesList?[EditIndex].XMLLocation = locationTextBox.Text;
            }
            else
            {
                ListViewItem item = new (keysTextBox?.Text);
                item.SubItems.Add(locationTextBox.Text);

                settingsForm?.KeysLocationsListView?.Items.Add(item);

                settingsForm?.LoadXMLFilesList?.Add(new XMLSettings.LoadXMLFile(keysList!, locationTextBox.Text));
            }
        }
        else
        {
            String? windowText = String.Empty;
            if (enableRestrictWindowCheckBox?.Checked == true && !String.IsNullOrEmpty(windowsComboBox?.SelectedItem as String)) windowText = windowsComboBox.SelectedItem as String;

            if (EditIndex > -1)
            {
                mainForm?.KeySoundsListView?.Items[EditIndex].Text = keysTextBox?.Text;
                mainForm?.KeySoundsListView?.Items[EditIndex].SubItems[1].Text = windowText;
                mainForm?.KeySoundsListView?.Items[EditIndex].SubItems[2].Text = locationTextBox.Text;

                mainForm?.SoundHotkeys[EditIndex] = new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!);
            }
            else
            {
                ListViewItem newItem = new (keysTextBox?.Text);
                newItem.SubItems.Add(windowText);
                newItem.SubItems.Add(locationTextBox.Text);

                mainForm?.KeySoundsListView?.Items.Add(newItem);

                mainForm?.SoundHotkeys.Add(new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!));
            }

            mainForm?.KeySoundsListView?.ListViewItemSorter = new Comparers.ListViewItemComparer(0);
            mainForm?.KeySoundsListView?.Sort();

            mainForm?.SoundHotkeys.Sort(new Comparers.SoundHotkeyComparer());

            mainForm?.KeysColumnHeader?.Width = -2;
            mainForm?.SoundLocationColumnHeader?.Width = -2;
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