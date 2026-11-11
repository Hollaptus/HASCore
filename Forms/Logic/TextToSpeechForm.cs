using System.Speech.Synthesis;
using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
// Declaring the using statement so we don't have to always prepend
// 'XMLSettings' to the static members of the class.
using static HASCore.Soundboard.XMLSettings;

namespace HASCore.Forms;

/// Description
/// <summary>
///		Provides text-to-speech WAV generation and the ability to register the
///		generated sound as a soundboard hotkey.
/// </summary>
///
/// Additional information
/// <remarks>
///		This partial class works together with the design's code
///		(<see cref="InitializeComponent"/>) to provide the TTS dialog.
///		It captures global keyboard input via <see cref="GlobalKeyboardHook"/>
///		to let the user define hotkey combinations, synthesizes speech to a WAV
///		file that can be saved standalone, and optionally adds the result as a
///		sound hotkey that is persisted to the settings XML.
/// </remarks>
public partial class TextToSpeechForm : Form
{
	/// Description
	/// <summary>
	///		Reference to the owning <see cref="MainForm"/>, used to add newly
	///		created TTS hotkeys to its <see cref="XMLSettings.SoundHotkey"/>
	///		collection and refresh the key sounds list view.
	/// </summary>
	private MainForm? _mainForm;

	/// Description
	/// <summary>
	///		The <see cref="SpeechSynthesizer"/> instance used to render spoken
	///		text into a WAV file during sound generation.
	/// </summary>
	private SpeechSynthesizer? _synth;

	/// Description
	/// <summary>
	///		For tracking the last processed key combination captured by the
	///		global keyboard hook, used to avoid duplicate updates to the hotkey
	///		text box.
	/// </summary>
	private HashSet<Keys>? _lastProcessedKeys = null;

	/// Description
	/// <summary>
	///		An enumerable of all <see cref="TextBox"/> controls on the form,
	///		including those nested inside containers, used to identify which
	///		read-only text box currently has focus for hotkey input.
	/// </summary>
	/// 
	/// Additional information
	/// <remarks>
	///		This field is initialized in the constructor using the
	///		<see cref="ControlExtensions.GetAllControls"/> extension method,
	///		which recursively retrieves all controls including those inside
	///		containers like <see cref="GroupBox"/> or <see cref="Panel"/>.
	/// </remarks>
	private IEnumerable<TextBox> _tbControls;

