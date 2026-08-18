using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
using HASCore.Soundboard;
// Declaring the using statement so we don't have to always prepend
// 'XMLSettings' to an already static fields of the class.
using static HASCore.Soundboard.XMLSettings;

namespace HASCore.Forms;

/// Description
/// <summary>
///     <see cref="SettingsForm"/> class part responsible for the form's logic,
///     event handling, and interaction with the global keyboard hook.
/// </summary>
/// 
/// Additional information
/// <remarks>
///     This partial class works together with the design's code
///     (<see cref="InitializeComponent"/>) to provide the full settings dialog.
///     It manages the list of XML presets (hotkeys + file paths) and allows
///     adding, editing, and removing them. It also captures global keyboard input
///     via <see cref="GlobalKeyboardHook"/> to let the user define hotkey combinations.
///     The form loads existing settings from <see cref="CurrentSettings"/> and
///     saves them back on confirmation.
/// </remarks>
public partial class SettingsForm : Form
{
    #region Private Fields

    /// Description
    /// <summary>
    ///     Stores the last set of pressed keys captured by the global hook,
    ///     used to avoid duplicate updates when the key set changes.
    /// </summary>
    private HashSet<Keys>? _lastProcessedKeys = null;

    /// Description
    /// <summary>
    ///     Tracks the index of the last text box that had focus,
    ///     so we know which text box to update when keys are pressed.
    /// </summary>
    private Int32 _lastFocusedIndex = -1;

    /// Description
    /// <summary>
    ///     An enumerable for handling the focus of the <see cref="TextBox"/>
    ///     controls on the form.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     This field is initialized in the constructor using the
    ///     <see cref="ControlExtensions.GetAllControls"/> extension method,
    ///     which recursively retrieves all controls including those inside
    ///     containers like <see cref="GroupBox"/> or <see cref="Panel"/>.
    /// </remarks>
    private IEnumerable<TextBox> _tbControls; 

    /// Description
    /// <summary>
    ///     The in-memory list of XML preset entries that is bound to the
    ///     <see cref="KeysLocationsListView"/>. Changes to this list are
    ///     reflected in the UI and vice versa.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     This list is initialized from <see cref="CurrentSettings.LoadXMLFiles"/>
    ///     when the form loads. Any modifications (add, edit, remove) are applied
    ///     to this list, and upon clicking <see cref="okButton"/>, the list is
    ///     written back to <see cref="CurrentSettings"/> and saved to disk.
    /// </remarks>
    internal List<LoadXMLFile>? LoadXMLFilesList = CurrentSettings?.LoadXMLFiles;

    /// Description
    /// <summary>
    ///     Flag indicating whether the user is currently editing an XML preset
    ///     via the <see cref="AddEditHotkeyForm"/> dialog.
    /// </summary>
    /// 
    /// Additional information
    /// <remarks>
    ///     This flag is static because it needs to be accessible from the
    ///     <see cref="AddEditHotkeyForm"/> to coordinate operations between
    ///     the two forms. It is set to <c>true</c> when <see cref="addButton"/>
    ///     or <see cref="editButton"/> is clicked, and reset to <c>false</c> after
    ///     the dialog closes.
    /// </remarks>
    internal static Boolean EditLoadXMLFile = false;

    #endregion

    #region Constructor

