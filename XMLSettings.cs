using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace JNSoundboardCore
{
    public class XMLSettings
    {
        readonly static SoundboardSettings DEFAULT_SOUNDBOARD_SETTINGS = new([], [], [new LoadXMLFile([], "")], true, "", "");

        internal static SoundboardSettings CurrentSettings = new();

        //saving XML files like this makes the XML messy, but it works
        #region Keys and sounds settings
        public class SoundHotkey
        {
            public List<Keys>? Keys;
            public String? WindowTitle;
            public List<String> SoundLocations = [];
            public SoundHotkey() {}

            public SoundHotkey(List<Keys> keys, String windowTitle, List<String> soundLocs)
            {
                Keys = keys;
                WindowTitle = windowTitle;
                SoundLocations = soundLocs;
            }
        }

        [Serializable]
        public class Settings
        {
            public List<SoundHotkey>? SoundHotkeys;

            public Settings() {}

            public Settings(List<SoundHotkey> sh)
            {
                SoundHotkeys = sh;
            }
        }
        #endregion

        #region Soundboard settings
        public class LoadXMLFile
        {
            public List<Keys>? Keys;
            public String? XMLLocation;
            public LoadXMLFile() {}
            public LoadXMLFile(List<Keys> keys, String xmlLocation)
            {
                Keys = keys;
                XMLLocation = xmlLocation;
            }
        }

        [Serializable]
        public class SoundboardSettings
        {
            public List<Keys>? EnableSoundboardKeys;
            public List<Keys>? StopSoundKeys;
            public List<LoadXMLFile>? LoadXMLFiles;
            public Boolean? MinimizeToTray;
            public String? LastPlaybackDevice, LastLoopbackDevice;
            public SoundboardSettings() { }

            public SoundboardSettings(
                List<Keys> enableSoundboard,
                List<Keys> stopSoundKeys,
                List<LoadXMLFile> loadXMLFiles,
                Boolean minimizeToTray,
                String lastPlaybackDevice,
                String lastLoopbackDevice)
            {
                EnableSoundboardKeys = enableSoundboard;
                StopSoundKeys = stopSoundKeys;
                LoadXMLFiles = loadXMLFiles;
                MinimizeToTray = minimizeToTray;
                LastPlaybackDevice = lastPlaybackDevice;
                LastLoopbackDevice = lastLoopbackDevice;
            }
        }
        #endregion

        internal static void WriteXML(Object kl, String xmlLoc)
        {
            XmlSerializer serializer = new(kl.GetType());

            using (MemoryStream memStream = new())
            {
                using (StreamWriter stream = new(memStream, Encoding.Unicode))
                {
                    XmlWriterSettings settings = new();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = true;

                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        XmlSerializerNamespaces emptyNamepsaces = new([XmlQualifiedName.Empty]);
                        serializer.Serialize(writer, kl, emptyNamepsaces);

                        Int32 count = (Int32)memStream.Length;

                        Byte[] arr = new Byte[count];
                        memStream.Seek(0, SeekOrigin.Begin);

                        memStream.Read(arr, 0, count);

                        using (BinaryWriter binWriter = new(File.Open(xmlLoc, FileMode.Create)))
                        {
                            binWriter.Write(arr);
                        }
                    }
                }
            }
        }

        internal static Object? ReadXML(Type type, String xmlLoc)
        {
            XmlSerializer serializer = new(type);

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

                CurrentSettings = settings;
            }
            else
            {
                WriteXML(DEFAULT_SOUNDBOARD_SETTINGS, filePath);
                CurrentSettings = DEFAULT_SOUNDBOARD_SETTINGS;
            }
        }
    }
}
