using System.Speech.Synthesis;
using HASCore.Helpers;
using HASCore.Helpers.Extensions;
using HASCore.Keyboard;
using HASCore.Soundboard;

namespace HASCore.Forms;

public partial class TextToSpeechForm : Form
{
    private MainForm? _mainForm;
    private SpeechSynthesizer? _synth;
    /// <summary>
    /// For tracking the last processed key combination.
    /// </summary>
    private HashSet<Keys>? _lastProcessedKeys = null;
    private IEnumerable<TextBox> _tbControls; 

    public TextToSpeechForm()
    {
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

    public void TextToSpeechForm_FormClosing(Object? sender, FormClosingEventArgs e)
    {
        GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
    }

    private void TTS_Load(Object? sender, EventArgs e)
    {
        _mainForm = Application.OpenForms[0] as MainForm;
    }

    private void BrowseFolderLocationButton_Click(Object? sender, EventArgs e)
    {
        FolderBrowserDialog diag = new ();

        if (diag.ShowDialog() == DialogResult.OK)
        {
            saveLocationTextBox?.Text = diag.SelectedPath;
        }
    }

    private void CreateWAVButton_Click(Object? sender, EventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(inputTextBox?.Text) && !String.IsNullOrWhiteSpace(saveLocationTextBox?.Text) && Directory.Exists(saveLocationTextBox.Text))
        {
            String path = saveLocationTextBox.Text + "\\" + Files.CleanFileName(inputTextBox.Text.Replace(" ", "") + ".wav");

            _synth = new ();
            _synth.SetOutputToWaveFile(path);

            PromptBuilder builder = new ();
            builder.AppendText(inputTextBox.Text);

            _synth.Speak(builder);

            _synth.Dispose();
            _synth = null;

            MessageBox.Show("File saved to " + path);
        }
        else
        {
            MessageBox.Show("No text in text box and/or where to save box... or the where to save folder does not exist");
        }
    }

    private void CreateWAVAddButton_Click(Object? sender, EventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(inputTextBox?.Text) && !String.IsNullOrWhiteSpace(keysTextBox?.Text) && !String.IsNullOrWhiteSpace(saveLocationTextBox?.Text) && Directory.Exists(saveLocationTextBox.Text))
        {

            if (Conversions.KeysArrayFromString(keysTextBox.Text, out List<Keys>? convertedKeys, out String? error))
            {
                if (convertedKeys?.Count > 0)
                {
                    XMLSettings.SoundHotkey newSH = new (convertedKeys, "", [saveLocationTextBox.Text + "\\" + Files.CleanFileName(inputTextBox.Text.Replace(" ", "") + ".wav")]);

                    _synth = new SpeechSynthesizer();
                    _synth.SetOutputToWaveFile(newSH.SoundLocations[0]);

                    PromptBuilder builder = new ();
                    builder.AppendText(inputTextBox.Text);

                    _synth.Speak(builder);

                    _synth.Dispose();
                    _synth = null;

                    _mainForm?.SoundHotkeys.Add(newSH);

                    ListViewItem newItem = new (keysTextBox.Text);
                    newItem.SubItems.Add(""); //window title
                    newItem.SubItems.Add(newSH.SoundLocations[0]);

                    _mainForm?.KeySoundsListView?.Items.Add(newItem);

                    _mainForm?.KeySoundsListView?.ListViewItemSorter = new Comparers.ListViewItemComparer(0);
                    _mainForm?.KeySoundsListView?.Sort();

                    _mainForm?.SoundHotkeys.Sort(new Comparers.SoundHotkeyComparer());

                    MessageBox.Show("File saved to " + newSH.SoundLocations[0]);
                }
            }
            else
            {
                MessageBox.Show("Keys String incorrectly made. Check for spelling errors");
            }
        }
        else
        {
            MessageBox.Show("No text in text box, keys box, and/or where to save box... or the where to save folder does not exist");
        }
    }

    private void CloseButton_Click(Object? sender, EventArgs e)
    {
        this.Close();
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