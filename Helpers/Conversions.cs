namespace HASCore.Helpers;

public class Conversions
{
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
        foreach (Keys key in keysList)
            keyStringsList.Add(key.ToString());

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
        foreach (String keyString in stringsList)
        {   
            // If the value can be converted as Keys, then we add it to the list.
            if (Enum.TryParse(keyString, out Keys key)) keysList.Add(key);
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
            foreach (String stringKey in stringKeys)
            {
                // If the value can be converted, we add it to the list.
                if (Enum.TryParse(stringKey, out Keys key)) keys.Add(key);  
                // Otherwise we return an error that we couldn't convert the values.
                else
                {
                    errorMessage = $"Key String \"{stringKey}\" doesn't exist.";
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
            errorMessage = $"Key String \"{keysString}\" doesn't exist.";
            keysList = null;
            return false;
        }
    }

    /// Description
    /// <summary>
    ///     Function to convert a set that is <see cref="IEnumerable"/> of <see cref="Keys"/> into a <see cref="String"/>.  
    /// </summary>
    /// 
    /// <param name="keys">
    ///     A set of keys that need to be converted to a <see cref="String"/> representation.
    /// </param>
    /// 
    /// <returns>
    ///     A <see cref="String"/> of combined keys with '+' separator.
    /// </returns>
    internal static String KeysToString(params IEnumerable<Keys> keys) 
        => String.Join('+', keys.Select(k => k.ToString()).OrderByDescending(s => s));

    internal static Boolean SoundLocsArrayFromString(String soundLocsStr, out List<String>? soundLocs, out String errorMessage)
    {
        if (soundLocsStr.Contains(';'))
        {
            List<String> sLocs = [.. soundLocsStr.Split(';', StringSplitOptions.TrimEntries)];

            foreach (String location in sLocs)
            {
                if (!File.Exists(location))
                {
                    errorMessage = $"File \"{location}\" does not exist";
                    soundLocs = null;
                    return false;
                }
            }

            soundLocs = sLocs;
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
                errorMessage = $"File \"{soundLocsStr}\" does not exist";
                soundLocs = null;
                return false;
            }
        }
    }

    internal static String SoundLocsArrayToString(List<String> soundLocations) => String.Join("; ", soundLocations);    
}