	/// Description
	/// <summary>
	///		Initializes a new instance of the <see cref="TextToSpeechForm"/> class.
	/// </summary>
	///
	/// Additional information
	/// <remarks>
	///		This constructor performs the following steps:
	///		<list type="number">
	///			<item>
	///				<term>Component initialization</term>
	///				<description>Calls <see cref="InitializeComponent"/> to set up the UI.</description>
	///			</item>
	///			<item>
	///				<term>Global hook setup</term>
	///				<description>Initializes <see cref="GlobalKeyboardHook"/> and subscribes to its <see cref="GlobalKeyboardHook.KeysChanged"/> event.</description>
	///			</item>
	///			<item>
	///				<term>Text box discovery</term>
	///				<description>Populates <see cref="_tbControls"/> with all <see cref="TextBox"/> controls on the form.</description>
	///			</item>
	///		</list>
	/// </remarks>
	public TextToSpeechForm()
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
	}

	/// Description
	/// <summary>
	///		Handles the <see cref="Form.FormClosing"/> event to clean up the
	///		global keyboard hook subscription.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event (the form itself).</param>
	/// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
	public void TextToSpeechForm_FormClosing(Object? sender, FormClosingEventArgs e)
		=> GlobalKeyboardHook.KeysChanged -= OnKeysChanged; // Unsubscribing from the global hook to avoid leaking the event handler.

	/// Description
	/// <summary>
	///		Handles the <see cref="Form.Load"/> event to capture a reference to
	///		the owning <see cref="MainForm"/>.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event (the form itself).</param>
	/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
	private void TTS_Load(Object? sender, EventArgs e)
		=> _mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault(); // Resolving the MainForm instance from the application's open forms.

	/// Description
	/// <summary>
	///		Handles the <see cref="Button.Click"/> event of the browse button,
	///		opening a folder browser dialog so the user can choose where to save
	///		the generated WAV file.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
	private void BrowseFolderLocationButton_Click(Object? sender, EventArgs e)
	{
		// Creating a new instance of the file browser dialog.
		FolderBrowserDialog diag = new ();

		// Showing the folder browser dialog and applying the chosen
		// path when the user confirms the selection.
		if (diag.ShowDialog() == DialogResult.OK)
			saveLocationTextBox?.Text = diag.SelectedPath;
	}

	/// Description
	/// <summary>
	///		Handles the <see cref="Button.Click"/> event of the create WAV button,
	///		synthesizing the input text to a WAV file at the chosen save location.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
	private void CreateWAVButton_Click(Object? sender, EventArgs e)
	{
		// Validating that all required fields are filled in and that the save
		// folder exists before attempting to generate the WAV file.
		if (!String.IsNullOrWhiteSpace(inputTextBox?.Text) && !String.IsNullOrWhiteSpace(saveLocationTextBox?.Text) && Directory.Exists(saveLocationTextBox.Text))
		{
			// Building the full output path for the WAV file using a cleaned file name.
			String path = $"{saveLocationTextBox.Text}\\{Files.CleanFileName(inputTextBox.Text.Replace(" ", "") + ".wav")}";

			// Initializing the speech synthesizer instance for audio generation.
			_synth = new ();
			// Directing the synthesizer output to the target WAV file path.
			_synth.SetOutputToWaveFile(path);

			// Creating a prompt builder and appending the input text to be spoken.
			PromptBuilder builder = new ();
			builder.AppendText(inputTextBox.Text);

			// Speaking the prompt to render the WAV file to disk.
			_synth.Speak(builder);

			// Disposing of the synthesizer and clearing the reference.
			_synth.Dispose();
			_synth = null;

			// Notifying the user that the WAV file was saved successfully.
			MessageBox.Show(
				caption: String.Empty,
				text: $"File saved to: \"{path}\"",
				buttons: MessageBoxButtons.OK,
				icon: MessageBoxIcon.None
			);
		}
		else
		{
			// Notifying the user that validation failed due to missing input,
			// an empty save location, or a non-existent folder.
			MessageBox.Show(
				caption: String.Empty,
				text: "No text in text box and/or where to save box... or the where to save folder does not exist",
				buttons: MessageBoxButtons.OK,
				icon: MessageBoxIcon.None
			);
		}
	}

	/// Description
	/// <summary>
	///		Handles the <see cref="Button.Click"/> event of the create WAV and add
	///		button, synthesizing the input text to a WAV file and registering it
	///		as a soundboard hotkey that is persisted to the settings XML.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
	private void CreateWAVAddButton_Click(Object? sender, EventArgs e)
	{
		// Validating that all required fields are filled in and that the save
		// folder exists before attempting to generate the WAV file and add the hotkey.
		if (!String.IsNullOrWhiteSpace(inputTextBox?.Text) && !String.IsNullOrWhiteSpace(keysTextBox?.Text) && !String.IsNullOrWhiteSpace(saveLocationTextBox?.Text) && Directory.Exists(saveLocationTextBox.Text))
		{
			// Parsing the key combination string into a list of Keys values.
			if (Conversions.KeysArrayFromString(keysTextBox.Text, out List<Keys>? convertedKeys, out String? error))
			{
				// Ensuring that at least one key was parsed successfully.
				if (convertedKeys?.Count > 0)
				{
					// Constructing the new sound hotkey record with the parsed keys,
					// no window restriction, and the generated WAV file location.
					SoundHotkey newSH = new (convertedKeys, "", [$"{saveLocationTextBox.Text}\\{Files.CleanFileName(inputTextBox.Text.Replace(" ", "") + ".wav")}"]);

					// Initializing the speech synthesizer for audio generation.
					_synth = new SpeechSynthesizer();
					// Directing the synthesizer output to the hotkey's WAV file location.
					_synth.SetOutputToWaveFile(newSH.SoundLocations[0]);

					// Creating a prompt builder and appending the input text to be spoken.
					PromptBuilder builder = new ();
					builder.AppendText(inputTextBox.Text);

					// Speaking the prompt to render the WAV file to disk.
					_synth.Speak(builder);

					// Disposing of the synthesizer and clearing the reference.
					_synth.Dispose();
					_synth = null;

					// Adding the new hotkey to the in-memory collection on MainForm.
					_mainForm?.SoundHotkeys.Add(newSH);

					// Creating a list view item with the keys, empty window title, and sound location.
					ListViewItem newItem = new (keysTextBox.Text);
					newItem.SubItems.Add(String.Empty); // Window title
					newItem.SubItems.Add(newSH.SoundLocations[0]);

					// Adding the new item to the key sounds list view on MainForm.
					_mainForm?.KeySoundsListView?.Items.Add(newItem);

					// Configuring the list view sorter by the keys column and sorting the view.
					_mainForm?.KeySoundsListView?.ListViewItemSorter = new Comparers.ListViewItemComparer(0);
					_mainForm?.KeySoundsListView?.Sort();

					// Sorting the sound hotkeys collection on MainForm.
					_mainForm?.SoundHotkeys.Sort(new Comparers.SoundHotkeyComparer());

					// Persisting the newly added TTS hotkey to the settings XML.
					SaveSoundboardSettingsXML();

					// Notifying the user that the WAV file was saved successfully.
					MessageBox.Show(
						caption: String.Empty,
						text: $"File saved to {newSH.SoundLocations[0]}",
						buttons: MessageBoxButtons.OK,
						icon: MessageBoxIcon.None
					);
				}
			}
			else
			{
				// Notifying the user that the keys string was incorrectly formed.
				MessageBox.Show(
					caption: String.Empty,
					text: "Keys String incorrectly made. Check for spelling errors",
					buttons: MessageBoxButtons.OK,
					icon: MessageBoxIcon.None
				);
			}
		}
		else
		{
			// Notifying the user that validation failed due to missing text,
			// keys, save location, or a non-existent folder.
			MessageBox.Show(
				caption: String.Empty,
				text: "No text in text box, keys box, and/or where to save box... or the where to save folder does not exist",
				buttons: MessageBoxButtons.OK,
				icon: MessageBoxIcon.None
			);
		}
	}

	/// Description
	/// <summary>
	///		Handles the <see cref="Button.Click"/> event of the close button,
	///		closing the TTS form.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
	private void CloseButton_Click(Object? sender, EventArgs e)
		=> this.Close(); // Just closing the form.

	/// Description
	/// <summary>
	///		Handles the <see cref="GlobalKeyboardHook.KeysChanged"/> event to
	///		capture global keyboard input for defining hotkey combinations.
	/// </summary>
	///
	/// Parameters
	/// <param name="sender">The source of the event (the global hook).</param>
	/// <param name="currentKeys">A <see cref="HashSet{Keys}"/> containing all currently pressed keys.</param>
	///
	/// Additional information
	/// <remarks>
	///		This method updates the text of the currently focused read-only
	///		text box with the string representation of the pressed key
	///		combination. It handles the <see cref="Keys.Back"/> key to clear the
	///		text box, and <see cref="Keys.Escape"/> to unfocus controls or close
	///		the form. The internal state (<see cref="_lastProcessedKeys"/>) is
	///		managed to avoid unnecessary updates and is reset when all keys are
	///		released.
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