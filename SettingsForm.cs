// Declaring the using statement so we don't have to always prepend
// 'XMLSettings' to an already static fields of the class.
using static HASCore.XMLSettings;

namespace HASCore
{
    /// Description
    /// <summary>
    ///     <see cref="SettingsForm"/> class part for initializing the Object and its event handlers.
    /// </summary>
    public partial class SettingsForm : Form
    {
        // Using List so we can dynamically add and remove entries of XMLSettings.
        internal List<LoadXMLFile>? LoadXMLFilesList = CurrentSettings?.LoadXMLFiles; 
        // Flag for checking if we are currently editing XML presets.
        internal static Boolean EditLoadXMLFile = false;
        private HashSet<Keys>? _lastDisplayedKeys = null;
        private Int32 _lastFocusedIndex = -1;


        /// Description
        /// <summary>
        ///     <see cref="SettingsForm"/> constructor for initialization of class properties.
        /// </summary>
        public SettingsForm()
        {
            // Calling initialization procedure from another part of the class.
            InitializeComponent();
            GlobalKeyboardHook.Initialize();
            GlobalKeyboardHook.KeysChanged += OnKeysChanged;
            
            // Iterating through list of XML presets.
            for (Int32 i = 0; i < LoadXMLFilesList?.Count; i++)
            {
                // Checking if there are any keys in the hotkeys set.
                Boolean correctKeysLength = LoadXMLFilesList[i].Keys?.Count > 0;
                // Checking if there is a path to a file in XML preset.
                Boolean locationNotEmpty = !String.IsNullOrWhiteSpace(LoadXMLFilesList[i].XMLLocation);

                // Remove entry if location is empty.
                if (!correctKeysLength && !locationNotEmpty) 
                {
                    // Removing the item at current 'i' value,
                    // then decreasing it so we have actual count
                    // of items in the list.
                    LoadXMLFilesList.RemoveAt(i--);
                    continue;
                }

                // Adding a new item to ListView with set hotkeys.
                ListViewItem item = new(correctKeysLength && LoadXMLFilesList[i].Keys is not null ? String.Join("+", LoadXMLFilesList[i].Keys!) : "");
                // Adding a path to XML preset that should be enabled through hotkeys.
                item.SubItems.Add(locationNotEmpty ? LoadXMLFilesList[i].XMLLocation : "");
                // Adding the item to the view.
                KeysLocationsListView?.Items.Add(item);
            }

            // Checking if there are any hotkeys mapped to specific actions.
            ToggleKeysTextBox?.Text = Helper.KeysToString(CurrentSettings?.EnableSoundboardKeys ?? []);
            StopKeysTextBox?.Text = Helper.KeysToString(CurrentSettings?.StopSoundKeys ?? []);

            // Also checking if there are any settings checked.
            MinimizeToTrayCheckBox?.Checked = CurrentSettings is not null 
                && CurrentSettings.MinimizeToTray.HasValue 
                && CurrentSettings.MinimizeToTray.Value;
            PlayOverEachotherCheckBox?.Checked = CurrentSettings is not null 
                && CurrentSettings.PlayOverEachother.HasValue 
                && CurrentSettings.PlayOverEachother.Value;
            RepeatOnHoldCheckBox?.Checked = CurrentSettings is not null 
                && CurrentSettings.RepeatOnHold.HasValue 
                && CurrentSettings.RepeatOnHold.Value;
        }

        private void SettingsForm_FormClosing(Object? sender, FormClosingEventArgs e)
        {
            GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
        }

        /// Description
        /// <summary>
        ///     Event handler for the 'Click' event of <see cref="AddButton">AddButton</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void AddButton_Click(Object? sender, EventArgs e)
        {
            // Setting the flag to true, signaling that 
            // we are currently editing the list of XML presets.
            EditLoadXMLFile = true;

            // Initializing the new form of editing XML dynamically.
            AddEditHotkeyForm form = new();
            // Then creating a window of that form so the user can
            // edit his presets through a modal dialog.
            form.ShowDialog();

            // Resetting the flag to false, indicating that
            // we have finished editing the presets.
            EditLoadXMLFile = false;
        }

        /// Description
        /// <summary>
        ///     Event handler for the 'Click' event of <see cref="EditButton">EditButton</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void EditButton_Click(Object? sender, EventArgs e)
        {
            // Checking if there any selected entires of the list.
            if (KeysLocationsListView?.SelectedIndices.Count > 0)
            {
                // Setting the flag to true, signaling that 
                // we are currently editing the list of XML presets.
                EditLoadXMLFile = true;

                // Initializing the new form of editing XML dynamically
                // with parameters of the current entry that we are editing.
                AddEditHotkeyForm form = new()
                {
                    EditIndex = KeysLocationsListView.SelectedIndices[0],
                    EditStrings = [KeysLocationsListView.SelectedItems[0].Text, KeysLocationsListView.SelectedItems[0].SubItems[1].Text]
                };

                // Then create a window of that form so the user can
                // edit his presets through a modal dialog.
                form.ShowDialog();

                // Resetting the flag to false, indicating that
                // we have finished editing the presets.
                EditLoadXMLFile = false;
            }
        }

