using System.Diagnostics;
using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
using HASCore.Soundboard;

namespace HASCore.Forms;

public partial class AddEditHotkeyForm : Form
{
    internal List<String>? EditStrings = null;
    internal Int32 EditIndex = -1;
    private MainForm? MainForm;
    private SettingsForm? SettingsForm;
    private HashSet<Keys>? _lastDisplayedKeys = null;

    public AddEditHotkeyForm()
    {
        InitializeComponent();
        GlobalKeyboardHook.Initialize();
        GlobalKeyboardHook.KeysChanged += OnKeysChanged;
    }

    private void AddEditHotkeyForm_FormClosing(Object? sender, FormClosingEventArgs e)
    {
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
    }

    private void AddEditSoundKeys_Load(Object sender, EventArgs e)
    {
        if (SettingsForm.EditLoadXMLFile)
        {
            //hide window restriction
            gbWindowRestriction.Visible = false;
            this.MinimumSize = new Size(375, 170);
            this.Size = new Size(375, 170);

            SettingsForm = Application.OpenForms[1] as SettingsForm;

            this.Text = "Add/edit keys and XML location";

            if (EditIndex != -1)
            {
                tbKeys.Text = EditStrings?[0];
                tbLocation.Text = EditStrings?[1];
            }
        }
        else
        {
            MainForm = Application.OpenForms[0] as MainForm;

            labelLoc.Text += " (use a semi-colon (;) to seperate multiple locations)";

            LoadWindows();

            if (EditIndex != -1)
            {
                tbKeys.Text = EditStrings?[0];

                if (!String.IsNullOrEmpty(EditStrings?[1]))
                {
                    cbEnableRestrictWindow.Checked = true;

                    Int32 index = cbWindows.Items.IndexOf(EditStrings?[1]);

                    if (index != -1) cbWindows.SelectedIndex = index;
                    else
                    {
                        cbWindows.Items.Add(EditStrings?[1]!);
                        cbWindows.SelectedIndex = cbWindows.Items.Count - 1;
                    }
                }

                tbLocation.Text = EditStrings?[2];
            }

            
        }
    }

    private void LoadWindows()
    {
        cbWindows.Items.Clear();

        cbWindows.Items.Add(String.Empty);

        Process[] processlist = Process.GetProcesses();

        foreach (Process process in processlist)
        {
            if (!String.IsNullOrEmpty(process.MainWindowTitle))
            {
                cbWindows.Items.Add(process.MainWindowTitle);
            }
        }
    }

    private void OKButton_Click(Object sender, EventArgs e)
    {
        if (String.IsNullOrWhiteSpace(tbLocation.Text))
        {
            MessageBox.Show("Location is empty");
            return;
        }

        if (SettingsForm.EditLoadXMLFile && String.IsNullOrWhiteSpace(tbKeys.Text))
        {
            MessageBox.Show("No keys entered");
            return;
        }

        List<String>? soundLocations = null;
        String? errorMessage = String.Empty;

        if (!SettingsForm.EditLoadXMLFile && Conversions.SoundLocsArrayFromString(tbLocation.Text, out soundLocations, out errorMessage))
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

        if (!Conversions.KeysArrayFromString(tbKeys.Text, out List<Keys>? keysList, out errorMessage)) keysList = [];

        if (SettingsForm.EditLoadXMLFile)
        {
            if (EditIndex != -1)
            {
                SettingsForm?.KeysLocationsListView?.Items[EditIndex].Text = tbKeys.Text;
                SettingsForm?.KeysLocationsListView?.Items[EditIndex].SubItems[1].Text = tbLocation.Text;

                SettingsForm?.LoadXMLFilesList?[EditIndex].Keys = keysList;
                SettingsForm?.LoadXMLFilesList?[EditIndex].XMLLocation = tbLocation.Text;
            }
            else
            {
                ListViewItem item = new (tbKeys.Text);
                item.SubItems.Add(tbLocation.Text);

                SettingsForm?.KeysLocationsListView?.Items.Add(item);

                SettingsForm?.LoadXMLFilesList?.Add(new XMLSettings.LoadXMLFile(keysList!, tbLocation.Text));
            }
        }
        else
        {
            String? windowText = String.Empty;
            if (cbEnableRestrictWindow.Checked && !String.IsNullOrEmpty(cbWindows.SelectedItem as String)) windowText = cbWindows.SelectedItem as String;

            if (EditIndex > -1)
            {
                MainForm?.KeySoundsListView?.Items[EditIndex].Text = tbKeys.Text;
                MainForm?.KeySoundsListView?.Items[EditIndex].SubItems[1].Text = windowText;
                MainForm?.KeySoundsListView?.Items[EditIndex].SubItems[2].Text = tbLocation.Text;

                MainForm?.SoundHotkeys[EditIndex] = new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!);
            }
            else
            {
                ListViewItem newItem = new (tbKeys.Text);
                newItem.SubItems.Add(windowText);
                newItem.SubItems.Add(tbLocation.Text);

                MainForm?.KeySoundsListView?.Items.Add(newItem);

                MainForm?.SoundHotkeys.Add(new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!));
            }

