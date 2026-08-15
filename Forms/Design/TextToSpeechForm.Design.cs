// Explicitly declaring libraries that will be used
// so we don't have gigantic lines of code of library imports.
using System.ComponentModel;

namespace HASCore.Forms;

/// <summary>
/// <see cref="TextToSpeechForm"/> class part for implementing the initialization 
/// </summary>
public partial class TextToSpeechForm
{
    /// Description
    /// <summary>
    ///     Container for components on this form.
    /// </summary>
    private Container? components = null;
    /// Description
    /// <summary>
    ///     Text box for entering the text to convert to speech.
    /// </summary>
    private TextBox? inputTextBox;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="inputTextBox">textTextBox</seealso>.
    /// </summary>
    private Label? inputLabel;
    /// Description
    /// <summary>
    ///     Button to close the form.
    /// </summary>
    private Button? closeButton;
    /// Description
    /// <summary>
    ///     Text box for entering the key combination to assign to the generated sound.
    /// </summary>
    private TextBox? keysTextBox;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="keysTextBox">keysTextBox</seealso>.
    /// </summary>
    private Label? keysLabel;
    /// Description
    /// <summary>
    ///     Label for <seealso cref="saveLocationTextBox">saveLocationTextBox</seealso>.
    /// </summary>
    private Label? saveLocationLabel;
    /// Description
    /// <summary>
    ///     Text box for the folder where the generated WAV file will be saved.
    /// </summary>
    private TextBox? saveLocationTextBox;
    /// Description
    /// <summary>
    ///     Button to browse for a folder to save the generated WAV.
    /// </summary>
    private Button? browseFolderButton;
    /// Description
    /// <summary>
    ///     Button to create a WAV file from the entered text (without adding to the soundboard).
    /// </summary>
    private Button? createWavButton;
    /// Description
    /// <summary>
    ///     Button to create a WAV file and add it to the soundboard list.
    /// </summary>
    private Button? createWavAddButton;

