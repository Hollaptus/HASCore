using System.Runtime.InteropServices;

namespace JNSoundboardCore
{
    /// Description
    /// <summary>
    /// Helper class for various functions used in other parts of the program, such as:
    /// <list type="bullet|number|table"> 
    ///     <item> 
    ///         <term>Working with windows</term>
    ///         <description>Checking if a certain window is the foreground window</description>
    ///     </item>
    ///     <item>
    ///         <term>Conversion</term>
    ///         <description>Converting between different object types.</description>
    ///     </item>
    /// </list>
    /// </summary>
    partial class Helper
    {
        /// Description
        /// <summary>
        ///     Function for finding top-level window
        /// </summary>
        /// <remarks>
        ///     Retrieves a handle to the top-level window whose class name and window name match the specified Strings. 
        /// </remarks>
        /// 
        /// Parameters
        /// <param name="lpClassName">
        ///     <para>The class name or a class atom created by a previous call to the <c>RegisterClass</c> or <c>RegisterClassEx</c> function.</para>
        ///     <para>If lpClassName points to a <see cref="String"/>, it specifies the window class name.</para>
        ///     <para>If lpClassName is <see cref="null"/>, it finds any window whose title matches the <paramref name="lpWindowName"/></para>
        /// </param>
        /// <param name="lpWindowName">
        ///     The window name (the window's title). If this parameter is <see cref="null"/>, all window names match.
        /// </param>
        /// 
        /// Return value
        /// <returns>
        ///     If the function succeeds, the return value is a handle to the window. 
        ///     If the function fails, the return value is <see cref="null"/>.
        /// </returns>
        [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr FindWindow(String? lpClassName, String lpWindowName);
        
        /// Description
        /// <summary>
        ///     Function for getting the active window
        /// </summary>
        /// <remarks>
        ///     Retrieves a handle to the foreground window (the window with which the user is currently working).
        /// </remarks>
        /// 
        /// Return value
        /// <returns>
        ///     <para>The return value is a handle to the foreground window.</para> 
        ///     <para>The foreground window can be NULL in certain circumstances, such as when a window is losing activation.</para>
        /// </returns>
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial IntPtr GetForegroundWindow();
        
        internal static Boolean IsForegroundWindow(String windowTitle)
        {
            IntPtr foregroundWindow = GetForegroundWindow();

            return IsForegroundWindow(foregroundWindow, windowTitle);
        }

        internal static Boolean IsForegroundWindow(IntPtr foregroundWindow, String windowTitle)
        {
            IntPtr window = FindWindow(null, windowTitle);

            if (window == IntPtr.Zero) return false; //not found

            return foregroundWindow == window;
        }

        internal static String UserGetXMLLocation()
        {
            SaveFileDialog dialog = new() { Filter = "XML file containing keys and sounds|*.xml" };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : "";            
        }

        internal static List<String> KeysArrayToStringArray(Keys[] keysArr)
        {
            List<String> arr = [];
            for (Int32 i = 0; i < keysArr.Length; i++)
                arr.Add(keysArr[i].ToString());
            return [.. arr];
        }

        internal static Keys[] StringArrayToKeysArray(List<String> strArr)
        {
            if (strArr is null) return [0];
            List<Keys> arr = [];

            for (Int32 i = 0; i < strArr.Count; i++)
            {
                if (Enum.TryParse(strArr[i], out Keys key)) arr.Add(key);
                else return [0];
            }

            return [.. arr];
        }

        internal static Boolean KeysArrayFromString(String key, out Keys[]? keysArr, out String errorMessage)
        {
            if (key.Contains('+'))
            {
                List<String> sKeys = [..key.Split('+')];
                List<Keys> kKeys = new List<Keys>();

                for (Int32 i = 0; i < sKeys.Count; i++)
                {
                    if (Enum.TryParse(sKeys[i], out Keys kKey)) kKeys.Add(kKey);  
                    else
                    {
                        errorMessage = $"Key String \"{sKeys[i]}\" doesn't exist";
                        keysArr = null;
                        return false;
                    }
                }

                keysArr = [..kKeys];
                errorMessage = String.Empty;
                return true;
            }
            else
            {
                if (Enum.TryParse(key, out Keys kKey))
                {
                    keysArr = [kKey];
                    errorMessage = String.Empty;
                    return true;
                }
                else
                {
                    errorMessage = "Key String \"" + key + "\" doesn't exist";
                    keysArr = null;
                    return false;
                }
            }
        }

        internal static String KeysToString(params Keys[] keysArr)
        {
            if (keysArr is null) return "";
            String temp = "";
            Int32 kLen = keysArr.Length;

            for (Int32 i = 0; i < kLen; i++)
            {
                temp += keysArr[i].ToString() + (i == kLen - 1 ? "" : "+");
            }

            return temp;
        }

        internal static Boolean SoundLocsArrayFromString(String soundLocsStr, out String[] soundLocs, out String errorMessage)
        {
            if (soundLocsStr.Contains(';'))
            {
                String[] sLocs = soundLocsStr.Split(';');
                List<String> lLocs = new List<String>();

                for (Int32 i = 0; i < sLocs.Length; i++)
                {
                    if (File.Exists(sLocs[i]))
                    {
                        lLocs.Add(sLocs[i]);
                    }
                    else
                    {
                        errorMessage = "File \"" + sLocs[i] + "\" does not exist";
                        soundLocs = null;
                        return false;
                    }
                }

                soundLocs = lLocs.ToArray();
                errorMessage = String.Empty;
                return true;
            }
            else
            {
                if (File.Exists(soundLocsStr))
                {
                    soundLocs = [soundLocsStr];
                    errorMessage = String.Empty;
                    return true;
                }
                else
                {
                    errorMessage = "File \"" + soundLocsStr + "\" does not exist";
                    soundLocs = null;
                    return false;
                }
            }
        }

        internal static String SoundLocsArrayToString(String[] soundLocations)
        {
            String temp = "";
            Int32 sLen = soundLocations.Length;

            for (Int32 i = 0; i < sLen; i++)
            {
                temp += soundLocations[i].ToString() + (i == sLen - 1 ? "" : ";");
            }

            return temp;
        }

        internal static String CleanFileName(String fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), String.Empty));
        }
    }
}
