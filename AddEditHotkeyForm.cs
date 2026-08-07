using System.Collections;
using System.Diagnostics;

namespace HASCore
{
    public partial class AddEditHotkeyForm : Form
    {
        internal class ListViewItemComparer : IComparer
        {
            private readonly Int32 Column;

            public ListViewItemComparer()
            {
                Column = 0;
            }

            public ListViewItemComparer(Int32 column)
            {
                Column = column;
            }

            public Int32 Compare(Object? x, Object? y)
            {
                return x is not null && y is not null ? String.Compare(((ListViewItem)x).SubItems[Column].Text, ((ListViewItem)y).SubItems[Column].Text) : 0;
            }
        }

        internal List<String>? EditStrings = null;
        internal Int32 EditIndex = -1;
        // private Int32 lastAmountPressed = 0;
        private MainForm? MainForm;
        private SettingsForm? SettingsForm;

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

            cbWindows.Items.Add("");

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

            if (!SettingsForm.EditLoadXMLFile && Helper.SoundLocsArrayFromString(tbLocation.Text, out soundLocations, out errorMessage))
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

            if (!Helper.KeysArrayFromString(tbKeys.Text, out List<Keys>? keysList, out errorMessage)) keysList = [];

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
                    ListViewItem item = new(tbKeys.Text);
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
                    ListViewItem newItem = new(tbKeys.Text);
                    newItem.SubItems.Add(windowText);
                    newItem.SubItems.Add(tbLocation.Text);

                    MainForm?.KeySoundsListView?.Items.Add(newItem);

                    MainForm?.SoundHotkeys.Add(new XMLSettings.SoundHotkey(keysList!, windowText!, soundLocations!));
                }

                MainForm?.KeySoundsListView?.ListViewItemSorter = new ListViewItemComparer(0);
                MainForm?.KeySoundsListView?.Sort();

                MainForm?.SoundHotkeys.Sort(delegate (XMLSettings.SoundHotkey x, XMLSettings.SoundHotkey y)
                {
                    if (x.Keys == null && y.Keys == null) return 0;
                    else if (x.Keys == null) return -1;
                    else if (y.Keys == null) return 1;
                    else return Helper.KeysToString(x.Keys).CompareTo(Helper.KeysToString(y.Keys));
                });

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
            OpenFileDialog diag = new()
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

        private HashSet<Keys>? _lastDisplayedKeys = null;

        private void OnKeysChanged(Object? sender, HashSet<Keys> currentKeys)
        {
            // Обработка Backspace – очищаем поле и сбрасываем состояние
            if (currentKeys.Contains(Keys.Back))
            {
                tbKeys.Text = String.Empty;
                _lastDisplayedKeys = null;
                return;
            }

            // Если клавиш нет – ничего не делаем
            if (currentKeys.Count == 0)
                return;

            // Обновляем текст только если количество клавиш увеличилось
            // или если это первая комбинация
            if (_lastDisplayedKeys == null || currentKeys.Count > _lastDisplayedKeys.Count)
            {
                String newText = Helper.KeysToString([.. currentKeys]);
                tbKeys.Text = newText;
                _lastDisplayedKeys = [.. currentKeys];
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
    }
}