    /// Description
    /// <summary>
    ///     Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    /// Description
    /// <summary>
    ///     Component initialization on program startup.
    /// </summary>
    /// <remarks>
    ///     This procedure is needed for two things:
    ///     <list type="number">
    ///         <item>
    ///             <term>Construction of a class</term>
    ///             <description>procedure is called upon constructing a class</description>
    ///         </item>
    ///         <item>
    ///             <term>Class properties initialization</term>
    ///             <description>procedure is initializing components that will be used in a form</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    private void InitializeComponent()
    {
        // ------------------------
        // Initializing new objects
        // ------------------------

        // Component resource manager
        ComponentResourceManager resources  = new(typeof(TextToSpeechForm));
        // Component container            
        this.components                     = new Container();
        // Labels
        this.inputLabel                     = new Label();
        this.keysLabel                      = new Label();
        this.saveLocationLabel              = new Label();
        // Buttons
        this.closeButton                    = new Button();
        this.browseFolderButton             = new Button();
        this.createWavButton                = new Button();
        this.createWavAddButton             = new Button();
        // Text boxes
        this.inputTextBox                   = new TextBox();
        this.keysTextBox                    = new TextBox();
        this.saveLocationTextBox            = new TextBox();

        // Suspending layout logic before adding controls
        // for child objects to initialize without firing events
        this.SuspendLayout();

        // ------------------------
        // Adding object properties
        // ------------------------

        // Labels

        // 
        // textLabel
        // 
        this.inputLabel.Anchor = AnchorStyles.Top;
        this.inputLabel.AutoSize = true;
        this.inputLabel.Location = new Point(193, 9);
        this.inputLabel.Name = "textLabel";
        this.inputLabel.Size = new Size(28, 13);
        this.inputLabel.TabIndex = 1;
        this.inputLabel.Text = "Text";
        // 
        // keysLabel
        // 
        this.keysLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.keysLabel.AutoSize = true;
        this.keysLabel.Location = new Point(122, 147);
        this.keysLabel.Name = "keysLabel";
        this.keysLabel.Size = new Size(30, 13);
        this.keysLabel.TabIndex = 8;
        this.keysLabel.Text = "Keys";
        // 
        // saveLocationLabel
        // 
        this.saveLocationLabel.Anchor = AnchorStyles.Top;
        this.saveLocationLabel.AutoSize = true;
        this.saveLocationLabel.Location = new Point(131, 58);
        this.saveLocationLabel.Name = "saveLocationLabel";
        this.saveLocationLabel.Size = new Size(143, 13);
        this.saveLocationLabel.TabIndex = 9;
        this.saveLocationLabel.Text = "Where to save perm TTS file";
        
        // Buttons

        // 
        // closeButton
        // 
        this.closeButton.Anchor = AnchorStyles.Bottom;
        this.closeButton.Location = new Point(170, 201);
        this.closeButton.Name = "closeButton";
        this.closeButton.Size = new Size(75, 23);
        this.closeButton.TabIndex = 5;
        this.closeButton.Text = "Close";
        this.closeButton.UseVisualStyleBackColor = true;
        this.closeButton.Click += this.CloseButton_Click;
        // 
        // browseFolderButton
        // 
        this.browseFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        this.browseFolderButton.Location = new Point(373, 72);
        this.browseFolderButton.Name = "browseFolderButton";
        this.browseFolderButton.Size = new Size(25, 23);
        this.browseFolderButton.TabIndex = 2;
        this.browseFolderButton.Text = "...";
        this.browseFolderButton.UseVisualStyleBackColor = true;
        this.browseFolderButton.Click += this.BrowseFolderLocationButton_Click;
        // 
        // createWavButton
        // 
        this.createWavButton.Anchor = AnchorStyles.Top;
        this.createWavButton.Location = new Point(162, 108);
        this.createWavButton.Name = "createWavButton";
        this.createWavButton.Size = new Size(98, 23);
        this.createWavButton.TabIndex = 12;
        this.createWavButton.Text = "Only create WAV";
        this.createWavButton.UseVisualStyleBackColor = true;
        this.createWavButton.Click += this.CreateWAVButton_Click;
        // 
        // createWavAddButton
        // 
        this.createWavAddButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.createWavAddButton.Location = new Point(255, 161);
        this.createWavAddButton.Name = "createWavAddButton";
        this.createWavAddButton.Size = new Size(143, 23);
        this.createWavAddButton.TabIndex = 4;
        this.createWavAddButton.Text = "Create WAV and add to list";
        this.createWavAddButton.UseVisualStyleBackColor = true;
        this.createWavAddButton.Click += this.CreateWAVAddButton_Click;
        
        // Text boxes

        // 
        // textTextBox
        // 
        this.inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.inputTextBox.Location = new Point(14, 25);
        this.inputTextBox.Name = "textTextBox";
        this.inputTextBox.Size = new Size(384, 20);
        this.inputTextBox.TabIndex = 0;
        // 
        // keysTextBox
        // 
        this.keysTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.keysTextBox.Location = new Point(14, 163);
        this.keysTextBox.Name = "keysTextBox";
        this.keysTextBox.ReadOnly = true;
        this.keysTextBox.Size = new Size(235, 20);
        this.keysTextBox.TabIndex = 3;
        // 
        // saveLocationTextBox
        // 
        this.saveLocationTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        this.saveLocationTextBox.Location = new Point(14, 74);
        this.saveLocationTextBox.Name = "saveLocationTextBox";
        this.saveLocationTextBox.Size = new Size(353, 20);
        this.saveLocationTextBox.TabIndex = 1;

        // Form

        // 
        // TextToSpeechForm
        // 
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(414, 231);
        this.Icon = (Icon?)resources.GetObject("$this.Icon");
        this.MaximizeBox = false;
        this.MaximumSize = new Size(4000, 270);
        this.MinimumSize = new Size(422, 270);
        this.Name = "TextToSpeechForm";
        this.Text = "TTS";
        this.Load += this.TTS_Load;
        this.FormClosing += this.TextToSpeechForm_FormClosing;

        // Adding the controls to the form
        this.Controls.Add(this.inputLabel);
        this.Controls.Add(this.keysLabel);
        this.Controls.Add(this.saveLocationLabel);
        this.Controls.Add(this.closeButton);
        this.Controls.Add(this.browseFolderButton);
        this.Controls.Add(this.createWavButton);
        this.Controls.Add(this.createWavAddButton);
        this.Controls.Add(this.inputTextBox);
        this.Controls.Add(this.keysTextBox);
        this.Controls.Add(this.saveLocationTextBox);

        // After initializing all the objects and their properties,
        // we need to resume layout logic and apply it forcibly
        // Remark: this doesn't equal to ResumeLayout(true)
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}