    /// Description
    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsForm"/> class.
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
    ///             <description>Initializes <see cref="GlobalKeyboardHook"/> and subscribes to its <see cref="GlobalKeyboardHook.KeysChanged"/> event.</description>
    ///         </item>
    ///         <item>
    ///             <term>Load existing presets</term>
    ///             <description>Populates <see cref="KeysLocationsListView"/> from <see cref="LoadXMLFilesList"/>.</description>
    ///         </item>
    ///         <item>
    ///             <term>Load settings</term>
    ///             <description>Fills the text boxes and check boxes with values from <see cref="CurrentSettings"/>.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public SettingsForm()
    {
        // Calling initialization procedure from another part of the class.
        InitializeComponent();

        // Initializing the global WinAPI keyboard hook for processing input,
        // so we can subscribe the event handler "KeysChanged" to an event 
        // "OnKeysChanged" that we can do some actions on pressing the hotkeys.
        GlobalKeyboardHook.Initialize();
        GlobalKeyboardHook.KeysChanged += OnKeysChanged;

        // Usually, using this.Controls.OfType<T>() method would be fine,
        // but if/when we will add some GroupBox, Panel or other container,
        // and put a TextBox inside of it - this will break, so instead
        // we use the extension to get all the forms controls, even inside containers.
        _tbControls = this.GetAllControls().OfType<TextBox>();

        if (LoadXMLFilesList is not null)
        {
            // Iterating through list of XML presets.
            foreach (LoadXMLFile file in LoadXMLFilesList)
            {
                // Checking if there are any keys in the hotkeys set.
                Boolean correctKeysLength = file.Keys?.Count > 0;
                // Checking if there is a path to a file in XML preset.
                Boolean locationNotEmpty = !String.IsNullOrWhiteSpace(file.XMLLocation);

                // Remove entry if location is empty.
                if (!correctKeysLength && !locationNotEmpty)
                {
                    LoadXMLFilesList.Remove(file);
                    continue;
                }

                // Adding a new item to ListView with set hotkeys.
                ListViewItem item = new(
                    correctKeysLength && file.Keys is not null
                    ? String.Join("+", file.Keys)
                    : String.Empty
                );

                // Adding a path to XML preset that should be enabled through hotkeys.
                item.SubItems.Add(locationNotEmpty ? file.XMLLocation : String.Empty);
                // Adding the item to the view.
                KeysLocationsListView?.Items.Add(item);
            }
        }

        // Checking if there are any hotkeys mapped to specific actions.
        toggleKeysTextBox?.Text = Conversions.KeysToString(CurrentSettings?.EnableSoundboardKeys ?? []);
        stopKeysTextBox?.Text = Conversions.KeysToString(CurrentSettings?.StopSoundKeys ?? []);

        // Also checking if there are any settings checked.
        minimizeToTrayCheckBox?.Checked = CurrentSettings is not null
            && CurrentSettings.MinimizeToTray.HasValue
            && CurrentSettings.MinimizeToTray.Value;

        playOverEachotherCheckBox?.Checked = CurrentSettings is not null
            && CurrentSettings.PlayOverEachother.HasValue
            && CurrentSettings.PlayOverEachother.Value;

        repeatOnHoldCheckBox?.Checked = CurrentSettings is not null
            && CurrentSettings.RepeatOnHold.HasValue
            && CurrentSettings.RepeatOnHold.Value;
    }

    #endregion

    #region Event Handlers

