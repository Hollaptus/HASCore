using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HASCore.Soundboard;

public class XMLSettings
{
    readonly static SoundboardSettings DEFAULT_SOUNDBOARD_SETTINGS = new (
        enableSoundboardKeys: [],
        stopSoundKeys: [],
        loadXMLFiles: [new LoadXMLFile([], String.Empty)],
        minimizeToTray: true, 
        playOverEachother: false, 
        repeatOnHold: false, 
        delayInMs: 500, 
        lastLoopbackDevice: String.Empty, 
        lastPlaybackDevice: String.Empty
    );

    internal static SoundboardSettings CurrentSettings = new ();

    #region Keys and sounds settings
    public class SoundHotkey
    {
        public SoundHotkey() {}

        public SoundHotkey(List<Keys> keys, String windowTitle, List<String> soundLocs)
        {
            Keys = keys;
            WindowTitle = windowTitle;
            SoundLocations = soundLocs;
        }

        public List<Keys>? Keys { get; set; } = null;
        public String? WindowTitle { get; set; } = null;
        public List<String> SoundLocations { get; set; } = [];
    }

    [Serializable]
    public class Settings
    {
        public Settings() {}

        public Settings(List<SoundHotkey> sh)
        {
            SoundHotkeys = sh;
        }

        public List<SoundHotkey>? SoundHotkeys;

    }
    #endregion

    #region Soundboard settings
    public class LoadXMLFile
    {
        public LoadXMLFile() {}
        
        public LoadXMLFile(List<Keys> keys, String xmlLocation)
        {
            Keys = keys;
            XMLLocation = xmlLocation;
        }
        
        public List<Keys>? Keys;
        public String? XMLLocation;
    }

    [Serializable]
    public class SoundboardSettings
    {
        public SoundboardSettings() { }

        public SoundboardSettings(
            List<Keys> enableSoundboardKeys,
            List<Keys> stopSoundKeys,
            List<LoadXMLFile> loadXMLFiles,
            Boolean minimizeToTray,
            Boolean playOverEachother,
            Boolean repeatOnHold,
            Int32 delayInMs,
            String lastPlaybackDevice,
            String lastLoopbackDevice)
        {
            EnableSoundboardKeys = enableSoundboardKeys;
            StopSoundKeys = stopSoundKeys;
            LoadXMLFiles = loadXMLFiles;
            MinimizeToTray = minimizeToTray;
            PlayOverEachother = playOverEachother;
            RepeatOnHold = repeatOnHold;
            DelayInMs = delayInMs;
            LastPlaybackDevice = lastPlaybackDevice;
            LastLoopbackDevice = lastLoopbackDevice;
        }

        public List<Keys>? EnableSoundboardKeys;
        public List<Keys>? StopSoundKeys;
        public List<LoadXMLFile>? LoadXMLFiles;
        public Boolean? MinimizeToTray;
        public Boolean? PlayOverEachother;
        public Boolean? RepeatOnHold;
        public Int32? DelayInMs;
        public String? LastPlaybackDevice;
        public String? LastLoopbackDevice;
    }
    #endregion

    internal static void WriteXML(Object kl, String xmlLoc)
    {
        XmlSerializer serializer = new (kl.GetType());

        using (MemoryStream memStream = new ())
        {
            using (StreamWriter stream = new (memStream, Encoding.Unicode))
            {
                XmlWriterSettings settings = new ()
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    XmlSerializerNamespaces emptyNamepsaces = new ([XmlQualifiedName.Empty]);
                    serializer.Serialize(writer, kl, emptyNamepsaces);

                    Int32 count = (Int32)memStream.Length;

                    Byte[] arr = new Byte[count];
                    memStream.Seek(0, SeekOrigin.Begin);

                    memStream.Read(arr, 0, count);

                    using (BinaryWriter binWriter = new (File.Open(xmlLoc, FileMode.Create)))
                    {
                        binWriter.Write(arr);
                    }
                }
            }
        }
    }

    internal static Object? ReadXML(Type type, String xmlLoc)
    {
        XmlSerializer serializer = new (type);

        using (XmlReader reader = XmlReader.Create(xmlLoc))
        {
            if (serializer.CanDeserialize(reader))
            {
                return serializer.Deserialize(reader)!;
            }
            else return null;
        }
    }

    internal static void SaveSoundboardSettingsXML()
    {
        WriteXML(CurrentSettings, Path.GetDirectoryName(Application.ExecutablePath) + "\\settings.xml");
    }

    internal static void LoadSoundboardSettingsXML()
    {
        String filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\settings.xml";

        if (File.Exists(filePath))
        {
            SoundboardSettings? settings;

            try
            {
                settings = ReadXML(typeof(SoundboardSettings), filePath) as SoundboardSettings;
            }
            catch
            {
                CurrentSettings = DEFAULT_SOUNDBOARD_SETTINGS;
                return;
            }

            if (settings == null)
            {
                CurrentSettings = DEFAULT_SOUNDBOARD_SETTINGS;
                return;
            }

            settings.StopSoundKeys ??= [];

            settings.LoadXMLFiles ??= [];

            settings.LastPlaybackDevice ??= String.Empty;

            settings.LastLoopbackDevice ??= String.Empty;

            settings.DelayInMs ??= 500;

            CurrentSettings = settings;
        }
        else
        {
            WriteXML(DEFAULT_SOUNDBOARD_SETTINGS, filePath);
            CurrentSettings = DEFAULT_SOUNDBOARD_SETTINGS;
        }
    }
}