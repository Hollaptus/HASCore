// Declaring use of this library because of the attributes
// on the partial functions that are being imported from system.
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
    public partial class Helper
    {
        /// Description
        /// <summary>
        ///     Function for finding top-level window through OS-level methods.
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
        ///     Function for getting the active window through OS-level methods.
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
        
        // internal static Boolean IsForegroundWindow(String windowTitle)
        // {
        //     // Trying to get the pointer to the foreground window.
        //     IntPtr foregroundWindow = GetForegroundWindow();

        //     // Return the result of the overloaded function IsForegroundWindow(),
        //     // finding the window by its pointer
        //     return IsForegroundWindow(windowTitle, foregroundWindow);
        // }

        /// Description
        /// <summary>
        ///     Function for checking if the specified window is the foreground window.
        /// </summary>
        /// 
        /// Parameters
        /// <param name="windowTitle">Title of the window that we need to find.</param>
        /// 
        /// Return value
        /// <returns>
        ///     <para>Returns <see cref="true"/> if the window specified by its title is, in fact, the foreground window.</para>
        ///     <para>Otherwise, returns <see cref="false"/>.</para> 
        /// </returns>
        internal static Boolean IsForegroundWindow(String? windowTitle, IntPtr? foregroundWindow = null)
        {
            // If the window title is empty, then there is no window to find.
            if (String.IsNullOrEmpty(windowTitle)) return false;

            // Trying to get a window by its title.
            IntPtr window = FindWindow(null, windowTitle);
            
            // The window hasn't been found, so we return 'false'.
            if (window == IntPtr.Zero) return false;

            // Otherwise we return the result of comparison between
            // the window pointer that has been found and the pointer of
            // a foreground window we try to compare to.
            return foregroundWindow == window;
        }

        /// Description
        /// <summary>
        ///     Function to invoke OS-level file manager for user to select a XML file path.
        /// </summary>
        /// 
        /// Return value
        /// <returns>
        ///     Path to a file that has been selected in the dialog box.
        /// </returns>
        internal static String UserGetXMLLocation()
        {
            SaveFileDialog dialog = new() { Filter = "XML file containing keys and sounds|*.xml" };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : String.Empty;            
        }

        /// Description
        /// <summary>
        ///     Function to convert a <see cref="List"/> of <see cref="Keys"/> into a <see cref="List"/> of <see cref="String"/> values.  
        /// </summary>
        /// 
        /// Parameters
        /// <param name="keysList">
        ///     A <see cref="List"/> of <see cref="Keys"/> that has to be converted to <see cref="String"/> <see cref="List"/>.
        /// </param>
        /// 
        /// Return value
        /// <returns>
        ///     A <see cref="List"/> of <see cref="String"/> values of key combination.
        /// </returns>
        internal static List<String> KeysArrayToStringArray(List<Keys> keysList)
        {
            // Initializing an empty List of Strings that will be converted to with string values of Keys. 
            List<String> keyStringsList = [];

            // Iterating through the list to convert values to String.
            for (Int32 i = 0; i < keysList.Count; i++)
                keyStringsList.Add(keysList[i].ToString());

            // Return the collection of strings that we have built.   
            return keyStringsList;
        }

        /// Description
        /// <summary>
        ///     Function to convert a <see cref="List"/> of <see cref="Keys"/> into an <see cref="List"/> of <see cref="String"/> values.  
        /// </summary>
        /// 
        /// Parameters
        /// <param name="stringsList"></param>
        /// 
        /// Return value
        /// <returns>
        ///     A <see cref="List"/> of <see cref="Keys"/> values of key combination.
        /// </returns>
        internal static List<Keys> StringArrayToKeysArray(List<String> stringsList)
        {
            // If the strings parameter is empty, return an empty array. 
            if (stringsList is null) return [];

            // Initializing an empty List of Keys that the string values will be converted to. 
            List<Keys> keysList = [];

            // Iterating through the list to convert values to Keys.
            for (Int32 i = 0; i < stringsList.Count; i++)
            {   
                // If the value can be converted as Keys, then we add it to the list.
                if (Enum.TryParse(stringsList[i], out Keys key)) keysList.Add(key);
                // Otherwise there is no reason to iterate through other values, 
                // just return an empty List.
                else return [];
            }

            // Return the collection of keys that we have built.
            return keysList;
        }

        /// Description
        /// <summary>
        ///     Function to convert a <see cref="String"/> into a <see cref="List"/> of <see cref="Keys"/>.  
        /// </summary>
        /// 
        /// Parameters
        /// <param name="keysString">A <see cref="String"/> of key combinations that has to be converted to <see cref="Keys"/> <see cref="List"/>.</param>
        /// <param name="keysList">A resulting <see cref="List"/> of <see cref="Keys"/> that has been converted from the <paramref name="keysString"/>.</param>
        /// <param name="errorMessage">An error message that has occured after this function tried to convert the values.</param>
        /// 
        /// Return value
        /// <returns>
        ///     A <see cref="List"/> of <see cref="Keys"/> converted from <paramref name="keysString"/>.
        /// </returns>
        internal static Boolean KeysArrayFromString(String? keysString, out List<Keys>? keysList, out String? errorMessage)
        {
            // If the string is not empty or null, and it is a combination:
            if (!String.IsNullOrEmpty(keysString) && keysString.Contains('+'))
            {
                // Initializing a new list to store different key strings.
                List<String> stringKeys = [..keysString.Split('+')];
                // Initializing a new list to store processed keys.
                List<Keys> keys = [];

                // Iterating through the stringKeys to convert their values to Keys.
                for (Int32 i = 0; i < stringKeys.Count; i++)
                {
                    // If the value can be converted, we add it to the list.
                    if (Enum.TryParse(stringKeys[i], out Keys kKey)) keys.Add(kKey);  
                    // Otherwise we return an error that we couldn't convert the values.
                    else
                    {
                        errorMessage = $"Key String \"{stringKeys[i]}\" doesn't exist.";
                        keysList = null;
                        return false;
                    }
                }

                // Assigning the variables to their respected new values. 
                keysList = [..keys];
                errorMessage = String.Empty;
                // Return that we successfully converted the string.
                return true;
            }
            // If the string is not empty or null, and it is just one key:
            else if (!String.IsNullOrEmpty(keysString) && Enum.TryParse(keysString, out Keys key))
            {
                // Just return a List with one value.
                keysList = [key];
                errorMessage = String.Empty;
                return true;
            }
            // Otherwise we return an error that we cannot convert this string to a List.
            else
            {
                errorMessage = "Key String \"" + keysString + "\" doesn't exist.";
                keysList = null;
                return false;
            }
        }

        /// Description
        /// <summary>
        ///     Function to convert a <see cref="List"/> of <see cref="Keys"> into a <see cref="String"/>.  
        /// </summary>
        /// <param name="keysList"></param>
        /// <returns></returns>
        internal static String KeysToString(params List<Keys> keysList) => String.Join('+', keysList);
        
        internal static Boolean SoundLocsArrayFromString(String soundLocsStr, out List<String>? soundLocs, out String errorMessage)
        {
            if (soundLocsStr.Contains(';'))
            {
                List<String> sLocs = [.. soundLocsStr.Split(';', StringSplitOptions.TrimEntries)];
                List<String> lLocs = [];

                for (Int32 i = 0; i < sLocs.Count; i++)
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

                soundLocs = lLocs;
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

        internal static String SoundLocsArrayToString(List<String> soundLocations) => String.Join("; ", soundLocations);
        
        internal static String CleanFileName(String fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), String.Empty));
        }
    }
}