            MainForm?.KeySoundsListView?.ListViewItemSorter = new Comparers.ListViewItemComparer(0);
            MainForm?.KeySoundsListView?.Sort();

            MainForm?.SoundHotkeys.Sort(new Comparers.SoundHotkeyComparer());

            MainForm?.KeysColumnHeader?.Width = -2;
            MainForm?.SoundLocationColumnHeader?.Width = -2;
        }

        this.Close();
    }

    private void CancelButton_Click(Object sender, EventArgs e)
    {
        this.Close();
    }

    private void BrowseSoundLocationButton_Click(Object sender, EventArgs e)
    {
        OpenFileDialog diag = new ()
        {
            Multiselect = !SettingsForm.EditLoadXMLFile,
            Filter = SettingsForm.EditLoadXMLFile ? "XML file containing keys and sounds|*.xml" : "Supported audio formats|*.mp3;*.m4a;*.wav;*.wma;*.ac3;*.aiff;*.mp2|All files|*.*"
        };

        if (diag.ShowDialog() == DialogResult.OK)
        {
            String text = String.Empty;

            for (Int32 i = 0; i < diag.FileNames.Length; i++)
            {
                String fileName = diag.FileNames[i];
                if (fileName != "") text += (i == 0 ? "" : ";") + fileName;
            }
            tbLocation.Text = text;
        }
    }



    private void EnableRestrictWindowCheckBox_CheckedChanged(Object sender, EventArgs e)
    {
        cbWindows.Enabled = cbEnableRestrictWindow.Checked;
        btnReloadWindows.Enabled = cbEnableRestrictWindow.Checked;
    }

    private void ReloadWindowsButton_Click(Object sender, EventArgs e)
    {
        LoadWindows();
    }

    private void OnKeysChanged(Object? sender, HashSet<Keys> currentKeys)
    {
        // On "Escape" we close the form gracefully.
        if (currentKeys.Contains(Keys.Escape))
            this.Close();

        // On "Backspace" we remove the contents of the textbox.
        if (currentKeys.Contains(Keys.Back))
        {
            // Usually, using this.Controls.OfType<T>() method would be fine,
            // but if/when we will add some GroupBox, Panel or other container,
            // and put a TextBox inside of it - this will break, so instead
            // we use the extension to get all the forms controls, even inside containers.
            IEnumerable<TextBox> controls = this.GetAllControls().OfType<TextBox>();
                
            foreach (TextBox tbControl in controls)
            {
                if (tbControl.Focused && tbControl.Parent is not null)
                {
                    tbControl.Text = String.Empty;
                    tbControl.Parent.Focus();
                    _lastDisplayedKeys = null;
                    break;     
                }
            }
            return;
        }

        if (currentKeys.Count == 0)
            return;

        if (_lastDisplayedKeys == null || currentKeys.Count > _lastDisplayedKeys.Count)
        {
            String newText = Conversions.KeysToString([.. currentKeys]);
            tbKeys.Text = newText;
            _lastDisplayedKeys = [.. currentKeys];
        }
    }
}