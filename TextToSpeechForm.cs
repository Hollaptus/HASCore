using System.Speech.Synthesis;
using static HASCore.AddEditHotkeyForm;

namespace HASCore
{
    public partial class TextToSpeechForm : Form
    {
        private MainForm? MainForm;
        private SpeechSynthesizer? Synth;

        public TextToSpeechForm()
        {
            InitializeComponent();
            GlobalKeyboardHook.Initialize();
            GlobalKeyboardHook.KeysChanged += OnKeysChanged;
        }

        public void TextToSpeechForm_FormClosing(Object? sender, FormClosingEventArgs e)
        {
            GlobalKeyboardHook.KeysChanged -= OnKeysChanged;
        }

        private void TTS_Load(Object sender, EventArgs e)
        {
            MainForm = Application.OpenForms[0] as MainForm;
        }

        private void BrowseFolderLocationButton_Click(Object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new();

            if (diag.ShowDialog() == DialogResult.OK)
            {
                tbWhereSave.Text = diag.SelectedPath;
            }
        }

        private void CreateWAVButton_Click(Object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbText.Text) && !String.IsNullOrWhiteSpace(tbWhereSave.Text) && Directory.Exists(tbWhereSave.Text))
            {
                String path = tbWhereSave.Text + "\\" + Helper.CleanFileName(tbText.Text.Replace(" ", "") + ".wav");

                Synth = new();
                Synth.SetOutputToWaveFile(path);

                PromptBuilder builder = new();
                builder.AppendText(tbText.Text);

                Synth.Speak(builder);

                Synth.Dispose();
                Synth = null;

                MessageBox.Show("File saved to " + path);
            }
            else
            {
                MessageBox.Show("No text in text box and/or where to save box... or the where to save folder does not exist");
            }
        }

        private void CreateWAVAddButton_Click(Object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tbText.Text) && !String.IsNullOrWhiteSpace(tbKeys.Text) && !String.IsNullOrWhiteSpace(tbWhereSave.Text) && Directory.Exists(tbWhereSave.Text))
            {

                if (Helper.KeysArrayFromString(tbKeys.Text, out List<Keys>? convertedKeys, out String? error))
                {
                    if (convertedKeys?.Count > 0)
                    {
                        XMLSettings.SoundHotkey newSH = new(convertedKeys, "", [tbWhereSave.Text + "\\" + Helper.CleanFileName(tbText.Text.Replace(" ", "") + ".wav")]);

                        Synth = new SpeechSynthesizer();
                        Synth.SetOutputToWaveFile(newSH.SoundLocations[0]);

                        PromptBuilder builder = new();
                        builder.AppendText(tbText.Text);

                        Synth.Speak(builder);

                        Synth.Dispose();
                        Synth = null;

                        MainForm?.SoundHotkeys.Add(newSH);

                        ListViewItem newItem = new(tbKeys.Text);
                        newItem.SubItems.Add(""); //window title
                        newItem.SubItems.Add(newSH.SoundLocations[0]);

                        MainForm?.KeySoundsListView?.Items.Add(newItem);

                        MainForm?.KeySoundsListView?.ListViewItemSorter = new ListViewItemComparer(0);
                        MainForm?.KeySoundsListView?.Sort();

                        MainForm?.SoundHotkeys.Sort(delegate (XMLSettings.SoundHotkey x, XMLSettings.SoundHotkey y)
                        {
                            if (x.Keys == null && y.Keys == null) return 0;
                            else if (x.Keys == null) return -1;
                            else if (y.Keys == null) return 1;
                            else return Helper.KeysToString(x.Keys).CompareTo(Helper.KeysToString(y.Keys));
                        });

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

        private void CloseButton_Click(Object sender, EventArgs e)
        {
            this.Close();
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
    }
}
