namespace JNSoundboardCore
{
    public partial class SettingsForm : Form
    {
        internal List<XMLSettings.LoadXMLFile> loadXMLFilesList = new List<XMLSettings.LoadXMLFile>(XMLSettings.soundboardSettings.LoadXMLFiles); //list so can dynamically add/remove

        internal static bool addingEditingLoadXMLFile = false;

        public SettingsForm()
        {
            InitializeComponent();
                        
            for (int i = 0; i < loadXMLFilesList.Count; i++)
            {
                bool keysLengthCorrect = loadXMLFilesList[i].Keys.Length > 0;
                bool xmlLocationUnempty = !string.IsNullOrWhiteSpace(loadXMLFilesList[i].XMLLocation);

                if (!keysLengthCorrect && !xmlLocationUnempty) //remove if empty
                {
                    loadXMLFilesList.RemoveAt(i);
                    i--;
                    continue;
                }

                ListViewItem item = new ListViewItem((keysLengthCorrect ? string.Join("+", loadXMLFilesList[i].Keys) : ""));
                item.SubItems.Add((xmlLocationUnempty ? loadXMLFilesList[i].XMLLocation : ""));

                KeysLocationsListView.Items.Add(item);
            }

            StopKeysTextBox.Text = Helper.KeysToString(XMLSettings.soundboardSettings.StopSoundKeys);
            ToggleKeysTextBox.Text = Helper.KeysToString(XMLSettings.soundboardSettings.EnableSoundboardKeys);

            MinimizeToTrayCheckBox.Checked = XMLSettings.soundboardSettings.MinimizeToTray;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            addingEditingLoadXMLFile = true;

            AddEditHotkeyForm form = new AddEditHotkeyForm();
            form.ShowDialog();

            addingEditingLoadXMLFile = false;
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (KeysLocationsListView.SelectedIndices.Count > 0)
            {
                addingEditingLoadXMLFile = true;

                AddEditHotkeyForm form = new AddEditHotkeyForm();

                form.editIndex = KeysLocationsListView.SelectedIndices[0];
                form.editStrings = [KeysLocationsListView.SelectedItems[0].Text, KeysLocationsListView.SelectedItems[0].SubItems[1].Text];

                form.ShowDialog();

                addingEditingLoadXMLFile = false;
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (KeysLocationsListView.SelectedIndices.Count > 0 && MessageBox.Show("Are you sure?", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int index = KeysLocationsListView.SelectedIndices[0];

                KeysLocationsListView.Items.RemoveAt(index);
                loadXMLFilesList.RemoveAt(index);
            }
        }
        
        private void OKButton_Click(object sender, EventArgs e)
        {
            Keys[] stopSoundKeysArr = null;
            Keys[] enableSoundboardKeysArr = null;

            if (
                (string.IsNullOrWhiteSpace(StopKeysTextBox.Text)
                || Helper.KeysArrayFromString(StopKeysTextBox.Text, out stopSoundKeysArr, out string error))
                &&
                (string.IsNullOrWhiteSpace(ToggleKeysTextBox.Text)
                || Helper.KeysArrayFromString(ToggleKeysTextBox.Text, out enableSoundboardKeysArr, out error)))
            {
                if (loadXMLFilesList.Count == 0 || loadXMLFilesList.All(x => x.Keys.Length > 0 && !string.IsNullOrWhiteSpace(x.XMLLocation) && File.Exists(x.XMLLocation)))
                {
                    XMLSettings.soundboardSettings.EnableSoundboardKeys = (enableSoundboardKeysArr == null ? [] : enableSoundboardKeysArr);

                    XMLSettings.soundboardSettings.StopSoundKeys = (stopSoundKeysArr == null ? [] : stopSoundKeysArr);

                    XMLSettings.soundboardSettings.LoadXMLFiles = loadXMLFilesList.ToArray();

                    XMLSettings.soundboardSettings.MinimizeToTray = MinimizeToTrayCheckBox.Checked;

                    XMLSettings.SaveSoundboardSettingsXML();

                    this.Close();
                }
                else MessageBox.Show("One or more entries either have no keys added, the location is empty, or the file the location points to does not exist");
            }
            else if (error != "")
            {
                MessageBox.Show(error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lvKeysLocs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EditButton_Click(null, null);
        }

        private void StopKeysTextBox_Enter(object sender, EventArgs e)
        {
            MainTimer.Enabled = true;
        }

        private void StopKeysTextBox_Leave(object sender, EventArgs e)
        {
            MainTimer.Enabled = false;
        }

        private void ToggleKeysTextBox_Enter(object sender, EventArgs e)
        {
            MainTimer.Enabled = true;
        }

        private void ToggleKeysTextBox_Leave(object sender, EventArgs e)
        {
            MainTimer.Enabled = false;
        }


        int lastAmountPressed = 0;

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            int amountPressed = 0;

            if (Keyboard.IsKeyDown(Keys.Escape))
            {
                lastAmountPressed = 50;

                StopKeysTextBox.Text = "";
            }
            else
            {
                List<Keys> pressedKeys = new List<Keys>();

                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (Keyboard.IsKeyDown(key))
                    {
                        amountPressed++;
                        pressedKeys.Add(key);
                    }
                }

                if (amountPressed > lastAmountPressed)
                {
                    if (StopKeysTextBox.Focused) StopKeysTextBox.Text = Helper.KeysToString(pressedKeys.ToArray());
                    if (ToggleKeysTextBox.Focused) ToggleKeysTextBox.Text = Helper.KeysToString(pressedKeys.ToArray());
                }

                lastAmountPressed = amountPressed;
            }
        }
    }
}