        /// Descriptions
        /// <summary>
        ///     Event handler for the 'Click' event of <see cref="RemoveButton">RemoveButton</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void RemoveButton_Click(Object? sender, EventArgs e)
        {
            // Checking if there are any selected entries.
            if (KeysLocationsListView?.SelectedIndices.Count > 0 
            && MessageBox.Show("Are you sure?", "Deletion of XML preset", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Passing the index of the current selected entry.
                Int32 index = KeysLocationsListView.SelectedIndices[0];
                // Removing the item in both ListView and local List.
                KeysLocationsListView.Items.RemoveAt(index);
                LoadXMLFilesList?.RemoveAt(index);
            }
        }
        
        /// Description
        /// <summary>
        ///     Event handler for the 'Click' event of <see cref="OKButton">OKButton</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void OKButton_Click(Object? sender, EventArgs e)
        {
            // Initializing the array of keys for hotkeys to toggle the soundboard's on/off state.
            List<Keys>? toggleKeysArr = [];
            // Also initializing the array of keys for hotkeys to stop all sounds from playing.
            List<Keys>? stopKeysArr = [];
            // Creating a local variable for saving the error message.
            String? error = String.Empty;

            try
            {
                // Checking if we are trying to load any keys, and if that's so,
                // validating that there is a path and it does exist.
                if (LoadXMLFilesList?.Count == 0 || (LoadXMLFilesList is not null && LoadXMLFilesList
                    .All(x => x.Keys?.Count > 0 && !String.IsNullOrWhiteSpace(x.XMLLocation) && File.Exists(x.XMLLocation))))
                {
                    // Trying to get array of Keys, and if we encounter an error,
                    // we throw an exception with the error specified.
                    if ((!String.IsNullOrEmpty(StopKeysTextBox?.Text) 
                        && !Helper.KeysArrayFromString(StopKeysTextBox?.Text, out stopKeysArr, out error))
                        || (!String.IsNullOrEmpty(ToggleKeysTextBox?.Text) 
                        && !Helper.KeysArrayFromString(ToggleKeysTextBox?.Text, out toggleKeysArr, out error)))
                        throw new ArgumentException("Keys mismatch");
                    
                    // Assigning values to the fields of settings.
                    CurrentSettings.EnableSoundboardKeys = toggleKeysArr ?? [];
                    CurrentSettings.StopSoundKeys = stopKeysArr ?? [];
                    CurrentSettings.LoadXMLFiles = [.. LoadXMLFilesList];
                    CurrentSettings.MinimizeToTray = MinimizeToTrayCheckBox?.Checked ?? false;
                    CurrentSettings.PlayOverEachother = PlayOverEachotherCheckBox?.Checked ?? false;
                    CurrentSettings.RepeatOnHold = RepeatOnHoldCheckBox?.Checked ?? false;
                    
                    // Calling the procedure to save changes.
                    SaveSoundboardSettingsXML();

                    // After we completed all the changes, close the form.
                    this.Close();
                }
                // If there are more than 0 presets and their paths are invalid,
                // we show a message with explanation why can't we save current changes.
                else MessageBox.Show("One or more entries either have no keys added, the location is empty, or the file the location points to does not exist");
            }
            // If the exception came from trying to get array of Keys,
            // we show a message box with exception's message 
            // and message inside the 'error' variable.
            catch (ArgumentException argEx)
            {
                MessageBox.Show($"{argEx.Message}: {error}");
            }
            // Otherwise, we show a generic exception error message.
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unknown exception has occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// Description
        /// <summary>
        ///     Event handler for the 'Click' event of <see cref="CancelButton">CancelButton</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event.</param>
        private void CancelButton_Click(Object? sender, EventArgs e) 
            => this.Close(); // Just closing the form.
        
        /// Description
        /// <summary>
        ///     Event handler for the 'Mouse Double-Click' event of <see cref="KeysLocationsListView">KeysLocationsListView</see>.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="e">Arguments of the event for the mouse control events.</param>
        private void KeysLocationsListView_MouseDoubleClick(Object? sender, MouseEventArgs e) 
            => EditButton_Click(sender, e); // Rerouting to an event of "EditButton"

        private void OnKeysChanged(Object? sender, HashSet<Keys> currentKeys)
        {
            TextBox? selectedTextBox = this.Controls
                .OfType<TextBox>()
                .FirstOrDefault(tb => tb.Focused);
            
            if (selectedTextBox is null)
                return;

            Int32 currentIndex = this.Controls.IndexOf(selectedTextBox);
            
            if (_lastFocusedIndex != currentIndex)
            {
                _lastDisplayedKeys = null;
                _lastFocusedIndex = currentIndex;
            }
            
            if (currentKeys.Count == 0)
            {
                _lastDisplayedKeys = null;
                return;
            }

            if (currentKeys.Contains(Keys.Back))
            {
                selectedTextBox.Text = String.Empty;
                _lastDisplayedKeys = null;
                return;    
            }

            if (_lastDisplayedKeys == null || currentKeys.Count > _lastDisplayedKeys.Count)
            {
                String newText = Helper.KeysToString(currentKeys);
                selectedTextBox?.Text = newText;
                _lastDisplayedKeys = [.. currentKeys];
            }
        }
    }
}