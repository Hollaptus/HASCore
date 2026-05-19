using System.Speech.Synthesis;
using static JNSoundboardCore.AddEditHotkeyForm;

namespace JNSoundboardCore
{
    public partial class TextToSpeechForm : Form
    {
        private MainForm? MainForm;
        private SpeechSynthesizer? Synth;

        public TextToSpeechForm()
        {
            InitializeComponent();
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

        private void KeysTextBox_Enter(Object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void KeysTextBox_Leave(Object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        int lastAmountPressed = 0;

        private void MainTimer_Tick(Object sender, EventArgs e)
        {
            int amountPressed = 0;

            if (Keyboard.IsKeyDown(Keys.Escape))
            {
                lastAmountPressed = 50;

                tbKeys.Text = "";
            }
            else
            {
                List<Keys> pressedKeys = [];

                foreach (Keys key in Enum.GetValues<Keys>())
                {
                    if (Keyboard.IsKeyDown(key))
                    {
                        amountPressed++;
                        pressedKeys.Add(key);
                    }
                }

                if (amountPressed > lastAmountPressed)
                {
                    tbKeys.Text = Helper.KeysToString(pressedKeys);
                }

                lastAmountPressed = amountPressed;
            }
        }
    }
}