    /// Description
    /// <summary>
    ///     Handles the <see cref="Form.FormClosing"/> event to clean up resources.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event (the form itself).</param>
    /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
    private void SettingsForm_FormClosing(Object? sender, FormClosingEventArgs e) =>
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="addButton">AddButton</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Opens the <see cref="AddEditHotkeyForm"/> in "Add" mode, allowing the user
    ///     to define a new XML preset. After the dialog closes, the <see cref="EditLoadXMLFile"/>
    ///     flag is reset.
    /// </remarks>
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
    ///     Handles the <see cref="Button.Click"/> event of <see cref="editButton">EditButton</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Opens the <see cref="AddEditHotkeyForm"/> in "Edit" mode, pre‑populated with
    ///     the selected preset's data. The form is only opened if exactly one item is
    ///     selected in <see cref="KeysLocationsListView"/>.
    /// </remarks>
    private void EditButton_Click(Object? sender, EventArgs e)
    {
        // Checking if there any selected entries of the list.
        if (KeysLocationsListView?.SelectedIndices.Count > 0)
        {
            // Setting the flag to true, signaling that
            // we are currently editing the list of XML presets.
            EditLoadXMLFile = true;

            // Build the record with values from ListViewItem.
            SoundHotkeyEditData editData = new(
                Keys: KeysLocationsListView.SelectedItems[0].Text,
                SoundLocation: KeysLocationsListView.SelectedItems[0].SubItems[1].Text
            );

            // Initializing the new form of editing XML dynamically
            // with parameters of the current entry that we are editing.
            AddEditHotkeyForm form = new()
            {
                EditIndex = KeysLocationsListView.SelectedIndices[0],
                EditData = editData,
            };

            // Then create a window of that form so the user can
            // edit his presets through a modal dialog.
            form.ShowDialog();

            // Resetting the flag to false, indicating that
            // we have finished editing the presets.
            EditLoadXMLFile = false;
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="removeButton">RemoveButton</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     After confirming with the user, removes the selected preset from both the
    ///     <see cref="KeysLocationsListView"/> and the underlying <see cref="LoadXMLFilesList"/>.
    /// </remarks>
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
    ///     Handles the <see cref="Button.Click"/> event of <see cref="okButton">OKButton</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Validates all user input, updates <see cref="CurrentSettings"/> with the
    ///     current form values, saves the settings to disk via
    ///     <see cref="SaveSoundboardSettingsXML"/>, and closes the form.
    ///     If validation fails, an appropriate error message is shown.
    /// </remarks>
    private void OKButton_Click(Object? sender, EventArgs e)
    {
        // Initializing the list of keys for hotkeys to toggle the soundboard's on/off state.
        List<Keys>? toggleKeysArr = [];
        // Also initializing the list of keys for hotkeys to stop all sounds from playing.
        List<Keys>? stopKeysArr = [];
        // Creating a local variable for saving the error message.
        String? error = String.Empty;

        try
        {
            // Checking if we are trying to load any keys, and if that's so,
            // validating that there is a path and it does exist.
            if (LoadXMLFilesList?.Count == 0 || (LoadXMLFilesList is not null && LoadXMLFilesList
                .All(x => x.Keys?.Count > 0 
                    && !String.IsNullOrWhiteSpace(x.XMLLocation) 
                    && File.Exists(x.XMLLocation)
            )))
            {
                // Trying to get array of Keys, and if we encounter an error,
                // we throw an exception with the error specified.
                if ((!String.IsNullOrEmpty(stopKeysTextBox?.Text)
                    && !Conversions.KeysArrayFromString(stopKeysTextBox?.Text, out stopKeysArr, out error))
                    || (!String.IsNullOrEmpty(toggleKeysTextBox?.Text)
                    && !Conversions.KeysArrayFromString(toggleKeysTextBox?.Text, out toggleKeysArr, out error)))
                    throw new ArgumentException("Keys mismatch");

                // Assigning values to the fields of settings.
                CurrentSettings.LoadXMLFiles            = [.. LoadXMLFilesList];
                CurrentSettings.EnableSoundboardKeys    = toggleKeysArr ?? [];
                CurrentSettings.StopSoundKeys           = stopKeysArr ?? [];
                CurrentSettings.MinimizeToTray          = minimizeToTrayCheckBox?.Checked ?? false;
                CurrentSettings.PlayOverEachother       = playOverEachotherCheckBox?.Checked ?? false;
                CurrentSettings.RepeatOnHold            = repeatOnHoldCheckBox?.Checked ?? false;

                // Calling the procedure to save changes.
                SaveSoundboardSettingsXML();

                // After we completed all the changes, close the form.
                this.Close();
            }
            // If there are more than 0 presets and their paths are invalid,
            // we show a message with explanation why can't we save current changes.
            else MessageBox.Show(
                caption: "Error has occured",
                text: "One or more entries either have no keys added, the location is empty, or the file the location points to does not exist",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        // If the exception came from trying to get array of Keys,
        // we show a message box with exception's message
        // and message inside the 'error' variable.
        catch (ArgumentException argEx)
        {
            MessageBox.Show(
                caption: "Error has occured",
                text: $"{argEx.Message}: {error}",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        // Otherwise, we show a generic exception error message.
        catch (Exception ex)
        {
            MessageBox.Show(
                caption: "Unknown exception has occured",
                text: ex.Message,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
    }

    /// Description
    /// <summary>
    ///     Handles the <see cref="Button.Click"/> event of <see cref="cancelButton">CancelButton</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Closes the form without saving any changes.
    /// </remarks>
    private void CancelButton_Click(Object? sender, EventArgs e)
        => this.Close(); // Just closing the form.

    /// Description
    /// <summary>
    ///     Handles the <see cref="Control.MouseDoubleClick"/> event of
    ///     <see cref="KeysLocationsListView">KeysLocationsListView</see>.
    /// </summary>
    /// 
    /// Parameters
    /// <param name="sender">The source of the event (the list view).</param>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    /// 
    /// Additional information
    /// <remarks>
    ///     Double‑clicking an entry in the list view has the same effect as pressing
    ///     the <see cref="editButton"/> – it opens the edit dialog for that entry.
    /// </remarks>
    private void KeysLocationsListView_MouseDoubleClick(Object? sender, MouseEventArgs e)
        => EditButton_Click(sender, e); // Rerouting to an event of "EditButton"

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
    ///     This method updates the text of the currently focused text box
    ///     (<see cref="stopKeysTextBox"/> or <see cref="toggleKeysTextBox"/>)
    ///     with the string representation of the pressed key combination.
    ///     It handles the <see cref="Keys.Back"/> key to clear the text box.
    ///     The method also manages the internal state (<see cref="_lastProcessedKeys"/>
    ///     and <see cref="_lastFocusedIndex"/>) to avoid unnecessary updates.
    /// </remarks>
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

        // Get the index of the currently focused textbox.
        Int32 currentIndex = this.Controls.IndexOf(readOnlyTextBox);

        // If the index doesn't match - clear previous combination
        // of keys and set the index to the current one.
        if (_lastFocusedIndex != currentIndex)
        {
            _lastProcessedKeys = null;
            _lastFocusedIndex = currentIndex;
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

        // If the hotkey textbox is focused - we remove the current keys and
        // unfocus the textbox so we don't capture the keys outside the focus.
        if (currentKeys.Contains(Keys.Back))
        {
            readOnlyTextBox?.Text = String.Empty;
            _lastProcessedKeys = null;
            return;
        }

        // If there are no pressed keys at the moment, the count of keys is more or equal
        // to the previous count, or if the currently pressed keys are not equal to previous
        // ones, then we update the keys, as long the textbox for hotkeys is focused.
        if (_lastProcessedKeys == null 
            || (currentKeys.Count >= _lastProcessedKeys.Count && !_lastProcessedKeys.SetEquals(currentKeys)))
        {
            _lastProcessedKeys = [.. currentKeys];
            readOnlyTextBox?.Text = Conversions.KeysToString(currentKeys);
        }
    }

    #endregion
}